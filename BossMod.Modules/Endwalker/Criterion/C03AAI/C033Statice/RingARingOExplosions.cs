namespace BossMod.Endwalker.Criterion.C03AAI.C033Statice;

class RingARingOExplosions(BossModule module) : Components.GenericAOEs(module)
{
    public List<Actor> ActiveBombs = [];
    public Angle? RelativeNorth;
    private readonly List<Actor> _bombs = [];
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(12);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => ActiveBombs.Select(b => new AOEInstance(_shape, b.Position, default, _activation));

    private static int Distance(Actor self, Actor other) => (int)float.Round((self.Position - other.Position).LengthSq());

    public override void Update()
    {
        if (_bombs.Count == 6 && ActiveBombs.Count == 0)
        {
            // Map the tethers
            Dictionary<Actor, Actor> incoming = []; // dest, source
            Dictionary<Actor, Actor> outgoing = []; // source, dest
            _bombs.ForEach(source =>
            {
                if (WorldState.Actors.Find(source.Tether.Target) is not { } target)
                    return;
                incoming[target] = source;
                outgoing[source] = target;
            });
            // Find our unsafe bombs
            ActiveBombs.AddRange(_bombs.Where(bomb => bomb.ModelState.AnimState1 == 1
                                                      || incoming.TryGetValue(bomb, out var source) && source.ModelState.AnimState1 == 1
                                                      || outgoing.TryGetValue(bomb, out var target) && target.ModelState.AnimState1 == 1));
            // Set the fuse timer if we found a lit bomb
            if (ActiveBombs.Count > 0)
                _activation = WorldState.FutureTime(17.4f);
            // Find relative north: the middle bomb (vertex angle) in the larger triangle
            var northBomb = _bombs.MaxBy(bomb =>
            {
                if (!incoming.TryGetValue(bomb, out var source) || !outgoing.TryGetValue(bomb, out var target))
                    return -1;
                var sideA = Distance(bomb, source);
                var sideB = Distance(bomb, target);
                var difference = Math.Abs(sideA - sideB);
                return difference != 0 ? -1 : sideA + sideB;
            });
            if (northBomb is not null)
                RelativeNorth = Angle.FromDirection(northBomb.Position - Module.Center);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (RelativeNorth is { } dir)
        {
            var origin = Module.Center + 24 * dir.ToDirection();
            var scale = 2;
            var innerAngle = 30.Degrees();
            Arena.AddTriangle(origin, origin - scale * (dir + innerAngle).ToDirection(), origin - scale * (dir - innerAngle).ToDirection(), ArenaColor.Enemy);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.NBomb or OID.SBomb)
            _bombs.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NBombBurst or AID.SBombBurst)
        {
            ++NumCasts;
            ActiveBombs.Remove(caster);
        }
    }
}
