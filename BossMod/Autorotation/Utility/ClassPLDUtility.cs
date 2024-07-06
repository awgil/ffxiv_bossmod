﻿namespace BossMod.Autorotation;

public sealed class ClassPLDUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { Sheltron = SharedTrack.Count, Sentinel, HallowedGround }
    public enum BWOption { None, Bloodwhetting, RawIntuition, NascentFlash }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(PLD.AID.LastBastion);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: PLD", "Planner support for utility actions", "veyn", RotationModuleQuality.WIP, BitMask.Build((int)Class.PLD), 60);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.Sheltron, "Sheltron", "", 380, PLD.AID.Sheltron, 6);
        DefineSimpleConfig(res, Track.Sentinel, "Sentinel", "", 550, PLD.AID.Sentinel, 15);
        DefineSimpleConfig(res, Track.HallowedGround, "HallowedGround", "Invuln", 400, PLD.AID.HallowedGround, 10);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget)
    {
        ExecuteShared(strategy, IDLimitBreak3);
        ExecuteSimple(strategy.Option(Track.Sheltron), PLD.AID.Sheltron, Player);
        ExecuteSimple(strategy.Option(Track.Sentinel), PLD.AID.Sentinel, Player);
        ExecuteSimple(strategy.Option(Track.HallowedGround), PLD.AID.HallowedGround, Player);
    }
}
