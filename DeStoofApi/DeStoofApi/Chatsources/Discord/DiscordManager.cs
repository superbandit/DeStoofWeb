using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DeStoofApi.Models;
using System.Collections.Generic;
using DeStoofApi.EventArgs;
using DeStoofApi.Models.Guilds;
using MongoDB.Driver;

namespace DeStoofApi.Chatsources.Discord
{
    public class DiscordManager
    {
        public delegate void MessageReceivedHandler(object sender, MessageReceivedEventArgs args);
        public event MessageReceivedHandler MessageReceived;

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMongoCollection<GuildSettings> _guildSettings;

        private List<SocketGuild> Guilds = new List<SocketGuild>();

        public DiscordManager(IConfiguration config, DiscordSocketClient client, CommandService commandService, IServiceProvider serviceProvider, IMongoDatabase mongoDatabase)
        {
            _config = config;
            _client = client;
            _commandService = commandService;
            _serviceProvider = serviceProvider;
            _guildSettings = mongoDatabase.GetCollection<GuildSettings>("guildSettings");
        }


        public async Task<bool> RunBotAsync()
        {
            if (_client == null)
                return false;

            string botToken = $"{_config["Secure:Discordtoken"]}";

            //event subscriptions
            _client.Log += Log;
            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, botToken);
            await _client.StartAsync();
            _client.Ready += ClientReady;
            _client.JoinedGuild += GuildJoined;

            return true;
        }

        private async Task GuildJoined(SocketGuild arg)
        {
            Guilds.Add(arg);

            GuildSettings guildSettings = new GuildSettings
            {
                GuildId = arg.Id
            };

            await _guildSettings.InsertOneAsync(guildSettings);
        }

        private Task ClientReady()
        {
            foreach (SocketGuild guild in _client.Guilds)
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
            _client.MessageReceived += HandleMessageAsync;

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleMessageAsync(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message) || message.Author.IsBot) return;

            if (MessageReceived == null)
                return;

            MessageReceived(this, new MessageReceivedEventArgs(new ChatMessage
            {
                Platform = Enums.Platforms.Discord,
                User = ((IGuildUser) message.Author).Nickname,
                Channel = message.Channel.Name,
                Message = message.Content,
                Date = DateTime.Now.ToString()
            }));

            int argPos = 0;

            if (message.HasStringPrefix("!", ref argPos))
            {
                SocketCommandContext context = new SocketCommandContext(_client, message);

                var result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
                if (!result.IsSuccess)
                {
                    Debug.WriteLine(result.ErrorReason);
                }
            }
        }

        public async void SendDiscordMessage(ulong channelNumber, string message)
        {
            SocketTextChannel channel = _client.GetChannel(channelNumber) as SocketTextChannel;
            if (channel != null) await channel.SendMessageAsync(message);
        }
    }
}
