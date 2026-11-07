using IronMagnate.Core.Simulation;
using Xunit;

namespace IronMagnate.Core.Tests.Simulation;

public class FactorySimulationTests
{
    [Fact]
    public void StartsOnDayOne()
    {
        var factory = new FactorySimulation();

        Assert.Equal(1, factory.CurrentDay);
    }

    [Fact]
    public void ClockStartsAtEightOClockAndPaused()
    {
        var factory = new FactorySimulation();

        Assert.Equal(480, factory.Clock.CurrentMinute);
        Assert.Equal(SimulationSpeed.Paused, factory.Clock.Speed);
    }

    [Fact]
    public void AdvanceDelegatesElapsedTimeToClock()
    {
        var factory = CreateFactoryAtSpeed(SimulationSpeed.Normal);

        factory.Advance(60);

        Assert.Equal(540, factory.Clock.CurrentMinute);
    }

    [Fact]
    public void AdvanceWhilePausedDoesNotChangeClockTime()
    {
        var factory = new FactorySimulation();

        factory.Advance(60);

        Assert.Equal(480, factory.Clock.CurrentMinute);
    }

    [Theory]
    [InlineData(SimulationSpeed.Normal, 540)]
    [InlineData(SimulationSpeed.Fast, 600)]
    [InlineData(SimulationSpeed.VeryFast, 720)]
    public void AdvanceUsesClockSpeedMultiplier(SimulationSpeed speed, double expectedMinute)
    {
        var factory = CreateFactoryAtSpeed(speed);

        factory.Advance(60);

        Assert.Equal(expectedMinute, factory.Clock.CurrentMinute);
    }

    [Fact]
    public void BeginNextDayBeforeClosingDoesNothing()
    {
        var factory = CreateFactoryAtSpeed(SimulationSpeed.Normal);
        factory.Advance(10);

        factory.BeginNextDay();

        Assert.Equal(1, factory.CurrentDay);
        Assert.Equal(490, factory.Clock.CurrentMinute);
        Assert.Equal(SimulationSpeed.Normal, factory.Clock.Speed);
    }

    [Fact]
    public void ReachingClosingTimeDoesNotIncrementDay()
    {
        var factory = FinishWorkday(new FactorySimulation());

        Assert.Equal(1, factory.CurrentDay);
        Assert.Equal(960, factory.Clock.CurrentMinute);
        Assert.True(factory.Clock.IsWorkdayFinished);
    }

    [Fact]
    public void BeginNextDayAfterClosingIncrementsDay()
    {
        var factory = FinishWorkday(new FactorySimulation());

        factory.BeginNextDay();

        Assert.Equal(2, factory.CurrentDay);
    }

    [Fact]
    public void NewDayStartsAtEightOClockAtNormalSpeed()
    {
        var factory = FinishWorkday(new FactorySimulation());

        factory.BeginNextDay();

        Assert.Equal(480, factory.Clock.CurrentMinute);
        Assert.Equal(SimulationSpeed.Normal, factory.Clock.Speed);
        Assert.False(factory.Clock.IsWorkdayFinished);
    }

    [Fact]
    public void RepeatedBeginNextDayDoesNotSkipDays()
    {
        var factory = FinishWorkday(new FactorySimulation());
        factory.BeginNextDay();

        factory.BeginNextDay();

        Assert.Equal(2, factory.CurrentDay);
        Assert.Equal(480, factory.Clock.CurrentMinute);
    }

    [Fact]
    public void MultipleCompletedWorkdaysIncrementDay()
    {
        var factory = FinishWorkday(new FactorySimulation());
        factory.BeginNextDay();
        FinishWorkday(factory);
        factory.BeginNextDay();

        Assert.Equal(3, factory.CurrentDay);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void AdvanceRejectsInvalidElapsedSeconds(double elapsedSeconds)
    {
        var factory = new FactorySimulation();

        Assert.Throws<ArgumentOutOfRangeException>(() => factory.Advance(elapsedSeconds));
    }

    private static FactorySimulation CreateFactoryAtSpeed(SimulationSpeed speed)
    {
        var factory = new FactorySimulation();
        factory.Clock.SetSpeed(speed);
        return factory;
    }

    private static FactorySimulation FinishWorkday(FactorySimulation factory)
    {
        factory.Clock.SetSpeed(SimulationSpeed.Normal);
        factory.Advance(480);
        return factory;
    }
}
