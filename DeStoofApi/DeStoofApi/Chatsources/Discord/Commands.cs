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

namespace DeStoofApi.Chatsources.Discord
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly DiscordManager _discordManager;

        private readonly IMongoCollection<GuildSettings> _guildSettings;
        private readonly IMongoCollection<TwitchChatMessage> _twitchChatMessages;
        private readonly IMongoCollection<DiscordChatMessage> _discordChatMessages;

        public Commands(IMongoDatabase mongoDatabase, CommandService service, DiscordManager discordManager, IConfiguration config)
        {
            _service = service;
            _discordManager = discordManager;
            _guildSettings = mongoDatabase.GetCollection<GuildSettings>(config["Secure:GuildSettings"]);
            _twitchChatMessages = mongoDatabase.GetCollection<ChatMessage>(config["Secure:Messages"]).OfType<TwitchChatMessage>();
            _discordChatMessages = mongoDatabase.GetCollection<ChatMessage>(config["Secure:Messages"]).OfType<DiscordChatMessage>();
        }

        [Command("Help")]
        [Summary("Lists all available commands.")]
        public async Task CommandsAsync([Summary("Command of which help should be displayed."), Remainder] string command = null)
        {
            var settings = await GetGuildSettings();

            var embedBuilder = new EmbedBuilder
            {               
                Color = new Color(10, 200, 10),
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Made by Superbandit | [{DateTime.Now.ToShortTimeString()}]"
                }
            };

            if (command == null)
            {
                embedBuilder.Title = "Commands for DeStoofBot";
                embedBuilder.Description = $"{settings.CommandPrefix}help [command] for command specific help.";
                foreach (var c in _service.Commands)
                {
                    embedBuilder.AddField(a =>
                    {
                        a.Name = $"{settings.CommandPrefix}{c.Aliases.FirstOrDefault()}";
                        a.Value = c.Summary ?? "Undocumented";
                    });

                }
            }
            else
            {
                var commandFound = _service.Commands.FirstOrDefault(c => c.Aliases.Contains(command));

                if (commandFound == null)
                {
                    await ReplyAsync("Command could not be found.");
                    return;
                }

                embedBuilder.Title = $"Help for {command}";
                embedBuilder.Description = $"{commandFound.Summary} \n" +
                                           $"{(commandFound.Parameters.Count > 0 ? "Parameters:" : "No parameters")}";

                foreach (var param in commandFound.Parameters)
                {
                    embedBuilder.AddField(f =>
                    {
                        f.Name = param.Name;
                        f.Value = $"{(param.IsOptional? "(optional)" : "")} {param.Summary ?? "Undocumented"}";
                    });
                }
            }

            var eb = embedBuilder.Build();
            await ReplyAsync("", false, eb);
        }

        [Command("LeaveServer")]
        [Summary("Leaves the current server")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task LeaveServer()
        {
            await ReplyAsync("Cya!");
            await _discordManager.PartGuild(Context.Guild.Id);
        }

        [Command("DeStoofBot SetPrefix")]
        [Summary("Sets the prefix for all commands for DeStoofBot.")]
        public async Task SetPrefixAsync([Summary("The prefix to be set.")]string prefix)
        {
            var update = Builders<GuildSettings>.Update.Set(g => g.CommandPrefix, prefix);
            await _guildSettings.FindOneAndUpdateAsync(g => g.GuildId == Context.Guild.Id, update);
            await ReplyAsync($"Command prefix has been set to {prefix}");
        }

        [Command("ResetSettings")]
        [Summary("Resets all saved settings.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task ResetSettingsAsync()
        {
            await _guildSettings.ReplaceOneAsync(g => g.GuildId == Context.Guild.Id,
                new GuildSettings { GuildId = Context.Guild.Id }, new UpdateOptions { IsUpsert = true });
            await ReplyAsync("Settings have been reset!");
        }

        [Command("Messages")]
        [Summary("Shows how many messages have been sent since the bot joined the server.")]
        public async Task GetMessagesAsync()
        {
            var settings = await GetGuildSettings();

            var discordMessages = await _discordChatMessages.CountAsync(m => m.GuildIds.Contains(Context.Guild.Id));
            var twitchMessages = await _twitchChatMessages.CountAsync(m => m.Channel == settings.TwitchSettings.TwitchChannel);

            await ReplyAsync(
                $"{discordMessages} discord messages and {twitchMessages} twitch messages have been sent since this bot joined the channel.");
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