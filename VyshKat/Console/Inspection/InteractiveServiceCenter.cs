using VyshKat.Domain;

namespace VyshKat.ConsoleApp;

/// Asks the operator to accept or reject any inventory object.
public sealed class InteractiveServiceCenter : IServiceCenter
{
    private readonly IConsoleIO _io;

    public InteractiveServiceCenter(IConsoleIO io) => _io = io;

    public bool Inspect(IInventoryItem asset)
    {
        while (true)
        {
            _io.WriteLine($"Техосмотр: {asset.Name} [{asset.InventoryNumber}]. Принять? (д/н):");
            var response = _io.ReadLine()?.Trim().ToLowerInvariant();

            if (response is "д" or "да" or "y" or "yes") return true;
            if (response is "н" or "нет" or "n" or "no") return false;
            if (response is null) return false;

            _io.WriteLine("Введите д или н.");
        }
    }
}
