namespace FunctionalGPT;

internal record CompletionRequest
{
    public CompletionRequest(string model, string messages)
    {
        Model = model;
        Messages = messages;
    }

    public string Model { get; set; }

    public string Messages { get; set; }
}
