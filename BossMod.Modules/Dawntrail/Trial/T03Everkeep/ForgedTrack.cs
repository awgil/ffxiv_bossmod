namespace BossMod.Dawntrail.Trial.T03Everkeep;

// Forged Track: 4 intercardinal platforms spawn sword Fangs that telegraph narrow lane charges
// across the small arena. Each preview (37729, 11.6s, 0-hit) has an outer-platform caster; after
// the preview resolves, a separate inner Fang instant-casts 37730 for the actual damage along a
// parallel lane.
//
// The lane relationship between preview and damage:
// - NW-SE diagonal (rot +45° / -135°): damage lane coincides with preview lane (no offset).
// - NE-SW diagonal (rot -45° / +135°): damage lane is 5m perpendicular from preview lane. Offset
//   direction is determined per-fang by which lane the outer fang stands on.
//
// The four valid NE-SW lanes within the arena lie at perpendicular offsets of -7.5, -2.5, +2.5,
// +7.5 from arena center (each 5m wide, the arena is 20m across the diamond). They form two
// interleaved pairs: outer fangs spawn on one pair, inner damage fangs on the other. So the
// offset sign alternates between adjacent lanes — `floor(perp / 5)` parity gives the right sign.
class ForgedTrack(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Dictionary<ulong, AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // Preview entries linger past activation until the actual 37730 fires — auto-cull stale ones.
        var cutoff = WorldState.CurrentTime.AddSeconds(-0.5);
        foreach (var id in _aoes.Where(kv => kv.Value.Activation < cutoff).Select(kv => kv.Key).ToList())
            _aoes.Remove(id);
        return _aoes.Values;
    }

    private static readonly AOEShapeRect _shape = new(20f, 2.5f);
    private const float ForwardOffset = 30f;
    private const float PerpOffsetMagnitude = 5f;
    // Activation = predicted damage moment so the rect stays forbidden through the preview→damage gap.
    private const float DamageDelayAfterPreview = 1.4f;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.ForgedTrackPreview)
            return;

        var dir = spell.Rotation.ToDirection();
        // NE-SW diagonal: dir.X and dir.Z have opposite signs; NW-SE: same signs (no offset).
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
}
