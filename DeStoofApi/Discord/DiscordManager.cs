using System.Threading.Tasks;
using Core;
using Core.Settings;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Discord
{
    public class DiscordManager : IMessagePlatform
    {
        private readonly DiscordSocketClient _client;
        private readonly DiscordMessageSanitizer _sanitizer;

        private readonly IConfiguration _config;

        public DiscordManager(IConfiguration config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;

            _sanitizer = new DiscordMessageSanitizer();
        }

        public async Task RunBotAsync()
        {
            await _client.LoginAsync(TokenType.Bot, _config["Secure:Discordtoken"]);
            await _client.StartAsync();

            _client.Ready += async () => await _client.SetGameAsync("!help to get started.");        
        }

        public async Task StreamMessage(GuildSettings settings, Core.Messages.IUserMessage message)
        {
            if (message is DiscordMessage) return;

            var channel = (SocketTextChannel)_client.GetChannel(settings.TwitchSettings.DiscordChatChannel.Id);

            string toSend = $"**{message.Author}**: {_sanitizer.Sanitize(message.Content)}";
            if (channel != null) await channel.SendMessageAsync(toSend);
        }

        public async Task SendMessage(string sourceId, string content) => await SendMessage(ulong.Parse(sourceId), content);

        public async Task SendMessage(ulong channelId, string message, Embed embed = null)
        {
            if (_client.GetChannel(channelId) is SocketTextChannel channel) await channel.SendMessageAsync(message, false, embed);
        }
    }
}
