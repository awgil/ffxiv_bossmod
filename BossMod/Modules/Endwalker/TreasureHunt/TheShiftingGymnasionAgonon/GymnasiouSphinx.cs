namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouSphinx;

public enum OID : uint
{
    Boss = 0x3D3B, //R=4.83
    BossAdd = 0x3D3C, //R=2.3
    VerdantPlume = 0x3D3D, //R=0.65
    BossHelper = 0x233C,
    GymnasticGarlic = 0x3D51, // R0,840, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticQueen = 0x3D53, // R0,840, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticEggplant = 0x3D50, // R0,840, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticOnion = 0x3D4F, // R0,840, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticTomato = 0x3D52, // R0,840, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    BonusAdds_Lampas = 0x3D4D, //R=2.001, bonus loot adds
    BonusAdds_Lyssa = 0x3D4E, //R=3.75, bonus loot adds
};

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // BossAdd->player, no cast, single-target
    FeatherWind = 32267, // Boss->self, 4,0s cast, single-target, spawns Verdant Plumes
    Explosion = 32273, // 3D3D->self, 5,0s cast, range 3-12 donut
    Scratch = 32265, // Boss->player, 5,0s cast, single-target
    AeroII = 32268, // Boss->self, 3,0s cast, single-target
    AeroII2 = 32269, // BossHelper->location, 3,0s cast, range 4 circle
    FervidPulse = 32272, // Boss->self, 5,0s cast, range 50 width 14 cross
    Spreadmarkers = 32199, // Boss->self, no cast, single-target
    FeatherRain = 32271, // BossHelper->player, 5,0s cast, range 6 circle
    FrigidPulse = 32270, // Boss->self, 5,0s cast, range 12-60 donut
    AlpineDraft = 32274, // BossAdd->self, 3,0s cast, range 45 width 5 rect
    MoltingPlumage = 32266, // Boss->self, 5,0s cast, range 60 circle
    PluckAndPrune = 32302, // GymnasticEggplant->self, 3,5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3,5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3,5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3,5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3,5s cast, range 7 circle
    Telega = 9630, // bonusadds->self, no cast, single-target, bonus add disappear
    HeavySmash = 32317, // 3D4E->location, 3,0s cast, range 6 circle
};

class Scratch : Components.SingleTargetCast
{
    public Scratch() : base(ActionID.MakeSpell(AID.Scratch)) { }
}

class Explosion : Components.SelfTargetedAOEs
{
    public Explosion() : base(ActionID.MakeSpell(AID.Explosion), new AOEShapeDonut(3, 12)) { }
}

class FrigidPulse : Components.SelfTargetedAOEs
{
    public FrigidPulse() : base(ActionID.MakeSpell(AID.FrigidPulse), new AOEShapeDonut(12, 60)) { }
}

class FervidPulse : Components.SelfTargetedAOEs
{
    public FervidPulse() : base(ActionID.MakeSpell(AID.FervidPulse), new AOEShapeCross(50, 7)) { }
}

class MoltingPlumage : Components.RaidwideCast
{
    public MoltingPlumage() : base(ActionID.MakeSpell(AID.MoltingPlumage)) { }
}

class AlpineDraft : Components.SelfTargetedAOEs
{
    public AlpineDraft() : base(ActionID.MakeSpell(AID.AlpineDraft), new AOEShapeRect(45, 2.5f)) { }
}

class FeatherRain : Components.SpreadFromCastTargets
{
    public FeatherRain() : base(ActionID.MakeSpell(AID.FeatherRain), 6) { }
}

class AeroII : Components.LocationTargetedAOEs
{
    public AeroII() : base(ActionID.MakeSpell(AID.AeroII2), 4) { }
}

class PluckAndPrune : Components.SelfTargetedAOEs
{
    public PluckAndPrune() : base(ActionID.MakeSpell(AID.PluckAndPrune), new AOEShapeCircle(7)) { }
}

class TearyTwirl : Components.SelfTargetedAOEs
{
    public TearyTwirl() : base(ActionID.MakeSpell(AID.TearyTwirl), new AOEShapeCircle(7)) { }
}

class HeirloomScream : Components.SelfTargetedAOEs
{
    public HeirloomScream() : base(ActionID.MakeSpell(AID.HeirloomScream), new AOEShapeCircle(7)) { }
}

class PungentPirouette : Components.SelfTargetedAOEs
{
    public PungentPirouette() : base(ActionID.MakeSpell(AID.PungentPirouette), new AOEShapeCircle(7)) { }
}

class Pollen : Components.SelfTargetedAOEs
{
    public Pollen() : base(ActionID.MakeSpell(AID.Pollen), new AOEShapeCircle(7)) { }
}

class HeavySmash : Components.LocationTargetedAOEs
{
    public HeavySmash() : base(ActionID.MakeSpell(AID.HeavySmash), 6) { }
}

class SphinxStates : StateMachineBuilder
{
    public SphinxStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Scratch>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<FrigidPulse>()
            .ActivateOnEnter<FervidPulse>()
            .ActivateOnEnter<MoltingPlumage>()
            .ActivateOnEnter<AlpineDraft>()
            .ActivateOnEnter<FeatherRain>()
            .ActivateOnEnter<AeroII>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.BonusAdds_Lyssa).All(e => e.IsDead) && module.Enemies(OID.BonusAdds_Lampas).All(e => e.IsDead) && module.Enemies(OID.GymnasticEggplant).All(e => e.IsDead) && module.Enemies(OID.GymnasticQueen).All(e => e.IsDead) && module.Enemies(OID.GymnasticOnion).All(e => e.IsDead) && module.Enemies(OID.GymnasticGarlic).All(e => e.IsDead) && module.Enemies(OID.GymnasticTomato).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12016)]
public class Sphinx : BossModule
{
    public Sphinx(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }

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
        foreach (var s in Enemies(OID.BonusAdds_Lampas))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.BonusAdds_Lyssa))
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
                OID.GymnasticQueen or OID.BonusAdds_Lampas or OID.BonusAdds_Lyssa => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
