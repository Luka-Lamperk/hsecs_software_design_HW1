namespace VyshKat.Domain;

/// An object that can be registered in the fleet inventory.
public interface IInventoryItem
{
    string InventoryNumber { get; }
    string Name { get; }
    int EaseOfUse { get; }
}

/// Implemented by inventory objects that consume energy.
public interface IEnergyConsumer
{
    decimal DailyEnergyKwh { get; }
}

/// Base type for inventory objects whose numbers are assigned by FleetService.
public abstract class Asset : IInventoryItem
{
    protected Asset(string name, int easeOfUse)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (easeOfUse is < 1 or > 10)
            throw new ArgumentOutOfRangeException(nameof(easeOfUse));

        Name = name.Trim();
        EaseOfUse = easeOfUse;
    }

    /// Empty until FleetService assigns a number during registration.
    public string InventoryNumber { get; private set; } = string.Empty;

    public string Name { get; }

    public int EaseOfUse { get; }

    internal void AssignInventoryNumber(string inventoryNumber)
    {
        if (string.IsNullOrWhiteSpace(inventoryNumber))
            throw new ArgumentException("Inventory number is required.", nameof(inventoryNumber));

        if (InventoryNumber.Length != 0 &&
            !InventoryNumber.Equals(inventoryNumber, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("An inventory number cannot be changed after assignment.");
        }

        InventoryNumber = inventoryNumber.Trim();
    }
}
