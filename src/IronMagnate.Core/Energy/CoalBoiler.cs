using IronMagnate.Core.Resources;
using IronMagnate.Core.Storage;
using SupplyStorage = IronMagnate.Core.Storage.Storage;

namespace IronMagnate.Core.Energy;

public sealed class CoalBoiler
{
    public static readonly Resource Coal = new(ResourceType.Coal, "Coal", ResourceCategory.Fuel);

    public const double MaxCoalCapacity = 300;
    public const double OperatingConsumptionPerHour = 4;
    public const double StandbyConsumptionPerHour = 1;

    public CoalBoiler(SupplyStorage? fuelStorage = null)
    {
        FuelStorage = fuelStorage ?? CreateStarterFuelStorage();
    }

    public SupplyStorage FuelStorage { get; }

    public double CurrentCoalSupply => FuelStorage.GetQuantity(Coal);

    public bool IsPoweredOn { get; private set; } = false;

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
        return FuelStorage.Deliver(Coal, amount).UnacceptedQuantity;

    }

    public double Advance(double simulatedMinutes) => Advance(simulatedMinutes, isOperating: false);

    public double Advance(double simulatedMinutes, bool isOperating)
    {
        if (double.IsNaN(simulatedMinutes) ||
            double.IsInfinity(simulatedMinutes) ||
            simulatedMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(simulatedMinutes));
        }
        if (!IsPoweredOn)
            return 0;

        var consumptionPerHour = isOperating
            ? OperatingConsumptionPerHour
            : StandbyConsumptionPerHour;
        var consumptionPerMinute = consumptionPerHour / 60;
        var availablePoweredMinutes = CurrentCoalSupply / consumptionPerMinute;

        double actualPoweredMinutes = Math.Min(
            simulatedMinutes,
            availablePoweredMinutes
        );

        var coalConsumed = consumptionPerMinute * actualPoweredMinutes;
        if (coalConsumed > 0)
        {
            FuelStorage.TryWithdraw(Coal, coalConsumed);
        }

        if (CurrentCoalSupply <= 0)
        {
            TurnOff();
        }

        return actualPoweredMinutes;
    }

    private static SupplyStorage CreateStarterFuelStorage()
    {
        var storage = new SupplyStorage(StorageType.Fuel, MaxCoalCapacity, [ResourceCategory.Fuel]);
        storage.Deliver(Coal, 200);
        return storage;
    }
}
