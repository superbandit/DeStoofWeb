using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using DeStoofApi.Models.Form.Discord;
using DeStoofApi.Models.Guilds;
using DeStoofApi.Models.Messages;
using DeStoofApi.Services;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace DeStoofApi.Chatsources.Discord
{
    public class DiscordManager
    {

        private readonly DiscordSocketClient _client;

        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMongoCollection<GuildSettings> _guildSettings;
        private readonly LoggingService _loggingService;

        public DiscordManager(IConfiguration config, DiscordSocketClient client, IServiceProvider serviceProvider, IMongoDatabase mongoDatabase, LoggingService loggingService)
        {
            _config = config;
            _client = client;
            _serviceProvider = serviceProvider;
            _loggingService = loggingService;
            _guildSettings = mongoDatabase.GetCollection<GuildSettings>(config["Secure:GuildSettings"]);
        }


        public async Task<bool> RunBotAsync()
        {
            _client.Log += Log;
            _serviceProvider.GetService<CommandService>().Log += Log;

            await _client.LoginAsync(TokenType.Bot, _config["Secure:Discordtoken"]);
            await _client.StartAsync();

            await _serviceProvider.GetService<CommandHandler>().InitializeAsync();

            _client.Ready += ClientReady;
            _client.JoinedGuild += GuildJoined;
            _client.LeftGuild += GuildLeft;
            
            return true;
        }

        private async Task GuildJoined(SocketGuild arg)
        {
            var result = await _guildSettings.ReplaceOneAsync(g => g.GuildId == arg.Id,
                new GuildSettings { GuildId = arg.Id }, new UpdateOptions { IsUpsert = true });

            await _loggingService.LogGuildJoin(arg, result.IsAcknowledged && result.MatchedCount > 0, true);
        }

        private async Task GuildLeft(SocketGuild arg)
        {
            var update = Builders<GuildSettings>.Update
                .Set(g => g.Active, false);

            await _guildSettings.UpdateOneAsync(x => x.GuildId == arg.Id, update);

            await _loggingService.LogGuildJoin(arg, false, false);
        }

        public async Task PartGuild(ulong guildId)
        {
            await _client.GetGuild(guildId).LeaveAsync();
        }

        private async Task ClientReady()
        {
            await _client.SetGameAsync("!help to get started.");
        }

        private Task Log(LogMessage arg)
        {
            var log = _client.GetChannel(445330969898254349) as SocketTextChannel;
            log?.SendMessageAsync(arg.ToString());
            return Task.CompletedTask;
        }

        public List<ActiveGuildsresult> GetActiveServers()
        {
            return _client.Guilds.Select(guild => new ActiveGuildsresult
            {
                GuildId = guild.Id,
                Name = guild.Name,
                Owner = guild.Owner?.Username
            }).ToList();
        }

        public async Task GlobalMessageToServerOwners(string message)
        {
            message += $"\n" +
                       $"\n" +
                       $"Questions? Want a feature? Found a bug? shoot the maker of this bot a message:{_client.GetUser(288764290519924736).Mention} :rocket:\n" +
                       $"You are receiving this message because you are the owner of one or more servers this bot is in.";
            foreach (var guild in _client.Guilds)
            {
                await guild.Owner.SendMessageAsync(message);
            }
        }

        public async void SendChatMessage(ulong channelNumber, ChatMessage message)
        {
            SocketTextChannel channel = _client.GetChannel(channelNumber) as SocketTextChannel;

            string toSend = $"**{message.User}**: {message.Message}";
            if (channel != null) await channel.SendMessageAsync(toSend);
        }

        public async Task SendMessage(ulong channelNumber, string message, Embed embed = null)
        {
            if (_client.GetChannel(channelNumber) is SocketTextChannel channel) await channel.SendMessageAsync(message, false, embed);
        }
    }
}
