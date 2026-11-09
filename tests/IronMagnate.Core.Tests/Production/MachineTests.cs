using IronMagnate.Core.Production;
using IronMagnate.Core.Resources;
using Xunit;

namespace IronMagnate.Core.Tests.Production;

public class MachineTests
{
    [Fact]
    public void CreatesADisabledMachineWithItsConfiguredRequirementsAndSchematics()
    {
        var first = CreateSchematic("Iron plate");
        var second = CreateSchematic("Copper plate");
        var machine = new Machine(Guid.NewGuid(), "Stamping press", 4, 2, [first, second], first);

        Assert.Equal("Stamping press", machine.Name);
        Assert.Equal(4, machine.RequiredPower);
        Assert.Equal(2, machine.RequiredCrew);
        Assert.Equal([first, second], machine.SupportedSchematics);
        Assert.Same(first, machine.SelectedSchematic);
        Assert.False(machine.IsEnabled);
    }

    [Fact]
    public void RejectsAnEmptyMachineId()
    {
        Assert.Throws<ArgumentException>(() =>
            new Machine(Guid.Empty, "Stamping press", 4, 2, [CreateSchematic()], CreateSchematic()));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void RejectsBlankNames(string name)
    {
        var schematic = CreateSchematic();

        Assert.Throws<ArgumentException>(() =>
            new Machine(Guid.NewGuid(), name, 4, 2, [schematic], schematic));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsNonPositivePowerRequirements(int requiredPower)
    {
        var schematic = CreateSchematic();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Machine(Guid.NewGuid(), "Stamping press", requiredPower, 2, [schematic], schematic));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsNonPositiveCrewRequirements(int requiredCrew)
    {
        var schematic = CreateSchematic();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Machine(Guid.NewGuid(), "Stamping press", 4, requiredCrew, [schematic], schematic));
    }

    [Fact]
    public void RejectsNullSupportedSchematics()
    {
        var schematic = CreateSchematic();

        Assert.Throws<ArgumentNullException>(() =>
            new Machine(Guid.NewGuid(), "Stamping press", 4, 2, null!, schematic));
    }

    [Fact]
    public void RejectsEmptySupportedSchematics()
    {
        var schematic = CreateSchematic();

        Assert.Throws<ArgumentException>(() =>
            new Machine(Guid.NewGuid(), "Stamping press", 4, 2, Array.Empty<Schematic>(), schematic));
    }

    [Fact]
    public void RejectsNullEntriesInSupportedSchematics()
    {
        var schematic = CreateSchematic();

        Assert.Throws<ArgumentException>(() =>
            new Machine(
                Guid.NewGuid(),
                "Stamping press",
                4,
                2,
                new List<Schematic> { schematic, null! },
                schematic));
    }

    [Fact]
    public void RejectsANullSelectedSchematic()
    {
        var schematic = CreateSchematic();

        Assert.Throws<ArgumentNullException>(() =>
            new Machine(Guid.NewGuid(), "Stamping press", 4, 2, [schematic], null!));
    }

    [Fact]
    public void RejectsAnUnsupportedSelectedSchematic()
    {
        var supported = CreateSchematic("Iron plate");

        Assert.Throws<ArgumentException>(() =>
            new Machine(
                Guid.NewGuid(),
                "Stamping press",
                4,
                2,
                [supported],
                CreateSchematic("Copper plate")));
    }

    [Fact]
    public void CopiesSupportedSchematicsAndPreventsExternalMutation()
    {
        var first = CreateSchematic("Iron plate");
        var second = CreateSchematic("Copper plate");
        var supportedSchematics = new List<Schematic> { first };
        var machine = new Machine(Guid.NewGuid(), "Stamping press", 4, 2, supportedSchematics, first);

        supportedSchematics.Clear();
        supportedSchematics.Add(second);

        Assert.Equal([first], machine.SupportedSchematics);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<Schematic>)machine.SupportedSchematics).Clear());
    }

    [Fact]
    public void EnableAndDisableAreIdempotent()
    {
        var machine = CreateMachine();

        machine.Enable();
        machine.Enable();

        Assert.True(machine.IsEnabled);

        machine.Disable();
        machine.Disable();

        Assert.False(machine.IsEnabled);
    }

    [Fact]
    public void SelectSchematicAcceptsASupportedSchematic()
    {
        var first = CreateSchematic("Iron plate");
        var second = CreateSchematic("Copper plate");
        var machine = new Machine(Guid.NewGuid(), "Stamping press", 4, 2, [first, second], first);

        machine.SelectSchematic(second);

        Assert.Same(second, machine.SelectedSchematic);
    }

    [Fact]
    public void SelectSchematicRejectsNullWithoutChangingTheExistingSelection()
    {
        var machine = CreateMachine();
        var initialSelection = machine.SelectedSchematic;

        Assert.Throws<ArgumentNullException>(() => machine.SelectSchematic(null!));

        Assert.Same(initialSelection, machine.SelectedSchematic);
    }

    [Fact]
    public void SelectSchematicRejectsAnUnsupportedSchematicWithoutChangingTheExistingSelection()
    {
        var machine = CreateMachine();
        var initialSelection = machine.SelectedSchematic;

        Assert.Throws<ArgumentException>(() => machine.SelectSchematic(CreateSchematic("Copper plate")));

        Assert.Same(initialSelection, machine.SelectedSchematic);
    }

    private static Machine CreateMachine()
    {
        var schematic = CreateSchematic();
        return new Machine(Guid.NewGuid(), "Stamping press", 4, 2, [schematic], schematic);
    }

    private static Schematic CreateSchematic(string name = "Iron plate") =>
        new(name, new Dictionary<ResourceType, int> { [ResourceType.IronOre] = 3 }, ResourceType.CopperOre, 1, 15);
}
