namespace BossMod;

// values of flexible timings of state machine phases and states
// this is used for e.g. cooldown planning
public class StateMachineTimings
{
    public List<float> PhaseDurations = [];

    public StateMachineTimings Clone()
    {
        StateMachineTimings res = new();
        res.PhaseDurations.AddRange(PhaseDurations);
        return res;
    }
}
