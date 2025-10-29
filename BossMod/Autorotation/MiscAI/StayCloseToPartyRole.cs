using System.Globalization;

namespace BossMod.Autorotation.MiscAI;

public sealed class StayCloseToPartyRole(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Tracks
    {
        Role,
        Range
    }

    public enum RangeDefinition
    {
        OnHitbox
    }

    public static RotationModuleDefinition Definition()
    {
        RotationModuleDefinition def = new("Misc AI: Stay within range of party role", "Module for use by AutoDuty preset.", "AI", "erdelf", RotationModuleQuality.Basic, new(~0ul), 1000);

        var roleRef = def.Define(Tracks.Role).As<Role>("Role", "Role to stay close to");

        foreach (var role in Enum.GetValues<Role>())
        {
            roleRef.AddOption(role, role.ToString());
        }

        var rangeRef = def.Define(Tracks.Range).As<RangeDefinition>("range");

        rangeRef.AddOption(RangeDefinition.OnHitbox, "OnHitbox", "Stay on edge of hitbox (+/- 1 unit)");
        for (var f = 1.1f; f <= 30f; f = MathF.Round(f + 0.1f, 1))
        {
            rangeRef.AddOption((RangeDefinition)(f * 10f - 10f), f.ToString(CultureInfo.InvariantCulture));
        }

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var role = strategy.Option(Tracks.Role).As<Role>();
        if (role != Role.None && role != Manager.Player?.Role)
        {
            var roleActor = World.Party.WithoutSlot(false, true).FirstOrDefault(a => a.Role == role);
            if (roleActor != null)
            {
                var position = roleActor.Position;
                var radius = roleActor.HitboxRadius;
                var range = strategy.Option(Tracks.Range);
                if (range.As<RangeDefinition>() == RangeDefinition.OnHitbox)
                    Hints.GoalZones.Add(p => p.InDonut(position, radius - 1, radius + 1) ? 0.5f : 0);
                else
                    Hints.GoalZones.Add(Hints.GoalSingleTarget(position, (range.Value.Option + 10f) / 10f + roleActor.HitboxRadius, 1f));
            }
        }
    }
}
