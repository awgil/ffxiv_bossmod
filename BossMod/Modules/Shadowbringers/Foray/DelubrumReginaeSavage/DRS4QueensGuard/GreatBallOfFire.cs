namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

class GreatBallOfFire(BossModule module) : Components.GenericAOEs(module)
{
    private readonly IReadOnlyList<Actor> _smallFlames = module.Enemies(OID.RagingFlame);
    private readonly IReadOnlyList<Actor> _bigFlames = module.Enemies(OID.ImmolatingFlame);
    private readonly DateTime _activation = module.WorldState.FutureTime(6.6f);

    private static readonly AOEShapeCircle _shapeSmall = new(10);
    private static readonly AOEShapeCircle _shapeBig = new(18);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var f in _smallFlames)
            yield return new(_shapeSmall, f.Position, new(), f.CastInfo?.NPCFinishAt ?? _activation);
        foreach (var f in _bigFlames)
            yield return new(_shapeBig, f.Position, new(), f.CastInfo?.NPCFinishAt ?? _activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BurnSmall or AID.BurnBig)
            ++NumCasts;
    }
}
