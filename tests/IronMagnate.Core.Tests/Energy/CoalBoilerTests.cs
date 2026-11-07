using IronMagnate.Core.Energy;
using Xunit;

namespace IronMagnate.Core.Tests.Energy;

public class CoalBoilerTests
{
    [Fact]
    public void StartsWithCoalAndPoweredOff()
    {
        var boiler = new CoalBoiler();

        Assert.Equal(100, boiler.CurrentCoalSupply);
        Assert.False(boiler.IsPoweredOn);
    }

    [Fact]
    public void TurnOnWithCoalPowersBoiler()
    {
        var boiler = new CoalBoiler();

        boiler.TurnOn();

        Assert.True(boiler.IsPoweredOn);
    }

    [Fact]
    public void TurnOffPowersBoilerOff()
    {
        var boiler = PoweredBoiler();

        boiler.TurnOff();

        Assert.False(boiler.IsPoweredOn);
    }

    [Fact]
    public void RepeatedPowerRequestsLeavePredictableState()
    {
        var boiler = new CoalBoiler();

        boiler.TurnOn();
        boiler.TurnOn();
        Assert.True(boiler.IsPoweredOn);

        boiler.TurnOff();
        boiler.TurnOff();
        Assert.False(boiler.IsPoweredOn);
    }

    [Fact]
    public void CannotTurnOnWithoutCoalAndAddingCoalDoesNotRestartIt()
    {
        var boiler = PoweredBoiler();
        boiler.Advance(50);

        boiler.TurnOn();
        boiler.AddCoal(10);

        Assert.False(boiler.IsPoweredOn);
        Assert.Equal(10, boiler.CurrentCoalSupply);
    }

    [Fact]
    public void AddCoalWithinCapacityStoresAllCoalAndReturnsZero()
    {
        var boiler = new CoalBoiler();

        var leftover = boiler.AddCoal(25);

        Assert.Equal(125, boiler.CurrentCoalSupply);
        Assert.Equal(0, leftover);
    }

    [Fact]
    public void AddCoalBeyondCapacityClampsAndReturnsLeftover()
    {
        var boiler = new CoalBoiler();
        boiler.AddCoal(90);

        var leftover = boiler.AddCoal(20);

        Assert.Equal(CoalBoiler.MaxCoalCapacity, boiler.CurrentCoalSupply);
        Assert.Equal(10, leftover);
    }

    [Fact]
    public void AddCoalToFullBoilerReturnsEntireRequestedAmount()
    {
        var boiler = new CoalBoiler();
        boiler.AddCoal(100);

        var leftover = boiler.AddCoal(30);

        Assert.Equal(CoalBoiler.MaxCoalCapacity, boiler.CurrentCoalSupply);
        Assert.Equal(30, leftover);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void AddCoalRejectsInvalidAmountsWithoutChangingState(double amount)
    {
        var boiler = new CoalBoiler();

        Assert.Throws<ArgumentOutOfRangeException>(() => boiler.AddCoal(amount));

        Assert.Equal(100, boiler.CurrentCoalSupply);
        Assert.False(boiler.IsPoweredOn);
    }

    [Fact]
    public void AddCoalWithLargeFiniteAmountDoesNotCreateNonFiniteSupply()
    {
        var boiler = new CoalBoiler();

        var leftover = boiler.AddCoal(double.MaxValue);

        Assert.Equal(CoalBoiler.MaxCoalCapacity, boiler.CurrentCoalSupply);
        Assert.True(double.IsFinite(boiler.CurrentCoalSupply));
        Assert.True(double.IsFinite(leftover));
    }

    [Fact]
    public void AdvanceWhileOffDoesNotConsumeCoalOrProvidePower()
    {
        var boiler = new CoalBoiler();

        var poweredMinutes = boiler.Advance(10);

        Assert.Equal(0, poweredMinutes);
        Assert.Equal(100, boiler.CurrentCoalSupply);
    }

    [Fact]
    public void AdvanceConsumesTwoCoalPerSimulatedMinute()
    {
        var boiler = PoweredBoiler();

        var poweredMinutes = boiler.Advance(10);

        Assert.Equal(10, poweredMinutes);
        Assert.Equal(80, boiler.CurrentCoalSupply);
    }

    [Fact]
    public void AdvanceSupportsFractionalSimulatedMinutes()
    {
        var boiler = PoweredBoiler();

        var poweredMinutes = boiler.Advance(0.25);

        Assert.Equal(0.25, poweredMinutes);
        Assert.Equal(99.5, boiler.CurrentCoalSupply);
    }

    [Fact]
    public void AdvanceWithZeroMinutesDoesNotConsumeCoal()
    {
        var boiler = PoweredBoiler();

        var poweredMinutes = boiler.Advance(0);

        Assert.Equal(0, poweredMinutes);
        Assert.Equal(100, boiler.CurrentCoalSupply);
        Assert.True(boiler.IsPoweredOn);
    }

    [Fact]
    public void ExhaustionReturnsActualPoweredDurationThenShutsDown()
    {
        var boiler = PoweredBoiler();

        var poweredMinutes = boiler.Advance(75);

        Assert.Equal(50, poweredMinutes);
        Assert.Equal(0, boiler.CurrentCoalSupply);
        Assert.False(boiler.IsPoweredOn);
        Assert.Equal(0, boiler.Advance(1));
    }

    [Fact]
    public void AdvanceNeverPowersLongerThanRequestedOrMakesCoalNegative()
    {
        var boiler = PoweredBoiler();

        var poweredMinutes = boiler.Advance(1);

        Assert.InRange(poweredMinutes, 0, 1);
        Assert.True(boiler.CurrentCoalSupply >= 0);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void AdvanceRejectsInvalidDurationsWithoutChangingState(double simulatedMinutes)
    {
        var boiler = PoweredBoiler();

        Assert.Throws<ArgumentOutOfRangeException>(() => boiler.Advance(simulatedMinutes));

        Assert.Equal(100, boiler.CurrentCoalSupply);
        Assert.True(boiler.IsPoweredOn);
    }

    private static CoalBoiler PoweredBoiler()
    {
        var boiler = new CoalBoiler();
        boiler.TurnOn();
        return boiler;
    }
}
