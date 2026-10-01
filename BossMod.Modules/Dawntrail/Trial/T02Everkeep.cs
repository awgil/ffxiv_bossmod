namespace BossMod.Dawntrail.Trial.T02Everkeep;

public enum OID : uint
{
    Boss = 0x42A9, // R2.500, phase 1 (human form)
    BossP2 = 0x42B4, // phase 2 (Vollok form) - spawns mid-fight
    Helper = 0x233C,
    ShadowOfTural = 0x43A8, // R0.500, initial add waves (phase 1)
    Fang = 0x42AA, // spawn during fight
    ShadowOfTuralSword = 0x42AC, // spawn during fight, later wave
    ShadowOfTuralSpear = 0x42AD, // spawn during fight, later wave
    FangSmall = 0x42B6, // R1.000, Phase 2 Fang that telegraphs Chasm of Vollok preview
    HalfCircuitHelper = 0x42B9, // R10.050, Smiting Circuit visuals (shared OID with Ex2)
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    SoulOverflow = 37707, // Boss->self, 4.7s cast, raidwide
    SoulOverflowEnrage = 37744, // Boss->self, 6.7s cast, raidwide + bleed (phase transition)
    PatricidalPique = 37715, // Boss->MT, 4.7s cast, single-target tankbuster
    CalamitysEdge = 37708, // Boss->self, 4.7s cast, raidwide
    Burst = 37709, // ShadowOfTural->self, 7.7s cast, range 8 circle

    VorpalTrailVisual = 37710, // Boss->self, 3.4s cast, single-target visual
    VorpalTrailSprint = 37711, // Fang->self, 0.7s cast, sprint telegraph
    VorpalTrailAOE = 37712, // Helper->location, 2.0s cast, rect puddle at sprint waypoint
    VorpalTrailInitial = 38183, // Fang->self, instant, TargetPos = first waypoint endpoint
    VorpalTrailTelegraph = 38184, // Helper->location, 4.0s cast, 0-hit path telegraph

    DoubleEdgedSwordsVisual = 37713, // Boss->self, 4.1s cast, single-target visual
    DoubleEdgedSwordsAOE = 37714, // Helper->self, 4.7s cast, range 60 width 120 rect

    DawnOfAnAge = 37716, // BossP2->self, 6.7s cast, raidwide + arena shrink
    Actualize = 37718, // BossP2->self, 4.7s cast, raidwide + arena restore

    Vollok = 37719, // BossP2->self, 3.7s cast, visual (spawns FangSmall)
    ChasmOfVollokPreview = 37720, // FangSmall->self, 6.7s cast, 5x5 rect telegraph
    Sync = 37721, // BossP2->self, 4.7s cast, visual
    ChasmOfVollokAOE = 37722, // Helper->self, 0.7s cast, range 5 width 5 rect

    Gateway = 37723, // BossP2->self, 3.7s cast, visual
    BladeWarp = 37726, // BossP2->self, 3.7s cast, visual
    ForgedTrackVisual = 37727, // BossP2->self, 3.7s cast, visual
    ForgedTrackPreview = 37729, // Helper->self, 11.6s cast, outer-arena sword-path telegraph
    ForgedTrackAOE = 37730, // Fang->self, instant, sword-charge damage along path

    DutysEdgeTarget = 35567, // Helper->player, instant, target marker
    DutysEdgeVisual = 37748, // BossP2->self, 4.6s cast, visual
    DutysEdgeRepeat = 37749, // BossP2->self, 2.1s cast, visual
    DutysEdgeAOE = 37750, // Helper->self, instant, range 100 width 8 rect line-stack

    HalfFullVisual = 37737, // BossP2->self, 5.7s cast, visual
    HalfFullAOE = 37738, // Helper->self, 6.0s cast, range 60 width 120 rect

    BitterReapingVisual = 37753, // BossP2->self, 4.1s cast, visual
    BitterReaping = 37754, // Helper->player, 4.7s cast, tankbuster (MT + OT)

