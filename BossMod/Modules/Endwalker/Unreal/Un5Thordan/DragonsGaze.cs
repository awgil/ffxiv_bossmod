namespace BossMod.Endwalker.Unreal.Un5Thordan;

class DragonsGaze : Components.GenericGaze
{
    private List<Actor> _casters = new();
    private WPos _posHint;

    public override IEnumerable<Eye> ActiveEyes(BossModule module, int slot, Actor actor) => _casters.Select(c => new Eye(c.Position, c.CastInfo!.NPCFinishAt));

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        base.DrawArenaForeground(module, pcSlot, pc, arena);
        if (_posHint != default)
            arena.AddCircle(_posHint, 1, ArenaColor.Safe);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DragonsGaze or AID.DragonsGlory)
        {
            _casters.Add(caster);
            _posHint = default;
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DragonsGaze or AID.DragonsGlory)
            _casters.Remove(caster);
    }

    public override void OnActorEAnim(BossModule module, Actor actor, uint state)
    {
        if (state == 0x00040008 && (OID)actor.OID is >= OID.DragonEyeN and <= OID.DragonEyeNW)
        {
            var index = actor.OID - (uint)OID.DragonEyeN; // 0 = N, then CW
            _posHint = module.Bounds.Center + 19 * (180 - (int)index * 45).Degrees().ToDirection();
        }
    }
}
