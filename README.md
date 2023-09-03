# FunctionalGPT
Lightweight C#/.NET OpenAI API wrapper with support of GPT function calling via reflection.

## Usage
### Simple Prompt
``` cs
using FunctionalGPT;

var chatGPT = new ChatGPT("<OPENAI API KEY>", "gpt-3.5-turbo");
var response = chatGPT.CompleteAsync("Write an article about Bitcoin.");

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

### Function Calling
> [!WARNING]
> Top-level and anonymous functions are currently not supported.

> [!WARNING]
> All parameters must be primitive types. Enums, arrays, and collections are allowed.

```cs
using FunctionalGPT;
using System.ComponentModel;

var chatGPT = new ChatGPT("<OPENAI API KEY>", "gpt-3.5-turbo");

chatGPT.AddFunction(SmartHome.LockDoor);
chatGPT.AddFunction(SmartHome.ChangeTemperature);

var conversation = new Conversation("You are a helpful assistant [...]");

while (true)
{
    var userMessage = Console.ReadLine()!;
    conversation.FromUser(userMessage);

    var assistantResponse = await chatGPT.CompleteAsync(conversation);
    Console.WriteLine(assistantResponse);
}

public static class SmartHome
{
    [Description("Here you can describe the function (optional).")]
    public static void LockDoor(bool isLocked)
    {
        // Perform some logic to lock/unlock the door:
        Console.WriteLine($"[SMART HOME] Changed door lock status to: {isLocked}.");

        // Functions don't have to return anything.
    }

    // Function parameters can have default values specified:
    public static async Task<string> ChangeTemperature(int newTemperature = 20)
    {
        // Async functions are also supported:
        await Task.Delay(5000);

        // Validate arguments like this:
        if (newTemperature is < 0 or > 30)
        {
            // Functions can respond directly to the model:
            return "ERROR! Temperature must be between 0 and 30.";
        }

        // Perform some logic to change the temperature:
        Console.WriteLine($"[SMART HOME] Changed temperature to: {newTemperature}.");

        return "SUCCESS!";
    }
}
```
