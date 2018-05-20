using System;
using System.Reflection;
using System.Threading.Tasks;
using DeStoofApi.EventArgs;
using DeStoofApi.Extensions;
using DeStoofApi.Models.ChatMessages;
using DeStoofApi.Models.Guilds;
using DeStoofApi.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace DeStoofApi.Chatsources.Discord
{
    public class CommandHandler
    {
        public delegate Task MessageReceivedHandler(object sender, MessageReceivedEventArgs args);
        public event MessageReceivedHandler MessageReceived;

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMongoCollection<GuildSettings> _guildSettings;
        private readonly LoggingService _loggingService;

        public CommandHandler(CommandService commandService, DiscordSocketClient client, IServiceProvider serviceProvider, IConfiguration config, IMongoDatabase mongoDatabase, LoggingService loggingService)
        {
            _commandService = commandService;
            _client = client;
            _serviceProvider = serviceProvider;
            _loggingService = loggingService;
            _guildSettings = mongoDatabase.GetCollection<GuildSettings>(config["Secure:GuildSettings"]);

            _client.MessageReceived += HandleMessageAsync;
        }

        public async Task InitializeAsync()
        {
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly());
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

            var settings = await (await _guildSettings.FindAsync(g => g.GuildId == channel.Guild.Id)).FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new GuildSettings { GuildId = channel.Guild.Id };
                await _guildSettings.InsertOneAsync(settings);
            }

            int argPos = 0;
            if (message.HasStringPrefix(settings.CommandPrefix, ref argPos))
                await HandleCommand(message, argPos, settings);

            var chatMessage = new DiscordChatMessage
            {
                User = ((IGuildUser)message.Author).Nickname ?? message.Author.Username,
                UserId = message.Author.Id,
                Message = message.Content,
                Date = DateTime.Now,
                SendTo = settings.DiscordSettings.SendTo,
                ChannelId = channel.Id
            };
            chatMessage.GuildIds.Add(channel.Guild.Id);

            if (MessageReceived != null) await MessageReceived(this, new MessageReceivedEventArgs(chatMessage));
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
