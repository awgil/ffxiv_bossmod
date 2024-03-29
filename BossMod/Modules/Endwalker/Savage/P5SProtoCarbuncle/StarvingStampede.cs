namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

class StarvingStampede : Components.GenericAOEs
{
    private List<WPos> _positions = new();

    private static readonly AOEShape _shape = new AOEShapeCircle(12);

    public StarvingStampede() : base(ActionID.MakeSpell(AID.StarvingStampede)) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        // TODO: timings...
        return _positions.Skip(NumCasts).Take(3).Select(p => new AOEInstance(_shape, p));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(module, caster, spell);
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
