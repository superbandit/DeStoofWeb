using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Discord.Extensions
{
    public class RequireBotOwnerAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return Task.FromResult(context.User.Id == 288764290519924736 ? 
                PreconditionResult.FromSuccess() : 
                PreconditionResult.FromError("You must be the owner of the bot to run this command."));
        }
    }
}
