namespace IronMagnate.Core.Resources;

public sealed record Resource(
    ResourceType Type,
    string Name,
    ResourceCategory Category
);
