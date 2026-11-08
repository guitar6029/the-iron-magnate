using IronMagnate.Core.Resources;
using System.Collections.ObjectModel;

namespace IronMagnate.Core.Production;

public sealed class Schematic
{
    public string Name { get; }

    public IReadOnlyDictionary<ResourceType, int> Inputs { get; }

    public ResourceType Output { get; }

    public int OutputQuantity { get; }

    public int ProcessingMinutes { get; }

    public Schematic(
        string name,
        IReadOnlyDictionary<ResourceType, int> inputs,
        ResourceType output,
        int outputQuantity,
        int processingMinutes)
    {

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(inputs);

        if (inputs.Count == 0)
            throw new ArgumentException(
                "A schematic must require at least one input.",
                nameof(inputs));

        if (inputs.Any(item => item.Value <= 0))
            throw new ArgumentException(
                "Input quantities must be positive.",
                nameof(inputs));

        if (outputQuantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(outputQuantity));

        if (processingMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(processingMinutes));


        Name = name;
        Inputs = new ReadOnlyDictionary<ResourceType, int>(new Dictionary<ResourceType, int>(inputs));
        Output = output;
        OutputQuantity = outputQuantity;
        ProcessingMinutes = processingMinutes;
    }
}
