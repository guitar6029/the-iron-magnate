using IronMagnate.Core.Production;
using IronMagnate.Core.Resources;
using Xunit;

namespace IronMagnate.Core.Tests.Production;

public class MachineOutputBufferTests
{
    [Fact]
    public void CreatesAnEmptyBufferWithConfiguredCapacity()
    {
        var buffer = new MachineOutputBuffer(5);

        Assert.Equal(5, buffer.Capacity);
        Assert.Equal(0, buffer.StoredQuantity);
        Assert.Equal(0, buffer.ReservedQuantity);
        Assert.Equal(5, buffer.AvailableCapacity);
        Assert.False(buffer.IsFull);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RejectsNonPositiveCapacity(int capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MachineOutputBuffer(capacity));
    }

    [Fact]
    public void CanReserveOnlyWhenTheEntireQuantityFits()
    {
        var buffer = new MachineOutputBuffer(5);
        buffer.TryReserve(2);

        Assert.True(buffer.CanReserve(3));
        Assert.False(buffer.CanReserve(4));
    }

    [Fact]
    public void TryReserveAddsCapacityWhenTheEntireQuantityFits()
    {
        var buffer = new MachineOutputBuffer(5);

        var reserved = buffer.TryReserve(3);

        Assert.True(reserved);
        Assert.Equal(3, buffer.ReservedQuantity);
        Assert.Equal(0, buffer.StoredQuantity);
        Assert.Equal(2, buffer.AvailableCapacity);
    }

    [Fact]
    public void TryReserveReturnsFalseWithoutChangingStateWhenCapacityIsInsufficient()
    {
        var buffer = new MachineOutputBuffer(5);
        buffer.TryReserve(3);

        var reserved = buffer.TryReserve(3);

        Assert.False(reserved);
        Assert.Equal(3, buffer.ReservedQuantity);
        Assert.Equal(0, buffer.StoredQuantity);
        Assert.Equal(2, buffer.AvailableCapacity);
    }

    [Fact]
    public void ReservationMakesTheBufferFullWhenItUsesTheLastAvailableSlot()
    {
        var buffer = new MachineOutputBuffer(2);

        Assert.True(buffer.TryReserve(2));
        Assert.Equal(0, buffer.AvailableCapacity);
        Assert.True(buffer.IsFull);
        Assert.False(buffer.TryReserve(1));
    }

    [Fact]
    public void CompleteTransfersReservedCapacityToStoredProducts()
    {
        var buffer = new MachineOutputBuffer(5);
        buffer.TryReserve(3);

        buffer.Complete(ResourceType.IronOre, 3);

        Assert.Equal(3, buffer.StoredQuantity);
        Assert.Equal(0, buffer.ReservedQuantity);
        Assert.Equal(2, buffer.AvailableCapacity);
    }

    [Fact]
    public void CompleteCanTransferPartOfTheReservedCapacity()
    {
        var buffer = new MachineOutputBuffer(5);
        buffer.TryReserve(4);

        buffer.Complete(ResourceType.IronOre, 2);

        Assert.Equal(2, buffer.StoredQuantity);
        Assert.Equal(2, buffer.ReservedQuantity);
        Assert.Equal(1, buffer.AvailableCapacity);
    }

    [Fact]
    public void CompleteRejectsMoreProductsThanReservedWithoutChangingState()
    {
        var buffer = new MachineOutputBuffer(5);
        buffer.TryReserve(2);

        Assert.Throws<InvalidOperationException>(() => buffer.Complete(ResourceType.IronOre, 3));

        Assert.Equal(0, buffer.StoredQuantity);
        Assert.Equal(2, buffer.ReservedQuantity);
        Assert.Equal(3, buffer.AvailableCapacity);
    }

    [Fact]
    public void TryReserveCompleteAndRemoveFormsACompleteLifecycle()
    {
        var buffer = new MachineOutputBuffer(5);

        Assert.True(buffer.TryReserve(3));
        buffer.Complete(ResourceType.IronOre, 3);

        Assert.True(buffer.TryRemove(ResourceType.IronOre, 3));
        Assert.Equal(0, buffer.StoredQuantity);
        Assert.Equal(0, buffer.ReservedQuantity);
        Assert.Equal(5, buffer.AvailableCapacity);
    }

