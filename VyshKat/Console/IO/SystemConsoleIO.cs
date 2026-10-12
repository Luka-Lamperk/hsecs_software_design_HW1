namespace VyshKat.ConsoleApp;

public sealed class SystemConsoleIO : IConsoleIO
{
    public string? ReadLine() => Console.ReadLine();

    public void WriteLine(string message = "") => Console.WriteLine(message);
}
