namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D042Protector;

public enum OID : uint
{
    Boss = 0x4237, // R5.830, x1
    LaserTurret = 0x4238, // R0.960, x16
    ExplosiveTurret = 0x4239, // R0.960, x8
    FulminousFence = 0x4255, // R1.000, x4
    Helper = 0x233C, // R0.500, x7, Helper type
}

public enum AID : uint
{
    AutoAttack = 878, // Boss->player, no cast, single-target
    Electrowave = 37161, // Boss->self, 5.0s cast, range 50 circle, raidwide
    SearchAndDestroy = 37154, // Boss->self, 3.0s cast, single-target, visual (turrets)
    HomingCannon = 37155, // LaserTurret->self, 2.5s cast, range 50 width 2 rect
    Shock = 37156, // ExplosiveTurret->location, 2.5s cast, range 3 circle
    SearchAndDestroyEnd = 37153, // Boss->self, no cast, single-target, visual (mechanic end)
    FulminousFence = 37149, // Boss->self, 3.0s cast, single-target, visual (barriers)
    ElectrostaticContact = 37158, // FulminousFence->player, no cast, single-target, damage + paralyze if hit by fence
    BatteryCircuit = 37159, // Boss->self, 5.0s cast, single-target, visual (rotating aoe)
    BatteryCircuitAOEFirst = 37351, // Helper->self, 5.0s cast, range 30 30-degree cone
    BatteryCircuitAOERest = 37344, // Helper->self, no cast, range 30 30-degree cone
    ElectrowhirlFirst = 37350, // Helper->self, 5.0s cast, range 6 circle
    ElectrowhirlRest = 37160, // Helper->self, 3.0s cast, range 6 circle
    Bombardment = 39016, // Helper->location, 3.0s cast, range 5 circle
    RapidThunder = 37162, // Boss->player, 5.0s cast, single-target, tankbuster
    MotionSensor = 37150, // Boss->self, 3.0s cast, single-target, visual (acceleration bombs)
    MotionSensorApply = 37343, // Helper->player, no cast, single-target, visual (apply bomb debuff)
    BlastCannon = 37151, // LaserTurret->self, 3.0s cast, range 26 width 4 rect
    HeavyBlastCannon = 37345, // Boss->self/players, 8.0s cast, range 36 width 8 rect, line stack
    HeavyBlastCannonTargetSelect = 37347, // Helper->player, no cast, single-target, target select
    TrackingBolt = 37348, // Boss->self, 8.0s cast, single-target, visual (spread)
    TrackingBoltAOE = 37349, // Helper->player, 8.0s cast, range 8 circle spread
}

public enum SID : uint
{
    AccelerationBomb1 = 3802, // Helper->player, extra=0x0
    AccelerationBomb2 = 4144, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    RotateCW = 167, // Boss
    RapidThunder = 218, // player
    MotionSensor = 267, // player
    TrackingBolt = 196, // player
}

class Electrowave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Electrowave));
class HomingCannon(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HomingCannon), new AOEShapeRect(50, 1));
class Shock(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Shock), 3);

class FulminousFence(BossModule module) : BossComponent(module)
{
    public record struct Line(WDir A, WDir B);

    private Line[] _curPattern = [];
    private float _curPatternZMult; // each pattern can be mirrored

