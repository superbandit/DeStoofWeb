using System;
using DeStoofApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Models.Domain.Guilds;
using Models.Domain.Messages;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using ChatMessage = TwitchLib.Client.Models.ChatMessage;

namespace DeStoofApi.Chatsources.Twitch
{
    public class TwitchCommandHandler
    {
        private readonly MessageService _messageService;
        private readonly IServiceProvider _serviceProvider;

        public TwitchCommandHandler(MessageService messageService, IServiceProvider serviceProvider, TwitchClient twitchClient)
        {
            _messageService = messageService;
            _serviceProvider = serviceProvider;

            twitchClient.OnMessageReceived += OnMessageReceived;
        }

        private async void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            ChatMessage message = e.ChatMessage;

            if (message.IsMe) return;

            using (var scope = _serviceProvider.CreateScope())
            {
                var session = scope.ServiceProvider.GetRequiredService<IAsyncDocumentSession>();

                var settings = await session.Query<GuildSettings>().FirstOrDefaultAsync(g =>
                    g.TwitchSettings.ChannelName.ToLower() == message.Channel.ToLower());

                var chatMessage = new TwitchChatMessage
                {
                    GuildId = ulong.Parse(settings.GuildId),
                    Channel = message.Channel,
                    Date = DateTime.Now,
                    Message = message.Message,
                    User = message.DisplayName,
                };

                var context = new CustomMessageContext(settings, chatMessage);

                await _messageService.OnChatMessageReceived(context);
            }
        }
    }
}
