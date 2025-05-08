namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarArachne;

public enum OID : uint
{
    Boss = 0x253B, //R=7.0
    Helper = 0x253C, //R=0.5
    Helper2 = 0x2565, //R=0.5
    BossHelper = 0x233C,
    BonusAddAltarMatanga = 0x2545, // R3.420
    BonusAddGoldWhisker = 0x2544, // R0.540
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/2544->player, no cast, single-target
    AutoAttack2 = 872, // BonusAddAltarMatanga->player, no cast, single-target
    DarkSpike = 13342, // Boss->player, 3.0s cast, single-target
    SilkenSpray = 13455, // Boss->self, 2.5s cast, range 17+R 60-degree cone
    FrondAffeared = 13784, // Boss->self, 3.0s cast, range 60 circle, gaze, applies hysteria
    Implosion = 13343, // Boss->self, 4.0s cast, range 50+R circle
    Earthquake1 = 13346, // 253C/2565->self, 3.5s cast, range 10+R circle
    Earthquake2 = 13345, // 253C/2565->self, 3.5s cast, range 10-20 donut
    Earthquake3 = 13344, // 253C/2565->self, 3.5s cast, range 20-30 donut
    unknown = 9636, // BonusAddAltarMatanga->self, no cast, single-target
    Spin = 8599, // BonusAddAltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // BonusAddAltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // BonusAddAltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
}

class DarkSpike(BossModule module) : Components.SingleTargetDelayableCast(module, AID.DarkSpike);
class FrondAffeared(BossModule module) : Components.CastGaze(module, AID.FrondAffeared);
class SilkenSpray(BossModule module) : Components.StandardAOEs(module, AID.SilkenSpray, new AOEShapeCone(24, 30.Degrees()));
class Implosion(BossModule module) : Components.RaidwideCast(module, AID.Implosion);
class Earthquake1(BossModule module) : Components.StandardAOEs(module, AID.Earthquake1, new AOEShapeCircle(10.5f));
class Earthquake2(BossModule module) : Components.StandardAOEs(module, AID.Earthquake2, new AOEShapeDonut(10, 20));
class Earthquake3(BossModule module) : Components.StandardAOEs(module, AID.Earthquake3, new AOEShapeDonut(20, 30));
class RaucousScritch(BossModule module) : Components.StandardAOEs(module, AID.RaucousScritch, new AOEShapeCone(8.42f, 30.Degrees()));
class Hurl(BossModule module) : Components.StandardAOEs(module, AID.Hurl, 6);
class Spin(BossModule module) : Components.Cleave(module, AID.Spin, new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.BonusAddAltarMatanga);

class ArachneStates : StateMachineBuilder
{
    public ArachneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DarkSpike>()
            .ActivateOnEnter<FrondAffeared>()
            .ActivateOnEnter<SilkenSpray>()
            .ActivateOnEnter<Implosion>()
            .ActivateOnEnter<Earthquake1>()
            .ActivateOnEnter<Earthquake2>()
            .ActivateOnEnter<Earthquake3>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BonusAddGoldWhisker).All(e => e.IsDead) && module.Enemies(OID.BonusAddAltarMatanga).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7623)]
public class Arachne(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BonusAddGoldWhisker))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.BonusAddAltarMatanga))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.BonusAddGoldWhisker => 3,
                OID.BonusAddAltarMatanga => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
