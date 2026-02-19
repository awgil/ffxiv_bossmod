namespace BossMod.RealmReborn.Novice.NoviceDPS05;

public enum OID : uint
{
    Boss = 0x157A,
    Helper = 0x233C,
}

class AttackReinforcements(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var stateActor in WorldState.Actors)
        {
            if (stateActor.OID != (uint)OID.Boss)
                hints.SetPriority(stateActor, 1);
        }
    }
}

class NoviceDPS05States : StateMachineBuilder
{
    public NoviceDPS05States(BossModule module) : base(module)
    {
        TrivialPhase()
           .ActivateOnEnter<AttackReinforcements>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "erdelf", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 161, NameID = 4784)]
public class NoviceDPS05(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20));
