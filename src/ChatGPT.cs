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

    public async Task<string> CompleteAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        var request = new CompletionRequest(Model, conversation.Messages);
        var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", request, cancellationToken);

        _ = response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStreamAsync(cancellationToken);
        var document = await JsonDocument.ParseAsync(responseContent, default, cancellationToken);
        var message = document.RootElement.GetProperty("choices")[0].GetProperty("message");
        var messageContent = message.GetProperty("content").GetString()!;

        conversation.FromAssistant(messageContent);
        return messageContent;
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
}
