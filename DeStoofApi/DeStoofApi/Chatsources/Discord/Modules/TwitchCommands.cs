using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeStoofApi.Chatsources.Twitch;
using DeStoofApi.Extensions;
using Discord;
using Discord.Commands;
using Models.Domain.Guilds;

namespace DeStoofApi.Chatsources.Discord.Modules
{
    [RequireContext(ContextType.Guild)]
    public class TwitchCommands : ModuleBase<SettingsCommandContext>
    {
        private readonly TwitchManager _twitchManager;

        public TwitchCommands(TwitchManager twitchManager)
        {
            _twitchManager = twitchManager;
        }

        [Command("SetChannel")]
        [Summary("Set the twitch channel for all actions.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetChannel([Summary("The name of the channel.")]string channel)
        {
            var id = (await _twitchManager.GetChannelusers(new List<string> { channel })).Users.FirstOrDefault()?.Id;
            if (id == null)
            {
                await ReplyAsync("Channel could not be found.");
                return;
            }

            Context.GuildSettings.SetTwitchChannel(channel, id);

            await ReplyAsync("Channel has been set.");
        }

        [Command("ConnectChat")]
        [Summary("Connects to the twitch chat and starts sending messages.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ConnectToTwitchAsync()
        {
            if (Context.GuildSettings.TwitchSettings == null)
            {
                await ReplyAsync($"Please set a channel before connecting to it with {Context.GuildSettings.CommandPrefix}SetChannel.");
                return;
            }

            _twitchManager.TryLeaveTwitchChannel(Context.GuildSettings.TwitchSettings.ChannelName);

            _twitchManager.JoinTwitchChannel(Context.GuildSettings.TwitchSettings.ChannelName);

            await ReplyAsync("Successfully connected.");
        }

        [Command("DisconnectChat")]
        [Summary("Disconnects the twitch chat.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task DisconnectTwitchAsync()
        {
            if (Context.GuildSettings.TwitchSettings == null)
            {
                await ReplyAsync("Maybe you should try setting a chennel before disconnecting :thinking:");
                return;
            }

            var success = _twitchManager.TryLeaveTwitchChannel(Context.GuildSettings.TwitchSettings.ChannelName);

            if (success) await ReplyAsync("The bot has left the twitch channel.");
            else await ReplyAsync("There is no channel to leave from :poop:");
        }

        [Command("Tracker")]
        [Summary("Toggle the bot sending a message whenever the twitch channel goes live. Messages will be sent to the channel the command is called in. If you dont go live in 1 week, the tracker will be disabled and you have to call this command again.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task WebhookAsync([Summary("Text to send whenever your channel goes live."), Remainder] string message)
        {
            if (Context.GuildSettings.TwitchSettings == null)
            {
                await ReplyAsync("Try connecting to a channel before requesting a tracker.");
                return;
            }

            var success = await _twitchManager.SubToChannelLiveWebhook(Context.GuildSettings.TwitchSettings.UserId);
            if (!success)
                await ReplyAsync("Could not create webhook.");
            else
            {
                var channel = new DiscordChannel(Context.Channel.Id, Context.Channel.Name);
                Context.GuildSettings.TwitchSettings.SetWebhook(channel, message);
                await ReplyAsync("You will now receive a message in this channel whenever your twitch channel goes live.");
            }
        }
    }
}
