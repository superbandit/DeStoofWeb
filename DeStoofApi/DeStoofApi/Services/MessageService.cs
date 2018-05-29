using MongoDB.Driver;
using System.Threading.Tasks;
using DeStoofApi.Chatsources.Discord;
using DeStoofApi.Chatsources.Twitch;
using DeStoofApi.EventArgs;
using DeStoofApi.Models;
using DeStoofApi.Models.Guilds;
using DeStoofApi.Models.Messages;
using Microsoft.Extensions.Configuration;

namespace DeStoofApi.Services
{
    public class MessageService
    {
        private readonly TwitchManager _twitchManager;
        private readonly DiscordManager _discordManager;
        private readonly CustomCommandService _customCommandService;

        private readonly IMongoCollection<ChatMessage> _messages;

        public MessageService(IMongoDatabase database, TwitchManager twitchManager, DiscordManager discordManager, IConfiguration config, CommandHandler commandHandler)
        {
            var db = database;
            _messages = db.GetCollection<ChatMessage>(config["Secure:Messages"]);

            _customCommandService = new CustomCommandService(discordManager, twitchManager);

            _twitchManager = twitchManager;
            _twitchManager.MessageReceived += OnChatMessageReceived;

            _discordManager = discordManager;
            commandHandler.MessageReceived += OnChatMessageReceived;
        }

        public async Task Startup()
        {
            _twitchManager.Start();
            await _discordManager.RunBotAsync();
        }


        private async Task OnChatMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            await _customCommandService.CheckForCustomCommands(args.ChatMessage, args.CommandContext);

            if (args.ChatMessage.Message.StartsWith(args.CommandContext.GuildSettings.CommandPrefix))
            {
                _messages.InsertOne(args.ChatMessage);
                return;
            }

            if (args.ChatMessage is DiscordChatMessage a && a.ChannelId != args.CommandContext.GuildSettings.TwitchSettings.DiscordChannel) return;

            await SendToPlatforms(args.ChatMessage, args.CommandContext.GuildSettings);
            _messages.InsertOne(args.ChatMessage);

            //await _chatHub.Clients.All.SendAsync("Send", args.ChatMessage);
        }

        private async Task SendToPlatforms(ChatMessage message, GuildSettings settings)
        {               
            if (message.SendTo.HasFlag(Enums.ChatPlatforms.Discord))
                if (settings.TwitchSettings.DiscordChannel != null)
                    _discordManager.SendChatMessage((ulong) settings.TwitchSettings.DiscordChannel, message);
            if (message.SendTo.HasFlag(Enums.ChatPlatforms.Twitch))
                await _twitchManager.SendChatMessage(message, settings.TwitchSettings.TwitchChannelName);            
        }
    }
}
