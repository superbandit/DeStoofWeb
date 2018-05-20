using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DeStoofApi.Services
{
    public class LoggingService
    {
        private readonly DiscordSocketClient _client;
        private const ulong LogChannel = 447341345393082368;

        public LoggingService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task LogCommand(ICommandContext context, IResult result)
        {
            if (context.Guild.Id == 264445053596991498 || context.Guild.Id == 110373943822540800) return; // Bot listing channels

            if (_client.GetChannel(LogChannel) is SocketTextChannel channel)
            {
                var embedBuilder = new EmbedBuilder
                {
                    Color = result.IsSuccess ? new Color(10, 200, 10) : new Color(200, 10, 10),
                };

                embedBuilder.AddField(f =>
                {
                    f.Name = "Caller:";
                    f.Value = context.User.Mention;
                });
                embedBuilder.AddField(f =>
                {
                    f.Name = "Guild:";
                    f.Value = $"**Name:** {context.Guild.Name} \n" +
                              $"**Id:** {context.Guild.Id}";
                    f.IsInline = true;
                });
                embedBuilder.AddField(f =>
                {
                    f.Name = "Channel:";
                    f.Value = $"**Name:** {context.Channel.Name} \n" +
                              $"**Id:** {context.Channel.Id}";
                    f.IsInline = true;
                });
                embedBuilder.AddField(f =>
                {
                    f.Name = "Content:";
                    f.Value = context.Message.Content;
                });
                embedBuilder.AddField(f =>
                {
                    f.Name = "Result:";
                    f.Value = result.ToString();
                });

                embedBuilder.WithCurrentTimestamp();
                await channel.SendMessageAsync("A command has been called!", false, embedBuilder.Build());
            }
        }

        public async Task LogGuildJoin(IGuild guild, bool reJoined, bool joined)
        {
            if (_client.GetChannel(LogChannel) is SocketTextChannel channel)
            {
                var embedBuilder = new EmbedBuilder
                {
                    Color = joined ? new Color(10, 200, 10) : new Color(200, 10, 10),
                    Description = reJoined ? "This is a rejoin!" : ""
                };

                embedBuilder.AddField(f =>
                {
                    f.Name = "Guild:";
                    f.Value = $"**Name:** {guild.Name} \n" +
                              $"**Id:** {guild.Id}";
                    f.IsInline = true;
                });
                embedBuilder.AddField(async f =>
                {
                    f.Name = "Owner:";
                    f.Value = $"**Name:** {(await guild.GetOwnerAsync()).Mention} \n" +
                              $"**Id:** {guild.OwnerId}";
                    f.IsInline = true;
                });

                embedBuilder.WithCurrentTimestamp();
                await channel.SendMessageAsync($"{(joined ? "The bot has joined a new guild. :tada:" : "The bot has left a guild. :skull:")}", false, embedBuilder.Build());
            }
        }
    }
}
