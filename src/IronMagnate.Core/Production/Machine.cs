namespace IronMagnate.Core.Production;

public sealed class Machine
{
    public Guid Id { get; }
    public string Name { get; }
    public int RequiredPower { get; }
    public int RequiredCrew { get; }
    public IReadOnlyList<Schematic> SupportedSchematics { get; }
    public Schematic SelectedSchematic { get; private set; }
    public bool IsEnabled { get; private set; }

    public Machine(
        Guid id,
        string name,
        int requiredPower,
        int requiredCrew,
        IReadOnlyList<Schematic> supportedSchematics,
        Schematic selectedSchematic
    )
    {

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(supportedSchematics);
        ArgumentNullException.ThrowIfNull(selectedSchematic);

        if (id == Guid.Empty)
            throw new ArgumentException(
                "Machine ID cannot be empty.",
                nameof(id)
            );


        if (requiredPower <= 0)
            throw new ArgumentOutOfRangeException(nameof(requiredPower));

        if (requiredCrew <= 0)
            throw new ArgumentOutOfRangeException(nameof(requiredCrew));


        if (supportedSchematics.Count == 0)
            throw new ArgumentException(
                "A machine must support at least one schematic.",
                nameof(supportedSchematics));


        if (supportedSchematics.Any(s => s is null))
            throw new ArgumentException(
                "Supported schematics cannot contain null entries.",
                nameof(supportedSchematics));

        if (!supportedSchematics.Contains(selectedSchematic))
            throw new ArgumentException(
                "Selected schematic must be supported by the machine.",
                nameof(selectedSchematic));



        Id = id;
        Name = name;
        RequiredPower = requiredPower;
        RequiredCrew = requiredCrew;
        SupportedSchematics =
    new List<Schematic>(supportedSchematics).AsReadOnly();
        SelectedSchematic = selectedSchematic;
        IsEnabled = false;
    }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    public void SelectSchematic(Schematic schematic)
    {
        ArgumentNullException.ThrowIfNull(schematic);

        if (!SupportedSchematics.Contains(schematic))
        {
            throw new ArgumentException(
                "Selected schematic must be supported by the machine.",
                nameof(schematic));
        }

        SelectedSchematic = schematic;
    }
}
