using System;
using System.Threading.Tasks;
using Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Core.Extensions
{
    public static class GeneralExtentions
    {
        public static RavenScope CreateRavenScope(this IServiceProvider provider) => new RavenScope(provider.CreateScope());

        public static async Task<GuildSettings> GetOrCreateSettings(this IAsyncDocumentSession session, string identifier)
        {
            var settings = await session.Query<GuildSettings>().FirstOrDefaultAsync(g => g.GuildId == identifier || g.TwitchSettings.ChannelName == identifier);

            if (settings != null) return settings;

            settings = new GuildSettings(identifier, "GuildSettings|");
            await session.StoreAsync(settings);

            return settings;
        }
    }

    public class RavenScope : IServiceScope
    {
        private readonly IServiceScope _serviceScope;
        public IServiceProvider ServiceProvider => _serviceScope.ServiceProvider;

        public RavenScope(IServiceScope scope)
        {
            _serviceScope = scope;
        }

        public void Dispose()
        {
            var session = _serviceScope.ServiceProvider.GetRequiredService<IAsyncDocumentSession>();
            session.SaveChangesAsync();
            session.Dispose();

            _serviceScope?.Dispose();
        }
    }
}
