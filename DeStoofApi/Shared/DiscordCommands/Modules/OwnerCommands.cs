using System.Threading.Tasks;
using DeStoofBot.DiscordCommands.Extensions;
using Discord.Commands;
using Discord.WebSocket;

namespace DeStoofBot.DiscordCommands.Modules
{
    [Group("Owner")]
    [RequireBotOwner]
    public class OwnerCommands : ModuleBase<SettingsCommandContext>
    {
        [Command("leaveGuild")]
        public async Task LeaveGuildAsync(ulong guildId) => await Context.Client.GetGuild(guildId).LeaveAsync();

        [Command("guildAmount")]
        public async Task GuildAmountAsync() => await ReplyAsync(Context.Client.Guilds.Count.ToString());

        [Command("say")]
        public async Task SendMessage(ulong channelid, [Remainder] string message) => 
            await ((SocketTextChannel)Context.Client.GetChannel(channelid)).SendMessageAsync(message);
    }
}