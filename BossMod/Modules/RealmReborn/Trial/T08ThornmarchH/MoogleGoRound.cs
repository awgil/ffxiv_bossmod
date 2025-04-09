namespace BossMod.RealmReborn.Trial.T08ThornmarchH;

class MoogleGoRound(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _casters = [];
    private static readonly AOEShape _shape = new AOEShapeCircle(20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _casters.Take(2).Select(c => new AOEInstance(_shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo!)));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // if there is a third cast, add a smaller shape to ensure people stay closer to eventual safespot
        if (_casters.Count > 2)
        {
            var f1 = ShapeContains.InvertedCircle(_casters[0].Position, 23);
            var f2 = ShapeContains.Circle(_casters[2].Position, 10);
            hints.AddForbiddenZone(p => f1(p) || f2(p), Module.CastFinishAt(_casters[1].CastInfo!));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MoogleGoRoundBoss or AID.MoogleGoRoundAdd)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MoogleGoRoundBoss or AID.MoogleGoRoundAdd)
            _casters.Remove(caster);
    }
}