    FireIII = 37752, // Helper->player, instant, spread circle

    HalfCircuitVisualCircle = 37739, // BossP2->self, 6.7s cast, visual
    HalfCircuitVisualDonut = 37740, // BossP2->self, 6.7s cast, visual
    HalfCircuitRect = 37741, // Helper->self, 7.0s cast, range 60 width 120 rect
    HalfCircuitDonut = 37742, // Helper->self, 6.7s cast, range 10-30 donut
    HalfCircuitCircle = 37743, // Helper->self, 6.7s cast, range 10 circle

    SmitingCircuitVisual = 37731, // BossP2->self, 6.7s cast, visual
    SmitingCircuitHelperDonut = 37732, // HalfCircuitHelper->self, visual
    SmitingCircuitHelperCircle = 37733, // HalfCircuitHelper->self, visual
    SmitingCircuitDonutAOE = 37734, // Helper->self, 6.7s cast, range 10-30 donut
    SmitingCircuitCircleAOE = 37735, // Helper->self, 6.7s cast, range 10 circle
}

public enum IconID : uint
{
    FireIII = 376, // player
}

class SoulOverflow(BossModule module) : Components.RaidwideCast(module, AID.SoulOverflow);
class SoulOverflowEnrage(BossModule module) : Components.RaidwideCast(module, AID.SoulOverflowEnrage);
class PatricidalPique(BossModule module) : Components.SingleTargetCast(module, AID.PatricidalPique);
class CalamitysEdge(BossModule module) : Components.RaidwideCast(module, AID.CalamitysEdge);
class Burst(BossModule module) : Components.StandardAOEs(module, AID.Burst, new AOEShapeCircle(8));
class ShadowOfTural(BossModule module) : Components.AddsMulti(module, [OID.ShadowOfTural, OID.ShadowOfTuralSword, OID.ShadowOfTuralSpear], 1);
class DoubleEdgedSwords(BossModule module) : Components.StandardAOEs(module, AID.DoubleEdgedSwordsAOE, new AOEShapeRect(60, 60));
class HalfFull(BossModule module) : Components.StandardAOEs(module, AID.HalfFullAOE, new AOEShapeRect(60, 60));
class BitterReaping(BossModule module) : Components.SingleTargetCast(module, AID.BitterReaping);
class FireIII(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.FireIII, AID.FireIII, 5, 5.1f);
class HalfCircuitRect(BossModule module) : Components.StandardAOEs(module, AID.HalfCircuitRect, new AOEShapeRect(60, 60));
class HalfCircuitDonut(BossModule module) : Components.StandardAOEs(module, AID.HalfCircuitDonut, new AOEShapeDonut(10, 30));
class HalfCircuitCircle(BossModule module) : Components.StandardAOEs(module, AID.HalfCircuitCircle, new AOEShapeCircle(10));
class SmitingCircuitDonut(BossModule module) : Components.StandardAOEs(module, AID.SmitingCircuitDonutAOE, new AOEShapeDonut(10, 30));
class SmitingCircuitCircle(BossModule module) : Components.StandardAOEs(module, AID.SmitingCircuitCircleAOE, new AOEShapeCircle(10));

class DawnOfAnAge(BossModule module) : Components.RaidwideCast(module, AID.DawnOfAnAge)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            Module.Arena.Bounds = T02Everkeep.SmallBounds;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var c in Casters)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Module.Center, 45.Degrees(), 20, 20, 20), Module.CastFinishAt(c.CastInfo));
    }

    private static readonly float OuterCardinal = 20f * MathF.Sqrt(2);
    private static readonly float InnerCardinal = 10f * MathF.Sqrt(2);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Casters.Count == 0)
            return;
        var c = Module.Center;
        var nO = new WPos(c.X, c.Z - OuterCardinal);
        var eO = new WPos(c.X + OuterCardinal, c.Z);
        var sO = new WPos(c.X, c.Z + OuterCardinal);
        var wO = new WPos(c.X - OuterCardinal, c.Z);
        var nI = new WPos(c.X, c.Z - InnerCardinal);
        var eI = new WPos(c.X + InnerCardinal, c.Z);
        var sI = new WPos(c.X, c.Z + InnerCardinal);
        var wI = new WPos(c.X - InnerCardinal, c.Z);
        Arena.ZonePoly("doaa-ne", [nO, eO, eI, nI], ArenaColor.AOE);
        Arena.ZonePoly("doaa-se", [eO, sO, sI, eI], ArenaColor.AOE);
        Arena.ZonePoly("doaa-sw", [sO, wO, wI, sI], ArenaColor.AOE);
        Arena.ZonePoly("doaa-nw", [wO, nO, nI, wI], ArenaColor.AOE);
    }
}

