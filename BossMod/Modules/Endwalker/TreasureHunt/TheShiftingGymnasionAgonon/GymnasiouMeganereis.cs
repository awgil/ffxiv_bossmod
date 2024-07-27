namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouMeganereis;

public enum OID : uint
{
    Boss = 0x3D39, //R=6.0
    BossAdd = 0x3D3A, //R=2.0
    BossHelper = 0x233C,
    GymnasticGarlic = 0x3D51, // R0.840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticQueen = 0x3D53, // R0.840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticEggplant = 0x3D50, // R0.840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticOnion = 0x3D4F, // R0.840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticTomato = 0x3D52, // R0.840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    BonusAddLampas = 0x3D4D, //R=2.001, bonus loot adds
    BonusAddLyssa = 0x3D4E, //R=3.75, bonus loot adds
}

public enum AID : uint
{
    WaveOfTurmoil = 32257, // 3D39->self, 5.0s cast, single-target
    WaveOfTurmoil2 = 32258, // 233C->self, 5.0s cast, range 40 circle, knockback 20 away from source
    AutoAttack1 = 870, // 3D4E->player, no cast, single-target
    AutoAttack2 = 871, // 3D3A->player, no cast, single-target
    AutoAttack3 = 872, // 3D39->player, no cast, single-target
    Hydrobomb = 32259, // 233C->location, 6.5s cast, range 10 circle
    Ceras = 32255, // 3D39->player, 5.0s cast, single-target
    Waterspout = 32261, // 3D39->self, 3.0s cast, single-target
    Waterspout2 = 32262, // 233C->location, 3.0s cast, range 8 circle
    unknown = 32199, // 3D39->self, no cast, single-target
    FallingWater = 32260, // 233C->player, 5.0s cast, range 8 circle
    Hydrocannon = 32264, // 3D3A->self, 3.6s cast, range 17 width 3 rect
    Hydrocannon2 = 32256, // 3D39->self, 3.0s cast, range 27 width 6 rect
    Immersion = 32263, // 3D39->self, 5.0s cast, range 50 circle

    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    Telega = 9630, // bonusadds->self, no cast, single-target, bonus add disappear
    HeavySmash = 32317, // 3D4E->location, 3.0s cast, range 6 circle
}

class Ceras(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Ceras));

class WaveOfTurmoil(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.WaveOfTurmoil), 20, stopAtWall: true)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.FindComponent<Hydrobomb>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
}

class Hydrobomb(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Hydrobomb), 10);
class Waterspout(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Waterspout2), 8);
class Hydrocannon(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Hydrocannon), new AOEShapeRect(17, 1.5f));
class Hydrocannon2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Hydrocannon2), new AOEShapeRect(27, 3));
class FallingWater(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.FallingWater), 8);
class Immersion(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Immersion));
class PluckAndPrune(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PluckAndPrune), new AOEShapeCircle(7));
class TearyTwirl(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TearyTwirl), new AOEShapeCircle(7));
class HeirloomScream(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HeirloomScream), new AOEShapeCircle(7));
class PungentPirouette(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PungentPirouette), new AOEShapeCircle(7));
class Pollen(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Pollen), new AOEShapeCircle(7));
class HeavySmash(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HeavySmash), 6);

class MeganereisStates : StateMachineBuilder
{
    public MeganereisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Ceras>()
            .ActivateOnEnter<WaveOfTurmoil>()
            .ActivateOnEnter<Hydrobomb>()
            .ActivateOnEnter<Waterspout>()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<Hydrocannon2>()
            .ActivateOnEnter<FallingWater>()
            .ActivateOnEnter<Immersion>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.BonusAddLyssa).All(e => e.IsDead) && module.Enemies(OID.BonusAddLampas).All(e => e.IsDead) && module.Enemies(OID.GymnasticEggplant).All(e => e.IsDead) && module.Enemies(OID.GymnasticQueen).All(e => e.IsDead) && module.Enemies(OID.GymnasticOnion).All(e => e.IsDead) && module.Enemies(OID.GymnasticGarlic).All(e => e.IsDead) && module.Enemies(OID.GymnasticTomato).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12014)]
public class Meganereis(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
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
        foreach (var s in Enemies(OID.BonusAddLampas))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.BonusAddLyssa))
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
                OID.GymnasticQueen or OID.BonusAddLampas or OID.BonusAddLyssa => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
