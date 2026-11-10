using IronMagnate.Core.Production;
using IronMagnate.Core.Resources;
using FactoryWorkforce = IronMagnate.Core.Workforce.Workforce;
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

    [Fact]
    public void IsFullyStaffedReturnsFalseWhenNoWorkersAreAssigned()
    {
        var coordinator = new ProductionCoordinator();
        var machine = CreateMachine(requiredCrew: 5);
        coordinator.RegisterMachine(machine);

        Assert.False(coordinator.IsFullyStaffed(machine.Id, CreateWorkforce(5)));
    }

    [Fact]
    public void IsFullyStaffedReturnsFalseWhenOnlyPartOfTheCrewIsAssigned()
    {
        var coordinator = new ProductionCoordinator();
        var machine = CreateMachine(requiredCrew: 5);
        var workforce = CreateWorkforce(5);
        AssignWorkers(workforce, machine.Id, 3);
        coordinator.RegisterMachine(machine);

        Assert.False(coordinator.IsFullyStaffed(machine.Id, workforce));
    }

    [Fact]
    public void IsFullyStaffedReturnsTrueWhenTheCompleteCrewIsAssigned()
    {
        var coordinator = new ProductionCoordinator();
        var machine = CreateMachine(requiredCrew: 5);
        var workforce = CreateWorkforce(5);
        AssignWorkers(workforce, machine.Id, 5);
        coordinator.RegisterMachine(machine);

        Assert.True(coordinator.IsFullyStaffed(machine.Id, workforce));
    }

    [Fact]
    public void IsFullyStaffedDoesNotCountWorkersAssignedToAnotherMachine()
    {
        var coordinator = new ProductionCoordinator();
        var machine = CreateMachine(requiredCrew: 5);
        var workforce = CreateWorkforce(6);
        AssignWorkers(workforce, machine.Id, 4);
        Assert.True(workforce.TryAssign(workforce.Employees[4].Id, Guid.NewGuid().ToString()));
        coordinator.RegisterMachine(machine);

        Assert.False(coordinator.IsFullyStaffed(machine.Id, workforce));
    }

    [Fact]
    public void IsFullyStaffedReturnsFalseForAnUnknownMachine()
    {
        var coordinator = new ProductionCoordinator();

        Assert.False(coordinator.IsFullyStaffed(Guid.NewGuid(), CreateWorkforce(5)));
    }

    [Fact]
    public void IsFullyStaffedReturnsTrueForADisabledMachineWithACompleteCrew()
    {
        var coordinator = new ProductionCoordinator();
        var machine = CreateMachine(requiredCrew: 5);
        var workforce = CreateWorkforce(5);
        AssignWorkers(workforce, machine.Id, 5);
        machine.Disable();
        coordinator.RegisterMachine(machine);

        Assert.True(coordinator.IsFullyStaffed(machine.Id, workforce));
    }

    [Fact]
    public void IsFullyStaffedRejectsANullWorkforce()
    {
        var coordinator = new ProductionCoordinator();
        var machine = CreateMachine(requiredCrew: 5);
        coordinator.RegisterMachine(machine);

        Assert.Throws<ArgumentNullException>(() => coordinator.IsFullyStaffed(machine.Id, null!));
    }

    [Fact]
    public void IsFullyStaffedDoesNotChangeWorkforceAssignments()
    {
        var coordinator = new ProductionCoordinator();
        var machine = CreateMachine(requiredCrew: 5);
        var workforce = CreateWorkforce(5);
        AssignWorkers(workforce, machine.Id, 3);
        var assignmentsBeforeReadinessCheck = workforce.Assignments;
        coordinator.RegisterMachine(machine);

        coordinator.IsFullyStaffed(machine.Id, workforce);

        Assert.Equal(assignmentsBeforeReadinessCheck, workforce.Assignments);
    }

    [Fact]
    public void IsFullyStaffedReturnsFalseAfterAnAssignedWorkerIsDismissed()
    {
        var coordinator = new ProductionCoordinator();
        var machine = CreateMachine(requiredCrew: 5);
        var workforce = CreateWorkforce(5);
        AssignWorkers(workforce, machine.Id, 5);
        coordinator.RegisterMachine(machine);

        Assert.True(workforce.Dismiss(workforce.Employees[0].Id));

        Assert.False(coordinator.IsFullyStaffed(machine.Id, workforce));
    }

    private static Machine CreateMachine(
        Guid? id = null,
        string name = "Stamping press",
        int requiredCrew = 2)
    {
        var schematic = new Schematic(
            "Iron plate",
            new Dictionary<ResourceType, int> { [ResourceType.IronOre] = 3 },
            ResourceType.CopperOre,
            1,
            15);

        return new Machine(id ?? Guid.NewGuid(), name, 4, requiredCrew, [schematic], schematic);
    }

    private static FactoryWorkforce CreateWorkforce(int workerCount)
    {
        var workforce = new FactoryWorkforce();
        for (var workerNumber = 1; workerNumber <= workerCount; workerNumber++)
        {
            workforce.Hire($"Worker {workerNumber}");
        }

        return workforce;
    }

    private static void AssignWorkers(FactoryWorkforce workforce, Guid machineId, int workerCount)
    {
        foreach (var worker in workforce.Employees.Take(workerCount))
        {
            Assert.True(workforce.TryAssign(worker.Id, machineId.ToString()));
        }
    }
}
