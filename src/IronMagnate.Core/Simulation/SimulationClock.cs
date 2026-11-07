namespace IronMagnate.Core.Simulation;

public class SimulationClock
{
    public SimulationSpeed Speed { get; private set; } = SimulationSpeed.Paused;

    public double CurrentMinute { get; private set; } = WorkdayStartMinute;

    private const double WorkdayStartMinute = 480;
    private const double WorkdayEndMinute = 960;

    public bool IsWorkdayFinished => CurrentMinute >= WorkdayEndMinute;


    public void SetSpeed(SimulationSpeed speed)
    {
        if (!Enum.IsDefined(speed))
        {
            throw new ArgumentOutOfRangeException(nameof(speed));
        }

        if (IsWorkdayFinished) return;

        Speed = speed;
    }

    public double Advance(double elapsedSeconds)
    {
        if (elapsedSeconds < 0 || double.IsNaN(elapsedSeconds) || double.IsInfinity(elapsedSeconds))
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        }

        var previousMinute = CurrentMinute;

        if (Speed == SimulationSpeed.Paused)
        {
            return 0;
        }

        int multiplier = (int)Speed;
        CurrentMinute = Math.Min(
            CurrentMinute + elapsedSeconds * multiplier,
            WorkdayEndMinute
        );

        if (IsWorkdayFinished)
        {
            Speed = SimulationSpeed.Paused;
        }

        return CurrentMinute - previousMinute;

    }

    public void BeginNextDay()
    {
        if (!IsWorkdayFinished) return;

        CurrentMinute = WorkdayStartMinute;
        Speed = SimulationSpeed.Normal;
    }
}
