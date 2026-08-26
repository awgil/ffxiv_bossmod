using BossMod.Autorotation.xan;
using BossMod.SAM;

namespace BossMod.Autorotation.Standard.xan.Utility;

public class ThirdEye(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID, ThirdEye.Strategy>(manager, player)
{
    public struct Strategy
    {
        [Track(Actions = [AID.ThirdEye, AID.Tengentsu])]
        public Track<ThirdEyeStrategy> ThirdEye;
    }

    public enum ThirdEyeStrategy
    {
        [Option("Use ~3s before predicted damage", Cooldown = 15, Effect = 4)]
        Automatic,
        [Option("Use 4s before predicted damage", Cooldown = 15, Effect = 4)]
        AutoMax,
        [Option("Don't use")]
        Delay
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Auto-ThirdEye", "Third Eye before incoming damage", "Utility (xan)", "xan", RotationModuleQuality.Excellent, BitMask.Build(Class.SAM), 100, 6).WithStrategies<Strategy>();
    }

    public override void Exec(in Strategy strategy, AIHints.Enemy? primaryTarget)
    {
        if (Player.FindStatus(SID.Meditate) != null)
            return;

        var advance = strategy.ThirdEye.Value switch
        {
            ThirdEyeStrategy.Automatic => 3,
            ThirdEyeStrategy.AutoMax => 4,
            _ => 0
        };

        if (advance > 0 && Hints.PredictedDamage.Any(x => x.Players[0] && x.Activation < World.FutureTime(advance)))
            PushOGCD(AID.ThirdEye, Player, -100);

        if (Hints.PotentialTargets.Any(t => t.Actor.TargetID == Player.InstanceID && t.Actor.CastInfo == null && t.Actor.DistanceToHitbox(Player) < 6))
            PushOGCD(AID.ThirdEye, Player, -100);
    }
}
