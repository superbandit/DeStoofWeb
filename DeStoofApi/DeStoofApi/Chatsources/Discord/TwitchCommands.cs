using System.Threading.Tasks;
using DeStoofApi.Models;
using DeStoofApi.Models.Guilds;
using DeStoofApi.Services;
using Discord;
using Discord.Commands;
using MongoDB.Driver;

namespace DeStoofApi.Chatsources.Discord
{
    [Group("twitch")]
    public class TwitchCommands : ModuleBase<SocketCommandContext>
    {
        private readonly MessageService _messageService;
        private readonly IMongoCollection<GuildSettings> _guildSettings;
        private readonly IMongoCollection<ChatMessage> _chatMessages;

        public TwitchCommands(MessageService messageService, IMongoDatabase mongoDatabase)
        {
            _messageService = messageService;
            _guildSettings = mongoDatabase.GetCollection<GuildSettings>("guildSettings");
            _chatMessages = mongoDatabase.GetCollection<ChatMessage>("Messages");
        }

        [Command("resetSettings")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ResetSettingsAsync()
        {
            await _guildSettings.ReplaceOneAsync(g => g.GuildId == Context.Guild.Id,
                new GuildSettings{ GuildId = Context.Guild.Id }, new UpdateOptions { IsUpsert = true });
            await ReplyAsync("Settings have been reset!");
        }

        [Command("messages")]
        public async Task GetMessagesAsync()
        {
            var discordMessages = await (await _chatMessages.FindAsync(c => c.Platform == Enums.Platforms.Discord)).ToListAsync();

            var twitchMessages = await (await _chatMessages.FindAsync(c => c.Platform == Enums.Platforms.Twitch)).ToListAsync();

            await ReplyAsync(
                $"{discordMessages.Count} discord messages and {twitchMessages.Count} twitch messages have been sent since this bot joined the channel.");
        }

        [Command("connectChat")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ConnectToTwitchAsync()
        {
            var filter = Builders<GuildSettings>.Filter.Eq(g => g.GuildId, Context.Guild.Id);
            var settings = await (await _guildSettings.FindAsync(filter)).FirstOrDefaultAsync();

            if (settings.TwitchSettings.TwitchChannel == null || settings.TwitchSettings.DiscordChannel == null)
            {
                await ReplyAsync(
                    "Settings cannot be found, consider setting them with !twitch setChannel [ChannelName]");
                return;
            }

            await ReplyAsync($"Found settings, connecting {settings.TwitchSettings.TwitchChannel} to {settings.TwitchSettings.DiscordChannelname}.");
            _messageService.JoinTwitchChannel(settings.TwitchSettings.TwitchChannel);
        }

        [Command("setChannel")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetChannelAsync(string channel)
        {
            var filter = Builders<GuildSettings>.Filter.Eq(g => g.GuildId, Context.Guild.Id);
            var update = Builders<GuildSettings>.Update
                .Set(g => g.TwitchSettings.DiscordChannel, Context.Channel.Id)
                .Set(g => g.TwitchSettings.TwitchChannel, channel)
                .Set(g => g.TwitchSettings.DiscordChannelname, Context.Channel.Name);

            await _guildSettings.UpdateOneAsync(filter, update);

            await ReplyAsync($"Settings saved! Twich chat will be sent from {channel} to {Context.Channel.Name}. Type !twitch connectchat to start.");
        }
    }
}
