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
}
