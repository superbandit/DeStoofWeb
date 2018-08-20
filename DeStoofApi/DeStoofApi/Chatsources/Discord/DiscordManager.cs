using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Models.Domain.Messages;
using Models.View.Discord;

namespace DeStoofApi.Chatsources.Discord
{
    public class DiscordManager
    {

        private readonly DiscordSocketClient _client;

        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;

        public DiscordManager(IConfiguration config, DiscordSocketClient client, IServiceProvider serviceProvider)
        {
            _config = config;
            _client = client;
            _serviceProvider = serviceProvider;
        }

        public async Task RunBotAsync()
        {
            await _client.LoginAsync(TokenType.Bot, _config["Secure:Discordtoken"]);
            await _client.StartAsync();

            await _serviceProvider.GetService<DiscordCommandHandler>().InitializeAsync();

            _client.Ready += async () => await _client.SetGameAsync("!help to get started.");        
        }

        public async Task PartGuild(ulong guildId)
        {
            await _client.GetGuild(guildId).LeaveAsync();
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

        public async void SendMessage(ulong channelNumber, ChatMessage message)
        {
            var channel = (SocketTextChannel)_client.GetChannel(channelNumber);

            string toSend = $"**{message.User}**: {message.Message}";
            if (channel != null) await channel.SendMessageAsync(toSend);
        }

        public async Task SendMessage(ulong channelNumber, string message, Embed embed = null)
        {
            if (_client.GetChannel(channelNumber) is SocketTextChannel channel) await channel.SendMessageAsync(message, false, embed);
        }
    }
}
