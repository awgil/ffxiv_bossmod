namespace BossMod.Endwalker.Alliance.A33Oschon;

class WanderingVolley(BossModule module) : Components.Knockback(module)
{
    private readonly List<Source> _sources = [];
    private static readonly AOEShapeCone _shape = new(30, 90.Degrees());

    public override IEnumerable<Source> Sources(int slot, Actor actor) => _sources;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WanderingVolley or AID.WanderingVolley2)
        {
            _sources.Clear();
            // happens through center, so create two sources with origin at center looking orthogonally
            _sources.Add(new(Module.Bounds.Center, 12, spell.NPCFinishAt, _shape, spell.Rotation + 90.Degrees(), Kind.DirForward));
            _sources.Add(new(Module.Bounds.Center, 12, spell.NPCFinishAt, _shape, spell.Rotation - 90.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WanderingVolley or AID.WanderingVolley2)
        {
            _sources.Clear();
            ++NumCasts;
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<DownhillBig>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.Bounds.Contains(pos);
}

class WanderingVolleyAOE(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeRect _shape = new(40, 5, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WanderingVolley or AID.WanderingVolley2)
            _aoe = new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WanderingVolley or AID.WanderingVolley2)
        {
            _aoe = null;
            ++NumCasts;
        }
    }
}

class WanderingVolleyRaidwide1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WanderingVolley));
class WanderingVolleyRaidwide2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WanderingVolley2));
