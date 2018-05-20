using System.Threading.Tasks;
using Discord.Commands;

namespace DeStoofApi.Chatsources.Discord.Modules
{
    [RequireContext(ContextType.DM)]
    public class DmCommands : ModuleBase<SocketCommandContext>
    {
        [Command("Invite")]
        [Summary("Show the invitelink to get this bot in your server! for free!")]
        public async Task InviteAsync()
        {
            await ReplyAsync("This bot was made to be used in servers/guilds. Call this command there to see a full list of options. \n" +
                             "InviteLink: https://discordapp.com/oauth2/authorize?client_id=416698597921521675&permissions=8&scope=bot \n" +
                             "Happy streaming! :purple_heart:");
        }
    }
}
