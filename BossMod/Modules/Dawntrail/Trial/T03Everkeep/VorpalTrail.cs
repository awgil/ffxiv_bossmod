namespace BossMod.Dawntrail.Trial.T03Everkeep;

// Each 42AA Fang does a sequence of sprints (5 inward + 5 outward = 10 segments per fang); the
// rectangle AOE is the line segment each sprint covers, from the fang's current position to the
// sprint endpoint (carried in the cast's TargetPos / LocXZ).
//
// - Initial sprint (VorpalTrailInitial / 38183): instant cast fired when the sprint begins from
//   the arena-edge spawn. Extend rect back by 3 so it reaches the diamond corner.
// - Subsequent sprints (VorpalTrailSprint / 37711): 0.7s cast fired while the fang is stationary
//   at the current waypoint. Rect spans interior waypoints with no back extension.
//
// One rect per fang is tracked; a new sprint cast replaces the previous rect, and rects auto-expire
// shortly after the sprint would complete.
class VorpalTrail(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Dictionary<ulong, AOEInstance> _fangAOE = [];
    private readonly Dictionary<ulong, AOEInstance> _predictedNext = []; // sprint N+1, painted at sprint N's resolution to give the AI extra lead time
    private readonly Dictionary<ulong, DateTime> _lastUnsafe = [];
    private DateTime _mechanicActiveUntil;
    private const float SettleSeconds = 4f; // require this much continuous safety before allowing casts
    private const float NextSprintExpirationSec = 5f; // covers dash → arrival → next-cast window; the precise rect overwrites once sprint N+1's CST+ fires
    private const float CenterAttractRadius = 0.1f; // hold-position tolerance for the center goal zone
    private const float CenterAttractWeight = 10f; // strong attractor; rect forbids still override when a sprint crosses center
    private const float AIRectHalfWidth = 3.5f; // AI-only padding beyond the 2.5f visible halfwidth so the AI commits to leaving instead of skimming the edge

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
        // Skipping base.AddAIHints here — we replace its rect-based forbidden zones with widened
        // AI-only versions below so the visual stays at 2.5f while the pathfinder treats 3.5f as forbidden.
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
                hints.AddForbiddenZone(aoe.Check, aoe.Activation);
            }
        }
        // Inverted strategy: park the AI at arena center and let rect avoidance pull it off when a
        // sprint actually crosses through. Most pinwheel rects miss exact center between converging
        // dashes, so holding station beats dancing around the perimeter trying to read the next sprint.
        hints.GoalZones.Add(hints.GoalSingleTarget(Module.Center, CenterAttractRadius, CenterAttractWeight));

        var inDanger = _fangAOE.Values.Any(a => a.Check(actor.Position)) || _predictedNext.Values.Any(a => a.Check(actor.Position));
        if (inDanger || !_lastUnsafe.ContainsKey(actor.InstanceID))
            _lastUnsafe[actor.InstanceID] = now;
        // Suppress casts (incl. instants) until the player has held a safe position long enough —
        // the pinwheel cycles fast and a 1.5s GCD that locks movement is enough to clip the next sprint.
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
            // Sprint N just resolved: fang dashes from caster.Position (A) to spell.TargetXZ (B).
            // Pinwheel is deterministic — sprint N+1 rotates 90° CW with the same dash length.
            // Pre-paint that rect now; sprint N+1's CST+ will overwrite the prediction with precise
            // geometry once the fang stops at B and starts casting again.
            var dirAB = spell.TargetXZ - caster.Position;
            if (dirAB.LengthSq() < 0.01f)
                return;
            var b = spell.TargetXZ;
            var c = b + dirAB.OrthoR(); // OrthoR = (-Z, X) = 90° CW; preserves length
            SetRectInto(_predictedNext, caster.InstanceID, b, c, 2.5f, 2.5f, 3.0f, WorldState.FutureTime(NextSprintExpirationSec));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (caster.OID != (uint)OID.Fang)
            return;
        if ((AID)spell.Action.ID != AID.VorpalTrailSprint)
            return;
        // Precise rect supersedes any prediction we put in _predictedNext for this fang.
        _predictedNext.Remove(caster.InstanceID);
        // Back extension reaches the 233C helper sitting 2 units behind the fang's sprint-start position.
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
        // Bridge the ~0.7s gaps between sprints + a tail past the final sprint so cast suppression
        // doesn't blink off mid-pinwheel just because the active rect just expired.
        var until = expiration.AddSeconds(3);
        if (until > _mechanicActiveUntil)
            _mechanicActiveUntil = until;
    }
}
