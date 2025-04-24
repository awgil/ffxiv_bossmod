﻿using BossMod.Autorotation.xan;
using BossMod.SAM;

namespace BossMod.Autorotation.Standard.xan.Utility;

public class ThirdEye(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public enum Track { ThirdEye }

    public enum ThirdEyeStrategy
    {
        Automatic,
        AutoMax,
        Delay
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Auto-ThirdEye", "Third Eye before incoming damage", "Utility (xan)", "xan", RotationModuleQuality.Excellent, BitMask.Build(Class.SAM), 100, 6);

        def.Define(Track.ThirdEye).As<ThirdEyeStrategy>("ThirdEye")
            .AddOption(ThirdEyeStrategy.Automatic, "Auto", "Use Third Eye ~3s before predicted damage", 15, 4)
            .AddOption(ThirdEyeStrategy.AutoMax, "AutoMax", "Use Third Eye 4s before predicted damage", 15, 4)
            .AddOption(ThirdEyeStrategy.Delay, "Delay", "Don't use")
            .AddAssociatedActions(AID.ThirdEye, AID.Tengentsu);

        return def;
    }

    public override void Exec(StrategyValues strategy, AIHints.Enemy? primaryTarget)
    {
        if (Player.FindStatus(SID.Meditate) != null)
            return;

        var advance = strategy.Option(Track.ThirdEye).As<ThirdEyeStrategy>() switch
        {
            ThirdEyeStrategy.Automatic => 3,
            ThirdEyeStrategy.AutoMax => 4,
            _ => 0
        };

        if (advance > 0 && Hints.PredictedDamage.Any(x => x.players[0] && x.activation < World.FutureTime(advance)))
            PushOGCD(AID.ThirdEye, Player, -100);
    }
}
