namespace IronMagnate.Core.Simulation;

public class SimulationClock
{
    public SimulationSpeed Speed { get; private set; } = SimulationSpeed.Paused;

    public double CurrentMinute { get; private set; } = 480;
    private const double MaxMinuteAllowed = 960;

    public bool IsWorkdayFinished => CurrentMinute >= MaxMinuteAllowed;


    public void SetSpeed(SimulationSpeed speed)
    {
        // if workday is finished, cannot change the speed
        if (IsWorkdayFinished) return;
        Speed = speed;
    }

    public void Advance(double elapsedSeconds)
    {

        // check for negative values
        if (elapsedSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        }

        int multiplier = (int)Speed;
        CurrentMinute = Math.Min(
            CurrentMinute + elapsedSeconds * multiplier,
            MaxMinuteAllowed
        );

        if (IsWorkdayFinished)
        {
            Speed = SimulationSpeed.Paused;
        }

    }

    public void BeginNextDay()
    {
        //first check if workday has finished
        // if not reject it , do nothing
        if (!IsWorkdayFinished) return;

        //reset the time and start again!
        CurrentMinute = 480;
        Speed = SimulationSpeed.Normal;
    }
}
