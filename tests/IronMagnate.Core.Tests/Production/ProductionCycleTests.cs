using IronMagnate.Core.Production;
using IronMagnate.Core.Resources;
using Xunit;

namespace IronMagnate.Core.Tests.Production;

public class ProductionCycleTests
{
    [Fact]
    public void NewCycleStartsWithoutProgressAndRetainsItsSchematic()
    {
        var schematic = CreateSchematic(processingMinutes: 5);
        var cycle = new ProductionCycle(schematic);

        Assert.Same(schematic, cycle.Schematic);
        Assert.Equal(0, cycle.ElapsedMinutes);
        Assert.False(cycle.IsComplete);
    }

    [Fact]
    public void RequiredMinutesComesFromTheSchematic()
    {
        var cycle = new ProductionCycle(CreateSchematic(processingMinutes: 15));

        Assert.Equal(15, cycle.RequiredMinutes);
    }

    [Fact]
    public void AdvanceRecordsPartialProgressAndRemainingTime()
    {
        var cycle = new ProductionCycle(CreateSchematic(processingMinutes: 5));

        var consumed = cycle.Advance(2);

        Assert.Equal(2, consumed);
        Assert.Equal(2, cycle.ElapsedMinutes);
        Assert.Equal(3, cycle.RemainingMinutes);
        Assert.False(cycle.IsComplete);
    }

    [Fact]
    public void AdvanceToTheRequiredDurationCompletesTheCycle()
    {
        var cycle = new ProductionCycle(CreateSchematic(processingMinutes: 5));

        var consumed = cycle.Advance(5);

        Assert.Equal(5, consumed);
        Assert.Equal(5, cycle.ElapsedMinutes);
        Assert.Equal(0, cycle.RemainingMinutes);
        Assert.True(cycle.IsComplete);
    }

    [Fact]
    public void AdvancePastCompletionCapsProgressAndReturnsOnlyConsumedMinutes()
    {
        var cycle = new ProductionCycle(CreateSchematic(processingMinutes: 5));

        var consumed = cycle.Advance(10);

        Assert.Equal(5, consumed);
        Assert.Equal(5, cycle.ElapsedMinutes);
        Assert.Equal(0, cycle.RemainingMinutes);
        Assert.True(cycle.IsComplete);
    }

    [Fact]
    public void AdvanceAfterCompletionReturnsZeroWithoutChangingProgress()
    {
        var cycle = new ProductionCycle(CreateSchematic(processingMinutes: 5));
        cycle.Advance(5);

        var consumed = cycle.Advance(1);

        Assert.Equal(0, consumed);
        Assert.Equal(5, cycle.ElapsedMinutes);
    }

    [Fact]
    public void AdvanceByZeroDoesNotChangeProgress()
    {
        var cycle = new ProductionCycle(CreateSchematic(processingMinutes: 5));

        var consumed = cycle.Advance(0);

        Assert.Equal(0, consumed);
        Assert.Equal(0, cycle.ElapsedMinutes);
        Assert.Equal(5, cycle.RemainingMinutes);
    }

    [Fact]
    public void AdvanceSupportsFractionalMinutesAtTheCompletionBoundary()
    {
        var cycle = new ProductionCycle(CreateSchematic(processingMinutes: 2));

        var firstConsumed = cycle.Advance(1.5);
        var secondConsumed = cycle.Advance(1);

        Assert.Equal(1.5, firstConsumed);
        Assert.Equal(0.5, secondConsumed);
        Assert.Equal(2, cycle.ElapsedMinutes);
        Assert.Equal(0, cycle.RemainingMinutes);
        Assert.True(cycle.IsComplete);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void AdvanceRejectsInvalidDurationsWithoutChangingProgress(double minutes)
    {
        var cycle = new ProductionCycle(CreateSchematic(processingMinutes: 5));
        cycle.Advance(2.5);

        Assert.Throws<ArgumentOutOfRangeException>(() => cycle.Advance(minutes));

        Assert.Equal(2.5, cycle.ElapsedMinutes);
        Assert.Equal(2.5, cycle.RemainingMinutes);
        Assert.False(cycle.IsComplete);
    }

    [Fact]
    public void CycleRetainsItsOriginalSchematicAfterTheMachineSelectsAnother()
    {
        var first = CreateSchematic("Iron plate", 5);
        var second = CreateSchematic("Copper plate", 10);
        var machine = new Machine(Guid.NewGuid(), "Stamping press", 4, 2, [first, second], first);
        var cycle = new ProductionCycle(machine.SelectedSchematic);

        machine.SelectSchematic(second);

        Assert.Same(first, cycle.Schematic);
        Assert.Equal(5, cycle.RequiredMinutes);
        Assert.Same(second, machine.SelectedSchematic);
    }

    private static Schematic CreateSchematic(string name = "Iron plate", int processingMinutes = 5) =>
        new(
            name,
            new Dictionary<ResourceType, int> { [ResourceType.IronOre] = 3 },
            ResourceType.CopperOre,
            1,
            processingMinutes);
}
