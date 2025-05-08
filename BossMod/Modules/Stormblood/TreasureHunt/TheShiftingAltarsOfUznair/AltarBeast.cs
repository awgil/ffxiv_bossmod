namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarBeast;

public enum OID : uint
{
    Boss = 0x2536, //R=4.6
    BossAdd = 0x2567, //R=1.25
    BossHelper = 0x233C,
    AltarQueen = 0x254A, // R0.840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    AltarGarlic = 0x2548, // R0.840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    AltarTomato = 0x2549, // R0.840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    AltarOnion = 0x2546, // R0.840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    AltarEgg = 0x2547, // R0.840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    BonusAddAltarMatanga = 0x2545, // R3.420
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // AltarQueen->player, no cast, single-target
    AutoAttack3 = 6499, // 2567->player, no cast, single-target
    RustingClaw = 13259, // Boss->self, 3.0s cast, range 8+R 120-degree cone
    WordsOfWoe = 13260, // Boss->self, 5.0s cast, range 45+R width 6 rect
    EyeOfTheFire = 13263, // Boss->self, 5.0s cast, range 50+R circle, gaze, applies hysteria
    TheSpin = 13262, // Boss->self, 4.0s cast, range 50+R circle, damage fall off with distance, estimated safety distance between 10 and 15
    VengefulSoul = 13740, // 2567->location, 3.0s cast, range 6 circle
    TailDrive = 13261, // Boss->self, 3.0s cast, range 30+R 120-degree cone

    Pollen = 6452, // 2A0A->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // 2A06->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // 2A09->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // 2A07->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // 2A08->self, 3.5s cast, range 6+R circle
    Telega = 9630, // BonusAdds->self, no cast, single-target, bonus adds disappear
    unknown = 9636, // BonusAddAltarMatanga->self, no cast, single-target
    Spin = 8599, // BonusAddAltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // BonusAddAltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // BonusAddAltarMatanga->location, 3.0s cast, range 6 circle
}

class RustingClaw(BossModule module) : Components.StandardAOEs(module, AID.RustingClaw, new AOEShapeCone(12.6f, 60.Degrees()));
class TailDrive(BossModule module) : Components.StandardAOEs(module, AID.TailDrive, new AOEShapeCone(34.6f, 60.Degrees()));
class TheSpin(BossModule module) : Components.StandardAOEs(module, AID.TheSpin, new AOEShapeCircle(15));
class WordsOfWoe(BossModule module) : Components.StandardAOEs(module, AID.WordsOfWoe, new AOEShapeRect(49.6f, 3));
class VengefulSoul(BossModule module) : Components.StandardAOEs(module, AID.VengefulSoul, 6);
class EyeOfTheFire(BossModule module) : Components.CastGaze(module, AID.EyeOfTheFire);
class PluckAndPrune(BossModule module) : Components.StandardAOEs(module, AID.PluckAndPrune, new AOEShapeCircle(6.84f));
class TearyTwirl(BossModule module) : Components.StandardAOEs(module, AID.TearyTwirl, new AOEShapeCircle(6.84f));
class HeirloomScream(BossModule module) : Components.StandardAOEs(module, AID.HeirloomScream, new AOEShapeCircle(6.84f));
class PungentPirouette(BossModule module) : Components.StandardAOEs(module, AID.PungentPirouette, new AOEShapeCircle(6.84f));
class Pollen(BossModule module) : Components.StandardAOEs(module, AID.Pollen, new AOEShapeCircle(6.84f));
class RaucousScritch(BossModule module) : Components.StandardAOEs(module, AID.RaucousScritch, new AOEShapeCone(8.42f, 30.Degrees()));
class Hurl(BossModule module) : Components.StandardAOEs(module, AID.Hurl, 6);
class Spin(BossModule module) : Components.Cleave(module, AID.Spin, new AOEShapeCone(9.42f, 60.Degrees()), (uint)OID.BonusAddAltarMatanga);

class BeastStates : StateMachineBuilder
{
    public BeastStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RustingClaw>()
            .ActivateOnEnter<TailDrive>()
            .ActivateOnEnter<TheSpin>()
            .ActivateOnEnter<WordsOfWoe>()
            .ActivateOnEnter<VengefulSoul>()
            .ActivateOnEnter<EyeOfTheFire>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.AltarEgg).All(e => e.IsDead) && module.Enemies(OID.AltarQueen).All(e => e.IsDead) && module.Enemies(OID.AltarOnion).All(e => e.IsDead) && module.Enemies(OID.AltarGarlic).All(e => e.IsDead) && module.Enemies(OID.AltarTomato).All(e => e.IsDead) && module.Enemies(OID.BonusAddAltarMatanga).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7588)]
public class Beast(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossAdd))
            Arena.Actor(s, ArenaColor.Object);
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
        foreach (var s in Enemies(OID.BonusAddAltarMatanga))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.AltarOnion => 7,
                OID.AltarEgg => 6,
                OID.AltarGarlic => 5,
                OID.AltarTomato => 4,
                OID.AltarQueen or OID.BonusAddAltarMatanga => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
