namespace IronMagnate.Core.Production;

using IronMagnate.Core.Workforce;

public sealed class ProductionCoordinator
{
    private readonly Dictionary<Guid, Machine> machines = [];

    public bool RegisterMachine(Machine machine)
    {
        ArgumentNullException.ThrowIfNull(machine);

        return machines.TryAdd(machine.Id, machine);
    }

    public Machine? GetMachine(Guid machineId)
    {
        return machines.GetValueOrDefault(machineId);
    }

    public bool IsFullyStaffed(Guid machineId, Workforce workforce)
    {
        ArgumentNullException.ThrowIfNull(workforce);

        var machine = GetMachine(machineId);

        if (machine is null)
        {
            return false;
        }

        var assignedCount = workforce.Assignments.Count(
            assigment => assigment.WorkstationId == machineId.ToString()
        );

        return assignedCount == machine.RequiredCrew;
    }

    public int FillCrew(Guid machineId, Workforce workforce)
    {
        /**
        Find the register machine
        Check whether its enabled
        calculate its vacant positions
        select available, unassigned workers in a consistent order
        assign only as many as needed, never exceeding
        */

        ArgumentNullException.ThrowIfNull(workforce);

        Machine? machine = GetMachine(machineId);

        if (machine is null || !machine.IsEnabled)
        {
            return 0;
        }


        int assignedCount = workforce.Assignments.Count(
    assignment => assignment.WorkstationId == machineId.ToString()
);

        int vacancies = machine.RequiredCrew - assignedCount;

        int workersToAssign = Math.Min(
            vacancies,
            workforce.IdleWorkers.Count
        );

        if (workersToAssign <= 0)
        {
            return 0;
        }

        var selectedWorkers = workforce.IdleWorkers
            .Take(workersToAssign)
            .ToList();

        int assigned = 0;

        foreach (var worker in selectedWorkers)
        {
            if (workforce.TryAssign(worker.Id, machineId.ToString()))
            {
                assigned++;
            }
        }

        return assigned;



    }

    public bool TryAssignWorker(
        Guid machineId,
        Guid workerId,
        Workforce workforce
    )
    {
        ArgumentNullException.ThrowIfNull(workforce);
        Machine? machine = GetMachine(machineId);

        if (machine is null)
        {
            return false;
        }

        string workstationId = machine.Id.ToString();

        int assignedCount = workforce.Assignments.Count(
            assignment => assignment.WorkstationId == workstationId
        );

        if (assignedCount >= machine.RequiredCrew)
        {
            return false;
        }

        return workforce.TryAssign(workerId, workstationId);
    }
}
