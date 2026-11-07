using IronMagnate.Core.Resources;

namespace IronMagnate.Core.Storage;

public sealed class Storage
{
    private readonly Dictionary<ResourceType, double> quantities = [];
    private readonly HashSet<ResourceCategory> acceptedCategories;

    public Storage(StorageType type, double capacity, IEnumerable<ResourceCategory> acceptedCategories)
    {
        if (!double.IsFinite(capacity) || capacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        ArgumentNullException.ThrowIfNull(acceptedCategories);

        Type = type;
        Capacity = capacity;
        this.acceptedCategories = [.. acceptedCategories];

        if (this.acceptedCategories.Count == 0)
        {
            throw new ArgumentException("At least one resource category must be accepted.", nameof(acceptedCategories));
        }
    }

    public StorageType Type { get; }

    public double Capacity { get; }

    public double OccupiedCapacity { get; private set; }

    public double AvailableCapacity => Capacity - OccupiedCapacity;

    public IReadOnlyCollection<ResourceCategory> AcceptedCategories => acceptedCategories;

    public double GetQuantity(ResourceType resourceType)
    {
        ResourceCatalog.Get(resourceType);
        return quantities.GetValueOrDefault(resourceType);
    }

    public DeliveryResult Deliver(ResourceType resourceType, double quantity)
    {
        ValidateResourceTypeAndQuantity(resourceType, quantity);

        var acceptedQuantity = Math.Min(quantity, AvailableCapacity);
        if (acceptedQuantity > 0)
        {
            quantities[resourceType] = GetQuantity(resourceType) + acceptedQuantity;
            OccupiedCapacity += acceptedQuantity;
        }

        return new DeliveryResult(quantity, acceptedQuantity);
    }

    public bool TryWithdraw(ResourceType resourceType, double quantity) =>
        TryWithdraw([new ResourceQuantity(resourceType, quantity)]);

    public bool TryWithdraw(IEnumerable<ResourceQuantity> requirements)
    {
        ArgumentNullException.ThrowIfNull(requirements);

        var combinedRequirements = new Dictionary<ResourceType, double>();
        foreach (var requirement in requirements)
        {
            ArgumentNullException.ThrowIfNull(requirement);
            ValidateResourceTypeAndQuantity(requirement.ResourceType, requirement.Quantity);
            combinedRequirements[requirement.ResourceType] = combinedRequirements.GetValueOrDefault(requirement.ResourceType) + requirement.Quantity;

            if (!double.IsFinite(combinedRequirements[requirement.ResourceType]))
            {
                throw new ArgumentOutOfRangeException(nameof(requirements));
            }
        }

        if (combinedRequirements.Any(requirement => GetQuantity(requirement.Key) < requirement.Value))
        {
            return false;
        }

        foreach (var requirement in combinedRequirements)
        {
            var remainingQuantity = GetQuantity(requirement.Key) - requirement.Value;
            if (remainingQuantity <= 0)
            {
                quantities.Remove(requirement.Key);
            }
            else
            {
                quantities[requirement.Key] = remainingQuantity;
            }

            OccupiedCapacity = Math.Max(0, OccupiedCapacity - requirement.Value);
        }

        return true;
    }

    private void ValidateResourceTypeAndQuantity(ResourceType resourceType, double quantity)
    {
        var resource = ResourceCatalog.Get(resourceType);

        if (!acceptedCategories.Contains(resource.Category))
        {
            throw new ArgumentException($"{resource.Name} is not accepted by {Type} storage.", nameof(resourceType));
        }

        if (!double.IsFinite(quantity) || quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }
    }
}
