using System.Threading.Tasks;
using DeStoofApi.Models;
using Microsoft.AspNetCore.SignalR;

namespace DeStoofApi.Hubs
{
    public class ChatHub : Hub
    {
        public async Task Send(ChatMessage message)
        {
             await Clients.All.SendAsync("Send", message.Message);
        }
    }
}
