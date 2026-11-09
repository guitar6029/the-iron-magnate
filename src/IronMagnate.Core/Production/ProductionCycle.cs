namespace IronMagnate.Core.Production;

public sealed class ProductionCycle
{
    public Schematic Schematic { get; }

    public double ElapsedMinutes { get; private set; }

    public double RequiredMinutes => Schematic.ProcessingMinutes;

    public bool IsComplete => ElapsedMinutes >= RequiredMinutes;

    public double RemainingMinutes =>
        Math.Max(0, RequiredMinutes - ElapsedMinutes);

    public ProductionCycle(
        Schematic schematic
    )
    {
        ArgumentNullException.ThrowIfNull(schematic);
        Schematic = schematic;
    }

    public double Advance(double minutes)
    {
        if (double.IsNaN(minutes) ||
            double.IsInfinity(minutes) ||
            minutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minutes));
        }

        /**
            Update ElapsedMinutes without exceeding the Schematic's required duration.
            Return how many minutes were actually consumed by this workpiece.
        */


        if (ElapsedMinutes >= RequiredMinutes)
        {
            return 0;
        }

        double remaining = RequiredMinutes - ElapsedMinutes;
        double consumed = Math.Min(minutes, remaining);

        ElapsedMinutes += consumed;

        return consumed;

    }
}
