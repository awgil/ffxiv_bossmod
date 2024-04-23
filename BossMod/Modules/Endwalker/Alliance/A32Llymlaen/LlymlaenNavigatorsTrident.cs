namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class NavigatorsTridentRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.NavigatorsTridentRectAOE));

class NavigatorsTridentRectAOE(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeRect _shape = new(20, 5, 20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NavigatorsTridentRectAOE)
            _aoe = new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt);
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.NavigatorsTridentRectAOE)
        {
            _aoe = null;
            ++NumCasts;
        }
    }
}

class NavigatorsTridentKnockback(BossModule module) : Components.Knockback(module)
{
    private readonly List<Source> _sources = [];
    private static readonly AOEShapeCone _shape = new(30, 90.Degrees());

    public override IEnumerable<Source> Sources(int slot, Actor actor) => _sources;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NavigatorsTridentRectAOE)
        {
            _sources.Clear();
            // knockback rect always happens through center, so create two sources with origin at center looking orthogonally
            _sources.Add(new(Module.Bounds.Center, 20, spell.NPCFinishAt, _shape, spell.Rotation + 90.Degrees(), Kind.DirForward));
            _sources.Add(new(Module.Bounds.Center, 20, spell.NPCFinishAt, _shape, spell.Rotation - 90.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.NavigatorsTridentRectAOE)
        {
            _sources.Clear();
            ++NumCasts;
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<SerpentsTide>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.Bounds.Contains(pos);
}
