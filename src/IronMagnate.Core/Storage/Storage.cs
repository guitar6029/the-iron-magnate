using IronMagnate.Core.Resources;

namespace IronMagnate.Core.Storage;

public sealed class Storage
{
    private readonly Dictionary<Resource, double> quantities = [];
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

    public double GetQuantity(Resource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return quantities.GetValueOrDefault(resource);
    }

    public DeliveryResult Deliver(Resource resource, double quantity)
    {
        ValidateResourceAndQuantity(resource, quantity);

        var acceptedQuantity = Math.Min(quantity, AvailableCapacity);
        if (acceptedQuantity > 0)
        {
            quantities[resource] = GetQuantity(resource) + acceptedQuantity;
            OccupiedCapacity += acceptedQuantity;
        }

        return new DeliveryResult(quantity, acceptedQuantity);
    }

    public bool TryWithdraw(Resource resource, double quantity) =>
        TryWithdraw([new ResourceQuantity(resource, quantity)]);

    public bool TryWithdraw(IEnumerable<ResourceQuantity> requirements)
    {
        ArgumentNullException.ThrowIfNull(requirements);

        var combinedRequirements = new Dictionary<Resource, double>();
        foreach (var requirement in requirements)
        {
            ArgumentNullException.ThrowIfNull(requirement);
            ValidateResourceAndQuantity(requirement.Resource, requirement.Quantity);
            combinedRequirements[requirement.Resource] = combinedRequirements.GetValueOrDefault(requirement.Resource) + requirement.Quantity;

            if (!double.IsFinite(combinedRequirements[requirement.Resource]))
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

    private void ValidateResourceAndQuantity(Resource resource, double quantity)
    {
        ArgumentNullException.ThrowIfNull(resource);

        if (!acceptedCategories.Contains(resource.Category))
        {
            throw new ArgumentException($"{resource.Name} is not accepted by {Type} storage.", nameof(resource));
        }

        if (!double.IsFinite(quantity) || quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }
    }
}
