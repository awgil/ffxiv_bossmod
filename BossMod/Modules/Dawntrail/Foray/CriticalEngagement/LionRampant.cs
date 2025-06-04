namespace BossMod.Dawntrail.Foray.CriticalEngagement.LionRampant;

public enum OID : uint
{
    Boss = 0x46DC, // R4.600, x1
    Helper = 0x233C, // R0.500, x22 (spawn during fight), Helper type
    DeathWallHelper = 0x46B4, // R1.000, x4
    AnimatedDoll1 = 0x45D1, // R0.500, x3 (spawn during fight)
    AnimatedDoll2 = 0x45D2, // R0.500, x5 (spawn during fight)
    RadiantBeacon = 0x46DD, // R3.000, x0 (spawn during fight)
    RadiantRoundel = 0x46DE, // R1.500, x0 (spawn during fight)
    LightSprite = 0x46DF, // R1.760, x0 (spawn during fight)
    OchreStone = 0x46E0, // R2.700, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    DeathWall = 41400, // DeathWallHelper->self, no cast, range 25-60 donut
    FearsomeGlint = 41411, // Boss->self, 6.0s cast, range 60 90-degree cone
    AugmentationOfBeacons = 41401, // Boss->self, 3.0s cast, single-target
    AetherialRay = 41402, // Helper->self, no cast, range 28 width 10 rect
    ShockwaveCast = 41412, // Boss->self, 5.0s cast, single-target
    Shockwave = 41414, // Helper->self, no cast, ???
    ShockwaveRepeat = 41413, // Boss->self, no cast, single-target
    AugmentationOfRoundels = 41404, // Boss->self, 3.0s cast, single-target
    AugmentationOfStones = 41405, // Boss->self, 3.0s cast, single-target
    FallingRockCast = 41409, // Boss->self, 3.0s cast, single-target
    FallingRock = 41410, // Helper->location, 4.0s cast, range 10 circle
    FlattenCast = 30787, // Boss->self, 5.0s cast, single-target
    Flatten1 = 41406, // OchreStone->self, 5.0s cast, single-target
    Decompress = 41407, // Helper->self, 5.0s cast, range 12 circle
    Flatten2 = 41408, // OchreStone->self, 5.0s cast, single-target
    BrightPulse = 41403, // Helper->location, no cast, range 12 circle
}

public enum TetherID : uint
{
    BeaconActivate = 311, // LionRampant->RadiantBeacon
    BeaconCountdown = 312, // LionRampant->RadiantBeacon
}

public enum SID : uint
{
    RadiantOmen = 4541, // none->RadiantBeacon/RadiantRoundel, extra=0x898/0x4B0
    RadiantSlow = 4176, // none->RadiantBeacon/RadiantRoundel, extra=0x0
    Unk1 = 2056, // none->RadiantRoundel, extra=0x369
    RoundelCountdown = 2193, // none->RadiantRoundel, extra=0x36A
}

class FearsomeGlint(BossModule module) : Components.StandardAOEs(module, AID.FearsomeGlint, new AOEShapeCone(60, 45.Degrees()));
class Shockwave(BossModule module) : Components.RaidwideCast(module, AID.ShockwaveCast);

class AetherialRay(BossModule module) : Components.GenericAOEs(module, AID.AetherialRay)
{
    private readonly List<Angle> _predicted = [];
    private DateTime _activation;

    public bool Pending => _predicted.Count > 0;

    public static readonly AOEShape Shape = new AOEShapeRect(28, 5);
    public const float PredictionCutoff = 0.8f;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_predicted.Count == 0)
            yield break;

        if (WorldState.FutureTime(PredictionCutoff) > _activation)
            foreach (var a in Module.Enemies(OID.RadiantBeacon))
                yield return new AOEInstance(Shape, Arena.Center, Angle.FromDirection(a.Position - Arena.Center), _activation);
        else
            foreach (var a in _predicted)
                yield return new AOEInstance(Shape, Arena.Center, a, _activation);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.BeaconCountdown && WorldState.Actors.Find(tether.Target) is { } beacon && beacon.OID == (uint)OID.RadiantBeacon)
        {
            var off = beacon.Position - Arena.Center;
            var cw = off.OrthoR().Dot(beacon.Rotation.ToDirection()) > 0;
            _predicted.Add(Angle.FromDirection(beacon.Position - Arena.Center) + (cw ? -160.Degrees() : 160.Degrees()));
            if (_activation == default)
                _activation = WorldState.FutureTime(5.2f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _predicted.Clear();
            _activation = default;
        }
    }
}

class BrightPulse(BossModule module) : Components.GenericAOEs(module, AID.BrightPulse)
{
    private readonly List<(Actor Actor, WPos Position, DateTime Activation)> _predicted = [];

    public static readonly AOEShape Shape = new AOEShapeCircle(12);
    public const float PredictionCutoff = 0.8f;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (a, p, t) in _predicted)
            yield return new AOEInstance(Shape, WorldState.FutureTime(PredictionCutoff) > t ? a.Position : p, Activation: t);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.RoundelCountdown && (OID)actor.OID == OID.RadiantRoundel)
        {
            var isCloseBeacon = actor.Position.InCircle(Arena.Center, 15);

            var off = actor.Position - Arena.Center;
            var cw = off.OrthoR().Dot(actor.Rotation.ToDirection()) > 0;
            var angle = isCloseBeacon ? -80.Degrees() : 148.Degrees();
            if (cw)
                angle = -angle;
            _predicted.Add((actor, Arena.Center + off.Rotate(angle), WorldState.FutureTime(5.2f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class FallingRock(BossModule module) : Components.StandardAOEs(module, AID.FallingRock, 10);
class Decompress(BossModule module) : Components.StandardAOEs(module, AID.Decompress, 12)
{
    private readonly AetherialRay _ray = module.FindComponent<AetherialRay>()!;

    // AI doesn't like trying to dodge both of these at once...TODO figure out why
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).Select(a => a with { Risky = !_ray.Pending });
}

class LionRampantStates : StateMachineBuilder
{
    public LionRampantStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FearsomeGlint>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<AetherialRay>()
            .ActivateOnEnter<BrightPulse>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<Decompress>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13809)]
public class LionRampant(WorldState ws, Actor primary) : BossModule(ws, primary, new(636, -54), new ArenaBoundsCircle(26))
{
    public override bool DrawAllPlayers => true;
}
