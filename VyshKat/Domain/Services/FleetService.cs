namespace VyshKat.Domain;

public enum AcceptanceResult
{
    Accepted,
    Rejected,
    Duplicate
}


/// Coordinates inspection, inventory numbering, registration and fleet reports.
public sealed class FleetService
{
    private readonly IAssetRepository _repository;
    private readonly IServiceCenter _serviceCenter;
    private int _nextVehicleNumber = 1;
    private int _nextEquipmentNumber = 1;

    public FleetService(IAssetRepository repository, IServiceCenter serviceCenter)
    {
        _repository = repository;
        _serviceCenter = serviceCenter;
    }

    public AcceptanceResult Register(IInventoryItem asset)
    {
        ArgumentNullException.ThrowIfNull(asset);

        if (asset is not Asset modelAsset)
        {
            throw new ArgumentException(
                "Inventory items must derive from Asset so the fleet can assign their inventory number.",
                nameof(asset));
        }

        // Re-registering the same accepted object must not trigger another inspection.
        if (!string.IsNullOrWhiteSpace(asset.InventoryNumber) &&
            _repository.Contains(asset.InventoryNumber))
        {
            return AcceptanceResult.Duplicate;
        }

        if (string.IsNullOrWhiteSpace(modelAsset.InventoryNumber))
            modelAsset.AssignInventoryNumber(AllocateInventoryNumber(modelAsset));

        // Allocate before inspection so the inspector can identify the candidate.
        // Rejected numbers are intentionally never reused.
        if (!_serviceCenter.Inspect(asset))
            return AcceptanceResult.Rejected;

        _repository.Add(asset);
        return AcceptanceResult.Accepted;
    }

    private string AllocateInventoryNumber(Asset asset) => asset is Vehicle
        ? $"VH-{_nextVehicleNumber++:D3}"
        : $"EQ-{_nextEquipmentNumber++:D3}";

    public IReadOnlyList<IInventoryItem> GetInventory() => _repository.GetAll();

    // The beginner rule intentionally applies to all assets, including equipment.
    public IReadOnlyList<IInventoryItem> GetBeginnerFriendly() =>
        _repository.GetAll().Where(asset => asset.EaseOfUse >= 6).ToArray();

    public int AssetCount => _repository.GetAll().Count;

    public int VehicleCount => _repository.GetAll().Count(asset => asset is Vehicle);

    public decimal TotalDailyEnergyKwh =>
        _repository.GetAll().OfType<IEnergyConsumer>().Sum(consumer => consumer.DailyEnergyKwh);
}
