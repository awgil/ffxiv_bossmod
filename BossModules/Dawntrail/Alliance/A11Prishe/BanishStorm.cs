namespace BossMod.Dawntrail.Alliance.A11Prishe;

class BanishStorm(BossModule module) : Components.Exaflare(module, 6)
{
    public bool Done;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is < 2 or > 13 and < 50 or > 53)
            return;
        switch (state)
        {
            case 0x00020001: // rod appear
                Done = false;
                var (offset, rot) = index switch
                {
                    // these are for triple exaflares
                    2 => (new(-15, -15), 180.Degrees()),
                    3 => (new(+15, 0), 90.Degrees()),
                    4 => (new(0, +15), 180.Degrees()),
                    5 => (new(-15, -15), 0.Degrees()),
                    6 => (new(+15, 0), -90.Degrees()),
                    7 => (new(0, +15), 0.Degrees()),
                    8 => (new(0, -15), 0.Degrees()),
                    9 => (new(-15, 0), -90.Degrees()),
                    10 => (new(+15, +15), 0.Degrees()),
                    11 => (new(0, -15), 180.Degrees()),
                    12 => (new(-15, 0), 90.Degrees()),
                    13 => (new(+15, +15), 180.Degrees()),
                    // these are for double exaflares
                    50 => (new(-15, -15), 180.Degrees()),
                    51 => (new(0, +15), 0.Degrees()),
                    52 => (new(+15, +15), 0.Degrees()),
                    53 => (new(0, -15), 180.Degrees()),
                    _ => (new WDir(), 0.Degrees())
                };
                AddLine(offset, rot);
                AddLine(offset, rot + 120.Degrees());
                AddLine(offset, rot - 120.Degrees());
                break;
            case 0x00080004: // rod disappear
                Done = true;
                //foreach (var l in Lines)
                //    ReportError($"Unfinished line at {l.Next}, {l.ExplosionsLeft} left, next in {(l.NextExplosion - WorldState.CurrentTime).TotalSeconds:f3}s");
                //Lines.Clear();
                break;
            default:
                // 0x00200010 - aoe direction indicator appear
                // 0x00800040 - aoe direction indicator disappear
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Banish)
        {
            ++NumCasts;
            for (int i = 0; i < Lines.Count; ++i)
            {
                if (!Lines[i].Next.AlmostEqual(caster.Position, 1))
                    continue;
                AdvanceLine(Lines[i], caster.Position);
                if (Lines[i].ExplosionsLeft == 0)
                    Lines.RemoveAt(i--);
            }
        }
    }

    private void AddLine(WDir origin, Angle rotation)
    {
        var dir = rotation.ToDirection();
        var count = (int)((Module.Bounds.IntersectRay(origin, dir) - 1) / 4) + 1;
        Lines.Add(new() { Next = Module.Center + origin, Advance = 4 * dir, Rotation = rotation, NextExplosion = WorldState.FutureTime(9.1f), TimeToMove = 0.8f, ExplosionsLeft = count, MaxShownExplosions = 100 });
    }
}