    [Fact]
    public void TryRemoveSupportsPartialRemovalWithoutLosingTheRemainingProducts()
    {
        var buffer = CompletedBuffer(ResourceType.IronOre, 5);

        Assert.True(buffer.TryRemove(ResourceType.IronOre, 2));
        Assert.Equal(3, buffer.StoredQuantity);
        Assert.Equal(2, buffer.AvailableCapacity);
        Assert.True(buffer.TryRemove(ResourceType.IronOre, 3));
        Assert.Equal(0, buffer.StoredQuantity);
    }

    [Fact]
    public void TryRemoveReturnsFalseWithoutChangingStateWhenStockIsInsufficient()
    {
        var buffer = CompletedBuffer(ResourceType.IronOre, 2);

        var removed = buffer.TryRemove(ResourceType.IronOre, 3);

        Assert.False(removed);
        Assert.Equal(2, buffer.StoredQuantity);
        Assert.Equal(3, buffer.AvailableCapacity);
    }

    [Fact]
    public void TryRemoveReturnsFalseForAResourceTypeThatIsNotStored()
    {
        var buffer = CompletedBuffer(ResourceType.IronOre, 2);

        var removed = buffer.TryRemove(ResourceType.CopperOre, 1);

        Assert.False(removed);
        Assert.Equal(2, buffer.StoredQuantity);
    }

    [Fact]
    public void TracksMixedCompletedResourceTypesIndependently()
    {
        var buffer = new MachineOutputBuffer(5);
        buffer.TryReserve(5);
        buffer.Complete(ResourceType.IronOre, 2);
        buffer.Complete(ResourceType.CopperOre, 3);

        Assert.True(buffer.TryRemove(ResourceType.IronOre, 2));
        Assert.False(buffer.TryRemove(ResourceType.IronOre, 1));
        Assert.True(buffer.TryRemove(ResourceType.CopperOre, 3));
        Assert.Equal(0, buffer.StoredQuantity);
        Assert.Equal(5, buffer.AvailableCapacity);
    }

    [Fact]
    public void ReleaseFreesReservationsWithoutTouchingCompletedProducts()
    {
        var buffer = new MachineOutputBuffer(5);
        buffer.TryReserve(5);
        buffer.Complete(ResourceType.IronOre, 2);

        buffer.Release(3);

        Assert.Equal(2, buffer.StoredQuantity);
        Assert.Equal(0, buffer.ReservedQuantity);
        Assert.Equal(3, buffer.AvailableCapacity);
        Assert.True(buffer.TryRemove(ResourceType.IronOre, 2));
    }

    [Fact]
    public void TryReserveThenReleaseCancelsTheReservation()
    {
        var buffer = new MachineOutputBuffer(5);
        buffer.TryReserve(3);

        buffer.Release(3);

        Assert.Equal(0, buffer.ReservedQuantity);
        Assert.Equal(0, buffer.StoredQuantity);
        Assert.Equal(5, buffer.AvailableCapacity);
    }

    [Fact]
    public void ReleaseRejectsMoreThanReservedWithoutChangingState()
    {
        var buffer = new MachineOutputBuffer(5);
        buffer.TryReserve(2);

        Assert.Throws<InvalidOperationException>(() => buffer.Release(3));

        Assert.Equal(2, buffer.ReservedQuantity);
        Assert.Equal(0, buffer.StoredQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void OperationsRejectNonPositiveQuantities(int quantity)
    {
        var buffer = new MachineOutputBuffer(5);

        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.CanReserve(quantity));
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.TryReserve(quantity));
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Complete(ResourceType.IronOre, quantity));
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.TryRemove(ResourceType.IronOre, quantity));
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Release(quantity));
        Assert.Equal(0, buffer.StoredQuantity);
        Assert.Equal(0, buffer.ReservedQuantity);
    }

    [Fact]
    public void CompleteAndRemoveRejectResourceTypesMissingFromTheCatalogWithoutChangingState()
    {
        var buffer = new MachineOutputBuffer(5);
        buffer.TryReserve(2);

        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Complete((ResourceType)99, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => buffer.TryRemove((ResourceType)99, 1));

        Assert.Equal(0, buffer.StoredQuantity);
        Assert.Equal(2, buffer.ReservedQuantity);
        Assert.Equal(3, buffer.AvailableCapacity);
    }

    private static MachineOutputBuffer CompletedBuffer(ResourceType type, int quantity)
    {
        var buffer = new MachineOutputBuffer(5);
        buffer.TryReserve(quantity);
        buffer.Complete(type, quantity);
        return buffer;
    }
}
