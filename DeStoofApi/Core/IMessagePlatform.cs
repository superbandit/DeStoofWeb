using System.Threading.Tasks;
using Core.Messages;
using Core.Settings;

namespace Core
{
    public interface IMessagePlatform
    {
        Task StreamMessage(GuildSettings settings, IUserMessage message);
        Task SendMessage(string sourceId, string content);
    }
}