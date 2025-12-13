namespace BossMod.RealmReborn.Dungeon.D08Qarn.D082TempleGuardian;

public enum OID : uint
{
    Boss = 0x477C, // R2.200, x1
    GolemSoulstone = 0x477D, // R2.200, x1, Part type, and more spawn during fight; applies vuln down to boss while alive
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    BoulderClap = 42234, // Boss->self, 2.5s cast, range 12+R 120-degree cone aoe
    TrueGrit = 42235, // Boss->self, 3.0s cast, range 12+R 120-degree cone aoe
    Rockslide = 42236, // Boss->self, 2.5s cast, range 14+R width 8 rect aoe
    StoneSkull = 42237, // Boss->player, no cast, random single-target stun + knockback
    Obliterate = 42238, // Boss->self, 2.0s cast, raidwide
}

public enum SID : uint
{
    Stun = 2, // Boss->player, extra=0x0
    VulnerabilityDown = 350, // None->boss, extra=0x0
}

class BoulderClap(BossModule module) : Components.StandardAOEs(module, AID.BoulderClap, new AOEShapeCone(14.2f, 60.Degrees()));
class TrueGrit(BossModule module) : Components.StandardAOEs(module, AID.TrueGrit, new AOEShapeCone(14.2f, 60.Degrees()));
class Rockslide(BossModule module) : Components.StandardAOEs(module, AID.Rockslide, new AOEShapeRect(16.2f, 4));
class Obliterate(BossModule module) : Components.RaidwideCast(module, AID.Obliterate);

class D082TempleGuardianStates : StateMachineBuilder
{
    public D082TempleGuardianStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BoulderClap>()
            .ActivateOnEnter<TrueGrit>()
            .ActivateOnEnter<Rockslide>()
            .ActivateOnEnter<Obliterate>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 9, NameID = 1569)]
public class D082TempleGuardian(WorldState ws, Actor primary) : BossModule(ws, primary, new(50, -10), new ArenaBoundsCircle(15))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.GolemSoulstone => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
