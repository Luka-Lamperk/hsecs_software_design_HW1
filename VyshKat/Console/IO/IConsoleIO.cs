namespace VyshKat.ConsoleApp;

public interface IConsoleIO
{
    string? ReadLine();
    void WriteLine(string message = "");
}
