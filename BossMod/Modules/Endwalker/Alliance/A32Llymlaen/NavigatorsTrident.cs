namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class DireStraits(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(40, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DireStraitsAOEFirst or AID.DireStraitsAOESecond)
        {
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DireStraitsAOEFirst or AID.DireStraitsAOESecond)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}

class NavigatorsTridentAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.NavigatorsTridentAOE), new AOEShapeRect(20, 5, 20));

class NavigatorsTridentKnockback(BossModule module) : Components.Knockback(module)
{
    private readonly SerpentsTide? _serpentsTide = module.FindComponent<SerpentsTide>();
    private readonly List<Source> _sources = [];

    private static readonly AOEShapeCone _shape = new(30, 90.Degrees());

    public override IEnumerable<Source> Sources(int slot, Actor actor) => _sources;
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos) || (_serpentsTide?.AOEs.Any(z => z.Check(pos)) ?? false);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.NavigatorsTridentAOE)
        {
            _sources.Clear();
            _sources.Add(new(caster.Position, 20, spell.NPCFinishAt, _shape, spell.Rotation + 90.Degrees(), Kind.DirForward));
            _sources.Add(new(caster.Position, 20, spell.NPCFinishAt, _shape, spell.Rotation - 90.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.NavigatorsTridentAOE)
        {
            _sources.Clear();
            ++NumCasts;
        }
    }
}
