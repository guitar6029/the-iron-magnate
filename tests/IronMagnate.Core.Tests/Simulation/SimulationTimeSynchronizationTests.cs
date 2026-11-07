using IronMagnate.Core.Simulation;
using Xunit;

namespace IronMagnate.Core.Tests.Simulation;

public class SimulationTimeSynchronizationTests
{
    [Theory]
    [InlineData(SimulationSpeed.Normal, 1)]
    [InlineData(SimulationSpeed.Fast, 2)]
    [InlineData(SimulationSpeed.VeryFast, 4)]
    public void AdvanceReturnsSimulatedMinutesForSpeed(SimulationSpeed speed, double expectedMinutes)
    {
        var clock = CreateClockAtSpeed(speed);

        var actualMinutes = clock.Advance(1);

        Assert.Equal(expectedMinutes, actualMinutes);
    }

    [Fact]
    public void AdvanceWhilePausedReturnsZeroWithoutChangingTime()
    {
        var clock = new SimulationClock();

        var actualMinutes = clock.Advance(60);

        Assert.Equal(0, actualMinutes);
        Assert.Equal(480, clock.CurrentMinute);
    }

    [Fact]
    public void AdvanceWithZeroElapsedSecondsReturnsZero()
    {
        var clock = CreateClockAtSpeed(SimulationSpeed.Normal);

        var actualMinutes = clock.Advance(0);

        Assert.Equal(0, actualMinutes);
        Assert.Equal(480, clock.CurrentMinute);
    }

    [Fact]
    public void AdvanceReturnsFractionalSimulatedMinutes()
    {
        var clock = CreateClockAtSpeed(SimulationSpeed.Fast);

        var actualMinutes = clock.Advance(0.25);

        Assert.Equal(0.5, actualMinutes);
        Assert.Equal(480.5, clock.CurrentMinute);
    }

    [Fact]
    public void AdvanceToClosingTimeReturnsElapsedDuration()
    {
        var clock = CreateClockAtSpeed(SimulationSpeed.Normal);

        var actualMinutes = clock.Advance(480);

        Assert.Equal(480, actualMinutes);
        Assert.Equal(960, clock.CurrentMinute);
        Assert.Equal(SimulationSpeed.Paused, clock.Speed);
    }

    [Fact]
    public void AdvancePastClosingReturnsOnlyRemainingMinutes()
    {
        var clock = CreateClockAtSpeed(SimulationSpeed.Normal);
        clock.Advance(478);
        clock.SetSpeed(SimulationSpeed.VeryFast);

        var actualMinutes = clock.Advance(5);

        Assert.Equal(2, actualMinutes);
        Assert.Equal(960, clock.CurrentMinute);
        Assert.Equal(SimulationSpeed.Paused, clock.Speed);
    }

    [Fact]
    public void AdvanceAfterWorkdayFinishedReturnsZero()
    {
        var clock = CreateClockAtSpeed(SimulationSpeed.Normal);
        clock.Advance(480);

        var actualMinutes = clock.Advance(1);

        Assert.Equal(0, actualMinutes);
        Assert.Equal(960, clock.CurrentMinute);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void AdvanceRejectsInvalidElapsedSecondsWithoutChangingState(double elapsedSeconds)
    {
        var clock = CreateClockAtSpeed(SimulationSpeed.Fast);
        var initialMinute = clock.CurrentMinute;
        var initialSpeed = clock.Speed;

        Assert.Throws<ArgumentOutOfRangeException>(() => clock.Advance(elapsedSeconds));

        Assert.Equal(initialMinute, clock.CurrentMinute);
        Assert.Equal(initialSpeed, clock.Speed);
    }

    [Fact]
    public void FactoryAdvanceReturnsSameDurationAsClock()
    {
        var factory = new FactorySimulation();
        factory.Clock.SetSpeed(SimulationSpeed.Fast);
        var clock = CreateClockAtSpeed(SimulationSpeed.Fast);

        var factoryMinutes = factory.Advance(30);
        var clockMinutes = clock.Advance(30);

        Assert.Equal(clockMinutes, factoryMinutes);
        Assert.Equal(clock.CurrentMinute, factory.Clock.CurrentMinute);
    }

    [Fact]
    public void SequentialAdvancesReturnIncrementalDurations()
    {
        var clock = CreateClockAtSpeed(SimulationSpeed.Fast);

        var firstAdvance = clock.Advance(1);
        var secondAdvance = clock.Advance(2);
        var thirdAdvance = clock.Advance(0.5);

        Assert.Equal(2, firstAdvance);
        Assert.Equal(4, secondAdvance);
        Assert.Equal(1, thirdAdvance);
        Assert.Equal(487, clock.CurrentMinute);
    }

    private static SimulationClock CreateClockAtSpeed(SimulationSpeed speed)
    {
        var clock = new SimulationClock();
        clock.SetSpeed(speed);
        return clock;
    }
}
