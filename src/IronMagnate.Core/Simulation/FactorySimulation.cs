using IronMagnate.Core.Energy;

namespace IronMagnate.Core.Simulation;

public class FactorySimulation
{
    public SimulationClock Clock { get; } = new();

    public CoalBoiler Boiler { get; } = new();
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

        double simulatedMinutes = Clock.Advance(elapsedSeconds);
        double poweredInMinutes = Boiler.Advance(simulatedMinutes);

        return simulatedMinutes;

    }
}
