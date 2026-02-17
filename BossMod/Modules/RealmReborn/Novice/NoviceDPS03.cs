namespace BossMod.RealmReborn.Novice.NoviceDPS03;

public enum OID : uint
{
    Boss = 0x1571,
    Helper = 0x233C,
    N12Gladiator = 0x1574
}

class AttackAsTargeted(BossModule module) : Components.GenericInvincible(module)
{
    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        var gladiator = WorldState.Actors.FirstOrDefault(a => a.OID == (uint)OID.N12Gladiator);
        if (gladiator != null && gladiator.TargetID != 0)
        {
            var target = WorldState.Actors.Find(gladiator.TargetID);

            return WorldState.Actors.Exclude(target);
        }
        return [];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var gladiator = WorldState.Actors.FirstOrDefault(a => a.OID == (uint)OID.N12Gladiator);
        if (gladiator != null && gladiator.TargetID != 0)
        {
            var target = WorldState.Actors.Find(gladiator.TargetID);
            hints.SetPriority(target, 1);
        }
    }
}

class NoviceDPS03States : StateMachineBuilder
{
    public NoviceDPS03States(BossModule module) : base(module)
    {
        TrivialPhase()
           .ActivateOnEnter<AttackAsTargeted>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "erdelf", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 159, NameID = 4784)]
public class NoviceDPS03(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20));
