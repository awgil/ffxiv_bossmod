namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.TheOlderOne;

public enum OID : uint
{
    Boss = 0x253F, //R=5.06
    BossAdd = 0x2572, //R=1.72, untargetable
    BossAdd2 = 0x2540, //R=1.72, untargetable
    BossAdd3 = 0x2573, //R=1.72, untargetable
    BossAdd4 = 0x2574, //R=1.72, untargetable
    BossHelper = 0x233C,
    BonusAddGoldWhisker = 0x2544, // R0.540
    AltarQueen = 0x254A, // R0.840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    AltarGarlic = 0x2548, // R0.840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    AltarTomato = 0x2549, // R0.840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    AltarOnion = 0x2546, // R0.840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    AltarEgg = 0x2547, // R0.840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    BonusAddAltarMatanga = 0x2545, // R3.420
}

public enum AID : uint
{
    AutoAttack = 870, // BonusAddGoldWhisker->player, no cast, single-target
    AutoAttack2 = 872, // BonusAddAltarMatanga,Mandragoras->player, no cast, single-target
    AutoAttack3 = 13478, // Boss->player, no cast, single-target
    MysticFlash = 13385, // Boss->player, 3.0s cast, single-target
    MysticLight = 13386, // Boss->self, 3.0s cast, range 40+R 60-degree cone
    MysticFlame = 13387, // Boss->self, 3.0s cast, single-target
    MysticFlame2 = 13388, // BossHelper->location, 3.0s cast, range 7 circle
    MysticLevin = 13389, // Boss->self, 3.0s cast, range 30+R circle
    MysticHeat = 13390, // BossAdds->self, 3.0s cast, range 40+R width 3 rect
    SelfDetonate = 13391, // BossAdds->self, 4.0s cast, range 9+R circle

    PluckAndPrune = 6449, // AltarEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // AltarGarlic->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // AltarOnion->self, 3.5s cast, range 6+R circle
    Pollen = 6452, // AltarQueen->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // AltarTomato->self, 3.5s cast, range 6+R circle
    unknown = 9636, // BonusAddAltarMatanga->self, no cast, single-target
    Spin = 8599, // BonusAddAltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // BonusAddAltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // BonusAddAltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630, // bonusadds->self, no cast, single-target, bonus add disappear
}

class MysticLight(BossModule module) : Components.StandardAOEs(module, AID.MysticLight, new AOEShapeCone(45.06f, 30.Degrees()));
class MysticFlame(BossModule module) : Components.StandardAOEs(module, AID.MysticFlame2, 7);
class MysticHeat(BossModule module) : Components.StandardAOEs(module, AID.MysticHeat, new AOEShapeRect(41.72f, 1.5f));
class SelfDetonate(BossModule module) : Components.StandardAOEs(module, AID.SelfDetonate, new AOEShapeCircle(10.72f));
class MysticLevin(BossModule module) : Components.RaidwideCast(module, AID.MysticLevin);
class MysticFlash(BossModule module) : Components.SingleTargetDelayableCast(module, AID.MysticFlash);
class PluckAndPrune(BossModule module) : Components.StandardAOEs(module, AID.PluckAndPrune, new AOEShapeCircle(6.84f));
class TearyTwirl(BossModule module) : Components.StandardAOEs(module, AID.TearyTwirl, new AOEShapeCircle(6.84f));
class HeirloomScream(BossModule module) : Components.StandardAOEs(module, AID.HeirloomScream, new AOEShapeCircle(6.84f));
class PungentPirouette(BossModule module) : Components.StandardAOEs(module, AID.PungentPirouette, new AOEShapeCircle(6.84f));
class Pollen(BossModule module) : Components.StandardAOEs(module, AID.Pollen, new AOEShapeCircle(6.84f));
class RaucousScritch(BossModule module) : Components.StandardAOEs(module, AID.RaucousScritch, new AOEShapeCone(8.42f, 30.Degrees()));
class Hurl(BossModule module) : Components.StandardAOEs(module, AID.Hurl, 6);
class Spin(BossModule module) : Components.Cleave(module, AID.Spin, new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.BonusAddAltarMatanga);

class TheOlderOneStates : StateMachineBuilder
{
    public TheOlderOneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MysticLight>()
            .ActivateOnEnter<MysticFlame>()
            .ActivateOnEnter<MysticHeat>()
            .ActivateOnEnter<MysticLevin>()
            .ActivateOnEnter<MysticFlash>()
            .ActivateOnEnter<SelfDetonate>()
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

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7597)]
public class TheOlderOne(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
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

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
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
