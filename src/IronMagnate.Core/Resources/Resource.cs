namespace IronMagnate.Core.Resources;

public sealed record Resource
{
    private Resource(ResourceType type, string name, ResourceCategory category)
    {
        Type = type;
        Name = name;
        Category = category;
    }

    public ResourceType Type { get; }

    public string Name { get; }

    public ResourceCategory Category { get; }

    internal static Resource CreateCanonical(
        ResourceType type,
        string name,
        ResourceCategory category) => new(type, name, category);
}
