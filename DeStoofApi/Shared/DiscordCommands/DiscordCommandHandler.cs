using System;
using System.Threading.Tasks;
using Core.Settings;
using DeStoofBot.DiscordCommands.Extensions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DeStoofBot.DiscordCommands
{
    public class DiscordCommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;

        private readonly IServiceProvider _serviceProvider;

        private readonly LoggingService _loggingService;

        public DiscordCommandHandler(CommandService commandService, DiscordSocketClient client, LoggingService loggingService, IServiceProvider serviceProvider)
        {
            _commandService = commandService;
            _client = client;
            _loggingService = loggingService;
            _serviceProvider = serviceProvider;
        }

        public async Task HandleMessageAsync(SocketMessage arg, GuildSettings settings)
        {
            if (!(arg is SocketUserMessage message)) return;
            switch (message.Channel)
            {
                case SocketGuildChannel _:
                    await HandleGuildmessage(message, settings);
                    break;
                case SocketDMChannel _:
                    await HandleDmMessage(message);
                    break;
            }
        }

        private async Task HandleGuildmessage(SocketUserMessage message, GuildSettings settings)
        {
            int argPos = 0;
            if (message.HasStringPrefix(settings.CommandPrefix, ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                await HandleCommand(message, argPos, settings);
        }

        private async Task HandleDmMessage(SocketUserMessage message)
        {
            int argPos = 0;
            if (message.HasStringPrefix("!", ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
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