    private static readonly Line[] _pattern1 = [
        new(new(-4, -12), new(+4, -12)), new(new(+4, -12), new(+4, -4)), new(new(+4, -12), new(+12, -12)), new(new(+12, -12), new(+12, -4)), new(new(+12, -4), new(+12, +4)), new(new(+12, +4), new(+4, +4)),
        new(new(+4, +12), new(-4, +12)), new(new(-4, +12), new(-4, +4)), new(new(-4, +12), new(-12, +12)), new(new(-12, +12), new(-12, +4)), new(new(-12, +4), new(-12, -4)), new(new(-12, -4), new(-4, -4))
    ];
    private static readonly Line[] _pattern2 = [
        new(new(0, -12), new(0, -8)), new(new(-8, -8), new(-4, -4)), new(new(+8, -8), new(+4, -4)), new(new(0, +4), new(0, +8)), new(new(-8, +8), new(-12, +12)), new(new(+8, +8), new(+12, +12))
    ];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (a, b) in ActiveLines())
        {
            var raw = ShapeDistance.Rect(a, b, 0);
            hints.AddForbiddenZone(p => raw(p) - 1);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (a, b) in ActiveLines())
            Arena.AddLine(a, b, ArenaColor.Danger, 2);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index != 13)
            return;
        switch (state)
        {
            case 0x00020001:
                _curPattern = _pattern1;
                _curPatternZMult = 1;
                break;
            case 0x00200010:
                _curPattern = _pattern1;
                _curPatternZMult = -1;
                break;
            case 0x01000080:
                _curPattern = _pattern2;
                _curPatternZMult = 1;
                break;
            case 0x08000400:
                _curPattern = _pattern2;
                _curPatternZMult = -1;
                break;
            case 0x00080004:
            case 0x00400004:
            case 0x02000004:
            case 0x10000004:
                _curPattern = [];
                _curPatternZMult = 0;
                break;
        }
    }

    private IEnumerable<(WPos a, WPos b)> ActiveLines() => _curPattern.Select(l => (ConvertEndpoint(l.A), ConvertEndpoint(l.B)));
    private WPos ConvertEndpoint(WDir p) => new(Module.Center.X + p.X, Module.Center.Z + p.Z * _curPatternZMult);
}

// note: never seen ccw rotation, assume it's not possible
class BatteryCircuit(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone _shape = new(30, 15.Degrees());
    private static readonly TimeSpan _reducedLeeway = TimeSpan.FromSeconds(1.5f); // aoes can appear mid mechanic and fuck up our careful plan

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BatteryCircuitAOEFirst)
            Sequences.Add(new(_shape, caster.Position, spell.Rotation, -11.Degrees(), Module.CastFinishAt(spell) - _reducedLeeway, 0.5f, 34, 10));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BatteryCircuitAOEFirst or AID.BatteryCircuitAOERest)
            AdvanceSequence(caster.Position, caster.Rotation, WorldState.CurrentTime - _reducedLeeway);
    }
}

class ElectrowhirlFirst(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ElectrowhirlFirst), new AOEShapeCircle(6));
class ElectrowhirlRest(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ElectrowhirlRest), new AOEShapeCircle(6));
class Bombardment(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Bombardment), 5);
class RapidThunder(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.RapidThunder));

class MotionSensor(BossModule module) : Components.StayMove(module)
{
    private readonly DateTime[] _expire = new DateTime[4];

    public override void Update()
    {
        base.Update();
        var deadline = WorldState.FutureTime(3);
        for (int i = 0; i < _expire.Length; ++i)
            Requirements[i] = _expire[i] != default && _expire[i] < deadline ? Requirement.Stay : Requirement.None;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_expire[slot] != default && (_expire[slot] - WorldState.CurrentTime).TotalSeconds < 0.5f)
            hints.ForcedMovement = new();
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.AccelerationBomb1 or SID.AccelerationBomb2 && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            _expire[slot] = status.ExpireAt;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.AccelerationBomb1 or SID.AccelerationBomb2 && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            _expire[slot] = default;
    }
}

class BlastCannon(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlastCannon), new AOEShapeRect(26, 2));
class HeavyBlastCannon(BossModule module) : Components.SimpleLineStack(module, 4, 36, ActionID.MakeSpell(AID.HeavyBlastCannonTargetSelect), ActionID.MakeSpell(AID.HeavyBlastCannon), 8);
class TrackingBolt(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.TrackingBoltAOE), 8);

class D042ProtectorStates : StateMachineBuilder
{
    public D042ProtectorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Electrowave>()
            .ActivateOnEnter<HomingCannon>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<FulminousFence>()
            .ActivateOnEnter<BatteryCircuit>()
            .ActivateOnEnter<ElectrowhirlFirst>()
            .ActivateOnEnter<ElectrowhirlRest>()
            .ActivateOnEnter<Bombardment>()
            .ActivateOnEnter<RapidThunder>()
            .ActivateOnEnter<MotionSensor>()
            .ActivateOnEnter<BlastCannon>()
            .ActivateOnEnter<HeavyBlastCannon>()
            .ActivateOnEnter<TrackingBolt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12757)]
public class D042Protector(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -100), new ArenaBoundsRect(12, 20));
