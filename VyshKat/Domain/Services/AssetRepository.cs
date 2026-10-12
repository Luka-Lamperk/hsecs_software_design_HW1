namespace VyshKat.Domain;

public interface IAssetRepository
{
    bool Contains(string inventoryNumber);
    void Add(IInventoryItem asset);
    IReadOnlyList<IInventoryItem> GetAll();
}

/// Simple in-memory inventory storage.
public sealed class InMemoryAssetRepository : IAssetRepository
{
    private readonly Dictionary<string, IInventoryItem> _assets = new(StringComparer.OrdinalIgnoreCase);

    public bool Contains(string inventoryNumber) => _assets.ContainsKey(inventoryNumber);

    public void Add(IInventoryItem asset)
    {
        if (!_assets.TryAdd(asset.InventoryNumber, asset))
        {
            throw new InvalidOperationException(
                $"Inventory number already exists: {asset.InventoryNumber}");
        }
    }

    public IReadOnlyList<IInventoryItem> GetAll() => _assets.Values.ToArray();
}
