# FunctionalGPT
Lightweight C#/.NET OpenAI API wrapper with support of GPT function calling via reflection.
> [!NOTE] 
> Function calling is not yet supported *(coming soon)*.

## Usage
### Simple Prompt
``` cs
using FunctionalGPT;

var chatGPT = new ChatGPT("<OPENAI API KEY>", "gpt-3.5-turbo");
var response = chatGPT.CompleteAsync("Write an article about bitcoin.");

Console.WriteLine(response);
```

### Conversation
```cs
using FunctionalGPT;

var chatGPT = new ChatGPT("<OPENAI API KEY>", "gpt-3.5-turbo");
var conversation = new Conversation("You are a helpful assistant [...]");

while (true)
{
    var userMessage = Console.ReadLine()!;
    conversation.FromUser(userMessage);

    var assistantResponse = await chatGPT.CompleteAsync(conversation);
    Console.WriteLine(assistantResponse);
}
```
> [!IMPORTANT] 
> Assistant replies are automatically added to the conversation.

### Conversation with Multiple Users
```cs
using FunctionalGPT;

var chatGPT = new ChatGPT("<OPENAI API KEY>", "gpt-3.5-turbo");
var conversation = new Conversation("Decide who is right and explain why.");

conversation.FromUser("Alice", "I think the egg came first.");
conversation.FromUser("Bob", "Nah, it must have been chicken.");

var response = await chatGPT.CompleteAsync(conversation);
Console.WriteLine(response);
```
