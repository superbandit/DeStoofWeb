using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using DeStoofApi.EventArgs;
using DeStoofApi.Models.ChatMessages;
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
            _guildSettings = mongoDatabase.GetCollection<GuildSettings>(config["Secure:GuildSettings"]);
        }


        public async Task<bool> RunBotAsync()
        {
            if (_client == null)
                return false;

            string botToken = _config["Secure:Discordtoken"];

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

            await _guildSettings.ReplaceOneAsync(g => g.GuildId == arg.Id,
                new GuildSettings { GuildId = arg.Id }, new UpdateOptions { IsUpsert = true });
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

        private async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleMessageAsync;

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleMessageAsync(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message) || message.Author.IsBot) return;
            if (!(message.Channel is SocketGuildChannel channel)) return;

            if (MessageReceived == null)
                return;

            MessageReceived(this, new MessageReceivedEventArgs(new DiscordChatMessage
            {
                User = ((IGuildUser) message.Author).Nickname ?? message.Author.Username,
                UserId = message.Author.Id,
                GuildId = channel.Guild.Id,
                Message = message.Content,
                Date = DateTime.Now
            }));

            var settings = await (await _guildSettings.FindAsync(g => g.GuildId == channel.Guild.Id)).FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new GuildSettings { GuildId = channel.Guild.Id};
                await _guildSettings.InsertOneAsync(settings);
            }

            int argPos = 0;

            if (message.HasStringPrefix(settings.CommandPrefix, ref argPos))
            {
                SocketCommandContext context = new SocketCommandContext(_client, message);

                var result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
                if (!result.IsSuccess)
                {
                    Debug.WriteLine(result.ErrorReason);
                }
            }
        }

        public async void SendDiscordMessage(ulong channelNumber, ChatMessage message)
        {
            SocketTextChannel channel = _client.GetChannel(channelNumber) as SocketTextChannel;

            string toSend = $"**{message.User}**: {message.Message}";
            if (channel != null) await channel.SendMessageAsync(toSend);
        }

        public async Task PartGuild(ulong guildId)
        {
            await _client.GetGuild(guildId).LeaveAsync();
        }
    }
}
