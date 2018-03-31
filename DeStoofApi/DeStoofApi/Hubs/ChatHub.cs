using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using DeStoofApi.Models;
using DeStoofApi.Chatsources;
using DeStoofApi.EventArguments;
using System;

namespace DeStoofApi.Services
{
    public class ChatHub : Hub
    {
        public async Task Send(ChatMessage message)
        {
            if(Clients != null)            
                await Clients.All.SendAsync("Send", message.Message);
        }
    }
}
