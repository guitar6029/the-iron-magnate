namespace IronMagnate.Core.Resources;

public static class ResourceCatalog
{
    private static readonly IReadOnlyDictionary<ResourceType, Resource> Definitions =
        new Dictionary<ResourceType, Resource>
        {
            [ResourceType.Coal] = new(ResourceType.Coal, "Coal", ResourceCategory.Fuel),
            [ResourceType.IronOre] = new(ResourceType.IronOre, "Iron ore", ResourceCategory.RawMaterial),
            [ResourceType.CopperOre] = new(ResourceType.CopperOre, "Copper ore", ResourceCategory.RawMaterial)
        };

    public static Resource Get(ResourceType type)
    {
        if (!Definitions.TryGetValue(type, out var resource))
        {
            throw new ArgumentOutOfRangeException(nameof(type));
        }

        return resource;
    }
}
