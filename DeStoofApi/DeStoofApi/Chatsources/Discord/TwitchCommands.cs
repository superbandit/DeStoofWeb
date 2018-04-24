using System;
using System.Threading.Tasks;
using DeStoofApi.Models;
using DeStoofApi.Models.Guilds;
using DeStoofApi.Services;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace DeStoofApi.Chatsources.Discord
{
    [Group("Twitch")]
    public class TwitchCommands : ModuleBase<SocketCommandContext>
    {
        private readonly MessageService _messageService;
        private readonly IMongoCollection<GuildSettings> _guildSettings;

        public TwitchCommands(MessageService messageService, IMongoDatabase mongoDatabase, IConfiguration config)
        {
            _messageService = messageService;
            _guildSettings = mongoDatabase.GetCollection<GuildSettings>(config["Secure:GuildSettings"]);
        }

        [Command("ConnectChat")]
        [Summary("Connects to the twitch chat and starts sending messages.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ConnectToTwitchAsync()
        {
            var settings = await (await _guildSettings.FindAsync(s => s.GuildId == Context.Guild.Id)).FirstOrDefaultAsync();

            if (settings.TwitchSettings.TwitchChannel == null || settings.TwitchSettings.DiscordChannel == null)
            {
                await ReplyAsync(
                    $"Settings cannot be found, consider setting them with {settings.CommandPrefix}twitch SetChannel [ChannelName]");
                return;
            }

            await ReplyAsync($"Found settings, connecting {settings.TwitchSettings.TwitchChannel} to {settings.TwitchSettings.DiscordChannelname}.");
            var success = _messageService.JoinTwitchChannel(settings.TwitchSettings.TwitchChannel);
            if (success)
                await ReplyAsync("Successfully connected.");
            else
                await ReplyAsync("Could not connect, maybe you are already connected.");
        }

        [Command("DisconnectChat")]
        [Summary("Disconnects the twitch chat.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task DisconnectTwitchAsync()
        {
            var settings = await (await _guildSettings.FindAsync(s => s.GuildId == Context.Guild.Id)).FirstOrDefaultAsync();

            if (settings.TwitchSettings.TwitchChannel == null || settings.TwitchSettings.DiscordChannel == null)
            {
                await ReplyAsync(
                    $"Settings cannot be found, consider setting them with {settings.CommandPrefix}twitch SetChannel [ChannelName]");
                return;
            }

            var success = _messageService.LeaveTwitchChannel(settings.TwitchSettings.TwitchChannel);
            if (success)
                await ReplyAsync("The bot has left all channels");
            else
                await ReplyAsync("There is no channel to leave from :poop:");
        }

        [Command("SetChannel")]
        [Summary("Saves what channel you want your twitch chat sent to")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetChannelAsync([Summary("The twitch channel to connect to")]string channel)
        {
            await ReplyAsync("Leaving possibly connected twitch channels...");
            await DisconnectTwitchAsync();

            var filter = Builders<GuildSettings>.Filter.Eq(g => g.GuildId, Context.Guild.Id);
            var update = Builders<GuildSettings>.Update
                .Set(g => g.TwitchSettings.DiscordChannel, Context.Channel.Id)
                .Set(g => g.TwitchSettings.TwitchChannel, channel)
                .Set(g => g.TwitchSettings.DiscordChannelname, Context.Channel.Name);

            await _guildSettings.UpdateOneAsync(filter, update);

            await ReplyAsync("Settings have been saved!");
        }

        [Command("SendMessagesTo")]
        [Summary("Specify what platforms you want the twitch chat messages sent to seperated by a space. Platforms: discord twitch")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SendMessagesToAsync([Summary("Platforms seperated by a space. Platforms: Twitch, Discord.")]params string[] platform)
        {
            var platforms = Enums.ChatPlatforms.None;
            foreach (var p in platform)
            {
                bool success = Enum.TryParse(p, true, out Enums.ChatPlatforms e);
                if (success)
                    platforms |= e;
                else
                    await ReplyAsync($"{p} is not a supported platform.");
            }

            if (platforms != 0)
            {
                var update = Builders<GuildSettings>.Update
                    .Set(s => s.TwitchSettings.SendTo, platforms);
                await _guildSettings.UpdateOneAsync(s => s.GuildId == Context.Guild.Id, update);
                await ReplyAsync("Settings have been updated. Dont forget to set a channel/place for the messages to arrive for each platform.");
            }
            else
            {
                await ReplyAsync("Settings have not been updated as all given parameters could not be understood.");
            }
        }
    }
}
