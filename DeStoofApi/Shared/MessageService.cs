using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Core.Extensions;
using Core.Messages;
using Core.Settings;
using DeStoofBot.CustomCommands;
using DeStoofBot.DiscordCommands;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents.Session;
using Twitch;
using TwitchLib.Client.Events;
using TwitchLib.Client.Interfaces;

namespace DeStoofBot
{
    public class MessageService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly List<IMessagePlatform> _platforms = new List<IMessagePlatform>();

        public MessageService(DiscordManager discordManager, TwitchManager twitchManager, IServiceProvider serviceProvider, DiscordSocketClient discordSocketClient, ITwitchClient twitchClient)
        {
            _platforms.Add(discordManager);
            _platforms.Add(twitchManager);
            _serviceProvider = serviceProvider;

            discordSocketClient.MessageReceived += OnDiscordMessageReceived;
            twitchClient.OnMessageReceived += OnTwitchmessageReceived;
        }

        private async void OnTwitchmessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.IsMe) return;
            using (var scope = _serviceProvider.CreateRavenScope())
            {
                await OnChatMessageReceived(null, new MessageReceivedEventArgs(new TwitchMessage(e.ChatMessage, e.ChatMessage.Channel), _platforms.First(p => p is TwitchManager)), scope);
            }
        }

        private async Task OnDiscordMessageReceived(SocketMessage message)
        {
            if (message.Author.IsBot) return;

            using (var scope = _serviceProvider.CreateRavenScope())
            {
                var commandHandler = scope.ServiceProvider.GetRequiredService<DiscordCommandHandler>();

                if (message.Channel is SocketTextChannel channel)
                {
                    var settings = await scope.ServiceProvider.GetRequiredService<IAsyncDocumentSession>().GetOrCreateSettings(channel.Guild.Id.ToString());
                    await commandHandler.HandleMessageAsync(message, settings);
                    await OnChatMessageReceived(settings, new MessageReceivedEventArgs(new DiscordMessage(message), _platforms.First(p => p is DiscordManager)), scope);
                }
                else
                {
                    await commandHandler.HandleMessageAsync(message, null);
                }
            }   
        }

        private async Task OnChatMessageReceived(GuildSettings settings, MessageReceivedEventArgs evt, IServiceScope scope)
        {
            if (settings == null) settings = await scope.ServiceProvider.GetRequiredService<IAsyncDocumentSession>().GetOrCreateSettings(evt.Message.SourceId);

            var context = new CommandContext(settings, evt.Message, evt.Platform);

            await scope.ServiceProvider.GetRequiredService<CustomCommandService>().CheckForCustomCommands(context);

            if (!context.GuildSettings.StreamMessages) return;

            foreach (var messagePlatform in _platforms)
                await messagePlatform.StreamMessage(context.GuildSettings, evt.Message);
        }
    }
}
