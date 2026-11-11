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
        var machine = new Machine(Guid.NewGuid(), "Stamping press", 4, 2, 10, [first, second], first);

        Assert.Equal("Stamping press", machine.Name);
        Assert.Equal(4, machine.RequiredPower);
        Assert.Equal(2, machine.RequiredCrew);
        Assert.Equal([first, second], machine.SupportedSchematics);
        Assert.Same(first, machine.SelectedSchematic);
        Assert.False(machine.IsEnabled);
        Assert.Null(machine.ActiveCycle);
        Assert.Equal(10, machine.OutputBuffer.Capacity);
    }

    [Fact]
    public void CreatesMachineWithoutAnActiveProductionCycle()
    {
        var machine = CreateMachine();

        Assert.Null(machine.ActiveCycle);
    }

    [Fact]
    public void CreatesMachineWithAnOutputBufferUsingTheConfiguredCapacity()
    {
        var machine = CreateMachine(outputBufferCapacity: 7);

        Assert.NotNull(machine.OutputBuffer);
        Assert.Equal(7, machine.OutputBuffer.Capacity);
    }

    [Fact]
    public void CreatesIndependentOutputBuffersForEachMachine()
    {
        var firstMachine = CreateMachine();
        var secondMachine = CreateMachine();

        Assert.NotSame(firstMachine.OutputBuffer, secondMachine.OutputBuffer);
        Assert.True(firstMachine.OutputBuffer.TryReserve(1));
        Assert.Equal(1, firstMachine.OutputBuffer.ReservedQuantity);
        Assert.Equal(0, secondMachine.OutputBuffer.ReservedQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsNonPositiveOutputBufferCapacity(int outputBufferCapacity)
    {
        var schematic = CreateSchematic();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Machine(
                Guid.NewGuid(),
                "Stamping press",
                4,
                2,
                outputBufferCapacity,
                [schematic],
                schematic));
    }

    [Fact]
    public void ProductionStateReferencesCannotBeReplacedThroughPublicSetters()
    {
        var activeCycle = typeof(Machine).GetProperty(nameof(Machine.ActiveCycle));
        var outputBuffer = typeof(Machine).GetProperty(nameof(Machine.OutputBuffer));

        Assert.NotNull(activeCycle);
        Assert.NotNull(outputBuffer);
        Assert.False(activeCycle.SetMethod!.IsPublic);
        Assert.Null(outputBuffer.SetMethod);
    }

    [Fact]
    public void RejectsAnEmptyMachineId()
    {
        Assert.Throws<ArgumentException>(() =>
            new Machine(Guid.Empty, "Stamping press", 4, 2, 10, [CreateSchematic()], CreateSchematic()));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void RejectsBlankNames(string name)
    {
        var schematic = CreateSchematic();

        Assert.Throws<ArgumentException>(() =>
            new Machine(Guid.NewGuid(), name, 4, 2, 10, [schematic], schematic));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsNonPositivePowerRequirements(int requiredPower)
    {
        var schematic = CreateSchematic();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Machine(Guid.NewGuid(), "Stamping press", requiredPower, 2, 10, [schematic], schematic));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsNonPositiveCrewRequirements(int requiredCrew)
    {
        var schematic = CreateSchematic();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Machine(Guid.NewGuid(), "Stamping press", 4, requiredCrew, 10, [schematic], schematic));
    }

    [Fact]
    public void RejectsNullSupportedSchematics()
    {
        var schematic = CreateSchematic();

        Assert.Throws<ArgumentNullException>(() =>
            new Machine(Guid.NewGuid(), "Stamping press", 4, 2, 10, null!, schematic));
    }

    [Fact]
    public void RejectsEmptySupportedSchematics()
    {
        var schematic = CreateSchematic();

        Assert.Throws<ArgumentException>(() =>
            new Machine(Guid.NewGuid(), "Stamping press", 4, 2, 10, Array.Empty<Schematic>(), schematic));
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
                10,
                new List<Schematic> { schematic, null! },
                schematic));
    }

    [Fact]
    public void RejectsANullSelectedSchematic()
    {
        var schematic = CreateSchematic();

        Assert.Throws<ArgumentNullException>(() =>
            new Machine(Guid.NewGuid(), "Stamping press", 4, 2, 10, [schematic], null!));
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
                10,
                [supported],
                CreateSchematic("Copper plate")));
    }

    [Fact]
    public void CopiesSupportedSchematicsAndPreventsExternalMutation()
    {
        var first = CreateSchematic("Iron plate");
        var second = CreateSchematic("Copper plate");
        var supportedSchematics = new List<Schematic> { first };
        var machine = new Machine(Guid.NewGuid(), "Stamping press", 4, 2, 10, supportedSchematics, first);

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
        var machine = new Machine(Guid.NewGuid(), "Stamping press", 4, 2, 10, [first, second], first);

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

    private static Machine CreateMachine(int outputBufferCapacity = 10)
    {
        var schematic = CreateSchematic();
        return new Machine(
            Guid.NewGuid(),
            "Stamping press",
            4,
            2,
            outputBufferCapacity,
            [schematic],
            schematic);
    }

    private static Schematic CreateSchematic(string name = "Iron plate") =>
        new(name, new Dictionary<ResourceType, int> { [ResourceType.IronOre] = 3 }, ResourceType.CopperOre, 1, 15);
}
