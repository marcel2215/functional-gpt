![hero-image](https://github.com/marcel-kwiatkowski/functional-gpt/assets/124832798/f6259a90-c55e-499c-b17a-cf31b1bc9ff3)
---
![example-screenshot](https://github.com/marcel-kwiatkowski/functional-gpt/assets/124832798/685fedc3-9dfd-4cbe-9375-5342564d4029)

# FunctionalGPT (.NET 7.0+)
Lightweight C#/.NET OpenAI API wrapper with support of GPT function calling via reflection.
> [!CAUTION]
> This project is now deprecated and being replaced by [GenerativeCS](https://github.com/chataize/generative-cs).

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

### Enums, Collections and Objects
```cs
using FunctionalGPT;

var chatGPT = new ChatGPT("<OPENAI API KEY>", "gpt-3.5-turbo");

chatGPT.AddFunction(Restaurant.GetAvailableTypes);
chatGPT.AddFunction(Restaurant.CreateOrder);
chatGPT.AddFunction(Restaurant.CancelOrder);

var conversation = new Conversation("You work in a pizzeria, and your goal is to collect orders.");

while (true)
{
    var userMessage = Console.ReadLine()!;
    conversation.FromUser(userMessage);

    var assistantResponse = await chatGPT.CompleteAsync(conversation);
    Console.WriteLine(assistantResponse);
}

public enum PizzaSize
{
    Small,
    Medium,
    Large
}

public record Pizza(string Type, PizzaSize Size, int Quantity);

public static class Restaurant
{
    private static readonly string[] _availableTypes =
    {
        "margherita",
        "pepperoni",
        "supreme",
        "vegetarian",
        "hawaiian"
    };

    public static string[] GetAvailableTypes()
    {
        return _availableTypes;
    }

    public static object CreateOrder(List<Pizza> pizzas)
    {
        // Validate the order:
        foreach (var pizza in pizzas)
        {
            if (!_availableTypes.Contains(pizza.Type.ToLower()))
            {
                return "ERROR! We don't serve this type of pizza.";
            }
        }

        // Perform some logic to save the order:
        foreach (var pizza in pizzas)
        {
            Console.WriteLine($"{pizza.Quantity}x {pizza.Size} {pizza.Type}");
        }

        // You can return anything serializable by System.Text.Json:
        return new { Success = true, OrderId = Random.Shared.Next() };
    }

    public static void CancelOrder(int id)
    {
        // Perform some logic to cancel the order.
        // WARNING: Make sure that the user can only cancel their own orders.

        Console.WriteLine($"Canceled order: {id}");
    }
}
```

