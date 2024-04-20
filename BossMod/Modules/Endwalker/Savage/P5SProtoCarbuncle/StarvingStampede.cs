namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

class StarvingStampede(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.StarvingStampede))
{
    private readonly List<WPos> _positions = [];

    private static readonly AOEShape _shape = new AOEShapeCircle(12);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // TODO: timings...
        return _positions.Skip(NumCasts).Take(3).Select(p => new AOEInstance(_shape, p));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        switch ((AID)spell.Action.ID)
        {
            case AID.JawsTeleport:
                if (_positions.Count == 0)
                    _positions.Add(caster.Position);
                _positions.Add(new(spell.TargetPos.XZ()));
                break;
        }
    }
}
