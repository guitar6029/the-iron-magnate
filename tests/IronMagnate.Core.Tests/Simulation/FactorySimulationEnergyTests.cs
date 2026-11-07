using IronMagnate.Core.Simulation;
using Xunit;

namespace IronMagnate.Core.Tests.Simulation;

public class FactorySimulationEnergyTests
{
    [Fact]
    public void CreatesItsOwnPoweredOffBoiler()
    {
        var factory = new FactorySimulation();

        Assert.NotNull(factory.Boiler);
        Assert.False(factory.Boiler.IsPoweredOn);
    }

    [Fact]
    public void AdvancingWhileBoilerIsOffDoesNotConsumeCoal()
    {
        var factory = CreateFactoryAtSpeed(SimulationSpeed.Normal);

        factory.Advance(10);

        Assert.Equal(100, factory.Boiler.CurrentCoalSupply);
    }

    [Theory]
    [InlineData(SimulationSpeed.Normal, 2)]
    [InlineData(SimulationSpeed.Fast, 4)]
    [InlineData(SimulationSpeed.VeryFast, 8)]
    public void AdvanceConsumesFuelForClockSimulatedTime(SimulationSpeed speed, double expectedCoalConsumed)
    {
        var factory = CreateFactoryAtSpeed(speed);
        factory.Boiler.TurnOn();

        factory.Advance(1);

        Assert.Equal(100 - expectedCoalConsumed, factory.Boiler.CurrentCoalSupply);
    }

    [Fact]
    public void PausingPreventsFuelConsumption()
    {
        var factory = new FactorySimulation();
        factory.Boiler.TurnOn();

        factory.Advance(10);

        Assert.Equal(100, factory.Boiler.CurrentCoalSupply);
    }

    [Fact]
    public void FuelConsumptionStopsAtWorkdayBoundary()
    {
        var factory = CreateFactoryAtSpeed(SimulationSpeed.Normal);
        factory.Advance(478);
        factory.Boiler.TurnOn();
        factory.Advance(0.5);
        factory.Clock.SetSpeed(SimulationSpeed.Fast);

        var simulatedMinutes = factory.Advance(5);

        Assert.Equal(1.5, simulatedMinutes);
        Assert.Equal(96, factory.Boiler.CurrentCoalSupply);
        Assert.Equal(960, factory.Clock.CurrentMinute);
    }

    [Fact]
    public void FuelExhaustionDoesNotPreventClockFromAdvancing()
    {
        var factory = CreateFactoryAtSpeed(SimulationSpeed.Normal);
        factory.Boiler.TurnOn();

        var simulatedMinutes = factory.Advance(60);

        Assert.Equal(60, simulatedMinutes);
        Assert.Equal(540, factory.Clock.CurrentMinute);
        Assert.Equal(0, factory.Boiler.CurrentCoalSupply);
        Assert.False(factory.Boiler.IsPoweredOn);
    }

    [Fact]
    public void NextDayDoesNotRefillOrRestartExhaustedBoiler()
    {
        var factory = CreateFactoryAtSpeed(SimulationSpeed.Normal);
        factory.Boiler.TurnOn();
        factory.Advance(50);
        factory.Advance(430);

        factory.BeginNextDay();

        Assert.Equal(0, factory.Boiler.CurrentCoalSupply);
        Assert.False(factory.Boiler.IsPoweredOn);
    }

    private static FactorySimulation CreateFactoryAtSpeed(SimulationSpeed speed)
    {
        var factory = new FactorySimulation();
        factory.Clock.SetSpeed(speed);
        return factory;
    }
}
