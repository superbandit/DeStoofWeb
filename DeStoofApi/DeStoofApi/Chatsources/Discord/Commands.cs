using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using static DeStoofApi.Models.Enums;

namespace DeStoofApi.Chatsources.Discord
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("commands")]
        public async Task CommandsAsync()
        {
            await ReplyAsync("!hugify [text] - Maak je text groot!");
        }

        [Command("pleh")]
        public async Task PlehAsync()
        {
            await ReplyAsync("Wtf lars...");
        }

        [Command("hugify")]
        public async Task HugifyAsync(params string[] text)
        {
            text = text.Select(t => t.ToLower()).ToArray();
            string final = "";
            foreach(string t in text)
            {
                foreach(char c in t)
                {
                    if (char.IsDigit(c))
                    {
                        int.TryParse(c.ToString(), out int x);
                        final += $":{((Numbers)x).ToString()}:";
                    }
                    else if (char.IsLetter(c))
                        final += $":regional_indicator_{c}:";
                }
                final += "   ";
            }
            await ReplyAsync(final);
        }

        [Command("vanish")]
        [Summary("Remove all user's messages")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task VanishAsync([Remainder] IUser user)
        {
            if (user == null)
                await ReplyAsync("Please specify a user");

            await ReplyAsync("Bye Bye messages :wave:");

            var messages = await Context.Channel.GetMessagesAsync().FlattenAsync();

            foreach (var message in messages)
            {
                if (message.Author == user)
                    await message.DeleteAsync();
            }
        }
    }
}