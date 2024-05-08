namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouMandragoras;

public enum OID : uint
{
    Boss = 0x3D41, //R=2.85
    BossAdd = 0x3D42, //R=0.85
    BossHelper = 0x233C,
    GymnasticGarlic = 0x3D51, // R0.840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticQueen = 0x3D53, // R0.840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticEggplant = 0x3D50, // R0.840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticOnion = 0x3D4F, // R0.840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticTomato = 0x3D52, // R0.840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
}

public enum AID : uint
{
    Attack = 872, // Boss/3D42->player, no cast, single-target
    Ram = 32297, // Boss->player, 5.0s cast, single-target
    LeafDagger = 32299, // Boss->location, 2.5s cast, range 3 circle
    SaibaiMandragora = 32300, // Boss->self, 3.0s cast, single-target, calls adds
    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    Telega = 9630, // bonusadds->self, no cast, single-target, bonus add disappear
}

class Ram(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Ram));
class SaibaiMandragora(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.SaibaiMandragora), "Calls adds");
class LeafDagger(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.LeafDagger), 3);
class PluckAndPrune(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PluckAndPrune), new AOEShapeCircle(7));
class TearyTwirl(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TearyTwirl), new AOEShapeCircle(7));
class HeirloomScream(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HeirloomScream), new AOEShapeCircle(7));
class PungentPirouette(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PungentPirouette), new AOEShapeCircle(7));
class Pollen(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Pollen), new AOEShapeCircle(7));

class MandragorasStates : StateMachineBuilder
{
    public MandragorasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Ram>()
            .ActivateOnEnter<SaibaiMandragora>()
            .ActivateOnEnter<LeafDagger>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.GymnasticEggplant).All(e => e.IsDead) && module.Enemies(OID.GymnasticQueen).All(e => e.IsDead) && module.Enemies(OID.GymnasticOnion).All(e => e.IsDead) && module.Enemies(OID.GymnasticGarlic).All(e => e.IsDead) && module.Enemies(OID.GymnasticTomato).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12022)]
public class Mandragoras(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
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

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
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
