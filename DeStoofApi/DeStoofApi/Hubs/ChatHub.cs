using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using DeStoofApi.Models;

namespace DeStoofApi.Services
{
    public class ChatHub : Hub
    {
        public async Task Send(ChatMessage message)
        {
             await Clients.All.SendAsync("Send", message.Message);
        }
    }
}
