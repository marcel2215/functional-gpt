namespace FunctionalGPT;

public record FunctionCall
{
    public FunctionCall(string name, string arguments)
    {
        Name = name;
        Arguments = arguments;
    }

    public string Name { get; set; }

    public string Arguments { get; set; }
}
