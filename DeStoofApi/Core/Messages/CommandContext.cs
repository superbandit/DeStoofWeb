using System;
using Core.Settings;

namespace Core.Messages
{
    public class CommandContext
    {
        public GuildSettings GuildSettings { get; }
        public IUserMessage Message { get; }
        public IMessagePlatform Platform { get; }

        public CommandContext(GuildSettings guildSettings, IUserMessage message, IMessagePlatform platform)
        {
            GuildSettings = guildSettings ?? throw new ArgumentNullException(nameof(guildSettings));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Platform = platform ?? throw new ArgumentNullException(nameof(platform));
        }
    }
}