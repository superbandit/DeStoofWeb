using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using DeStoofApi.EventArgs;
using DeStoofApi.Models.ChatMessages;
using DeStoofApi.Models.Guilds;
using MongoDB.Driver;

namespace DeStoofApi.Chatsources.Discord
{
    public class DiscordManager
    {
        public delegate Task MessageReceivedHandler(object sender, MessageReceivedEventArgs args);
        public event MessageReceivedHandler MessageReceived;

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMongoCollection<GuildSettings> _guildSettings;

        private readonly List<SocketGuild> _guilds = new List<SocketGuild>();

        public DiscordManager(IConfiguration config, DiscordSocketClient client, CommandService commandService, IServiceProvider serviceProvider, IMongoDatabase mongoDatabase)
        {
            _config = config;
            _client = client;
            _commandService = commandService;
            _serviceProvider = serviceProvider;
            _guildSettings = mongoDatabase.GetCollection<GuildSettings>(config["Secure:GuildSettings"]);
        }


        public async Task<bool> RunBotAsync()
        {
            if (_client == null)
                return false;

            string botToken = _config["Secure:Discordtoken"];

            //event subscriptions
            _client.Log += Log;
            await RegisterCommandsAsync();
            await _client.LoginAsync(TokenType.Bot, botToken);
            await _client.StartAsync();
            _client.Ready += ClientReady;
            _client.JoinedGuild += GuildJoined;

            //TODO update all live settings

            return true;
        }

        private async Task GuildJoined(SocketGuild arg)
        {
            _guilds.Add(arg);

            await _guildSettings.ReplaceOneAsync(g => g.GuildId == arg.Id,
                new GuildSettings { GuildId = arg.Id }, new UpdateOptions { IsUpsert = true });
        }

        public async Task PartGuild(ulong guildId)
        {
            await _client.GetGuild(guildId).LeaveAsync();
        }

        private async Task ClientReady()
        {
            foreach (SocketGuild guild in _client.Guilds)
                _guilds.Add(guild);

            await _client.SetGameAsync("!help to get started.");
        }

        private Task Log(LogMessage arg)
        {
            Debug.WriteLine(arg);
            return Task.CompletedTask;
        }

        private async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleMessageAsync;

            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleMessageAsync(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message) || message.Author.IsBot) return;
            if (!(message.Channel is SocketGuildChannel channel))
            {
                await message.Channel.SendMessageAsync("Want this bot in your server? Click this link : https://discordapp.com/oauth2/authorize?client_id=416698597921521675&permissions=67112000&scope=bot");
                return;
            }

            if (MessageReceived == null)
                return;

            var settings = await (await _guildSettings.FindAsync(g => g.GuildId == channel.Guild.Id)).FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new GuildSettings { GuildId = channel.Guild.Id };
                await _guildSettings.InsertOneAsync(settings);
            }

            var chatMessage = new DiscordChatMessage
            {
                User = ((IGuildUser) message.Author).Nickname ?? message.Author.Username,
                UserId = message.Author.Id,
                Message = message.Content,
                Date = DateTime.Now,
                SendTo = settings.DiscordSettings.SendTo,
                ChannelId = channel.Id
            };
            chatMessage.GuildIds.Add(channel.Guild.Id);                      

            int argPos = 0;

            if (message.HasStringPrefix(settings.CommandPrefix, ref argPos))
            {
                SocketCommandContext context = new SocketCommandContext(_client, message);

                var result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
                if (!result.IsSuccess)
                {
                    Debug.WriteLine(result.ErrorReason);
                }
            }

            await MessageReceived(this, new MessageReceivedEventArgs(chatMessage));
        }

        public List<string> GetActiveServers()
        {
            return _client.Guilds.Select(guild => guild.Name).ToList();
        }

        public async Task GlobalMessageToServerOwners(string message)
        {
            message += $"\n" +
                       $"\n" +
                       $"Questions? Want a feature? Found a bug? shoot the maker of this bot a message:{_client.GetUser(288764290519924736).Mention} :rocket:\n" +
                       $"You are receiving this message because you are the owner of one or more servers this bot is in.";
            foreach (var guild in _client.Guilds)
            {
                await guild.Owner.SendMessageAsync(message);
            }
        }

        public async void SendChatMessage(ulong channelNumber, ChatMessage message)
        {
            SocketTextChannel channel = _client.GetChannel(channelNumber) as SocketTextChannel;

            string toSend = $"**{message.User}**: {message.Message}";
            if (channel != null) await channel.SendMessageAsync(toSend);
        }

        public async void SendMessage(ulong channelNumber, string message, Embed embed = null)
        {
            SocketTextChannel channel = _client.GetChannel(channelNumber) as SocketTextChannel;

            if (channel != null) await channel.SendMessageAsync(message, false, embed);
        }
    }
}
