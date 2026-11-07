namespace IronMagnate.Core.Energy;

public class CoalBoiler
{
    public double CurrentCoalSupply { get; private set; } = 100;

    public const double MaxCoalCapacity = 200;

    public bool IsPoweredOn { get; private set; } = false;

    public const int ConsumptionPerMinute = 2;

    public void TurnOn()
    {
        if (CurrentCoalSupply <= 0)
        {
            return;
        }
        IsPoweredOn = true;
    }

    public void TurnOff()
    {
        IsPoweredOn = false;
    }

    public double AddCoal(double amount)
    {

        if (Double.IsNaN(amount) || Double.IsInfinity(amount) || amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        var coalSupplyBeforeAdd = CurrentCoalSupply;
        CurrentCoalSupply = Math.Min(
                CurrentCoalSupply + amount, MaxCoalCapacity
            );


        return amount - (CurrentCoalSupply - coalSupplyBeforeAdd);

    }

    public double Advance(double simulatedMinutes)
    {
        if (double.IsNaN(simulatedMinutes) ||
            double.IsInfinity(simulatedMinutes) ||
            simulatedMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(simulatedMinutes));
        }
        if (!IsPoweredOn)
            return 0;

        // How long COULD the boiler operate with its current coal?
        double availablePoweredMinutes =
             CurrentCoalSupply / ConsumptionPerMinute;

        double actualPoweredMinutes = Math.Min(
            simulatedMinutes,
            availablePoweredMinutes
        );

        double coalConsumed =
            ConsumptionPerMinute * actualPoweredMinutes;

        CurrentCoalSupply = Math.Max(
            0,
            CurrentCoalSupply - coalConsumed
        );

        if (CurrentCoalSupply <= 0)
        {
            TurnOff();
        }

        return actualPoweredMinutes;
    }

}
