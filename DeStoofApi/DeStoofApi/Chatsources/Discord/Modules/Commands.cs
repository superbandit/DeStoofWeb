using System;
using System.Linq;
using System.Threading.Tasks;
using DeStoofApi.Models;
using DeStoofApi.Models.ChatMessages;
using DeStoofApi.Models.Guilds;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace DeStoofApi.Chatsources.Discord.Modules
{
    [RequireContext(ContextType.Guild)]
    public class Commands : ModuleBase<SocketCommandContext>
    {

        private readonly IMongoCollection<GuildSettings> _guildSettings;
        private readonly IMongoCollection<TwitchChatMessage> _twitchChatMessages;
        private readonly IMongoCollection<DiscordChatMessage> _discordChatMessages;

        public Commands(IMongoDatabase mongoDatabase, IConfiguration config)
        {
            _guildSettings = mongoDatabase.GetCollection<GuildSettings>(config["Secure:GuildSettings"]);
            _twitchChatMessages = mongoDatabase.GetCollection<ChatMessage>(config["Secure:Messages"]).OfType<TwitchChatMessage>();
            _discordChatMessages = mongoDatabase.GetCollection<ChatMessage>(config["Secure:Messages"]).OfType<DiscordChatMessage>();
        }       

        [Command("Settings")]
        [Summary("Shows a list of the settings regarding StreamerCompanion.")]
        public async Task Settings()
        {
            var settings = await GetGuildSettings();

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
                f.Value = settings.CommandPrefix;
                f.IsInline = true;
            });
            embedBuilder.AddField(f =>
            {
                f.Name = ":purple_heart: Twitch channel:";
                f.Value = settings.TwitchSettings.TwitchChannelName ?? "Not set.";
                f.IsInline = true;
            });
            embedBuilder.AddField(f =>
            {
                f.Name = ":mailbox_with_mail: Twitch sending to:";
                f.Value = settings.TwitchSettings.SendTo.ToString();
                f.IsInline = true;
            });
            embedBuilder.AddField(f =>
            {
                f.Name = ":blue_heart: Discord channel:";
                f.Value = settings.TwitchSettings.DiscordChannelname ?? "Not set.";
                f.IsInline = true;
            });
            embedBuilder.AddField(f =>
            {
                f.Name = ":mailbox_with_mail: Discord sending to:";
                f.Value = settings.DiscordSettings.SendTo;
                f.IsInline = true;
            });
            embedBuilder.AddField(f =>
            {
                f.Name = ":fishing_pole_and_fish: Webhook discord channel:";
                f.Value = settings.TwitchSettings.WebhookDiscordChannelName ?? "Not set.";
                f.IsInline = true;
            });

            embedBuilder.WithCurrentTimestamp();
            var embed = embedBuilder.Build();
            await ReplyAsync("", false, embed);
        }

        [Command("StreamerCompanion SetPrefix")]
        [Summary("Sets the prefix for all commands for StreamerCompanion.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetPrefixAsync([Summary("The prefix to be set.")]string prefix)
        {
            var update = Builders<GuildSettings>.Update.Set(g => g.CommandPrefix, prefix);
            await _guildSettings.FindOneAndUpdateAsync(g => g.GuildId == Context.Guild.Id, update);
            await ReplyAsync($"Command prefix has been set to {prefix}");
        }

        [Command("Stats")]
        [Summary("Shows how many messages have been sent since the bot joined the server.")]
        public async Task StatsAsync([Summary("User to get the stats from.")]IUser user = null)
        {
            var settings = await GetGuildSettings();

            var embedBuilder = new EmbedBuilder
            {
                Color = new Color(200, 10, 200),
                Title = $"Stats for {user?.Username ?? Context.Guild.Name}",
                ThumbnailUrl = user?.GetAvatarUrl() ?? Context.Guild.IconUrl,
                Footer = new EmbedFooterBuilder
                {
                    Text = "Made by Superbandit"
                }
            };

            if (user == null)
            {
                embedBuilder.Description = "Specify a user to see user specific stats.";
                var discordMessages = await (await _discordChatMessages.FindAsync(m => m.GuildIds.Contains(Context.Guild.Id))).ToListAsync();
                var twitchMessages = await _twitchChatMessages.CountAsync(m => m.Channel == settings.TwitchSettings.TwitchChannelName);

                DateTime oldestMessageDate = discordMessages.Min(d => d.Date);
                DiscordChatMessage oldestMessage = discordMessages.FirstOrDefault(m => m.Date == oldestMessageDate);
                embedBuilder.AddField(f =>
                {
                    f.Name = "First recorded command:";
                    f.Value = $"**Date:** {oldestMessageDate} \n" +
                              $"**User:** {oldestMessage?.User} \n" +
                              $"**Content:** {(oldestMessage?.Message.Length >= 30 ? oldestMessage.Message.Substring(0, 30) + "..." : oldestMessage?.Message)} \n ";
                });

                embedBuilder.AddField(f =>
                {
                    f.Name = "Command amount:";
                    f.Value = $"**Discord:** {discordMessages.Count} \n" +
                              $"**Twitch:** {twitchMessages}";
                });
            }
            else
            {
                var discordMessages = await (await _discordChatMessages.FindAsync(m => m.GuildIds.Contains(Context.Guild.Id) && m.UserId == user.Id)).ToListAsync();

                DateTime oldestMessageDate = discordMessages.Min(d => d.Date);
                DiscordChatMessage oldestMessage = discordMessages.FirstOrDefault(m => m.Date == oldestMessageDate);
                embedBuilder.AddField(f =>
                {
                    f.Name = "First recorded command:";
                    f.Value = $"**Date:** {oldestMessageDate} \n" +
                              $"**Username at the time:** {oldestMessage?.User} \n" +
                              $"**Content:** {(oldestMessage?.Message.Length >= 30 ? oldestMessage.Message.Substring(0, 30) + "..." : oldestMessage?.Message)} \n ";
                });

                embedBuilder.AddField(f =>
                {
                    f.Name = "Discord messages:";
                    f.Value = discordMessages.Count;
                });
            }

            embedBuilder.WithCurrentTimestamp();
            var embed = embedBuilder.Build();
            await ReplyAsync("", false, embed);
        }

        [Command("SendMessagesTo")]
        [Summary("Specify what platforms you want the discord messages sent to seperated by a space. Platforms: discord twitch")]
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
                    .Set(s => s.DiscordSettings.SendTo, platforms);
                await _guildSettings.UpdateOneAsync(s => s.GuildId == Context.Guild.Id, update);
                await ReplyAsync("Settings have been updated. Dont forget to set a channel/place for the messages to arrive for each platform.");
            }
            else
            {
                await ReplyAsync("Settings have not been updated as all given parameters could not be understood.");
            }
        }

        private async Task<GuildSettings> GetGuildSettings()
        {
            return await(await _guildSettings.FindAsync(s => s.GuildId == Context.Guild.Id)).FirstOrDefaultAsync();
        }
    }
}