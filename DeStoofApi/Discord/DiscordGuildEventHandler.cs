using System;
using System.Threading.Tasks;
using Core.Extensions;
using Core.Settings;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Discord
{
    public class DiscordGuildEventHandler
    {

        private readonly IServiceProvider _serviceProvider;

        public DiscordGuildEventHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task GuildJoined(SocketGuild guild)
        {
            using (var scope = _serviceProvider.CreateRavenScope())
            {
                var loggingService = scope.ServiceProvider.GetRequiredService<LoggingService>();
                var session = scope.ServiceProvider.GetRequiredService<IAsyncDocumentSession>();

                var guildSettings = await session.Query<GuildSettings>().FirstOrDefaultAsync(g => g.GuildId == guild.Id.ToString());
                bool firstJoin = false;

                if (guildSettings == null)
                {
                    firstJoin = true;
                    guildSettings = new GuildSettings(guild.Id.ToString(), "GuildSettings|");
                    await session.StoreAsync(guildSettings);
                }

                await loggingService.LogGuildJoin(guild, !firstJoin, true);
            }
        }

        public async Task GuildLeft(SocketGuild guild)
        {
            using (var scope = _serviceProvider.CreateRavenScope())
            {
                var loggingService = scope.ServiceProvider.GetRequiredService<LoggingService>();
                await loggingService.LogGuildJoin(guild, false, true);
            }
        }
    }
}