class Actualize(BossModule module) : Components.RaidwideCast(module, AID.Actualize)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            Module.Arena.Bounds = T02Everkeep.NormalBounds;
    }
}

class DutysEdge(BossModule module) : Components.GenericWildCharge(module, 4, AID.DutysEdgeAOE, 100)
{
    private const int TotalHits = 4;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DutysEdgeTarget:
                Source = caster;
                NumCasts = 0;
                foreach (var (i, p) in Raid.WithSlot(true))
                    PlayerRoles[i] = p.InstanceID == spell.MainTargetID ? PlayerRole.Target : PlayerRole.Share;
                break;
            case AID.DutysEdgeAOE:
                if (++NumCasts >= TotalHits)
                    Source = null;
                break;
        }
    }
}

// Mode A: 37720 cells inside the arena are the damage. Mode B: 37720 is an outer-ring preview;
// 37722 hits inner cells ~6s later. Outer cells map inward by 15√2 on X and Z.
class ChasmOfVollok(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Dictionary<ulong, AOEInstance> _previewAoes = [];
    private readonly Dictionary<ulong, AOEInstance> _damageAoes = [];

    private static readonly AOEShapeRect _shape = new(5f, 2.5f);
    private const float ArenaRadius = 15f;
    private const float TranslateMagnitude = 21.21f;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => [.. _previewAoes.Values, .. _damageAoes.Values];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ChasmOfVollokPreview:
                var pos = spell.LocXZ;
                var dx = pos.X - Module.Center.X;
                var dz = pos.Z - Module.Center.Z;
                if (dx * dx + dz * dz > ArenaRadius * ArenaRadius)
                    pos = new WPos(pos.X - TranslateMagnitude * MathF.Sign(dx),
                                   pos.Z - TranslateMagnitude * MathF.Sign(dz));
                _previewAoes[caster.InstanceID] = new AOEInstance(_shape, pos, spell.Rotation, Module.CastFinishAt(spell));
                break;
            case AID.ChasmOfVollokAOE:
                _damageAoes[caster.InstanceID] = new AOEInstance(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ChasmOfVollokPreview: _previewAoes.Remove(caster.InstanceID); break;
            case AID.ChasmOfVollokAOE: _damageAoes.Remove(caster.InstanceID); break;
        }
    }
}

// NW-SE lanes stay on the preview; NE-SW lanes shift 5m perpendicular, sign from floor(perp/5) parity.
class ForgedTrack(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Dictionary<ulong, AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var cutoff = WorldState.CurrentTime.AddSeconds(-0.5);
        foreach (var id in _aoes.Where(kv => kv.Value.Activation < cutoff).Select(kv => kv.Key).ToList())
            _aoes.Remove(id);
        return _aoes.Values;
    }

    private static readonly AOEShapeRect _shape = new(20f, 2.5f);
    private const float ForwardOffset = 30f;
    private const float PerpOffsetMagnitude = 5f;
    private const float DamageDelayAfterPreview = 1.4f;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.ForgedTrackPreview)
            return;

        var dir = spell.Rotation.ToDirection();
        var perpOffset = default(WDir);
        if (dir.X * dir.Z < 0)
        {
            var orthoL = dir.OrthoL();
            var perp = (caster.Position - Module.Center).Dot(orthoL);
            var laneIndex = (int)MathF.Floor(perp / PerpOffsetMagnitude);
            var perpSign = laneIndex % 2 == 0 ? 1f : -1f;
            perpOffset = orthoL * (PerpOffsetMagnitude * perpSign);
        }
        var origin = caster.Position + dir * ForwardOffset + perpOffset;
        _aoes[caster.InstanceID] = new AOEInstance(_shape, origin, spell.Rotation, Module.CastFinishAt(spell, DamageDelayAfterPreview));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.ForgedTrackAOE)
            return;
        var id = _aoes.FirstOrDefault(kv => kv.Value.Rotation.AlmostEqual(caster.Rotation, 0.1f)).Key;
        if (id != 0)
            _aoes.Remove(id);
        else if (_aoes.Count > 0)
            _aoes.Remove(_aoes.Keys.First());
    }
}

