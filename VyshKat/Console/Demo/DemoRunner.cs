namespace VyshKat.ConsoleApp;

// All operations invoke the SAME public UI contract as the interactive application.
public sealed class DemoRunner
{
    // The composition root supplies an isolated application scope for demo inputs.
    // The demo only calls the same IApplicationUi methods as the interactive menu.
    private readonly Action<IConsoleIO, Action<IApplicationUi>> _executeInScope;
    public DemoRunner(Action<IConsoleIO, Action<IApplicationUi>> executeInScope)
        => _executeInScope = executeInScope;

    public void Run()
    {
        var io = new DemoConsoleIO(Inputs);
        io.WriteLine("=== ДЕМО: автоинвентарные номера, приём, отказ, энергия, новички ===");
        _executeInScope(io, ui =>
        {
            ui.RegisterTransport(); // VH-001: accepted electric scooter
            ui.RegisterTransport(); // VH-002: rejected bicycle; this number is not reused
            ui.RegisterTransport(); // VH-003: accepted electric bicycle
            ui.RegisterEquipment(); // EQ-001: accepted helmet
            ui.RegisterEquipment(); // EQ-002: rejected dock station; this number is not reused
            ui.RegisterEquipment(); // EQ-003: accepted charging cabinet
            ui.RegisterTransport(); // VH-004: accepted regular scooter (no energy)
            ui.ShowSummary();
            ui.ShowBeginnerFriendly();
            ui.ShowInventory();
        });
        io.AssertConsumed();
        io.WriteLine("=== ДЕМО завершено. Основной парк не изменён. ===");
    }

    // Inventory numbers are intentionally absent from these inputs: FleetService generates them.
    public static IReadOnlyList<string> Inputs => new[]
    {
        "1", "Электросамокат Nine", "8", "1.2", "д",
        "2", "Велосипед Trek", "7", "н",
        "3", "Электровелосипед Cube", "5", "0.9", "д",
        "1", "Шлем Giro", "9", "д",
        "2", "Док-станция", "7", "2.5", "н",
        "3", "Зарядный шкаф", "6", "3.4", "д",
        "4", "Обычный самокат", "9", "д"
    };
}
