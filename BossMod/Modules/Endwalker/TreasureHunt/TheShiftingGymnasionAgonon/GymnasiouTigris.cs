namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouTigris;

public enum OID : uint
{
    Boss = 0x3D29, //R=5.75
    BossAdd = 0x3D2A, //R=3.5
    BossHelper = 0x233C,
    GymnasticGarlic = 0x3D51, // R0.840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticQueen = 0x3D53, // R0.840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticEggplant = 0x3D50, // R0.840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticOnion = 0x3D4F, // R0.840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticTomato = 0x3D52, // R0.840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    BonusAddLyssa = 0x3D4E, //R=3.75, bonus loot adds
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/BossAdd/BonusAddLyssa->player->player, no cast, single-target
    AbsoluteZero = 32208, // Boss->self, 4.0s cast, range 45 90-degree cone
    BlizzardIII = 32209, // Boss->location, 3.0s cast, range 6 circle
    FrumiousJaws = 32206, // Boss->player, 5.0s cast, single-target
    Eyeshine = 32207, // Boss->self, 4.0s cast, range 35 circle
    CatchingClaws = 32210, // BossAdd->self, 3.0s cast, range 12 90-degree cone

    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    Telega = 9630, // bonusadds->self, no cast, single-target, bonus add disappear
    HeavySmash = 32317, // BossAdd->location, 3.0s cast, range 6 circle
}

class HeavySmash(BossModule module) : Components.StandardAOEs(module, AID.HeavySmash, 6);
class AbsoluteZero(BossModule module) : Components.StandardAOEs(module, AID.AbsoluteZero, new AOEShapeCone(45, 45.Degrees()));
class FrumiousJaws(BossModule module) : Components.SingleTargetCast(module, AID.FrumiousJaws);
class BlizzardIII(BossModule module) : Components.StandardAOEs(module, AID.BlizzardIII, 6);
class Eyeshine(BossModule module) : Components.CastGaze(module, AID.Eyeshine);
class CatchingClaws(BossModule module) : Components.StandardAOEs(module, AID.CatchingClaws, new AOEShapeCone(12, 45.Degrees()));
class PluckAndPrune(BossModule module) : Components.StandardAOEs(module, AID.PluckAndPrune, new AOEShapeCircle(7));
class TearyTwirl(BossModule module) : Components.StandardAOEs(module, AID.TearyTwirl, new AOEShapeCircle(7));
class HeirloomScream(BossModule module) : Components.StandardAOEs(module, AID.HeirloomScream, new AOEShapeCircle(7));
class PungentPirouette(BossModule module) : Components.StandardAOEs(module, AID.PungentPirouette, new AOEShapeCircle(7));
class Pollen(BossModule module) : Components.StandardAOEs(module, AID.Pollen, new AOEShapeCircle(7));

class TigrisStates : StateMachineBuilder
{
    public TigrisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AbsoluteZero>()
            .ActivateOnEnter<FrumiousJaws>()
            .ActivateOnEnter<BlizzardIII>()
            .ActivateOnEnter<Eyeshine>()
            .ActivateOnEnter<CatchingClaws>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.GymnasticEggplant).All(e => e.IsDead) && module.Enemies(OID.GymnasticQueen).All(e => e.IsDead) && module.Enemies(OID.GymnasticOnion).All(e => e.IsDead) && module.Enemies(OID.GymnasticGarlic).All(e => e.IsDead) && module.Enemies(OID.GymnasticTomato).All(e => e.IsDead) && module.Enemies(OID.BonusAddLyssa).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 11999)]
public class Tigris(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.BossAdd))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var s in Enemies(OID.GymnasticEggplant))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.GymnasticTomato))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.GymnasticQueen))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.GymnasticGarlic))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.GymnasticOnion))
            Arena.Actor(s, ArenaColor.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.GymnasticOnion => 7,
                OID.GymnasticEggplant => 6,
                OID.GymnasticGarlic => 5,
                OID.GymnasticTomato => 4,
                OID.GymnasticQueen => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
