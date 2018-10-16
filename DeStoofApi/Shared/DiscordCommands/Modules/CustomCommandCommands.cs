using System;
using System.Threading.Tasks;
using Core.Settings;
using DeStoofBot.DiscordCommands.Extensions;
using Discord;
using Discord.Commands;

namespace DeStoofBot.DiscordCommands.Modules
{
    public class CustomCommandCommands : ModuleBase<SettingsCommandContext>
    {
        [Command("CreateCommand")]
        [Summary("Create a custom command! Command will be active in twitch and discord. For more information call help on this command.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task CreateCustomCommand([Summary("The keyword needed for the command to be triggered. This can be anywhere in a message")] string keyword,
            [Summary("The text to be displayed after someone calls this command. You can add litlle snippets to the output command that will be computed on the spot!\n" +
                     "These snippets will be replaced when the command is called. More to come!\n \n" +
                     "Available snippets: \n" +
                     "{time} - Print the current time. \n" +
                     "{caller} - Prints the caller of the command. \n" +
                     "{channel} - Prints your twitchchannel if it has been set."), Remainder] string output)
        {
            var customCommand = new CustomCommand(keyword, output);
            Context.GuildSettings.AddCustomCommand(customCommand);
            await ReplyAsync("Custom command has been added. Try it!");
        }

        [Command("CustomCommands")]
        [Summary("A list of all your custom commands.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AllCustomCommands()
        {
            var embedBuilder = new EmbedBuilder
            {
                Color = new Color(200, 10, 200),
                Title = "This servers custom commands",
                Footer = new EmbedFooterBuilder
                {
                    Text = "Made by Superbandit"
                }
            };

            foreach (var customCommand in Context.GuildSettings.CustomCommands)
            {
                embedBuilder.AddField(f =>
                {
                    f.Name = customCommand.Prefix;
                    f.Value = customCommand.InputString;
                });
            }

            embedBuilder.WithTimestamp(DateTimeOffset.Now);
            await ReplyAsync("", false, embedBuilder.Build());
        }

        [Command("DeleteCommand")]
        [Summary("Delete a custom command.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task DeleteCommand([Summary("Keyword of command to be deleted.")]string keyword)
        {
            var removed = Context.GuildSettings.DeleteCustomCommand(keyword);
            if (removed)
            {
                await ReplyAsync("Custom command has been deleted.");
                return;
            }

            await ReplyAsync("Custom command does not exist. Please only specify the keyword.");
        }
    }
}
