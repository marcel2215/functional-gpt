namespace FunctionalGPT;

internal record CompletionRequest
{
    public CompletionRequest(string model, IEnumerable<Message> messages)
    {
        Model = model;
        Messages = messages;
    }

    public string Model { get; set; }

    public IEnumerable<Message> Messages { get; set; }
}
