using System.Collections.ObjectModel;

namespace IronMagnate.Core.Workforce;

public sealed class Workforce
{
    private readonly List<Worker> employees = [];
    private readonly List<Worker> workersEmployedToday = [];
    private readonly Dictionary<Guid, WorkerAssignment> assignmentsByWorker = [];
    private readonly ReadOnlyCollection<Worker> employeesView;
    private readonly ReadOnlyCollection<Worker> workersEmployedTodayView;

    public Workforce()
    {
        employeesView = employees.AsReadOnly();
        workersEmployedTodayView = workersEmployedToday.AsReadOnly();
    }

    public IReadOnlyList<Worker> Employees => employeesView;

    public IReadOnlyList<Worker> IdleWorkers =>
        employees.Where(worker => !assignmentsByWorker.ContainsKey(worker.Id)).ToList();

    public IReadOnlyList<WorkerAssignment> Assignments => assignmentsByWorker.Values.ToList();

    // Includes workers dismissed after being employed, so a future Treasury slice can accrue a full-day wage once.
    public IReadOnlyList<Worker> WorkersEmployedToday => workersEmployedTodayView;

    public Worker Hire(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A worker name is required.", nameof(name));
        }

        var worker = new Worker(Guid.NewGuid(), name);
        employees.Add(worker);
        workersEmployedToday.Add(worker);
        return worker;
    }

    public bool Dismiss(Guid workerId)
    {
        var worker = FindEmployee(workerId);
        if (worker is null)
        {
            return false;
        }

        assignmentsByWorker.Remove(workerId);
        employees.Remove(worker);
        return true;
    }

    public bool TryAssign(Guid workerId, string workstationId)
    {
        ValidateWorkstationId(workstationId);

        if (FindEmployee(workerId) is null || assignmentsByWorker.ContainsKey(workerId))
        {
            return false;
        }

        assignmentsByWorker.Add(workerId, new WorkerAssignment(workerId, workstationId));
        return true;
    }

    public bool TryAllocateCrew(
        string workstationId,
        int requiredCrew,
        out IReadOnlyList<Worker> assignedWorkers)
    {
        ValidateWorkstationId(workstationId);
        if (requiredCrew <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(requiredCrew));
        }

        var existingCrew = employees
            .Where(worker => assignmentsByWorker.GetValueOrDefault(worker.Id)?.WorkstationId == workstationId)
            .ToList();

        if (existingCrew.Count > requiredCrew)
        {
            assignedWorkers = [];
            return false;
        }

        var additionalWorkersNeeded = requiredCrew - existingCrew.Count;
        if (additionalWorkersNeeded == 0)
        {
            assignedWorkers = existingCrew;
            return true;
        }

        var availableWorkers = IdleWorkers;
        if (availableWorkers.Count < additionalWorkersNeeded)
        {
            assignedWorkers = [];
            return false;
        }

        var additionalWorkers = availableWorkers.Take(additionalWorkersNeeded).ToList();
        foreach (var worker in additionalWorkers)
        {
            assignmentsByWorker.Add(worker.Id, new WorkerAssignment(worker.Id, workstationId));
        }

        assignedWorkers = [.. existingCrew, .. additionalWorkers];
        return true;
    }

    public IReadOnlyList<Worker> ReleaseWorkstation(string workstationId)
    {
        ValidateWorkstationId(workstationId);

        var workerIds = assignmentsByWorker.Values
            .Where(assignment => assignment.WorkstationId == workstationId)
            .Select(assignment => assignment.WorkerId)
            .ToList();

        foreach (var workerId in workerIds)
        {
            assignmentsByWorker.Remove(workerId);
        }

        return employees.Where(worker => workerIds.Contains(worker.Id)).ToList();
    }

    public void BeginNewWorkday()
    {
        workersEmployedToday.Clear();
        workersEmployedToday.AddRange(employees);
    }

    private Worker? FindEmployee(Guid workerId) =>
        employees.SingleOrDefault(worker => worker.Id == workerId);

    private static void ValidateWorkstationId(string workstationId)
    {
        if (string.IsNullOrWhiteSpace(workstationId))
        {
            throw new ArgumentException("A workstation identifier is required.", nameof(workstationId));
        }
    }
}
