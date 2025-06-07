namespace BossMod.Dawntrail.Foray.ForkedTower.FT03MarbleDragon;

class ImitationRain : Components.RaidwideInstant
{
    public ImitationRain(BossModule module) : base(module, AID._Ability_ImitationRain1, 0)
    {
        Activation = WorldState.FutureTime(5);
    }
}

class ImitationBlizzard(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _puddles = [];

    private readonly List<(Actor Actor, DateTime Activation)> _lines = [];

    public static readonly AOEShape Circle = new AOEShapeCircle(20);
    public static readonly AOEShape Cross = new AOEShapeCross(60, 8);

    public bool Enabled;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_lines.Count == 0 || !Enabled)
            yield break;

        var firstActivation = _lines[0].Activation.AddSeconds(0.5f);
        foreach (var cur in _lines)
        {
            var shape = cur.Actor.OID == (uint)OID.IcePuddle ? Circle : Cross;

            if (cur.Activation < firstActivation)
                yield return new AOEInstance(shape, cur.Actor.Position, cur.Actor.Rotation, cur.Activation, ArenaColor.Danger);
            else if (cur.Activation < firstActivation.AddSeconds(1))
                yield return new AOEInstance(shape, cur.Actor.Position, cur.Actor.Rotation, cur.Activation);
            else
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_ImitationBlizzard or AID._Ability_ImitationBlizzard1)
        {
            NumCasts++;
            _lines.RemoveAll(l => l.Actor.Position.AlmostEqual(caster.Position, 0.5f));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.IcePuddle or OID.CrossPuddle)
            _puddles.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.IcePuddle or OID.CrossPuddle)
            _puddles.Remove(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_ImitationIcicle1)
        {
            var path = GetPath(spell.LocXZ);
            var nextActivation = Module.CastFinishAt(spell, 4.6f);
            foreach (var puddles in path)
            {
                foreach (var p in puddles)
                    _lines.Add((p, nextActivation));
                nextActivation = nextActivation.AddSeconds(1);
            }
            _lines.SortBy(l => l.Activation);
        }
    }

    private IEnumerable<(int, Actor)> PuddlesTouched(WPos origin) => _puddles.Select((p, i) => (i, p)).Where(p => p.p.Position.InCircle(origin, 16));

    private List<List<Actor>> GetPath(WPos origin)
    {
        List<List<Actor>> _path = [];
        var visited = new BitMask();
        List<WPos> sources = [origin];
        while (true)
        {
            var ps = sources.SelectMany(PuddlesTouched).ExcludedFromMask(visited).ToList();
            if (ps.Count > 0)
            {
                sources = [.. ps.Select(p => p.Item2.Position)];
                _path.Add([.. ps.Select(p => p.Item2)]);
                visited |= ps.Mask();
            }
            else
            {
                return _path;
            }
        }
    }
}
