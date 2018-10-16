namespace Core.Messages
{
    public interface IMessageSanitizer
    {
        string Sanitize(string content);
    }
}