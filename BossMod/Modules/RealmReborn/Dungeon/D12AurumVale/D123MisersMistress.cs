namespace BossMod.RealmReborn.Dungeon.D12AurumVale.D123MisersMistress;

public enum OID : uint
{
    Boss = 0x3AF, // x1
    MorbolFruit = 0x5BC, // spawn during fight
}

public enum AID : uint
{
    AutoAttack = 1350, // Boss->player, no cast, single-target
    VineProbe = 1037, // Boss->self, 1.0s cast, range 6+R width 8 rect cleave (due to short cast time...)
    BadBreath = 1036, // Boss->self, 2.5s cast, range 12+R 120-degree cone aoe
    BurrBurrow = 1038, // Boss->self, 3.0s cast, raidwide?
    HookedBurrs = 1039, // Boss->player, 1.5s cast, single-target
    Sow = 1081, // Boss->player, 3.0s cast, single-target, spawns adds
}

class VineProbe(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.VineProbe), new AOEShapeRect(10, 4));
class BadBreath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BadBreath), new AOEShapeCone(16, 60.Degrees()));

// arena has multiple weirdly-shaped puddles, so just prefer standing in large safe zone
class AIPosition(BossModule module) : BossComponent(module)
{
    private readonly WPos[] _centers = [new(-395, -130), new(-402, -114)];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(_centers.MinBy(p => (p - Module.PrimaryActor.Position).LengthSq()), 5));
    }
}

class D123MisersMistressStates : StateMachineBuilder
{
    public D123MisersMistressStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VineProbe>()
            .ActivateOnEnter<BadBreath>()
            .ActivateOnEnter<AIPosition>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 5, NameID = 1532)]
public class D123MisersMistress(WorldState ws, Actor primary) : BossModule(ws, primary, new(-400, -130), new ArenaBoundsSquare(25))
{
    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.MorbolFruit => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
