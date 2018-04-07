using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using static DeStoofApi.Models.Enums;

namespace DeStoofApi.Chatsources
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
                foreach(char c in t.ToCharArray())
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
    }
}