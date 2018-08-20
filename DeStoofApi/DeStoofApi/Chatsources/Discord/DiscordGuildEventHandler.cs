using System;
using System.Threading.Tasks;
using DeStoofApi.Services;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Models.Domain.Guilds;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace DeStoofApi.Chatsources.Discord
{
    public class DiscordGuildEventHandler
    {

        private readonly IServiceProvider _serviceProvider;

        public DiscordGuildEventHandler(DiscordSocketClient discordSocketClient, IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            discordSocketClient.JoinedGuild += GuildJoined;
            discordSocketClient.LeftGuild += GuildLeft;
        }

        private async Task GuildJoined(SocketGuild guild)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var loggingService = scope.ServiceProvider.GetRequiredService<LoggingService>();
                var session = scope.ServiceProvider.GetRequiredService<IAsyncDocumentSession>();

                var guildSettings = await session.Query<GuildSettings>().FirstOrDefaultAsync(g => g.GuildId == guild.Id.ToString());
                bool firstJoin = false;

                if (guildSettings == null)
                {
                    firstJoin = true;
                    guildSettings = new GuildSettings(guild.Id, "GuildSettings|");
                    await session.StoreAsync(guildSettings);
                }

                await loggingService.LogGuildJoin(guild, !firstJoin, true);

                await session.SaveChangesAsync();
            }
        }

        private async Task GuildLeft(SocketGuild guild)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var loggingService = scope.ServiceProvider.GetRequiredService<LoggingService>();
                await loggingService.LogGuildJoin(guild, false, true);
            }
        }
    }
}