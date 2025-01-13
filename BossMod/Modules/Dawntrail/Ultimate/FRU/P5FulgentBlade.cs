namespace BossMod.Dawntrail.Ultimate.FRU;

class P5FulgentBlade : Components.Exaflare
{
    private readonly List<Actor> _lines = [];
    private DateTime _nextBundle;

    public P5FulgentBlade(BossModule module) : base(module, new AOEShapeRect(5, 40))
    {
        ImminentColor = ArenaColor.AOE;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        //// debug
        //foreach (var l in _lines)
        //{
        //    var d = (Module.Center - l.Position).Normalized().OrthoL();
        //    Arena.AddLine(l.Position - 50 * d, l.Position + 50 * d, ArenaColor.Object);
        //}

        var safespot = SafeSpots().FirstOrDefault();
        if (safespot != default)
            Arena.AddCircle(safespot, 1, ArenaColor.Safe);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.FulgentBladeLine)
        {
            _lines.Add(actor);
            //if (_lines.Count == 6)
            //    _lines.SortByReverse(l => (l.Position - Module.Center).LengthSq()); // TODO: this isn't right, there are lines with same distance...
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.PathOfLightFirst or AID.PathOfDarknessFirst)
        {
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

    private IEnumerable<WPos> SafeSpots()
    {
        if (_lines.Count != 6)
            yield break;
        //if (NumCasts < 2)
        //    yield return SafeSpot(_lines[0], _lines[1]);
        //if (NumCasts < 4)
        //    yield return SafeSpot(_lines[2], _lines[3]);
        //if (NumCasts < 6)
        //    yield return SafeSpot(_lines[4], _lines[5]);

        if (NumCasts == 0)
        {
            WDir avgOff = default;
            foreach (var l in _lines)
                avgOff += (l.Position - Module.Center).Normalized();
            yield return Module.Center - 5 * avgOff;
        }
    }

    //private WPos SafeSpot(Actor line1, Actor line2)
    //{
    //    var d1 = (Module.Center - line1.Position).Normalized();
    //    var d2 = (Module.Center - line2.Position).Normalized();
    //    var n1 = d1.OrthoL();
    //    var n2 = d2.OrthoL();
    //    var p1 = line1.Position + 11 * d1 - 50 * n1;
    //    var p2 = line2.Position + 11 * d2 - 50 * n2;
    //    var t = Intersect.RayLine(p1, n1, p2, n2);
    //    return t is > 0 and < 100 ? p1 + t * n1 : default;
    //}
}
