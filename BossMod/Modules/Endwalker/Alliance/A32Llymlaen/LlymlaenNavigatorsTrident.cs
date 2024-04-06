namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class NavigatorsTridentRaidwide : Components.RaidwideCast
{
    public NavigatorsTridentRaidwide() : base(ActionID.MakeSpell(AID.NavigatorsTridentRectAOE)) { }
}

class NavigatorsTridentRectAOE : Components.GenericAOEs
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeRect _shape = new(20, 5, 20);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NavigatorsTridentRectAOE)
            _aoe = new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt);
    }
    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.NavigatorsTridentRectAOE)
        {
            _aoe = null;
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
            // knockback rect always happens through center, so create two sources with origin at center looking orthogonally
            _sources.Add(new(module.Bounds.Center, 20, spell.NPCFinishAt, _shape, spell.Rotation + 90.Degrees(), Kind.DirForward));
            _sources.Add(new(module.Bounds.Center, 20, spell.NPCFinishAt, _shape, spell.Rotation - 90.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.NavigatorsTridentRectAOE)
        {
            _sources.Clear();
            ++NumCasts;
        }
    }

    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => module.FindComponent<SerpentsTide>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false || !module.Bounds.Contains(pos);
}
