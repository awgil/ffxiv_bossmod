namespace BossMod.RealmReborn.Novice.NoviceBasicFinal;

public enum OID : uint
{
    Boss = 0x1591,
    Helper = 0x233C,
}

class N12AttackAsTargeted(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var stateActor in WorldState.Actors)
        {
            if (stateActor.OID == (uint)OID.Boss)
                hints.SetPriority(stateActor, 1);
        }
    }
}

class NoviceBasicFinalStates : StateMachineBuilder
{
    public NoviceBasicFinalStates(BossModule module) : base(module)
    {
        TrivialPhase()
           .ActivateOnEnter<N12AttackAsTargeted>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 166, NameID = 4787)]
public class NoviceBasicFinal(WorldState ws, Actor primary) : BossModule(ws, primary, new(457.383f, 271.721f), new ArenaBoundsCircle(20));
