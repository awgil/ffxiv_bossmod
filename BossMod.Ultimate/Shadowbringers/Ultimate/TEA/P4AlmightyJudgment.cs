namespace BossMod.Shadowbringers.Ultimate.TEA;

// note: sets are 2s apart, 8-9 casts per set
class P4AlmightyJudgment(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos pos, DateTime activation)> _casters = [];

    private static readonly AOEShapeCircle _shape = new(6);

    public bool Active => _casters.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_casters.Count > 0)
        {
            var deadlineImminent = _casters[0].activation.AddSeconds(1);
            var deadlineFuture = _casters[0].activation.AddSeconds(3);
            foreach (var c in Enumerable.Reverse(_casters).SkipWhile(c => c.activation > deadlineFuture))
            {
                yield return new(_shape, c.pos, default, c.activation, c.activation < deadlineImminent ? ArenaColor.Danger : ArenaColor.AOE);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AlmightyJudgmentVisual)
            _casters.Add((caster.Position, WorldState.FutureTime(8)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AlmightyJudgmentAOE)
            _casters.RemoveAll(c => c.pos.AlmostEqual(caster.Position, 1));
    }
}
