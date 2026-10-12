namespace VyshKat.Domain;

/// Decides whether a vehicle or piece of equipment passes inspection.
public interface IServiceCenter
{
    bool Inspect(IInventoryItem asset);
}
