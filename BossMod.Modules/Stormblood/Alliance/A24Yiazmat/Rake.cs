namespace BossMod.Stormblood.Alliance.A24Yiazmat;

class RakeLine(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(10))
{
    int TotalCastsLeft;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RakeLineFirst)
        {
            TotalCastsLeft = 10;
            AddLineFrom(spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RakeLineFirst && Lines.Count > 0)
        {
            AdvanceLine(Lines[0], Lines[0].Next);
            CleanupLines();
        }

        if ((AID)spell.Action.ID == AID.RakeLineRest && Lines.Count > 0)
        {
            AdvanceLine(Lines[0], spell.TargetXZ);
            Lines[0].Rotation = Module.PrimaryActor.Rotation;
            Lines[0].Advance = Module.PrimaryActor.Rotation.ToDirection() * 7.5f;
            Lines[0].Next = spell.TargetXZ + Lines[0].Advance;
            CleanupLines();
        }
    }

    void CleanupLines()
    {
        Lines.RemoveAll(l => l.ExplosionsLeft == 0 || !l.Next.InCircle(Arena.Center, Arena.Bounds.Radius));
    }

    void AddLineFrom(WPos start, Angle rotation, DateTime nextCast)
    {
        var dir = rotation.ToDirection();
        var maxCastsInbounds = (int)MathF.Floor(Intersect.RayCircle(start - Arena.Center, dir, Arena.Bounds.Radius) / 7.5f) + 1;
        var numExpected = Math.Min(Math.Min(maxCastsInbounds, 4), TotalCastsLeft);
        TotalCastsLeft -= numExpected;
        Lines.Add(new()
        {
            Next = start,
            Advance = dir * 7.5f,
            Rotation = rotation,
            NextExplosion = nextCast,
            TimeToMove = 1.1f,
            ExplosionsLeft = numExpected,
            MaxShownExplosions = 3
        });
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var diff = (IconID)iconID switch
        {
            IconID.TurnLeft => 120.Degrees(),
            IconID.TurnRight => -120.Degrees(),
            _ => default
        };

        if (diff != default)
        {
            var (pos, deadline, angle) = ImminentAOEs().Concat(FutureAOEs()).LastOrDefault();
            if (pos == default)
            {
                Service.Log($"hmm, I don't know what's going on here");
                return;
            }
            var bossDestination = pos - angle.ToDirection() * 10;
            var nextTurn = angle + diff;
            var nextRake = bossDestination + nextTurn.ToDirection() * 17.5f;
            AddLineFrom(nextRake, nextTurn, deadline.AddSeconds(10));
        }
    }
}
