namespace BossMod.Endwalker.Ultimate.DSW2;

class P6HotWingTail(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shapeWing = new(50, 10.5f);
    private static readonly AOEShapeRect _shapeTail = new(50, 8);

    public int NumAOEs => _aoes.Count; // 0 if not started, 1 if tail, 2 if wings

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Skip(NumCasts);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.HotWingAOE => _shapeWing,
            AID.HotTailAOE => _shapeTail,
            _ => null
        };
        if (shape != null)
            _aoes.Add(new(shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
    }

    // note: we don't remove aoe's, since that is used e.g. by spreads component
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HotWingAOE or AID.HotTailAOE)
            ++NumCasts;
    }
}
