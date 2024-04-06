namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarMandragora;

public enum OID : uint
{
    Boss = 0x2542, //R=2.85
    BossAdd = 0x255C, //R=0.84
    BossHelper = 0x233C,
    AltarQueen = 0x254A, // R0,840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    AltarGarlic = 0x2548, // R0,840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    AltarTomato = 0x2549, // R0,840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    AltarOnion = 0x2546, // R0,840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    AltarEgg = 0x2547, // R0,840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
};

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    AutoAttack2 = 6499, // BossAdd->player, no cast, single-target
    OpticalIntrusion = 13367, // Boss->player, 3,0s cast, single-target
    LeafDagger = 13369, // Boss->location, 2,5s cast, range 3 circle
    SaibaiMandragora = 13370, // Boss->self, 3,0s cast, single-target
    Hypnotize = 13368, // Boss->self, 2,5s cast, range 20+R 90-degree cone, gaze, paralysis

    PluckAndPrune = 6449, // AltarEgg->self, 3,5s cast, range 6+R circle
    PungentPirouette = 6450, // AltarGarlic->self, 3,5s cast, range 6+R circle
    TearyTwirl = 6448, // AltarOnion->self, 3,5s cast, range 6+R circle
    Pollen = 6452, // AltarQueen->self, 3,5s cast, range 6+R circle
    HeirloomScream = 6451, // AltarTomato->self, 3,5s cast, range 6+R circle
    Telega = 9630, // bonusadds->self, no cast, single-target, bonus add disappear
};

class OpticalIntrusion : Components.SingleTargetDelayableCast
{
    public OpticalIntrusion() : base(ActionID.MakeSpell(AID.OpticalIntrusion)) { }
}

class Hypnotize : Components.SelfTargetedAOEs
{
    public Hypnotize() : base(ActionID.MakeSpell(AID.Hypnotize), new AOEShapeCone(22.85f, 45.Degrees())) { }
}

class SaibaiMandragora : Components.CastHint
{
    public SaibaiMandragora() : base(ActionID.MakeSpell(AID.SaibaiMandragora), "Calls adds") { }
}

class LeafDagger : Components.LocationTargetedAOEs
{
    public LeafDagger() : base(ActionID.MakeSpell(AID.LeafDagger), 3) { }
}

class PluckAndPrune : Components.SelfTargetedAOEs
{
    public PluckAndPrune() : base(ActionID.MakeSpell(AID.PluckAndPrune), new AOEShapeCircle(6.84f)) { }
}

class TearyTwirl : Components.SelfTargetedAOEs
{
    public TearyTwirl() : base(ActionID.MakeSpell(AID.TearyTwirl), new AOEShapeCircle(6.84f)) { }
}

class HeirloomScream : Components.SelfTargetedAOEs
{
    public HeirloomScream() : base(ActionID.MakeSpell(AID.HeirloomScream), new AOEShapeCircle(6.84f)) { }
}

class PungentPirouette : Components.SelfTargetedAOEs
{
    public PungentPirouette() : base(ActionID.MakeSpell(AID.PungentPirouette), new AOEShapeCircle(6.84f)) { }
}

class Pollen : Components.SelfTargetedAOEs
{
    public Pollen() : base(ActionID.MakeSpell(AID.Pollen), new AOEShapeCircle(6.84f)) { }
}

class MandragoraStates : StateMachineBuilder
{
    public MandragoraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OpticalIntrusion>()
            .ActivateOnEnter<SaibaiMandragora>()
            .ActivateOnEnter<LeafDagger>()
            .ActivateOnEnter<Hypnotize>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.AltarEgg).All(e => e.IsDead) && module.Enemies(OID.AltarQueen).All(e => e.IsDead) && module.Enemies(OID.AltarOnion).All(e => e.IsDead) && module.Enemies(OID.AltarGarlic).All(e => e.IsDead) && module.Enemies(OID.AltarTomato).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7600)]
public class Mandragora : BossModule
{
    public Mandragora(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

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
    }

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.AltarOnion => 7,
                OID.AltarEgg => 6,
                OID.AltarGarlic => 5,
                OID.AltarTomato => 4,
                OID.AltarQueen => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
