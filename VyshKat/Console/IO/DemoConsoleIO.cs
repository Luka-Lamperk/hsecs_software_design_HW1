namespace VyshKat.ConsoleApp;

/// Feeds predefined answers through the same IConsoleIO contract as the real console.
public sealed class DemoConsoleIO : IConsoleIO
{
    private readonly Queue<string> _answers;

    public DemoConsoleIO(IEnumerable<string> answers) => _answers = new Queue<string>(answers);

    public string? ReadLine()
    {
        if (_answers.Count == 0)
            throw new InvalidOperationException("Demo input exhausted.");

        var answer = _answers.Dequeue();
        Console.WriteLine($"> {answer}");
        return answer;
    }

    public void WriteLine(string message = "") => Console.WriteLine(message);

    public void AssertConsumed()
    {
        if (_answers.Count != 0)
            throw new InvalidOperationException($"Unused demo inputs: {_answers.Count}");
    }
}
