using IronMagnate.Core.Workforce;
using FactoryWorkforce = IronMagnate.Core.Workforce.Workforce;
using Xunit;

namespace IronMagnate.Core.Tests.Workforce;

public class WorkforceTests
{
    [Fact]
    public void HiringAddsAnIdleFactoryHandWithAUniqueIdentity()
    {
        var workforce = new FactoryWorkforce();

        var ada = workforce.Hire("Ada");
        var bea = workforce.Hire("Bea");

        Assert.NotEqual(ada.Id, bea.Id);
        Assert.Equal([ada, bea], workforce.Employees);
        Assert.Equal([ada, bea], workforce.IdleWorkers);
    }

    [Fact]
    public void DismissalReleasesAnAssignedWorkerAndKeepsTheirDayOfEmploymentForPayroll()
    {
        var workforce = new FactoryWorkforce();
        var ada = workforce.Hire("Ada");
        workforce.TryAssign(ada.Id, "machine-a");

        var dismissed = workforce.Dismiss(ada.Id);

        Assert.True(dismissed);
        Assert.Empty(workforce.Employees);
        Assert.Empty(workforce.Assignments);
        Assert.Equal([ada], workforce.WorkersEmployedToday);
    }

    [Fact]
    public void AutomaticAllocationAssignsACompleteCrewInHiringOrder()
    {
        var workforce = WorkforceWith("Ada", "Bea", "Cleo");

        var allocated = workforce.TryAllocateCrew("machine-a", 2, out var crew);

        Assert.True(allocated);
        Assert.Equal(["Ada", "Bea"], crew.Select(worker => worker.Name));
        Assert.Equal(["Cleo"], workforce.IdleWorkers.Select(worker => worker.Name));
    }

    [Fact]
    public void AutomaticAllocationDoesNotPartiallyAssignAnInsufficientCrew()
    {
        var workforce = WorkforceWith("Ada");

        var allocated = workforce.TryAllocateCrew("machine-a", 2, out var crew);

        Assert.False(allocated);
        Assert.Empty(crew);
        Assert.Single(workforce.IdleWorkers);
        Assert.Empty(workforce.Assignments);
    }

    [Fact]
    public void AutomaticAllocationCompletesAnExistingPartialManualCrew()
    {
        var workforce = WorkforceWith("Ada", "Bea", "Cleo");
        var ada = workforce.Employees.Single(worker => worker.Name == "Ada");
        workforce.TryAssign(ada.Id, "machine-a");

        var allocated = workforce.TryAllocateCrew("machine-a", 3, out var crew);

        Assert.True(allocated);
        Assert.Equal(["Ada", "Bea", "Cleo"], crew.Select(worker => worker.Name));
        Assert.Equal(3, workforce.Assignments.Count);
    }

    [Fact]
    public void AutomaticAllocationDoesNotChangeAPartialCrewWhenAdditionalWorkersAreInsufficient()
    {
        var workforce = WorkforceWith("Ada", "Bea");
        var ada = workforce.Employees.Single(worker => worker.Name == "Ada");
        workforce.TryAssign(ada.Id, "machine-a");

        var allocated = workforce.TryAllocateCrew("machine-a", 3, out var crew);

        Assert.False(allocated);
        Assert.Empty(crew);
        Assert.Equal(new WorkerAssignment(ada.Id, "machine-a"), workforce.Assignments.Single());
        Assert.Equal(["Bea"], workforce.IdleWorkers.Select(worker => worker.Name));
    }

    [Fact]
    public void AutomaticAllocationSucceedsWithoutChangingAnAlreadyCompleteCrew()
    {
        var workforce = WorkforceWith("Ada", "Bea");
        var ada = workforce.Employees.Single(worker => worker.Name == "Ada");
        var bea = workforce.Employees.Single(worker => worker.Name == "Bea");
        workforce.TryAssign(ada.Id, "machine-a");
        workforce.TryAssign(bea.Id, "machine-a");

        var allocated = workforce.TryAllocateCrew("machine-a", 2, out var crew);

        Assert.True(allocated);
        Assert.Equal(["Ada", "Bea"], crew.Select(worker => worker.Name));
        Assert.Equal(2, workforce.Assignments.Count);
    }

    [Fact]
    public void AutomaticAllocationRejectsAnOverstaffedWorkstationWithoutChangingIt()
    {
        var workforce = WorkforceWith("Ada", "Bea", "Cleo");
        foreach (var worker in workforce.Employees)
        {
            workforce.TryAssign(worker.Id, "machine-a");
        }

        var allocated = workforce.TryAllocateCrew("machine-a", 2, out var crew);

        Assert.False(allocated);
        Assert.Empty(crew);
        Assert.Equal(3, workforce.Assignments.Count);
    }

    [Fact]
    public void AWorkerCannotBeAssignedToMultipleWorkstations()
    {
        var workforce = WorkforceWith("Ada");
        var ada = workforce.Employees.Single();

        Assert.True(workforce.TryAssign(ada.Id, "machine-a"));
        Assert.False(workforce.TryAssign(ada.Id, "machine-b"));
        Assert.Equal("machine-a", workforce.Assignments.Single().WorkstationId);
    }

    [Fact]
    public void ManualAssignmentOnlyAcceptsAnAvailableEmployee()
    {
        var workforce = WorkforceWith("Ada");
        var ada = workforce.Employees.Single();

        Assert.False(workforce.TryAssign(Guid.NewGuid(), "machine-a"));
        Assert.True(workforce.TryAssign(ada.Id, "machine-a"));
        Assert.Empty(workforce.IdleWorkers);
        Assert.Equal(new WorkerAssignment(ada.Id, "machine-a"), workforce.Assignments.Single());
    }

    [Fact]
    public void ReleasingAWorkstationReturnsItsCrewToTheIdlePool()
    {
        var workforce = WorkforceWith("Ada", "Bea");
        workforce.TryAllocateCrew("machine-a", 2, out _);

        var released = workforce.ReleaseWorkstation("machine-a");

        Assert.Equal(["Ada", "Bea"], released.Select(worker => worker.Name));
        Assert.Equal(["Ada", "Bea"], workforce.IdleWorkers.Select(worker => worker.Name));
        Assert.Empty(workforce.Assignments);
    }

    [Fact]
    public void NewWorkdayCarriesOnlyCurrentEmployeesIntoPayrollEligibility()
    {
        var workforce = WorkforceWith("Ada", "Bea");
        var ada = workforce.Employees.Single(worker => worker.Name == "Ada");
        workforce.Dismiss(ada.Id);

        workforce.BeginNewWorkday();

        Assert.Equal(["Bea"], workforce.WorkersEmployedToday.Select(worker => worker.Name));
    }

    [Fact]
    public void EmployeeCollectionsCannotBeCastBackToMutableLists()
    {
        var workforce = WorkforceWith("Ada");

        Assert.Null(workforce.Employees as List<Worker>);
        Assert.Null(workforce.WorkersEmployedToday as List<Worker>);
        Assert.Throws<NotSupportedException>(() => ((IList<Worker>)workforce.Employees).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<Worker>)workforce.WorkersEmployedToday).Clear());
        Assert.Single(workforce.Employees);
        Assert.Single(workforce.WorkersEmployedToday);
    }

    private static FactoryWorkforce WorkforceWith(params string[] names)
    {
        var workforce = new FactoryWorkforce();
        foreach (var name in names)
        {
            workforce.Hire(name);
        }

        return workforce;
    }
}
