using System.Threading.Tasks;
using Core.Messages;

namespace DeStoofBot.CustomCommands
{
    public class CustomCommandService
    {
        public async Task CheckForCustomCommands(CommandContext receivedEventArgs)
        {
            var compiler = new CustomCommandCompiler(receivedEventArgs);

            foreach (var c in receivedEventArgs.GuildSettings.CustomCommands)
            {
                if (!receivedEventArgs.Message.Content.ToLower().Contains(c.Prefix.ToLower())) continue;

                var result = compiler.CompileCustomCommand(c);
                await receivedEventArgs.Platform.SendMessage(receivedEventArgs.Message.SourceId, result);
            }
        }
    }
}
