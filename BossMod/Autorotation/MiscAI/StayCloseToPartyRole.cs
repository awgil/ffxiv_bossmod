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
            roleRef.AddOption(role);

        def.DefineFloat(Tracks.Range, "Range", minValue: 1.1f, maxValue: 30f);

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
                var range = strategy.GetFloat(Tracks.Range);
                Hints.GoalZones.Add(Hints.GoalSingleTarget(position, range + roleActor.HitboxRadius, 1f));
            }
        }
    }
}
