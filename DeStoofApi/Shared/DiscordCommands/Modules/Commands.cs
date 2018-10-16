using System;
using System.Threading.Tasks;
using DeStoofBot.DiscordCommands.Extensions;
using Discord;
using Discord.Commands;

namespace DeStoofBot.DiscordCommands.Modules
{
    [RequireContext(ContextType.Guild)]
    public class Commands : ModuleBase<SettingsCommandContext>
    {   
        [Command("Settings")]
        [Summary("Shows a list of the settings regarding StreamerCompanion.")]
        public async Task Settings()
        {
            var embedBuilder = new EmbedBuilder
            {
                Color = new Color(200, 10, 200),
                Title = "Settings for StreamerCompanion",
                Description = "Thank you for using StreamerCompanion!",
                Footer = new EmbedFooterBuilder
                {
                    Text = "Made by Superbandit"
                }
            };

            embedBuilder.AddField(f =>
            {
                f.Name = ":exclamation: Prefix:";
                f.Value = Context.GuildSettings.CommandPrefix;
                f.IsInline = true;
            });
            embedBuilder.AddField(f =>
            {
                f.Name = ":purple_heart: Twitch channel:";
                f.Value = Context.GuildSettings.TwitchSettings?.ChannelName ?? "Not set.";
                f.IsInline = true;
            });

            embedBuilder.AddField(f =>
            {
                f.Name = ":purple_heart: Twitch tracker:";
                f.Value = Context.GuildSettings.TwitchSettings?.DiscordWebhookChannel?.Name ?? "Not set.";
                f.IsInline = true;
            });

            embedBuilder.WithTimestamp(DateTimeOffset.Now);
            var embed = embedBuilder.Build();
            await ReplyAsync("", false, embed);
        }

        [Command("StreamerCompanion SetPrefix")]
        [Summary("Sets the prefix for all commands for StreamerCompanion.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetPrefixAsync([Summary("The prefix to be set.")]string prefix)
        {
            Context.GuildSettings.CommandPrefix = prefix;
            await ReplyAsync($"Command prefix has been set to {prefix}");
        }
    }
}