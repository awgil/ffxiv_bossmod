﻿namespace BossMod.Autorotation;

public sealed class ClassRPRUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { ArcaneCrest = SharedTrack.Count }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(RPR.AID.TheEnd);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: RPR", "Planner support for utility actions", "lazylemo", RotationModuleQuality.WIP, BitMask.Build((int)Class.RPR), 90);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.ArcaneCrest, "ArcaneCrest", "", 600, RPR.AID.ArcaneCrest, 5);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget)
    {
        ExecuteShared(strategy, IDLimitBreak3);
        ExecuteSimple(strategy.Option(Track.ArcaneCrest), RPR.AID.ArcaneCrest, Player);
    }
}
