﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DeStoofApi.EventArguments;
using DeStoofApi.Models;
using System.Collections.Generic;

namespace DeStoofApi.Chatsources
{
    public class DiscordManager
    {
        public delegate void MessageReceivedHandler(object sender, MessageReceivedEventArgs args);
        public event MessageReceivedHandler MessageReceived;

        public DiscordSocketClient client;
        private CommandService commands;
        private IServiceProvider services;
        readonly IConfiguration Config;

        public List<SocketGuild> Guilds = new List<SocketGuild>();

        public DiscordManager(IConfiguration config)
        {
            Config = config;
        }


        public async Task<bool> RunBotAsync()
        {
            if (client != null)
                return false;
            client = new DiscordSocketClient();
            commands = new CommandService();

            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .BuildServiceProvider();

            string botToken = $"{Config["Secure:Discordtoken"]}";

            //event subscriptions
            client.Log += Log;

            await RegisterCommandsAsync();
            await client.LoginAsync(TokenType.Bot, botToken);
            await client.StartAsync();
            client.Ready += ClientReady;

            return true;
        }

        private Task ClientReady()
        {
            foreach (SocketGuild guild in client.Guilds)
                Guilds.Add(guild);
            return Task.FromResult(0);
        }

        private Task Log(LogMessage arg)
        {
            Debug.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            client.MessageReceived += HandleMessageAsync;

            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleMessageAsync(SocketMessage arg)
        {
            SocketUserMessage message = arg as SocketUserMessage;

            if (message is null || message.Author.IsBot) return;

            MessageReceived(this, new MessageReceivedEventArgs(new ChatMessage
            {
                Platform = Enums.Platforms.discord,
                User = ((IGuildUser)message.Author).Nickname,
                Channel = message.Channel.Name,
                Message = message.Content,
                Date = DateTime.Now.ToString()
            }));

            int argPos = 0;

            if (message.HasStringPrefix("!", ref argPos))
            {
                SocketCommandContext context = new SocketCommandContext(client, message);

                var result = await commands.ExecuteAsync(context, argPos, services);
                if (!result.IsSuccess)
                {
                    Debug.WriteLine(result.ErrorReason);
                }
            }
        }

        public async void SendDiscordMessage(ulong channelNumber, string message)
        {
            SocketTextChannel channel = client.GetChannel(channelNumber) as SocketTextChannel;
            await channel.SendMessageAsync(message);
        }
    }
}