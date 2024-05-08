namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.TheWinged;

public enum OID : uint
{
    Boss = 0x253D, //R=3.36
    Featherofthewinged = 0x253E, //R=0.5
    BossHelper = 0x233C,
    AltarQueen = 0x254A, // R0.840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    AltarGarlic = 0x2548, // R0.840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    AltarTomato = 0x2549, // R0.840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    AltarOnion = 0x2546, // R0.840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    AltarEgg = 0x2547, // R0.840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    BonusAddAltarMatanga = 0x2545, // R3.420
    BonusAddGoldWhisker = 0x2544, // R0.540
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // BonusAdds->player, no cast, single-target
    Filoplumes = 13376, // Boss->self, 3.0s cast, range 8+R width 4 rect
    Wingbeat = 13377, // Boss->self, 3.0s cast, range 40+R 60-degree cone, knockback 20 away from source
    FeatherSquall = 13378, // Boss->self, 3.0s cast, single-target
    FeatherSquall2 = 13379, // BossHelper->location, 3.0s cast, range 6 circle
    Sideslip = 13380, // Boss->self, 3.5s cast, range 50+R circle
    Pinion = 13381, // Featherofthewinged->self, 3.0s cast, range 40+R width 3 rect

    Pollen = 6452, // 2A0A->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // 2A06->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // 2A09->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // 2A07->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // 2A08->self, 3.5s cast, range 6+R circle
    unknown = 9636, // BonusAddAltarMatanga->self, no cast, single-target
    Spin = 8599, // BonusAddAltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // BonusAddAltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // BonusAddAltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
}

class Filoplumes(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Filoplumes), new AOEShapeRect(11.36f, 2));
class Wingbeat(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Wingbeat), new AOEShapeCone(43.36f, 30.Degrees()));
class WingbeatKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Wingbeat), 20, false, 1, new AOEShapeCone(43.36f, 30.Degrees()), stopAtWall: true);
class FeatherSquall(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.FeatherSquall2), 6);
class Pinion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Pinion), new AOEShapeRect(40.5f, 1.5f));
class Sideslip(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Sideslip));
class PluckAndPrune(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PluckAndPrune), new AOEShapeCircle(6.84f));
class TearyTwirl(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TearyTwirl), new AOEShapeCircle(6.84f));
class HeirloomScream(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HeirloomScream), new AOEShapeCircle(6.84f));
class PungentPirouette(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PungentPirouette), new AOEShapeCircle(6.84f));
class Pollen(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Pollen), new AOEShapeCircle(6.84f));
class RaucousScritch(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RaucousScritch), new AOEShapeCone(8.42f, 30.Degrees()));
class Hurl(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Hurl), 6);
class Spin(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Spin), new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.BonusAddAltarMatanga);

class TheWingedStates : StateMachineBuilder
{
    public TheWingedStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Filoplumes>()
            .ActivateOnEnter<Wingbeat>()
            .ActivateOnEnter<WingbeatKB>()
            .ActivateOnEnter<FeatherSquall>()
            .ActivateOnEnter<Sideslip>()
            .ActivateOnEnter<Pinion>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BonusAddAltarMatanga).All(e => e.IsDead) && module.Enemies(OID.BonusAddGoldWhisker).All(e => e.IsDead) && module.Enemies(OID.AltarEgg).All(e => e.IsDead) && module.Enemies(OID.AltarQueen).All(e => e.IsDead) && module.Enemies(OID.AltarOnion).All(e => e.IsDead) && module.Enemies(OID.AltarGarlic).All(e => e.IsDead) && module.Enemies(OID.AltarTomato).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7595)]
public class TheWinged(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.AltarEgg))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.AltarTomato))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.AltarQueen))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.AltarGarlic))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.AltarOnion))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.BonusAddGoldWhisker))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.BonusAddAltarMatanga))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.AltarOnion => 6,
                OID.AltarEgg => 5,
                OID.AltarGarlic => 4,
                OID.AltarTomato => 3,
                OID.AltarQueen or OID.BonusAddGoldWhisker or OID.BonusAddAltarMatanga => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
