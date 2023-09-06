namespace FunctionalGPT;

public record Conversation
{
    public Conversation() { }

    public Conversation(string systemMessage)
    {
        FromSystem(systemMessage);
    }

    public List<Message> Messages { get; set; } = new();

    public List<Delegate> Functions { get; set; } = new();

    public void FromSystem(string message)
    {
        Messages.Add(Message.FromSystem(message));
    }

    public void FromUser(string message)
    {
        Messages.Add(Message.FromUser(message));
    }

    public void FromUser(string name, string message)
    {
        Messages.Add(Message.FromUser(name, message));
    }

    public void FromAssistant(string message)
    {
        Messages.Add(Message.FromAssistant(message));
    }

    public void FromAssistant(FunctionCall functionCall)
    {
        Messages.Add(Message.FromAssistant(functionCall));
    }

    public void FromFunction(string name, string message)
    {
        Messages.Add(Message.FromFunction(name, message));
    }

    public bool Unsend(Message message)
    {
        return Messages.Remove(message);
    }

    public void AddFunction(Delegate function)
    {
        Functions.Add(function);
    }

    public bool RemoveFunction(Delegate function)
    {
        return Functions.Remove(function);
    }

    public void Clear()
    {
        ClearMessages();
        ClearFunctions();
    }

    public void ClearMessages()
    {
        Messages.Clear();
    }

    public void ClearFunctions()
    {
        Functions.Clear();
    }
}
