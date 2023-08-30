namespace FunctionalGPT;

public record Message
{
    public Message(Role role, string content)
    {
        Role = role;
        Content = content;
    }

    public Message(Role role, string name, string content)
    {
        Role = role;
        Name = name;
        Content = content;
    }

    public Message(FunctionCall functionCall)
    {
        Role = Role.Assistant;
        FunctionCall = functionCall;
    }

    public Role Role { get; set; }

    public string? Name { get; set; }

    public string? Content { get; set; }

    public FunctionCall? FunctionCall { get; set; }
}
