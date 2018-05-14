using System;
using System.Reflection;
using System.Threading.Tasks;
using DeStoofApi.EventArgs;
using DeStoofApi.Models.ChatMessages;
using DeStoofApi.Models.Guilds;
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


        public CommandHandler(CommandService commandService, DiscordSocketClient client, IServiceProvider serviceProvider, IConfiguration config, IMongoDatabase mongoDatabase)
        {
            _commandService = commandService;
            _client = client;
            _serviceProvider = serviceProvider;
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
            if (!(message.Channel is SocketGuildChannel channel))
            {
                await message.Channel.SendMessageAsync("Want this bot in your server? Click this link : https://discordapp.com/oauth2/authorize?client_id=416698597921521675&permissions=67112000&scope=bot");
                return;
            }

            var settings = await (await _guildSettings.FindAsync(g => g.GuildId == channel.Guild.Id)).FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new GuildSettings { GuildId = channel.Guild.Id };
                await _guildSettings.InsertOneAsync(settings);
            }

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

            int argPos = 0;

            if (message.HasStringPrefix(settings.CommandPrefix, ref argPos))
            {
                SocketCommandContext context = new SocketCommandContext(_client, message);

                var result = await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
                switch (result.Error)
                {
                    case CommandError.BadArgCount:
                        await message.Channel.SendMessageAsync($"Invalid usage of command. Type {settings.CommandPrefix}help [command] to see how to use it.");
                        break;
                    case CommandError.UnmetPrecondition:
                        await message.Channel.SendMessageAsync("You are not authorized to use this command.");
                        break;
                }
                if (result.Error != CommandError.UnknownCommand) await LogCommand(context, result);
            }

            if (MessageReceived != null) await MessageReceived(this, new MessageReceivedEventArgs(chatMessage));
        }

        private async Task LogCommand(ICommandContext context, IResult result)
        {
            if (_client.GetChannel(445330969898254349) is SocketTextChannel channel)
            {
                var embedBuilder = new EmbedBuilder
                {
                    Color = result.IsSuccess ? new Color(10, 200, 10) : new Color(200, 10, 10),
                };

                embedBuilder.AddField(f =>
                {
                    f.Name = "Caller:";
                    f.Value = context.User.Mention;
                });
                embedBuilder.AddField(f =>
                {
                    f.Name = "Guild:";
                    f.Value = $"**Name:** {context.Guild.Name} \n" +
                              $"**Id:** {context.Guild.Id}";
                    f.IsInline = true;
                });
                embedBuilder.AddField(f =>
                {
                    f.Name = "Channel:";
                    f.Value = $"**Name:** {context.Channel.Name} \n" +
                              $"**Id:** {context.Channel.Id}";
                    f.IsInline = true;
                });
                embedBuilder.AddField(f =>
                {
                    f.Name = "Content:";
                    f.Value = context.Message.Content;
                });
                embedBuilder.AddField(f =>
                {
                    f.Name = "Result:";
                    f.Value = result.ToString();
                });

                embedBuilder.WithCurrentTimestamp();
                var embed = embedBuilder.Build();
                await channel.SendMessageAsync("", false, embed);
            }
        }
    }
}
