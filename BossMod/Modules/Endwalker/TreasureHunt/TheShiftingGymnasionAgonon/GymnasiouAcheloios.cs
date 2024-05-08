namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouAcheloios;

public enum OID : uint
{
    Boss = 0x3D3E, //R=4.0
    BossAdd = 0x3D3F, //R=2.7
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
    AutoAttack = 870, // Boss/BossAdd->player, no cast, single-target
    DoubleHammerA = 32284, // Boss->self, 4.2s cast, single-target
    DoubleHammerB = 32281, // Boss->self, 4.2s cast, single-target
    DoubleHammer = 32859, // BossHelper->self, 5.0s cast, range 30 180-degree cone
    RightHammer1 = 32282, // Boss->self, 0.5s cast, single-target
    RightHammer2 = 32860, // BossHelper->self, 1.0s cast, range 30 180-degree cone
    TailSwing = 32279, // Boss->self, 3.5s cast, range 13 circle
    QuadrupleHammerA = 32280, // Boss->self, 4.2s cast, single-target
    QuadrupleHammerB = 32283, // Boss->self, 4.2s cast, single-target
    QuadrupleHammer2 = 32858, // BossHelper->self, 5.0s cast, range 30 180-degree cone
    LeftHammer1 = 32285, // Boss->self, 0.5s cast, single-target
    LeftHammer2 = 32861, // BossHelper->self, 1.0s cast, range 30 180-degree cone
    CriticalBite = 32286, // BossAdd->self, 3.0s cast, range 10 120-degree cone
    DeadlyHold = 32275, // Boss->player, 5.0s cast, single-target
    Earthbreak = 32277, // Boss->self, 2.1s cast, single-target
    Earthbreak2 = 32278, // BossHelper->location, 3.0s cast, range 5 circle
    VolcanicHowl = 32276, // Boss->self, 5.0s cast, range 40 circle
    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    Telega = 9630, // bonusadds->self, no cast, single-target, bonus add disappear
    HeavySmash = 32317, // BonusAddLyssa->location, 3.0s cast, range 6 circle
}

public enum IconID : uint
{
    RotateCW = 167, // Boss
    RotateCCW = 168, // Boss
}

class Slammer(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;
    private Angle _rotation;
    private DateTime _activation;
    private static readonly AOEShapeCone _shape = new(30, 90.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        // note that boss switches hands, so CW rotation means CCW aoe sequence and vice versa
        var increment = (IconID)iconID switch
        {
            IconID.RotateCW => 90.Degrees(),
            IconID.RotateCCW => 90.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            _increment = increment;
            InitIfReady(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DoubleHammer)
        {
            Sequences.Add(new(_shape, caster.Position, spell.Rotation, 180.Degrees(), spell.NPCFinishAt, 3.9f, 2, 1));
            ImminentColor = ArenaColor.AOE;
        }
        if ((AID)spell.Action.ID == AID.QuadrupleHammer2)
        {
            ImminentColor = ArenaColor.Danger;
            _rotation = spell.Rotation;
            _activation = spell.NPCFinishAt;
        }
        if (_rotation != default)
            InitIfReady(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count > 0 && (AID)spell.Action.ID is AID.QuadrupleHammer2 or AID.LeftHammer2 or AID.RightHammer2 or AID.DoubleHammerA or AID.DoubleHammerB)
            AdvanceSequence(0, WorldState.CurrentTime);
    }

    private void InitIfReady(Actor source)
    {
        if (_rotation != default && _increment != default)
        {
            Sequences.Add(new(_shape, source.Position, _rotation, _increment, _activation, 3.3f, 4));
            _rotation = default;
            _increment = default;
        }
    }
}

class VolcanicHowl(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.VolcanicHowl));
class Earthbreak(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Earthbreak2), 5);
class DeadlyHold(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.DeadlyHold));
class TailSwing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TailSwing), new AOEShapeCircle(13));
class CriticalBite(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CriticalBite), new AOEShapeCone(10, 60.Degrees()));
class PluckAndPrune(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PluckAndPrune), new AOEShapeCircle(7));
class TearyTwirl(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TearyTwirl), new AOEShapeCircle(7));
class HeirloomScream(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HeirloomScream), new AOEShapeCircle(7));
class PungentPirouette(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PungentPirouette), new AOEShapeCircle(7));
class Pollen(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Pollen), new AOEShapeCircle(7));
class HeavySmash(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HeavySmash), 6);

class AcheloiosStates : StateMachineBuilder
{
    public AcheloiosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Slammer>()
            .ActivateOnEnter<TailSwing>()
            .ActivateOnEnter<CriticalBite>()
            .ActivateOnEnter<DeadlyHold>()
            .ActivateOnEnter<Earthbreak>()
            .ActivateOnEnter<VolcanicHowl>()
            .ActivateOnEnter<PluckAndPrune>()
            .ActivateOnEnter<TearyTwirl>()
            .ActivateOnEnter<HeirloomScream>()
            .ActivateOnEnter<PungentPirouette>()
            .ActivateOnEnter<Pollen>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.BossAdd).All(e => e.IsDead) && module.Enemies(OID.BonusAddLyssa).All(e => e.IsDead) && module.Enemies(OID.BonusAddLampas).All(e => e.IsDead) && module.Enemies(OID.GymnasticEggplant).All(e => e.IsDead) && module.Enemies(OID.GymnasticQueen).All(e => e.IsDead) && module.Enemies(OID.GymnasticOnion).All(e => e.IsDead) && module.Enemies(OID.GymnasticGarlic).All(e => e.IsDead) && module.Enemies(OID.GymnasticTomato).All(e => e.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12019)]
public class Acheloios(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
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
                OID.GymnasticQueen or OID.BonusAddLampas or OID.BonusAddLyssa => 3,
                OID.BossAdd => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
