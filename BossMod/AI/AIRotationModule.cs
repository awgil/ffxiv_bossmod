using BossMod.Autorotation;

namespace BossMod.AI;

public abstract class AIRotationModule(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    protected float Deadline(DateTime deadline) => Math.Max(0, (float)(deadline - Manager.WorldState.CurrentTime).TotalSeconds);
    protected float Speed() => Player.FindStatus(50) != null ? 7.8f : 6;

    protected void SetForcedMovement(WPos? pos, float tolerance = 0.1f)
    {
        var dir = (pos ?? Player.Position) - Player.Position;
        Hints.ForcedMovement = dir.LengthSq() > tolerance * tolerance ? new(dir.X, Player.PosRot.Y, dir.Z) : default;
    }

    protected WPos ClosestInRange(WPos pos, WPos target, float maxRange)
    {
        var toPlayer = pos - target;
        var range = toPlayer.Length();
        return range <= maxRange ? pos : target + maxRange * toPlayer.Normalized();
    }

    protected WPos ClosestInMelee(WPos pos, Actor target, float tolerance = 0.25f) => ClosestInRange(pos, target.Position, target.HitboxRadius + 3 - tolerance);

    // return uptime position if player will be able to reach downtime position within deadline having started movement after next action, otherwise downtime position
    protected WPos UptimeDowntimePos(WPos uptimePos, WPos downtimePos, float nextAction, float deadline)
    {
        var timeToSafety = (uptimePos - downtimePos).Length() / Speed();
        return nextAction + timeToSafety < deadline ? uptimePos : downtimePos;
    }

    // return position that will make the target (assumed aggro'd) move to specified position, ensuring we're back in melee range by the deadline
    protected WPos MoveTarget(Actor target, WPos desired, float nextAction, float targetMeleeRange = 2)
    {
        var dir = desired - target.Position;
        if (dir.LengthSq() < 0.01f)
            return ClosestInRange(Player.Position, target.Position, target.HitboxRadius + targetMeleeRange - 0.1f);
        dir = dir.Normalized();
        var ideal = desired + dir * (target.HitboxRadius + targetMeleeRange); // if we just stay here, boss should go to the desired position
        var targetMoveDir = (ideal - target.Position).Normalized();
        var playerDotTargetMove = targetMoveDir.Dot(ideal - Player.Position);
        if (playerDotTargetMove < 0)
            ideal -= playerDotTargetMove * targetMoveDir; // don't move towards boss, though
        var targetRemaining = (ideal - target.Position).Length() - target.HitboxRadius - targetMeleeRange - (target.Position - target.PrevPosition).Length() / Manager.WorldState.Frame.Duration * nextAction - Speed() * nextAction;
        if (targetRemaining > 0)
            ideal += targetRemaining * (target.Position - ideal).Normalized();
        return ideal;
    }
}
