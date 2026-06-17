namespace BossMod.Dawntrail.Trial.T03Everkeep;

// Two flavors of Chasm of Vollok exist:
// - Single-cast (Mode A): 37720 cells spawn inside the arena and ARE the damage; 37722 doesn't
//   fire. We just render the 37720 cell with its 6.7s cast as the warning.
// - Telegraphed (Mode B): 37720 cells spawn on the OUTER ring (visual only, no damage), then
//   37722 fires 6s later on different cells inside the arena. The user only gets ~0.7s warning
//   from 37722 alone, which the AI cannot react to in time.
//
// In Mode B the outer 37720 cell maps deterministically to its inner 37722 damage cell: shift
// ~21.21m (= 30/√2, three 5m cells along the rotated grid) toward the arena center along both
// world X and Z axes. We render the predicted damage cell with 6.7s warning so the AI has time
// to reposition. When 37722 actually fires, its rect also renders (final-moment reminder,
// redundant with the predicted one).
//
// Origin-at-back-corner shape (5f forward + 0 back): cast targets sit at the back corner of each
// 5×5 cell, not the center, so we extend the rect 5 forward from its origin.
class ChasmOfVollok(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Dictionary<ulong, AOEInstance> _previewAoes = [];
    private readonly Dictionary<ulong, AOEInstance> _damageAoes = [];

    private static readonly AOEShapeRect _shape = new(5f, 2.5f);
    private const float ArenaRadius = 15f; // cells beyond this are outer-ring previews (Mode B)
    private const float TranslateMagnitude = 21.21f; // 15√2; shift toward center per world axis

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
