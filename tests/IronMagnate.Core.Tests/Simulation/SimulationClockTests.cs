using IronMagnate.Core.Simulation;
using Xunit;

namespace IronMagnate.Core.Tests.Simulation;

public class SimulationClockTests
{
    [Fact]
    public void StartsAtEightOClockAndPaused()
    {
        var clock = new SimulationClock();

        Assert.Equal(480, clock.CurrentMinute);
        Assert.Equal(SimulationSpeed.Paused, clock.Speed);
        Assert.False(clock.IsWorkdayFinished);
    }

    [Fact]
    public void AdvanceAtNormalSpeedUsesOneMinutePerSecond()
    {
        var clock = CreateClockAtSpeed(SimulationSpeed.Normal);

        clock.Advance(60);

        Assert.Equal(540, clock.CurrentMinute);
    }

    [Fact]
    public void AdvanceAtFastSpeedUsesTwoMinutesPerSecond()
    {
        var clock = CreateClockAtSpeed(SimulationSpeed.Fast);

        clock.Advance(60);

        Assert.Equal(600, clock.CurrentMinute);
    }

    [Fact]
    public void AdvanceAtVeryFastSpeedUsesFourMinutesPerSecond()
    {
        var clock = CreateClockAtSpeed(SimulationSpeed.VeryFast);

        clock.Advance(60);

        Assert.Equal(720, clock.CurrentMinute);
    }

    [Fact]
    public void AdvanceWhilePausedDoesNotChangeTime()
    {
        var clock = new SimulationClock();

        clock.Advance(60);

        Assert.Equal(480, clock.CurrentMinute);
    }

    [Fact]
    public void AdvanceWithZeroElapsedSecondsDoesNotChangeTime()
    {
        var clock = CreateClockAtSpeed(SimulationSpeed.Normal);

        clock.Advance(0);

        Assert.Equal(480, clock.CurrentMinute);
    }

    [Fact]
    public void AdvanceSupportsFractionalElapsedSeconds()
    {
        var clock = CreateClockAtSpeed(SimulationSpeed.Fast);

        clock.Advance(0.25);

        Assert.Equal(480.5, clock.CurrentMinute);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void AdvanceRejectsNegativeAndNonFiniteElapsedSeconds(double elapsedSeconds)
    {
        var clock = new SimulationClock();

        Assert.Throws<ArgumentOutOfRangeException>(() => clock.Advance(elapsedSeconds));
    }

    [Fact]
    public void SetSpeedRejectsUndefinedEnumValues()
    {
        var clock = new SimulationClock();

        Assert.Throws<ArgumentOutOfRangeException>(() => clock.SetSpeed((SimulationSpeed)3));
    }

    [Fact]
    public void AdvanceReachingClosingTimeFinishesAndPausesWorkday()
    {
        var clock = CreateClockAtSpeed(SimulationSpeed.Normal);

        clock.Advance(480);

        Assert.Equal(960, clock.CurrentMinute);
        Assert.True(clock.IsWorkdayFinished);
        Assert.Equal(SimulationSpeed.Paused, clock.Speed);
    }

    [Fact]
    public void AdvancePastClosingTimeStopsAtClosingTime()
    {
        var clock = CreateClockAtSpeed(SimulationSpeed.Fast);

        clock.Advance(500);

        Assert.Equal(960, clock.CurrentMinute);
        Assert.True(clock.IsWorkdayFinished);
    }

    [Fact]
    public void SetSpeedAfterClosingDoesNotRestartWorkday()
    {
        var clock = FinishWorkday();

        clock.SetSpeed(SimulationSpeed.VeryFast);

        Assert.Equal(SimulationSpeed.Paused, clock.Speed);
        Assert.Equal(960, clock.CurrentMinute);
    }

    [Fact]
    public void BeginNextDayBeforeClosingDoesNothing()
    {
        var clock = CreateClockAtSpeed(SimulationSpeed.Normal);
        clock.Advance(10);

        clock.BeginNextDay();

        Assert.Equal(490, clock.CurrentMinute);
        Assert.Equal(SimulationSpeed.Normal, clock.Speed);
    }

    [Fact]
    public void BeginNextDayAfterClosingRestoresStartTimeAndNormalSpeed()
    {
        var clock = FinishWorkday();

        clock.BeginNextDay();

        Assert.Equal(480, clock.CurrentMinute);
        Assert.Equal(SimulationSpeed.Normal, clock.Speed);
        Assert.False(clock.IsWorkdayFinished);
    }

    [Fact]
    public void RepeatedBeginNextDayDoesNotRestartAnActiveWorkday()
    {
        var clock = FinishWorkday();
        clock.BeginNextDay();
        clock.Advance(5);

        clock.BeginNextDay();

        Assert.Equal(485, clock.CurrentMinute);
        Assert.Equal(SimulationSpeed.Normal, clock.Speed);
    }

    private static SimulationClock CreateClockAtSpeed(SimulationSpeed speed)
    {
        var clock = new SimulationClock();
        clock.SetSpeed(speed);
        return clock;
    }

    private static SimulationClock FinishWorkday()
    {
        var clock = CreateClockAtSpeed(SimulationSpeed.Normal);
        clock.Advance(480);
        return clock;
    }
}
