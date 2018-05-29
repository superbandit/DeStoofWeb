using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeStoofApi.Chatsources.Twitch;
using DeStoofApi.Extensions;
using DeStoofApi.Models.Guilds;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace DeStoofApi.Chatsources.Discord.Modules
{
    [Group("Twitch")]
    //[RequireContext(ContextType.Guild)]
    public class TwitchCommands : ModuleBase<SettingsCommandContext>
    {
        private readonly TwitchManager _twitchManager;
        private readonly IMongoCollection<GuildSettings> _guildSettings;

        public TwitchCommands(IMongoDatabase mongoDatabase, IConfiguration config, TwitchManager twitchManager)
        {
            _twitchManager = twitchManager;
            _guildSettings = mongoDatabase.GetCollection<GuildSettings>(config["Secure:GuildSettings"]);
        }

        [Command("ConnectChat")]
        [Summary("Connects to the twitch chat and starts sending messages.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ConnectToTwitchAsync([Summary("The twitch channel to connect to")]string channel = null)
        {
            if (Context.GuildSettings.TwitchSettings?.TwitchChannelName != null)
                _twitchManager.LeaveTwitchChannel(Context.GuildSettings.TwitchSettings.TwitchChannelName);
            else if (Context.GuildSettings.TwitchSettings?.TwitchChannelName == null && channel == null)
            {
                await ReplyAsync("You have to specify a twitch channel the first time you call this command. You can find this by clicking on your profile in the top right, then click on channel. Your channel name will be in the addressbar after the standard twitch address.");
                return;
            }

            var twitchuser = await _twitchManager.GetChannelusers(new List<string> { channel ?? Context.GuildSettings.TwitchSettings.TwitchChannelName });
            if (twitchuser.Users.Length < 1)
            {
                await ReplyAsync("Channel does not exist");
                return;
            }

            var update = Builders<GuildSettings>.Update
                .Set(g => g.TwitchSettings.DiscordChannel, Context.Channel.Id)
                .Set(g => g.TwitchSettings.TwitchChannelName, channel ?? Context.GuildSettings.TwitchSettings.TwitchChannelName)
                .Set(g => g.TwitchSettings.UserId, Int32.Parse(twitchuser.Users[0].Id))
                .Set(g => g.TwitchSettings.DiscordChannelname, Context.Channel.Name);
            await _guildSettings.UpdateOneAsync(g => g.GuildId == Context.Guild.Id, update);

            var success = _twitchManager.JoinTwitchChannel(channel ?? Context.GuildSettings.TwitchSettings.TwitchChannelName);
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
            if (Context.GuildSettings.TwitchSettings.TwitchChannelName == null || Context.GuildSettings.TwitchSettings.DiscordChannel == null)
            {
                await ReplyAsync(
                    $"Maybe you should try connecting first :thinking:");
                return;
            }

            var success = _twitchManager.LeaveTwitchChannel(Context.GuildSettings.TwitchSettings.TwitchChannelName);
            if (success)
                await ReplyAsync("The bot has left the twitch channel");
            else
                await ReplyAsync("There is no channel to leave from :poop:");
        }

        [Command("Tracker")]
        [Summary("Toggle the bot sending a message whenever the twitch channel goes live. Messages will be sent to the channel the command is called in. You need to have connected to a chat once before calling this command.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task WebhookAsync([Summary("Text to send whenever your channel goes live."), Remainder] string message)
        {
            var update = Builders<GuildSettings>.Update
                .Set(g => g.TwitchSettings.WebhookDiscordChannel, Context.Channel.Id)
                .Set(g => g.TwitchSettings.WebhookDiscordChannelName, Context.Channel.Name)
                .Set(g => g.TwitchSettings.WebhookMessage, message);
            await _guildSettings.UpdateOneAsync(g => g.GuildId == Context.Guild.Id, update);

            if (Context.GuildSettings.TwitchSettings.UserId == null)
            {
                await ReplyAsync("Try connecting to a channel before requesting a tracker.");
                return;
            }

            var success = await _twitchManager.SubToChannelLiveWebhook((int)Context.GuildSettings.TwitchSettings.UserId);
            if (!success)
                await ReplyAsync("Could not create webhook.");
            else
                await ReplyAsync("You will now receive a message in this channel whenever your twitch channel goes live. Keep in mind that this is experimental and might not yet work correctly.");
        }
        /*
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
        }*/
    }
}
