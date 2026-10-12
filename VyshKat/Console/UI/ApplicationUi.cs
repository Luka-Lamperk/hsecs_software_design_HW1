using System.Globalization;
using VyshKat.Domain;

namespace VyshKat.ConsoleApp;

public sealed class ApplicationUi : IApplicationUi
{
    private readonly FleetService _fleet;
    private readonly IConsoleIO _io;
    private readonly DemoRunner _demo;
    public ApplicationUi(FleetService fleet, IConsoleIO io, DemoRunner demo)
    {
        _fleet = fleet;
        _io = io;
        _demo = demo;
    }

    public void RegisterTransport()
    {
        _io.WriteLine("Транспорт: 1 — самокат, 2 — электросамокат, 3 — велосипед, 4 — электровелосипед, ");
        var kind = AskChoice(1, 4);
        var name = AskNonEmpty("Название:");
        var ease = AskInt("Простота для новичка (1–10):", 1, 10);
        Vehicle asset = kind switch
        {
            1 => new Scooter(name, ease),
            2 => new ElectricScooter(name, ease, AskEnergy()),
            3 => new Bicycle(name, ease),
            4 => new ElectricBicycle(name, ease, AskEnergy()),
            _ => throw new InvalidOperationException()
        };
        ReportRegistration(_fleet.Register(asset), asset);
    }

    public void RegisterEquipment()
    {
        _io.WriteLine("Вещи: 1 — шлем, 2 — док-станция, 3 — зарядный шкаф");
        var kind = AskChoice(1, 3);
        var name = AskNonEmpty("Название:");
        var ease = AskInt("Простота для новичка (1–10):", 1, 10);
        Equipment asset = kind switch
        {
            1 => new Helmet(name, ease),
            2 => new DockStation(name, ease, AskEnergy()),
            3 => new ChargingCabinet(name, ease, AskEnergy()),
            _ => throw new InvalidOperationException()
        };
        ReportRegistration(_fleet.Register(asset), asset);
    }

    private void ReportRegistration(AcceptanceResult result, IInventoryItem asset) => _io.WriteLine(result switch
    {
        AcceptanceResult.Accepted => $"ПРИНЯТ: техосмотр пройден, объект поставлен на баланс. Инвентарный номер: {asset.InventoryNumber}.",
        AcceptanceResult.Rejected => $"ОТКЛОНЁН: техосмотр не пройден. Выделенный номер {asset.InventoryNumber} повторно не используется.",
        AcceptanceResult.Duplicate => $"ОТКЛОНЁН: объект уже состоит на балансе под номером {asset.InventoryNumber}.",
        _ => throw new ArgumentOutOfRangeException(nameof(result))
    });

    public void ShowSummary()
    {
        _io.WriteLine($"Всего объектов: {_fleet.AssetCount}; транспорта: {_fleet.VehicleCount}; вещей: {_fleet.AssetCount - _fleet.VehicleCount}.");
        _io.WriteLine($"Суммарное суточное потребление: {_fleet.TotalDailyEnergyKwh.ToString(CultureInfo.InvariantCulture)} кВт·ч.");
    }

    public void ShowBeginnerFriendly()
    {
        _io.WriteLine("Объекты для новичков (простота >= 6):");
        PrintAssets(_fleet.GetBeginnerFriendly());
    }

    public void ShowInventory()
    {
        _io.WriteLine("Все объекты на балансе:");
        PrintAssets(_fleet.GetInventory());
    }

    private void PrintAssets(IReadOnlyList<IInventoryItem> assets)
    {
        if (assets.Count == 0) { _io.WriteLine("(пусто)"); return; }
        foreach (var a in assets)
            _io.WriteLine($"{a.InventoryNumber} — {a.Name} ({a.GetType().Name}; простота {a.EaseOfUse})");
    }

    public void RunDemo() => _demo.Run();

    public void RunInteractive()
    {
        while (true)
        {
            _io.WriteLine("\n=== ВышКат ===\n1 — Добавить транспорт\n2 — Добавить вещь\n3 — Сводный отчёт\n4 — Для новичков\n5 — Инвентарная ведомость\n6 — Запустить демо\n0 — Выход");
            string? input = _io.ReadLine();
            if (input is null or "0") return;
            switch (input.Trim())
            {
                case "1": RegisterTransport(); break;
                case "2": RegisterEquipment(); break;
                case "3": ShowSummary(); break;
                case "4": ShowBeginnerFriendly(); break;
                case "5": ShowInventory(); break;
                case "6": RunDemo(); break;
                default: _io.WriteLine("Неизвестная команда."); break;
            }
        }
    }

    private string AskNonEmpty(string prompt)
    {
        while (true)
        {
            _io.WriteLine(prompt);
            var text = _io.ReadLine()?.Trim();
            if (text is null) throw new EndOfStreamException("Ввод завершён.");
            if (text.Length != 0) return text;
            _io.WriteLine("Значение не может быть пустым.");
        }
    }
    private int AskChoice(int min, int max) => AskInt("Выберите тип:", min, max);
    private int AskInt(string prompt, int min, int max)
    {
        while (true)
        {
            _io.WriteLine(prompt);
            var input = _io.ReadLine();
            if (input is null) throw new EndOfStreamException("Ввод завершён.");
            if (int.TryParse(input, out var n) && n >= min && n <= max) return n;
            _io.WriteLine($"Введите целое число от {min} до {max}.");
        }
    }
    private decimal AskEnergy()
    {
        while (true)
        {
            _io.WriteLine("Суточный расход энергии в кВт·ч (например, 1.5):");
            var input = _io.ReadLine();
            if (input is null) throw new EndOfStreamException("Ввод завершён.");
            if (decimal.TryParse(input.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var n) && n >= 0) return n;
            _io.WriteLine("Введите неотрицательное число.");
        }
    }
}
