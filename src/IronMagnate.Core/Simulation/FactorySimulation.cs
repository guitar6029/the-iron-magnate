namespace IronMagnate.Core.Simulation;

public class FactorySimulation
{
    public SimulationClock Clock { get; } = new();
    public int CurrentDay { get; private set; } = 1;

    public void BeginNextDay()
    {
        // check the clock, is the work day finished ?
        var isWorkdayFinished = Clock.IsWorkdayFinished;
        if (!isWorkdayFinished) return;

        Clock.BeginNextDay();

        CurrentDay += 1;
    }

    public double Advance(double elapsedSeconds)
    {
        return Clock.Advance(elapsedSeconds);
    }
}
