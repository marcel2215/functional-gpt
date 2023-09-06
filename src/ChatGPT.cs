using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FunctionalGPT;

public class ChatGPT
{
    private readonly HttpClient _httpClient = new();

    public ChatGPT(string apiKey, string model = "gpt-3.5-turbo")
    {
        ApiKey = apiKey;
        Model = model;
    }

    public string ApiKey
    {
        get => _httpClient.DefaultRequestHeaders.Authorization?.Parameter!;
        set => _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value);
    }

    public string Model { get; set; }

    public List<Delegate> Functions { get; set; } = new();

    public async Task<string> CompleteAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        var request = new CompletionRequest(Model, conversation.Messages);
        var functions = new List<Delegate>();

        if (Functions != null)
        {
            functions.AddRange(Functions);
        }

        if (conversation.Functions != null)
        {
            functions.AddRange(conversation.Functions);
        }

        if (functions.Any())
        {
            request.Functions = FunctionSerializer.Serialize(functions);
        }

        var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", request, cancellationToken);
        _ = response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStreamAsync(cancellationToken);
        var responseDocument = await JsonDocument.ParseAsync(responseContent, default, cancellationToken);
        var messageElement = responseDocument.RootElement.GetProperty("choices")[0].GetProperty("message");
        var message = JsonSerializer.Deserialize<Message>(messageElement)!;

        if (message.FunctionCall != null)
        {
            conversation.FromAssistant(message.FunctionCall);

            var simplifiedName = message.FunctionCall.Name.Replace("_", "");
            var function = functions.First(f => f.Method.Name.Equals(simplifiedName, StringComparison.InvariantCultureIgnoreCase));
            var functionResult = await FunctionInvoker.InvokeAsync(function, message.FunctionCall.Arguments, cancellationToken);

            conversation.FromFunction(message.FunctionCall.Name, functionResult);
            return await CompleteAsync(conversation, cancellationToken);
        }

        conversation.FromAssistant(message.Content!);
        return message.Content!;
    }

    public async Task<string> CompleteAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var conversation = new Conversation();
        conversation.FromUser(prompt);

        return await CompleteAsync(conversation, cancellationToken);
    }

    public async Task<string> CompleteAsync(string systemMessage, string prompt, CancellationToken cancellationToken = default)
    {
        var conversation = new Conversation(systemMessage);
        conversation.FromUser(prompt);

        return await CompleteAsync(conversation, cancellationToken);
    }

    public void AddFunction(Delegate function)
    {
        Functions.Add(function);
    }

    public bool RemoveFunction(Delegate function)
    {
        return Functions.Remove(function);
    }

    public void ClearFunctions()
    {
        Functions.Clear();
    }
}
