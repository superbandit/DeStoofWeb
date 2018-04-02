using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DeStoofApi.EventArguments;
using DeStoofApi.Models;

namespace DeStoofApi.Chatsources
{
    public class DiscordManager
    {
        public delegate void MessageReceivedHandler(object sender, MessageReceivedEventArgs args);
        public event MessageReceivedHandler MessageReceived;

        public DiscordSocketClient client;
        private CommandService commands;
        private IServiceProvider services;

        public SocketTextChannel channel;


        public async Task RunBotAsync()
        {
            client = new DiscordSocketClient();
            commands = new CommandService();

            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .BuildServiceProvider();

            string botToken = "NDE2Njk4NTk3OTIxNTIxNjc1.DXIQjw.MQ5hZ4b3GV4St1mDEM-573tnUF4";

            //event subscriptions
            client.Log += Log;

            await RegisterCommandsAsync();
            await client.LoginAsync(TokenType.Bot, botToken);
            await client.StartAsync();
            client.Ready += ClientReady;
            //await Task.Delay(-1);
        }

        private Task ClientReady()
        {
            channel = client.GetChannel(416714064283303956) as SocketTextChannel;
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

            if (message.HasStringPrefix("!", ref argPos) && message.Channel == channel)
            {
                SocketCommandContext context = new SocketCommandContext(client, message);

                var result = await commands.ExecuteAsync(context, argPos, services);
                if (!result.IsSuccess)
                {
                    Debug.WriteLine(result.ErrorReason);
                }
            }
        }

        public async void SendDiscordMessage(string message)
        {
            SocketTextChannel channel = client.GetChannel(416714064283303956) as SocketTextChannel;
            await channel.SendMessageAsync(message);
        }
    }
}
