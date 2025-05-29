namespace BossMod.Dawntrail.Foray.CriticalEngagement.OccultKnight;

public enum OID : uint
{
    Boss = 0x471E,
    Helper = 0x233C,
    Megaloknight = 0x4720,
}

class OccultKnightStates : StateMachineBuilder
{
    public OccultKnightStates(BossModule module) : base(module)
    {
        SimplePhase(0, id => Timeout(id, 9999), "P1")
            .Raw.Update = () => Module.Enemies(OID.Megaloknight).Any(m => m.IsTargetable);
        SimplePhase(1, id => Timeout(id, 9999), "P2")
            .Raw.Update = () => Module.Enemies(OID.Megaloknight).All(m => m.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13728, DevOnly = true)]
public class OccultKnight(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
