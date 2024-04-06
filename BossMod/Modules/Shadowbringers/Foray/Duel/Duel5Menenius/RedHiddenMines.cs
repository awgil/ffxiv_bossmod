namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

class RedHiddenMines : Components.GenericAOEs
{
    private List<AOEInstance> _mines = new();
    private static readonly AOEShapeCircle _shapeTrigger = new(3.6f);
    private static readonly AOEShapeCircle _shapeExplosion = new(8f);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _mines;

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ActivateRedMine)
        {
            _mines.Add(new(_shapeTrigger, caster.Position, color: ArenaColor.Trap));
        }
        if ((AID)spell.Action.ID is AID.DetonateRedMine)
        {
            _mines.RemoveAll(t => t.Origin.AlmostEqual(caster.Position, 1));
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.IndiscriminateDetonation)
        {
            List<AOEInstance> _detonatingMines = new();
            for (int i = 0; i < _mines.Count; i++)
            {
                _detonatingMines.Add(new(_shapeExplosion, _mines[i].Origin, color: ArenaColor.AOE));
            }
            _mines = _detonatingMines;
        }
    }
}
