namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class NavigatorsTridentRectAOE : Components.GenericAOEs
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(20, 5, 20);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NavigatorsTridentRectAOE)
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt.AddSeconds(7.3f)));
    }
    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NavigatorsTridentVisual1 or AID.NavigatorsTridentVisual2 or AID.NavigatorsTridentRectAOE)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}

class NavigatorsTridentKnockback : Components.Knockback
{
    private readonly List<Source> _sources = [];
    private static readonly AOEShapeCone _shape = new(30, 90.Degrees());

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor) => _sources;

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NavigatorsTridentRectAOE)
        {
            _sources.Clear();
            // charge always happens through center, so create two sources with origin at center looking orthogonally
            _sources.Add(new(module.Bounds.Center, 12, spell.NPCFinishAt, _shape, spell.Rotation + 90.Degrees(), Kind.DirForward));
            _sources.Add(new(module.Bounds.Center, 12, spell.NPCFinishAt, _shape, spell.Rotation - 90.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NavigatorsTridentVisual1 or AID.NavigatorsTridentVisual2 or AID.NavigatorsTridentRectAOE)
        {
            _sources.Clear();
            ++NumCasts;
        }
    }
}