class VorpalTrail(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Dictionary<ulong, AOEInstance> _fangAOE = [];
    private readonly Dictionary<ulong, AOEInstance> _predictedNext = [];
    private readonly Dictionary<ulong, DateTime> _lastUnsafe = [];
    private DateTime _mechanicActiveUntil;
    private const float SettleSeconds = 4f;
    private const float NextSprintExpirationSec = 5f;
    private const float CenterAttractRadius = 0.1f;
    private const float CenterAttractWeight = 10f;
    private const float AIRectHalfWidth = 3.5f;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var now = WorldState.CurrentTime;
        foreach (var id in _fangAOE.Where(kv => kv.Value.Activation < now).Select(kv => kv.Key).ToList())
            _fangAOE.Remove(id);
        foreach (var id in _predictedNext.Where(kv => kv.Value.Activation < now).Select(kv => kv.Key).ToList())
            _predictedNext.Remove(id);
        return _fangAOE.Values.Concat(_predictedNext.Values);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var now = WorldState.CurrentTime;
        var mechanicActive = _fangAOE.Count > 0 || _mechanicActiveUntil > now;
        if (!mechanicActive)
        {
            _lastUnsafe.Remove(actor.InstanceID);
            return;
        }
        foreach (var aoe in ActiveAOEs(slot, actor))
        {
            if (!aoe.Risky)
                continue;
            if (aoe.Shape is AOEShapeRect rect)
            {
                var paddedHalfWidth = MathF.Max(rect.HalfWidth, AIRectHalfWidth);
                var widened = new AOEShapeRect(rect.LengthFront, paddedHalfWidth, rect.LengthBack, rect.DirectionOffset);
                hints.AddForbiddenZone(widened, aoe.Origin, aoe.Rotation, aoe.Activation);
            }
            else
            {
                hints.AddForbiddenZone(aoe.Distance, aoe.Activation);
            }
        }
        hints.GoalZones.Add(AIHints.GoalSingleTarget(Module.Center, CenterAttractRadius, CenterAttractWeight));

        var inDanger = _fangAOE.Values.Any(a => a.Check(actor.Position)) || _predictedNext.Values.Any(a => a.Check(actor.Position));
        if (inDanger || !_lastUnsafe.ContainsKey(actor.InstanceID))
            _lastUnsafe[actor.InstanceID] = now;
        if ((now - _lastUnsafe[actor.InstanceID]).TotalSeconds < SettleSeconds)
            hints.MaxCastTime = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (caster.OID != (uint)OID.Fang)
            return;
        var aid = (AID)spell.Action.ID;
        if (aid == AID.VorpalTrailInitial)
        {
            SetRect(caster.InstanceID, caster.Position, spell.TargetXZ, 4f, 0f, 3f, WorldState.FutureTime(1.5f));
            return;
        }
        if (aid == AID.VorpalTrailSprint)
        {
            var dirAB = spell.TargetXZ - caster.Position;
            if (dirAB.LengthSq() < 0.01f)
                return;
            var b = spell.TargetXZ;
            var c = b + dirAB.OrthoR();
            SetRectInto(_predictedNext, caster.InstanceID, b, c, 2.5f, 2.5f, 3.0f, WorldState.FutureTime(NextSprintExpirationSec));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (caster.OID != (uint)OID.Fang)
            return;
        if ((AID)spell.Action.ID != AID.VorpalTrailSprint)
            return;
        _predictedNext.Remove(caster.InstanceID);
        SetRect(caster.InstanceID, caster.Position, spell.LocXZ, 2.5f, 2.5f, 3.0f, Module.CastFinishAt(spell, 1.0f));
    }

    private void SetRect(ulong fangId, WPos from, WPos to, float halfWidth, float frontExt, float backExt, DateTime expiration)
        => SetRectInto(_fangAOE, fangId, from, to, halfWidth, frontExt, backExt, expiration);

    private void SetRectInto(Dictionary<ulong, AOEInstance> dict, ulong fangId, WPos from, WPos to, float halfWidth, float frontExt, float backExt, DateTime expiration)
    {
        var diff = to - from;
        var length = diff.Length();
        if (length < 0.1f)
        {
            dict.Remove(fangId);
            return;
        }
        var mid = from + diff * 0.5f;
        var angle = Angle.FromDirection(diff);
        dict[fangId] = new AOEInstance(new AOEShapeRect(length * 0.5f + frontExt, halfWidth, length * 0.5f + backExt), mid, angle, expiration);
        var until = expiration.AddSeconds(3);
        if (until > _mechanicActiveUntil)
            _mechanicActiveUntil = until;
    }
}

