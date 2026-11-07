using IronMagnate.Core.Energy;
using IronMagnate.Core.Resources;
using IronMagnate.Core.Storage;
using SupplyStorage = IronMagnate.Core.Storage.Storage;

namespace IronMagnate.Core.Simulation;

public class FactorySimulation
{
    public SimulationClock Clock { get; } = new();

    public SupplyStorage RawMaterialsWarehouse { get; } = new(
        StorageType.RawMaterials,
        500,
        [ResourceCategory.RawMaterial]);

    public SupplyStorage PartsAndProductsWarehouse { get; } = new(
        StorageType.PartsAndProducts,
        250,
        [ResourceCategory.ProcessedMaterial, ResourceCategory.ManufacturedGood]);

    public SupplyStorage CoalYard { get; } = CreateStarterCoalYard();

    public CoalBoiler Boiler { get; }

    public int CurrentDay { get; private set; } = 1;

    public FactorySimulation()
    {
        Boiler = new CoalBoiler(CoalYard);
    }

    public void BeginNextDay()
    {
        // check the clock, is the work day finished ?
        var isWorkdayFinished = Clock.IsWorkdayFinished;
        if (!isWorkdayFinished) return;

        Clock.BeginNextDay();

        CurrentDay += 1;
    }

    public double Advance(double elapsedSeconds)
    {

        double simulatedMinutes = Clock.Advance(elapsedSeconds);
        Boiler.Advance(simulatedMinutes);

        return simulatedMinutes;

    }

    private static SupplyStorage CreateStarterCoalYard()
    {
        var coalYard = new SupplyStorage(StorageType.Fuel, 300, [ResourceCategory.Fuel]);
        coalYard.Deliver(CoalBoiler.Coal, 200);
        return coalYard;
    }
}
