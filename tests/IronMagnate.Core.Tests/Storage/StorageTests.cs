using IronMagnate.Core.Resources;
using IronMagnate.Core.Storage;
using IronMagnate.Core.Simulation;
using SupplyStorage = IronMagnate.Core.Storage.Storage;
using Xunit;

namespace IronMagnate.Core.Tests.Storage;

public class StorageTests
{
    [Fact]
    public void DeliveryAcceptsOnlyTheQuantityThatFitsAndReportsTheExcess()
    {
        var storage = RawMaterialsStorage(capacity: 5);

        var delivery = storage.Deliver(ResourceType.IronOre, 8);

        Assert.Equal(8, delivery.RequestedQuantity);
        Assert.Equal(5, delivery.AcceptedQuantity);
        Assert.Equal(3, delivery.UnacceptedQuantity);
        Assert.Equal(5, storage.OccupiedCapacity);
        Assert.Equal(0, storage.AvailableCapacity);
    }

    [Fact]
    public void DeliveryRejectsIncompatibleResourcesWithoutChangingInventory()
    {
        var storage = RawMaterialsStorage();

        Assert.Throws<ArgumentException>(() => storage.Deliver(ResourceType.Coal, 1));

        Assert.Equal(0, storage.OccupiedCapacity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void OperationsRejectInvalidQuantities(double quantity)
    {
        var storage = RawMaterialsStorage();

        Assert.Throws<ArgumentOutOfRangeException>(() => storage.Deliver(ResourceType.IronOre, quantity));
        Assert.Throws<ArgumentOutOfRangeException>(() => storage.TryWithdraw(ResourceType.IronOre, quantity));
    }

    [Fact]
    public void MultipleResourceWithdrawalIsAtomicWhenAnyMaterialIsMissing()
    {
        var storage = RawMaterialsStorage();
        storage.Deliver(ResourceType.IronOre, 4);
        storage.Deliver(ResourceType.CopperOre, 2);

        var withdrawn = storage.TryWithdraw([
            new ResourceQuantity(ResourceType.IronOre, 3),
            new ResourceQuantity(ResourceType.CopperOre, 3)
        ]);

        Assert.False(withdrawn);
        Assert.Equal(4, storage.GetQuantity(ResourceType.IronOre));
        Assert.Equal(2, storage.GetQuantity(ResourceType.CopperOre));
    }

    [Fact]
    public void MultipleResourceWithdrawalCombinesRepeatedRequirements()
    {
        var storage = RawMaterialsStorage();
        storage.Deliver(ResourceType.IronOre, 5);

        var withdrawn = storage.TryWithdraw([
            new ResourceQuantity(ResourceType.IronOre, 2),
            new ResourceQuantity(ResourceType.IronOre, 3)
        ]);

        Assert.True(withdrawn);
        Assert.Equal(0, storage.GetQuantity(ResourceType.IronOre));
        Assert.Equal(0, storage.OccupiedCapacity);
    }

    [Fact]
    public void FactoryUsesTheConfiguredStarterStorageCapacitiesAndCoal()
    {
        var factory = new FactorySimulation();

        Assert.Equal(500, factory.RawMaterialsWarehouse.Capacity);
        Assert.Equal(250, factory.PartsAndProductsWarehouse.Capacity);
        Assert.Equal(300, factory.CoalYard.Capacity);
        Assert.Equal(200, factory.CoalYard.GetQuantity(IronMagnate.Core.Energy.CoalBoiler.FuelType));
    }

    [Fact]
    public void ResourceTypeIsTheInventoryIdentityRegardlessOfDisplayMetadata()
    {
        var coal = ResourceCatalog.Get(ResourceType.Coal);

        Assert.Equal(ResourceType.Coal, coal.Type);
        Assert.Equal("Coal", coal.Name);
        Assert.Equal(ResourceCategory.Fuel, coal.Category);
        Assert.Empty(typeof(Resource).GetConstructors());
    }

    [Theory]
    [InlineData(1.5)]
    [InlineData(-1)]
    [InlineData(0)]
    public void StorageRejectsFractionalOrNonPositiveCapacity(double capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SupplyStorage(StorageType.RawMaterials, capacity, [ResourceCategory.RawMaterial]));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.5)]
    public void PhysicalInventoryRejectsFractionalQuantities(double quantity)
    {
        var storage = RawMaterialsStorage();

        Assert.Throws<ArgumentOutOfRangeException>(() => storage.Deliver(ResourceType.IronOre, quantity));
        Assert.Throws<ArgumentOutOfRangeException>(() => storage.TryWithdraw(ResourceType.IronOre, quantity));
    }

    [Fact]
    public void FuelStorageAllowsFractionalCoalForBoilerConsumption()
    {
        var fuelStorage = new SupplyStorage(StorageType.Fuel, 2, [ResourceCategory.Fuel]);

        fuelStorage.Deliver(ResourceType.Coal, 1.5);

        Assert.True(fuelStorage.TryWithdraw(ResourceType.Coal, 0.5));
        Assert.Equal(1, fuelStorage.GetQuantity(ResourceType.Coal));
    }

    private static SupplyStorage RawMaterialsStorage(double capacity = 10) =>
        new(StorageType.RawMaterials, capacity, [ResourceCategory.RawMaterial]);
}
