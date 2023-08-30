namespace FunctionalGPT;

public record Conversation
{
    public List<Message> Messages { get; set; } = new();
}
