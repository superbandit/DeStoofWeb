using System;
using System.Threading.Tasks;
using Core.Extensions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Discord
{
    public class DiscordEventHandler
    {
        private readonly IServiceProvider _serviceProvider;

        public DiscordEventHandler(IServiceProvider serviceProvider, DiscordSocketClient discordClient)
        {
            _serviceProvider = serviceProvider;

            discordClient.JoinedGuild += GuildJoined;
            discordClient.LeftGuild += GuildLeft;
        }

        private async Task GuildLeft(SocketGuild arg)
        {
            using (var scope = _serviceProvider.CreateRavenScope())
            {
                await scope.ServiceProvider.GetRequiredService<DiscordGuildEventHandler>().GuildLeft(arg);
            }
        }

        private async Task GuildJoined(SocketGuild arg)
        {
            using (var scope = _serviceProvider.CreateRavenScope())
            {
                await scope.ServiceProvider.GetRequiredService<DiscordGuildEventHandler>().GuildJoined(arg);
            }
        }
    }
}
