using IronMagnate.Core.Resources;

namespace IronMagnate.Core.Production;

public sealed class MachineOutputBuffer
{
    public int Capacity { get; }

    public int ReservedQuantity { get; private set; }

    public int StoredQuantity { get; private set; }

    public int AvailableCapacity =>
        Capacity - ReservedQuantity - StoredQuantity;

    public bool IsFull => AvailableCapacity == 0;

    private readonly Dictionary<ResourceType, int> _stored = new();

    public MachineOutputBuffer(
        int capacity
    )
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        Capacity = capacity;
    }

    public bool CanReserve(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        return AvailableCapacity >= quantity;
    }

    public bool TryReserve(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        if (!CanReserve(quantity))
            return false;

        ReservedQuantity += quantity;
        return true;

    }

    public bool TryRemove(ResourceType type, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));


        ResourceCatalog.Get(type);

        if (!_stored.TryGetValue(type, out int available))
            return false;

        if (available < quantity)
            return false;

        if (available == quantity)
        {
            _stored.Remove(type);
        }
        else
        {
            _stored[type] = available - quantity;
        }

        StoredQuantity -= quantity;

        return true;

    }

    public void Release(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        if (quantity > ReservedQuantity)
            throw new InvalidOperationException(
                "Cannot release more capacity than is reserved.");

        ReservedQuantity -= quantity;
    }

    public void Complete(ResourceType type, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        ResourceCatalog.Get(type);

        if (quantity > ReservedQuantity)
            throw new InvalidOperationException(
                "Cannot complete more products than reserved.");

        _stored.TryGetValue(type, out int existing);

        _stored[type] = checked(existing + quantity);
        StoredQuantity = checked(StoredQuantity + quantity);
        ReservedQuantity -= quantity;
    }
}
