namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

// state related to exoterikos, trimorphos exoterikos and triple esoteric ray mechanics
class Exoterikos(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor, AOEShape)> _sources = [];

    private static readonly AOEShapeRect _aoeSquare = new(21, 21);
    private static readonly AOEShapeCone _aoeTriangle = new(47, 30.Degrees());
    private static readonly AOEShapeRect _aoeRay = new(42, 7);

    public bool Done => _sources.Count == 0;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveSources().Any(actShape => actShape.Item2.Check(actor.Position, actShape.Item1)))
            hints.Add("GTFO from exo aoe!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (src, shape) in ActiveSources())
            shape.Draw(Arena, src);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        // tethers appear much earlier than cast start, but not for all sigils
        var target = WorldState.Actors.Find(tether.Target);
        if (source != Module.PrimaryActor || target == null)
            return;

        var shape = ShapeForSigil(target);
        if (shape != null)
            _sources.Add((target, shape));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForSigil(caster);
        if (shape != null && !_sources.Any(actShape => actShape.Item1 == caster))
            _sources.Add((caster, shape));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        _sources.RemoveAll(actShape => actShape.Item1 == caster);
    }

    private AOEShape? ShapeForSigil(Actor sigil)
    {
        return (OID)sigil.OID switch
        {
            OID.ExoSquare => _aoeSquare,
            OID.ExoTri => _aoeTriangle,
            OID.ExoGreen => _aoeRay,
            _ => null
        };
    }

    private IEnumerable<(Actor, AOEShape)> ActiveSources()
    {
        bool hadSideSquare = false; // we don't show multiple side-squares, since that would cover whole arena and be useless
        DateTime lastRay = new(); // we only show first rays, otherwise triple rays would cover whole arena and be useless
        foreach (var (actor, shape) in _sources)
        {
            if (shape == _aoeSquare && MathF.Abs(actor.Position.X - Module.Center.X) > 10)
            {
                if (hadSideSquare)
                    continue;
                hadSideSquare = true;
            }
            else if (shape == _aoeRay)
            {
                if (lastRay != default && (actor.CastInfo == null || (actor.CastInfo.NPCFinishAt - lastRay).TotalSeconds > 2))
                    continue;
                lastRay = actor.CastInfo?.NPCFinishAt ?? new();
            }
            yield return (actor, shape);
        }
    }
}
