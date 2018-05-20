using System.Linq;
using System.Threading.Tasks;
using DeStoofApi.Extensions;
using Discord;
using Discord.Commands;

namespace DeStoofApi.Chatsources.Discord.Modules
{
    public class HelpCommands : ModuleBase<SettingsCommandContext>
    {
        private readonly CommandService _service;

        public HelpCommands(CommandService service)
        {
            _service = service;
        }

        [Command("Help")]
        [Summary("Lists all commands you can use right here. Takes permissions into account.")]
        public async Task CommandsAsync([Summary("Command of which help should be displayed."), Remainder] string command = null)
        {
            if (command == null)
            {
                var embedBuilder = new EmbedBuilder
                {
                    Color = new Color(200, 10, 200),
                    Title = "Commands for StreamerCompanion",
                    Description = $"{(Context.GuildSettings == null ? "Call this command in a server/guild to see the full ist of options." : $"Call {Context.GuildSettings.CommandPrefix}[Command] for command-specific help")}",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Made by Superbandit"
                    }
                };

                foreach (var c in _service.Commands)
                {
                    if (!(await c.CheckPreconditionsAsync(Context)).IsSuccess) continue;

                    embedBuilder.AddField(a =>
                    {
                        a.Name = $"{Context.GuildSettings?.CommandPrefix ?? "!"}{c.Aliases.FirstOrDefault()}";
                        a.Value = c.Summary ?? "Undocumented";
                    });
                }

                embedBuilder.WithCurrentTimestamp();
                var embed = embedBuilder.Build();
                await ReplyAsync("", false, embed);
            }
            else
            {
                var commandFound = _service.Commands.FirstOrDefault(c => c.Aliases.Contains(command));

                if (commandFound == null)
                {
                    await ReplyAsync("Command could not be found.");
                    return;
                }

                if (commandFound.Attributes.Any(a => a is RequireUserPermissionAttribute) && Context.User is IGuildUser user && !user.GuildPermissions.ManageChannels)
                {
                    await ReplyAsync("Command could not be found.");
                    return;
                }

                var embedBuilder = new EmbedBuilder
                {
                    Color = new Color(10, 200, 10),
                    Title = $"Help for {command}",
                    Description = $"{commandFound.Summary} \n" +
                                  $"{(commandFound.Parameters.Count > 0 ? "Parameters:" : "No parameters")}",
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Made by Superbandit"
                    }
                };

                foreach (var param in commandFound.Parameters)
                {
                    embedBuilder.AddField(f =>
                    {
                        f.Name = param.Name;
                        f.Value = $"{(param.IsOptional ? "(optional)" : "")} {param.Summary ?? "Undocumented"}";
                    });
                }

                embedBuilder.WithCurrentTimestamp();
                var embed = embedBuilder.Build();
                await ReplyAsync("", false, embed);
            }
        }
    }
}