class T02EverkeepStates : StateMachineBuilder
{
    private readonly T02Everkeep _module;

    public T02EverkeepStates(T02Everkeep module) : base(module)
    {
        _module = module;
        SimplePhase(0, Phase1, "P1")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || (Module.PrimaryActor.CastInfo?.IsSpell(AID.SoulOverflowEnrage) ?? false);
        SimplePhase(1, Phase2, "Enrage")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && (_module.BossP2()?.IsDead ?? false);
    }

    private void Phase1(uint id)
    {
        SimpleState(id, 10000, "Enrage")
            .ActivateOnEnter<SoulOverflow>()
            .ActivateOnEnter<SoulOverflowEnrage>()
            .ActivateOnEnter<PatricidalPique>()
            .ActivateOnEnter<CalamitysEdge>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<ShadowOfTural>()
            .ActivateOnEnter<VorpalTrail>()
            .ActivateOnEnter<DoubleEdgedSwords>();
    }

    private void Phase2(uint id)
    {
        SimpleState(id, 10000, "Enrage")
            .ActivateOnEnter<DawnOfAnAge>()
            .ActivateOnEnter<Actualize>()
            .ActivateOnEnter<ChasmOfVollok>()
            .ActivateOnEnter<DutysEdge>()
            .ActivateOnEnter<ForgedTrack>()
            .ActivateOnEnter<FireIII>()
            .ActivateOnEnter<HalfFull>()
            .ActivateOnEnter<BitterReaping>()
            .ActivateOnEnter<HalfCircuitRect>()
            .ActivateOnEnter<HalfCircuitDonut>()
            .ActivateOnEnter<HalfCircuitCircle>()
            .ActivateOnEnter<SmitingCircuitDonut>()
            .ActivateOnEnter<SmitingCircuitCircle>();
    }
}

[ModuleInfo(Contributors = "Gabriel Deleon, Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 995, NameID = 12881)]
public class T02Everkeep(ModuleInit init) : BossModule(init, new(100, 100), NormalBounds)
{
    public static readonly ArenaBoundsRect NormalBounds = new(20, 20, 45.Degrees());
    public static readonly ArenaBoundsRect SmallBounds = new(10, 10, 45.Degrees(), 20);

    public Actor? BossP2() => Enemies(OID.BossP2).FirstOrDefault(a => !a.IsDestroyed);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(BossP2(), ArenaColor.Enemy);
    }
}
