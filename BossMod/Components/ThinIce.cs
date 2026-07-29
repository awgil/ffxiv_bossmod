namespace BossMod.Components;

// component for ThinIce mechanic
// observation: for SID 911 the distance is 0.1 * status extra
[SkipLocalsInit]
public abstract class ThinIce(BossModule module, float distance, bool createforbiddenzones = false, uint statusID = 911u, bool stopAtWall = false, bool stopAfterWall = false) : GenericKnockback(module, stopAtWall: stopAtWall, stopAfterWall: stopAfterWall)
{
    public readonly uint StatusID = statusID;
    public readonly float Distance = distance;
    private static readonly WDir offset = new(default, 1f);
    public BitMask Mask;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (Mask[slot])
        {
            return new Knockback[1] { new(actor.Position, Distance, default, default, MovementOverride.Instance!.LegacyMode ? Camera.Instance!.CameraAzimuth.Radians() + 180f.Degrees() : actor.Rotation, Kind.DirForward) };
        }
        return [];
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == StatusID)
        {
            Mask.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == StatusID)
        {
            Mask.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var movements = CalculateMovements(slot, actor);
        if (movements.Count != 0 && DestinationUnsafe(slot, actor, movements[0].to))
        {
            hints.Add("You are risking to slide into danger!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (Mask[pcSlot])
        {
            Arena.ZoneCircleOutline(pc.Position, Distance, Colors.Vulnerable);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (createforbiddenzones && Mask[slot])
        {
            var pos = actor.Position;
            var ddistance = 2f * Distance;
            var forbidden = new ShapeDistance[3]
            {
                new SDInvertedDonut(pos, Distance, Distance + 1.2f),
                new SDInvertedDonut(pos, ddistance, ddistance + 1.2f),
                new SDInvertedRect(pos, offset, 0.5f, 0.5f, 0.5f)
            };
            hints.AddForbiddenZone(new SDIntersection(forbidden), DateTime.MaxValue);
        }
    }
}
