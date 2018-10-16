namespace Core.Messages
{
    public interface IUserMessage
    {
        string Author { get; }
        string Content { get; }
        string SourceId { get; }
    }
}