namespace BossMod.Endwalker.Unreal.Un5Thordan;

class DragonsGaze(BossModule module) : Components.GenericGaze(module)
{
    private readonly List<Actor> _casters = [];
    private WPos _posHint;

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor) => _casters.Select(c => new Eye(c.Position, c.CastInfo!.NPCFinishAt));

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (_posHint != default)
            Arena.AddCircle(_posHint, 1, ArenaColor.Safe);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DragonsGaze or AID.DragonsGlory)
        {
            _casters.Add(caster);
            _posHint = default;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DragonsGaze or AID.DragonsGlory)
            _casters.Remove(caster);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008 && (OID)actor.OID is >= OID.DragonEyeN and <= OID.DragonEyeNW)
        {
            var index = actor.OID - (uint)OID.DragonEyeN; // 0 = N, then CW
            _posHint = Module.Center + 19 * (180 - (int)index * 45).Degrees().ToDirection();
        }
    }
}
