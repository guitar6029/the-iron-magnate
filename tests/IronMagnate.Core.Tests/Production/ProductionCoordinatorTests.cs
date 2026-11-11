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

    [Fact]
    public void FillCrewAssignsOnlyTheTwoVacanciesWhenThreeOfFiveWorkersAreAlreadyAssigned()
    {
        var coordinator = new ProductionCoordinator();
        var machine = RegisterEnabledMachine(coordinator, requiredCrew: 5);
        var workforce = CreateWorkforce(7);
        AssignWorkers(workforce, machine.Id, 3);

        var assigned = coordinator.FillCrew(machine.Id, workforce);

        Assert.Equal(2, assigned);
        Assert.Equal(5, AssignedCrewCount(workforce, machine.Id));
        Assert.Equal(2, workforce.IdleWorkers.Count);
    }

    [Fact]
    public void FillCrewPartiallyStaffsAMachineWhenFewerWorkersAreAvailableThanRequired()
    {
        var coordinator = new ProductionCoordinator();
        var machine = RegisterEnabledMachine(coordinator, requiredCrew: 5);
        var workforce = CreateWorkforce(2);

        var assigned = coordinator.FillCrew(machine.Id, workforce);

        Assert.Equal(2, assigned);
        Assert.Equal(2, AssignedCrewCount(workforce, machine.Id));
    }

    [Fact]
    public void FillCrewAssignsNoWorkersWhenTheMachineAlreadyHasACompleteCrew()
    {
        var coordinator = new ProductionCoordinator();
        var machine = RegisterEnabledMachine(coordinator, requiredCrew: 5);
        var workforce = CreateWorkforce(5);
        AssignWorkers(workforce, machine.Id, 5);

        Assert.Equal(0, coordinator.FillCrew(machine.Id, workforce));
        Assert.Equal(5, AssignedCrewCount(workforce, machine.Id));
    }

    [Fact]
    public void RepeatedFillCrewCallsNeverExceedTheRequiredCrewCapacity()
    {
        var coordinator = new ProductionCoordinator();
        var machine = RegisterEnabledMachine(coordinator, requiredCrew: 5);
        var workforce = CreateWorkforce(10);

        Assert.Equal(5, coordinator.FillCrew(machine.Id, workforce));
        Assert.Equal(0, coordinator.FillCrew(machine.Id, workforce));
        Assert.Equal(5, AssignedCrewCount(workforce, machine.Id));
    }

    [Fact]
    public void FillCrewAssignsExactlyFiveOfTenAvailableWorkersToAMachineRequiringFive()
    {
        var coordinator = new ProductionCoordinator();
        var machine = RegisterEnabledMachine(coordinator, requiredCrew: 5);
        var workforce = CreateWorkforce(10);

        Assert.Equal(5, coordinator.FillCrew(machine.Id, workforce));
        Assert.Equal(5, AssignedCrewCount(workforce, machine.Id));
        Assert.Equal(5, workforce.IdleWorkers.Count);
        Assert.All(
            workforce.Employees.Take(5),
            worker => Assert.Contains(
                workforce.Assignments,
                assignment => assignment.WorkerId == worker.Id && assignment.WorkstationId == machine.Id.ToString()));
        Assert.All(
            workforce.Employees.Skip(5),
            worker => Assert.DoesNotContain(
                workforce.Assignments,
                assignment => assignment.WorkerId == worker.Id));
    }

    [Fact]
    public void FillCrewAssignsNoWorkersForAnUnknownMachine()
    {
        var coordinator = new ProductionCoordinator();
        var workforce = CreateWorkforce(2);

        Assert.Equal(0, coordinator.FillCrew(Guid.NewGuid(), workforce));
        Assert.Empty(workforce.Assignments);
    }

    [Fact]
    public void FillCrewDoesNotReleaseExistingAssignmentsForADisabledMachine()
    {
        var coordinator = new ProductionCoordinator();
        var machine = CreateMachine(requiredCrew: 5);
        var workforce = CreateWorkforce(5);
        AssignWorkers(workforce, machine.Id, 3);
        var assignmentsBeforeFillAttempt = workforce.Assignments;
        coordinator.RegisterMachine(machine);

        Assert.Equal(0, coordinator.FillCrew(machine.Id, workforce));
        Assert.Equal(assignmentsBeforeFillAttempt, workforce.Assignments);
    }

    [Fact]
    public void FillCrewAssignsNoWorkersWhenNoWorkersAreAvailable()
    {
        var coordinator = new ProductionCoordinator();
        var machine = RegisterEnabledMachine(coordinator, requiredCrew: 5);
        var workforce = CreateWorkforce(2);
        AssignWorkers(workforce, Guid.NewGuid(), 2);

        Assert.Equal(0, coordinator.FillCrew(machine.Id, workforce));
        Assert.Equal(0, AssignedCrewCount(workforce, machine.Id));
    }

    [Fact]
    public void FillCrewDoesNotStealWorkersAssignedToAnotherMachine()
    {
        var coordinator = new ProductionCoordinator();
        var machine = RegisterEnabledMachine(coordinator, requiredCrew: 5);
        var otherMachineId = Guid.NewGuid();
        var workforce = CreateWorkforce(6);
        Assert.True(workforce.TryAssign(workforce.Employees[0].Id, otherMachineId.ToString()));

        Assert.Equal(5, coordinator.FillCrew(machine.Id, workforce));
        Assert.Contains(
            workforce.Assignments,
            assignment => assignment.WorkerId == workforce.Employees[0].Id
                && assignment.WorkstationId == otherMachineId.ToString());
        Assert.Equal(5, AssignedCrewCount(workforce, machine.Id));
    }

    [Fact]
    public void FillCrewRejectsANullWorkforce()
    {
        var coordinator = new ProductionCoordinator();

        Assert.Throws<ArgumentNullException>(() => coordinator.FillCrew(Guid.NewGuid(), null!));
    }

    [Fact]
    public void FillCrewLeavesExistingAssignmentsUnchangedWhenItCannotAssignWorkers()
    {
        var coordinator = new ProductionCoordinator();
        var machine = RegisterEnabledMachine(coordinator, requiredCrew: 5);
        var otherMachineId = Guid.NewGuid();
        var workforce = CreateWorkforce(3);
        AssignWorkers(workforce, machine.Id, 2);
        Assert.True(workforce.TryAssign(workforce.Employees[2].Id, otherMachineId.ToString()));
        var assignmentsBeforeFillAttempt = workforce.Assignments;

        Assert.Equal(0, coordinator.FillCrew(machine.Id, workforce));
        Assert.Equal(assignmentsBeforeFillAttempt, workforce.Assignments);
    }

    [Fact]
    public void TryAssignWorkerCapsOneHundredSequentialAttemptsAtFiveSuccessfulAssignments()
    {
        var coordinator = new ProductionCoordinator();
        var machine = RegisterEnabledMachine(coordinator, requiredCrew: 5);
        var workforce = CreateWorkforce(20);

        var successfulAssignments = Enumerable.Range(0, 100)
            .Count(attempt => coordinator.TryAssignWorker(
                machine.Id,
                workforce.Employees[attempt % workforce.Employees.Count].Id,
                workforce));

        Assert.Equal(5, successfulAssignments);
        Assert.Equal(5, AssignedCrewCount(workforce, machine.Id));
    }

    [Fact]
    public void TryAssignWorkerOnlyAssignsTheSameWorkerOnce()
    {
        var coordinator = new ProductionCoordinator();
        var machine = RegisterEnabledMachine(coordinator, requiredCrew: 5);
        var workforce = CreateWorkforce(1);
        var worker = workforce.Employees.Single();

        var successfulAssignments = Enumerable.Range(0, 10)
            .Count(_ => coordinator.TryAssignWorker(machine.Id, worker.Id, workforce));

        Assert.Equal(1, successfulAssignments);
        Assert.Equal(1, AssignedCrewCount(workforce, machine.Id));
    }

    [Fact]
    public void TryAssignWorkerRejectsAdditionalWorkersWhenTheMachineIsFullWithoutChangingAssignments()
    {
        var coordinator = new ProductionCoordinator();
        var machine = RegisterEnabledMachine(coordinator, requiredCrew: 5);
        var workforce = CreateWorkforce(8);
        foreach (var worker in workforce.Employees.Take(5))
        {
            Assert.True(coordinator.TryAssignWorker(machine.Id, worker.Id, workforce));
        }

        var assignmentsBeforeRejectedRequests = workforce.Assignments;

        Assert.All(
            workforce.Employees.Skip(5),
            worker => Assert.False(coordinator.TryAssignWorker(machine.Id, worker.Id, workforce)));
        Assert.Equal(assignmentsBeforeRejectedRequests, workforce.Assignments);
    }

    [Fact]
    public void TryAssignWorkerFillsOnlyTheRemainingTwoVacanciesOfAPartiallyStaffedMachine()
    {
        var coordinator = new ProductionCoordinator();
        var machine = RegisterEnabledMachine(coordinator, requiredCrew: 5);
        var workforce = CreateWorkforce(7);
        AssignWorkers(workforce, machine.Id, 3);

        var successfulAssignments = workforce.Employees.Skip(3)
            .Count(worker => coordinator.TryAssignWorker(machine.Id, worker.Id, workforce));

        Assert.Equal(2, successfulAssignments);
        Assert.Equal(5, AssignedCrewCount(workforce, machine.Id));
    }

    [Fact]
    public void OneHundredFillCrewCallsNeverExceedTheRequiredCrewCapacity()
    {
        var coordinator = new ProductionCoordinator();
        var machine = RegisterEnabledMachine(coordinator, requiredCrew: 5);
        var workforce = CreateWorkforce(20);

        var successfulAssignments = Enumerable.Range(0, 100)
            .Sum(_ => coordinator.FillCrew(machine.Id, workforce));

        Assert.Equal(5, successfulAssignments);
        Assert.Equal(5, AssignedCrewCount(workforce, machine.Id));
    }

    [Fact]
    public void MixedManualAndAutomaticAssignmentPathsNeverExceedTheRequiredCrew()
    {
        var coordinator = new ProductionCoordinator();
        var machine = RegisterEnabledMachine(coordinator, requiredCrew: 5);
        var workforce = CreateWorkforce(20);

        for (var attempt = 0; attempt < 100; attempt++)
        {
            if (attempt % 2 == 0)
            {
                coordinator.TryAssignWorker(
                    machine.Id,
                    workforce.Employees[attempt % workforce.Employees.Count].Id,
                    workforce);
            }
            else
            {
                coordinator.FillCrew(machine.Id, workforce);
            }
        }

        Assert.Equal(5, AssignedCrewCount(workforce, machine.Id));
    }

    [Fact]
    public void AWorkerAssignedToOneMachineCannotBeAssignedToAnotherMachine()
    {
        var coordinator = new ProductionCoordinator();
        var firstMachine = RegisterEnabledMachine(coordinator, requiredCrew: 5);
        var secondMachine = RegisterEnabledMachine(coordinator, requiredCrew: 5);
        var workforce = CreateWorkforce(1);
        var worker = workforce.Employees.Single();

        Assert.True(coordinator.TryAssignWorker(firstMachine.Id, worker.Id, workforce));
        Assert.False(coordinator.TryAssignWorker(secondMachine.Id, worker.Id, workforce));
        Assert.Equal(1, AssignedCrewCount(workforce, firstMachine.Id));
        Assert.Equal(0, AssignedCrewCount(workforce, secondMachine.Id));
    }

    [Fact]
    public void TryAssignWorkerAllowsManualAssignmentToADisabledMachineButFillCrewDoesNotAddWorkers()
    {
        var coordinator = new ProductionCoordinator();
        var machine = CreateMachine(requiredCrew: 5);
        var workforce = CreateWorkforce(2);
        coordinator.RegisterMachine(machine);

        Assert.True(coordinator.TryAssignWorker(machine.Id, workforce.Employees[0].Id, workforce));
        Assert.Equal(0, coordinator.FillCrew(machine.Id, workforce));
        Assert.Equal(1, AssignedCrewCount(workforce, machine.Id));
        Assert.Single(workforce.IdleWorkers);
    }

    [Fact]
    public void TryAssignWorkerRejectsAnUnknownMachineWithoutModifyingWorkforceState()
    {
        var coordinator = new ProductionCoordinator();
        var workforce = CreateWorkforce(1);
        var assignmentsBeforeRejectedRequest = workforce.Assignments;

        Assert.False(coordinator.TryAssignWorker(Guid.NewGuid(), workforce.Employees.Single().Id, workforce));
        Assert.Equal(assignmentsBeforeRejectedRequest, workforce.Assignments);
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

    private static Machine RegisterEnabledMachine(ProductionCoordinator coordinator, int requiredCrew)
    {
        var machine = CreateMachine(requiredCrew: requiredCrew);
        machine.Enable();
        Assert.True(coordinator.RegisterMachine(machine));
        return machine;
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

    private static int AssignedCrewCount(FactoryWorkforce workforce, Guid machineId) =>
        workforce.Assignments.Count(assignment => assignment.WorkstationId == machineId.ToString());
}
