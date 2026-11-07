using IronMagnate.Core.Resources;
using IronMagnate.Core.Storage;
using IronMagnate.Core.Simulation;
using SupplyStorage = IronMagnate.Core.Storage.Storage;
using Xunit;

namespace IronMagnate.Core.Tests.Storage;

public class StorageTests
{
    private static readonly Resource IronOre = new(ResourceType.IronOre, "Iron ore", ResourceCategory.RawMaterial);
    private static readonly Resource CopperOre = new(ResourceType.CopperOre, "Copper ore", ResourceCategory.RawMaterial);
    private static readonly Resource Coal = new(ResourceType.Coal, "Coal", ResourceCategory.Fuel);

    [Fact]
    public void DeliveryAcceptsOnlyTheQuantityThatFitsAndReportsTheExcess()
    {
        var storage = RawMaterialsStorage(capacity: 5);

        var delivery = storage.Deliver(IronOre, 8);

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

        Assert.Throws<ArgumentException>(() => storage.Deliver(Coal, 1));

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

        Assert.Throws<ArgumentOutOfRangeException>(() => storage.Deliver(IronOre, quantity));
        Assert.Throws<ArgumentOutOfRangeException>(() => storage.TryWithdraw(IronOre, quantity));
    }

    [Fact]
    public void MultipleResourceWithdrawalIsAtomicWhenAnyMaterialIsMissing()
    {
        var storage = RawMaterialsStorage();
        storage.Deliver(IronOre, 4);
        storage.Deliver(CopperOre, 2);

        var withdrawn = storage.TryWithdraw([
            new ResourceQuantity(IronOre, 3),
            new ResourceQuantity(CopperOre, 3)
        ]);

        Assert.False(withdrawn);
        Assert.Equal(4, storage.GetQuantity(IronOre));
        Assert.Equal(2, storage.GetQuantity(CopperOre));
    }

    [Fact]
    public void MultipleResourceWithdrawalCombinesRepeatedRequirements()
    {
        var storage = RawMaterialsStorage();
        storage.Deliver(IronOre, 5);

        var withdrawn = storage.TryWithdraw([
            new ResourceQuantity(IronOre, 2),
            new ResourceQuantity(IronOre, 3)
        ]);

        Assert.True(withdrawn);
        Assert.Equal(0, storage.GetQuantity(IronOre));
        Assert.Equal(0, storage.OccupiedCapacity);
    }

    [Fact]
    public void FactoryUsesTheConfiguredStarterStorageCapacitiesAndCoal()
    {
        var factory = new FactorySimulation();

        Assert.Equal(500, factory.RawMaterialsWarehouse.Capacity);
        Assert.Equal(250, factory.PartsAndProductsWarehouse.Capacity);
        Assert.Equal(300, factory.CoalYard.Capacity);
        Assert.Equal(200, factory.CoalYard.GetQuantity(IronMagnate.Core.Energy.CoalBoiler.Coal));
    }

    private static SupplyStorage RawMaterialsStorage(double capacity = 10) =>
        new(StorageType.RawMaterials, capacity, [ResourceCategory.RawMaterial]);
}
