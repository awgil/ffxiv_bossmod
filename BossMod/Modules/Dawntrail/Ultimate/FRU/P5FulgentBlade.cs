namespace BossMod.Dawntrail.Ultimate.FRU;

class P5FulgentBlade : Components.Exaflare
{
    private readonly List<(Actor actor, WDir dir)> _lines = []; // before first line starts, it is sorted either in correct or reversed order - i don't think we can predict it?..
    private WDir _initialSafespot;
    private DateTime _nextBundle;

    public P5FulgentBlade(BossModule module) : base(module, new AOEShapeRect(5, 40))
    {
        ImminentColor = ArenaColor.AOE;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var safespot = SafeSpots().FirstOrDefault();
        if (safespot != default)
            Arena.AddCircle(safespot, 1, ArenaColor.Safe);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.FulgentBladeLine)
        {
            var dir = (actor.Position - Module.Center).Normalized();
            _lines.Add((actor, dir));
            _initialSafespot -= dir;
            if (_lines.Count == 6)
            {
                // sort in arbitrary order (say, CW), until we know better
                _lines.SortBy(l => l.dir.Cross(_initialSafespot));
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.PathOfLightFirst or AID.PathOfDarknessFirst)
        {
            if (Lines.Count == 0)
                UpdateOrder(caster.Position); // first line - we should have all 6 line actors already created, and it should match position of first or last two

            var dir = spell.Rotation.ToDirection();
            var distanceToBorder = Intersect.RayCircle(caster.Position - Module.Center, dir, 22);
            Lines.Add(new() { Next = caster.Position, Advance = 5 * dir, Rotation = spell.Rotation, NextExplosion = Module.CastFinishAt(spell), TimeToMove = 2, ExplosionsLeft = (int)(distanceToBorder / 5) + 1, MaxShownExplosions = 1 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PathOfLightFirst or AID.PathOfDarknessFirst or AID.PathOfLightRest or AID.PathOfDarknessRest)
        {
            if (WorldState.CurrentTime > _nextBundle)
            {
                ++NumCasts;
                _nextBundle = WorldState.FutureTime(1);
            }

            int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1) && item.Rotation.AlmostEqual(spell.Rotation, 0.1f));
            if (index == -1)
            {
                ReportError($"Failed to find entry for {caster.InstanceID:X}");
                return;
            }

            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }

    private void UpdateOrder(WPos firstPos)
    {
        if (_lines.Count != 6)
        {
            ReportError($"Unexpected number of lines at cast start: {_lines.Count}");
        }
        else if (_lines[^1].actor.Position.AlmostEqual(firstPos, 1) || _lines[^2].actor.Position.AlmostEqual(firstPos, 1))
        {
            _lines.Reverse(); // we guessed incorrectly, update the order
        }
        else if (!_lines[0].actor.Position.AlmostEqual(firstPos, 1) && !_lines[1].actor.Position.AlmostEqual(firstPos, 1))
        {
            ReportError($"First cast at {firstPos} does not correspond to any of the first/last two lines");
        }
    }

    private IEnumerable<WPos> SafeSpots()
    {
        if (_lines.Count != 6)
            yield break; // not enough data

        if (Lines.Count == 0)
        {
            // we don't yet know the direction, so just give the approximate safespot (average direction of missing lines)
            yield return Module.Center + 5 * _initialSafespot;
            yield break;
        }

        if (NumCasts < 2)
            yield return SafeSpot(0, 1, 11); // prepare to dodge into third exaline of the first pair
        if (NumCasts < 3)
            yield return SafeSpot(0, 1, 9); // dodge into third exaline of the first pair
        if (NumCasts < 4)
            yield return SafeSpot(2, 3, 11); // prepare to dodge into third exaline of the second pair
        if (NumCasts < 5)
            yield return SafeSpot(2, 3, 9); // dodge into third exaline of the second pair
        if (NumCasts < 6)
            yield return SafeSpot(4, 5, 11); // prepare to dodge into third exaline of the third pair
        yield return SafeSpot(4, 5, 9); // dodge into third exaline of the third pair
    }

    private WPos SafeSpot(int i1, int i2, float distance)
    {
        var l1 = _lines[i1];
        var l2 = _lines[i2];
        var p1 = l1.actor.Position - distance * l1.dir;
        var p2 = l2.actor.Position - distance * l2.dir;
        var d1 = l1.dir.OrthoL();
        var d2 = l2.dir.OrthoL();
        p1 -= 50 * d1; // rays are 0 to infinity, oh well
        p2 -= 50 * d2;
        var t = Intersect.RayLine(p1, d1, p2, d2);
        return t is > 0 and < 100 ? p1 + t * d1 : default;
    }
}
