namespace IronMagnate.Core.Production;

public sealed class ProductionCoordinator
{
    private readonly Dictionary<Guid, Machine> machines = [];

    public bool RegisterMachine(Machine machine)
    {
        ArgumentNullException.ThrowIfNull(machine);

        return machines.TryAdd(machine.Id, machine);
    }
}
