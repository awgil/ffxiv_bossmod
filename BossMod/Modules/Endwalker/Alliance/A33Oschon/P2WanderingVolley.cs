namespace BossMod.Endwalker.Alliance.A33Oschon;

class P2WanderingVolleyDownhill(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.WanderingVolleyDownhillAOE), 8);

class P2WanderingVolleyKnockback(BossModule module) : Components.Knockback(module)
{
    private readonly P2WanderingVolleyDownhill? _downhill = module.FindComponent<P2WanderingVolleyDownhill>();
    private readonly List<Source> _sources = [];
    private static readonly AOEShapeCone _shape = new(30, 90.Degrees());

    public override IEnumerable<Source> Sources(int slot, Actor actor) => _sources;
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos) || (_downhill?.ActiveAOEs(slot, actor).Any(z => z.Check(pos)) ?? false);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WanderingVolleyN or AID.WanderingVolleyS)
        {
            _sources.Clear();
            // happens through center, so create two sources with origin at center looking orthogonally
            _sources.Add(new(Module.Center, 12, spell.NPCFinishAt, _shape, spell.Rotation + 90.Degrees(), Kind.DirForward));
            _sources.Add(new(Module.Center, 12, spell.NPCFinishAt, _shape, spell.Rotation - 90.Degrees(), Kind.DirForward));
        }
    }
}

class P2WanderingVolleyAOE(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeRect _shape = new(40, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WanderingVolleyN or AID.WanderingVolleyS)
            _aoe = new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WanderingVolleyN or AID.WanderingVolleyS)
        {
            _aoe = null;
            ++NumCasts;
        }
    }
}
