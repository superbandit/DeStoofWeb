using System;
using System.Reflection;
using System.Threading.Tasks;
using DeStoofApi.Extensions;
using DeStoofApi.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Models.Domain.Guilds;
using Models.Domain.Messages;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace DeStoofApi.Chatsources.Discord
{
    public class DiscordCommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;

        private readonly IServiceProvider _serviceProvider;
        private LoggingService _loggingService;

        public DiscordCommandHandler(CommandService commandService, DiscordSocketClient client, IServiceProvider serviceProvider)
        {
            _commandService = commandService;
            _client = client;
            _serviceProvider = serviceProvider;

            _client.MessageReceived += HandleMessageAsync;
        }

        public async Task InitializeAsync()
        {
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        private async Task HandleMessageAsync(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message) || message.Author.IsBot) return;
            switch (message.Channel)
            {
                case SocketGuildChannel _:
                    await HandleGuildmessage(message);
                    break;
                case SocketDMChannel _:
                    await HandleDmMessage(message);
                    break;
            }
        }

        private async Task HandleGuildmessage(SocketUserMessage message)
        {
            if (!(message.Channel is SocketGuildChannel channel)) return;

            using (var scope = _serviceProvider.CreateScope())
            {
                var session = scope.ServiceProvider.GetRequiredService<IAsyncDocumentSession>();
                var messageService = scope.ServiceProvider.GetRequiredService<MessageService>();
                _loggingService = scope.ServiceProvider.GetRequiredService<LoggingService>();

                var settings = await session.Query<GuildSettings>().FirstOrDefaultAsync(g => g.GuildId == channel.Guild.Id.ToString());

                if (settings == null)
                {
                    settings = new GuildSettings(channel.Guild.Id, "GuildSettings|");
                    await session.StoreAsync(settings);
                }

                int argPos = 0;
                if (message.HasStringPrefix(settings.CommandPrefix, ref argPos))
                    await HandleCommand(message, argPos, settings);

                var chatMessage = new DiscordChatMessage
                {
                    GuildId = channel.Guild.Id,
                    User = ((IGuildUser) message.Author).Nickname ?? message.Author.Username,
                    UserId = message.Author.Id,
                    Message = message.Content,
                    Date = DateTime.Now,
                    ChannelId = channel.Id
                };

                var context = new CustomMessageContext(settings, chatMessage);
                await messageService.OnChatMessageReceived(context);

                await session.SaveChangesAsync();
            }
        }

        private async Task HandleDmMessage(SocketUserMessage message)
        {
            int argPos = 0;
            if (message.HasStringPrefix("!", ref argPos))
                await HandleCommand(message, argPos);
        }

        private async Task HandleCommand(SocketUserMessage message, int argPos, GuildSettings settings = null)
        {
            var context = new SettingsCommandContext(_client, message, settings);

            var result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
            switch (result.Error)
            {
                case CommandError.BadArgCount:
                    await message.Channel.SendMessageAsync($"Invalid usage of command. Type {settings?.CommandPrefix ?? "!"}help [command] to see how to use it.");
                    break;
                case CommandError.UnmetPrecondition:
                    await message.Channel.SendMessageAsync("You are not authorized to use this command.");
                    break;
            }

            if (result.Error != CommandError.UnknownCommand) await _loggingService.LogCommand(context, result);
        }
    }
}
