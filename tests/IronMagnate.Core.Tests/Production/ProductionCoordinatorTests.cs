using IronMagnate.Core.Production;
using IronMagnate.Core.Resources;
using Xunit;

namespace IronMagnate.Core.Tests.Production;

public class ProductionCoordinatorTests
{
    [Fact]
    public void RegistersAValidMachine()
    {
        var coordinator = new ProductionCoordinator();
        var machine = CreateMachine();

        var registered = coordinator.RegisterMachine(machine);

        Assert.True(registered);
        Assert.Same(machine, coordinator.GetMachine(machine.Id));
    }

    [Fact]
    public void RejectsRegisteringTheSameMachineTwice()
    {
        var coordinator = new ProductionCoordinator();
        var machine = CreateMachine();

        coordinator.RegisterMachine(machine);

        Assert.False(coordinator.RegisterMachine(machine));
        Assert.Same(machine, coordinator.GetMachine(machine.Id));
    }

    [Fact]
    public void RejectsADifferentMachineWithAnAlreadyRegisteredId()
    {
        var coordinator = new ProductionCoordinator();
        var machineId = Guid.NewGuid();
        var registeredMachine = CreateMachine(machineId, "Stamping press");
        var duplicateIdMachine = CreateMachine(machineId, "Rolling mill");

        coordinator.RegisterMachine(registeredMachine);

        Assert.False(coordinator.RegisterMachine(duplicateIdMachine));
        Assert.Same(registeredMachine, coordinator.GetMachine(machineId));
    }

    [Fact]
    public void RejectsNullMachineRegistration()
    {
        var coordinator = new ProductionCoordinator();

        Assert.Throws<ArgumentNullException>(() => coordinator.RegisterMachine(null!));
    }

    [Fact]
    public void RetrievesARegisteredMachineById()
    {
        var coordinator = new ProductionCoordinator();
        var machine = CreateMachine();
        coordinator.RegisterMachine(machine);

        var retrievedMachine = coordinator.GetMachine(machine.Id);

        Assert.Same(machine, retrievedMachine);
    }

    [Fact]
    public void ReturnsNullWhenRetrievingAnUnknownMachine()
    {
        var coordinator = new ProductionCoordinator();

        Assert.Null(coordinator.GetMachine(Guid.NewGuid()));
    }

    [Fact]
    public void FailedRegistrationDoesNotModifyExistingRegistryEntries()
    {
        var coordinator = new ProductionCoordinator();
        var firstMachine = CreateMachine();
        var secondMachine = CreateMachine();
        var duplicateIdMachine = CreateMachine(firstMachine.Id, "Rolling mill");
        coordinator.RegisterMachine(firstMachine);
        coordinator.RegisterMachine(secondMachine);

        coordinator.RegisterMachine(duplicateIdMachine);

        Assert.Same(firstMachine, coordinator.GetMachine(firstMachine.Id));
        Assert.Same(secondMachine, coordinator.GetMachine(secondMachine.Id));
    }

    private static Machine CreateMachine(Guid? id = null, string name = "Stamping press")
    {
        var schematic = new Schematic(
            "Iron plate",
            new Dictionary<ResourceType, int> { [ResourceType.IronOre] = 3 },
            ResourceType.CopperOre,
            1,
            15);

        return new Machine(id ?? Guid.NewGuid(), name, 4, 2, [schematic], schematic);
    }
}
