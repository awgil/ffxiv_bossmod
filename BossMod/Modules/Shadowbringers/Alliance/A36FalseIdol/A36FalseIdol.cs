namespace BossMod.Shadowbringers.Alliance.A36FalseIdol;

public enum OID : uint
{
    Boss = 0x318D, // R26.000, x1
    BossP2 = 0x3190, // R5.999, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x21, Helper type
    LighterNoteEW = 0x318E, // R1.000, x0 (spawn during fight)
    LighterNoteNS = 0x318F, // R1.000, x0 (spawn during fight)
    RedGirl = 0x3191, // R3.450, x0 (spawn during fight)
    Energy = 0x3192, // R1.000, x0 (spawn during fight)
    MagicalInterference = 0x1EB169,
    WhiteDissonance = 0x1EB16A,
    BlackDissonance = 0x1EB16B,
    RecreateStructure = 0x1EB16C,
}

public enum AID : uint
{
    AutoAttack = 24572, // Boss->player, no cast, single-target
    ScreamingScore = 23517, // Boss->self, 5.0s cast, range 60 circle
    MadeMagic1 = 23511, // Boss->self, 7.0s cast, range 50 width 30 rect
    MadeMagic2 = 23510, // Boss->self, 7.0s cast, range 50 width 30 rect
    LighterNoteCast = 23512, // Boss->self, 3.0s cast, single-target
    LighterNoteFirst = 23513, // Helper->location, no cast, range 6 circle
    LighterNoteRest = 23514, // Helper->location, no cast, range 6 circle
    RhythmRings = 23508, // Boss->self, 3.0s cast, single-target
    MagicalInterference = 23509, // Helper->self, no cast, range 50 width 10 rect
    SeedOfMagic = 23518, // Boss->self, 3.0s cast, single-target
    ScatteredMagic = 23519, // Helper->location, 3.0s cast, range 4 circle
    DarkerNoteCast = 23515, // Boss->self, 5.0s cast, single-target
    DarkerNote = 23516, // Helper->player, 5.0s cast, range 6 circle
    Eminence = 24021, // Boss->location, 5.0s cast, range 60 circle, stun, phase transition

    AutoAttackP2 = 24575, // BossP2->player, no cast, single-target
    Pervasion = 23520, // BossP2->self, 3.0s cast, single-target
    RecreateStructure = 23521, // BossP2->self, 3.0s cast, single-target
    UnevenFooting = 23522, // Helper->self, 1.9s cast, range 80 width 30 rect
    RecreateSignal = 23523, // BossP2->self, 3.0s cast, single-target
    MixedSignals = 23524, // BossP2->self, 3.0s cast, single-target
    Crash = 23525, // Helper->self, 0.8s cast, range 50 width 10 rect
    LighterNoteCastP2 = 23564, // BossP2->self, 3.0s cast, single-target
    ScreamingScoreP2 = 23541, // BossP2->self, 5.0s cast, range 71 circle
    DarkerNoteP2 = 23562, // BossP2->self, 5.0s cast, single-target
    HeavyArmsCast = 23534, // BossP2->self, 7.0s cast, single-target
    HeavyArmsSides = 23535, // Helper->self, 7.0s cast, range 44 width 100 rect
    HeavyArmsMiddle = 23533, // BossP2->self, 7.0s cast, range 100 width 12 rect
    Distortion1 = 23529, // BossP2->self, 3.0s cast, range 60 circle
    TheFinalSong = 23530, // BossP2->self, 3.0s cast, single-target
    PlaceOfPower = 23565, // Helper->location, 3.0s cast, range 6 circle
    WhiteDissonance = 23531, // Helper->self, no cast, range 60 circle, gaze mechanic
    BlackDissonance = 23532, // Helper->self, no cast, range 60 circle, gaze mechanic
    PillarImpact = 23536, // BossP2->self, 10.0s cast, single-target
    Shockwave = 23538, // Helper->self, 6.5s cast, range 71 circle, distance 35 kb, can be invulned
    ShockwaveCircle = 23537, // Helper->self, 6.5s cast, range 7 circle, instant kill
    PillarImpactInstant = 23566, // BossP2->self, no cast, single-target
    TowerfallCast = 23539, // BossP2->self, 3.0s cast, single-target
    Towerfall = 23540, // Helper->self, 3.0s cast, range 70 width 14 rect
    Distortion2 = 24664, // BossP2->self, 3.0s cast, range 60 circle
    ScatteredMagicP2 = 23528, // 3192->player, no cast, single-target
    RhythmRingsP2 = 23563, // BossP2->self, 3.0s cast, single-target
}

public enum SID : uint
{
    Distorted = 2535, // BossP2->player, extra=0x0
}

public enum IconID : uint
{
    LighterNote = 1, // player->self
    Spread = 139, // player->self
}

public enum TetherID : uint
{
    TrafficLights = 54, // Helper/BossP2->BossP2/Helper
}

class ScreamingScore(BossModule module) : Components.RaidwideCast(module, AID.ScreamingScore);
class MadeMagic(BossModule module) : Components.GroupedAOEs(module, [AID.MadeMagic1, AID.MadeMagic2], new AOEShapeRect(50, 15));
class ScatteredMagic(BossModule module) : Components.StandardAOEs(module, AID.ScatteredMagic, 4);
class DarkerNote(BossModule module) : Components.BaitAwayCast(module, AID.DarkerNote, new AOEShapeCircle(6), centerAtTarget: true);
class Eminence(BossModule module) : Components.RaidwideCast(module, AID.Eminence, "Knockback + stun");

class RecreateStructure(BossModule module) : Components.GenericAOEs(module, AID.UnevenFooting)
{
    private readonly List<(Actor Actor, DateTime Activation)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(new AOEShapeRect(15, 80, 15), c.Actor.Position, c.Actor.Rotation, c.Activation));

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.RecreateStructure && state == 0x00010002)
            _casters.Add((actor, WorldState.FutureTime(9)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _casters.Clear();
        }
    }
}

class HeavyArms1(BossModule module) : Components.StandardAOEs(module, AID.HeavyArmsSides, new AOEShapeRect(44, 50));
class HeavyArms2(BossModule module) : Components.StandardAOEs(module, AID.HeavyArmsMiddle, new AOEShapeRect(100, 6));

class A36FalseIdolStates : StateMachineBuilder
{
    public A36FalseIdolStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ScreamingScore>()
            .ActivateOnEnter<MadeMagic>()
            .ActivateOnEnter<LighterNote>()
            .ActivateOnEnter<LighterNoteSpread>()
            .ActivateOnEnter<MagicalInterference>()
            .ActivateOnEnter<ScatteredMagic>()
            .ActivateOnEnter<DarkerNote>()
            .ActivateOnEnter<Eminence>()
            .ActivateOnEnter<RecreateStructure>()
            .ActivateOnEnter<MixedSignals>()
            .ActivateOnEnter<HeavyArms1>()
            .ActivateOnEnter<HeavyArms2>()
            .ActivateOnEnter<Distortion>()
            .ActivateOnEnter<Dissonance>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed
                && module.Enemies(OID.BossP2).All(i => i.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9948)]
public class A36FalseIdol(WorldState ws, Actor primary) : BossModule(ws, primary, new(-700, -700), new ArenaBoundsSquare(24.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.BossP2), ArenaColor.Enemy);
    }
}
