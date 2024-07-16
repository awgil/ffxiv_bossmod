﻿namespace BossMod.Autorotation;

public sealed class ClassWARUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { Thrill = SharedTrack.Count, Vengeance, Holmgang, Bloodwhetting, Equilibrium, ShakeItOff }
    public enum BWOption { None, Bloodwhetting, RawIntuition, NascentFlash }
    public enum VengOption { None, Vengeance, Damnation }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(WAR.AID.LandWaker);
    public static readonly ActionID IDStanceApply = ActionID.MakeSpell(WAR.AID.Defiance);
    public static readonly ActionID IDStanceRemove = ActionID.MakeSpell(WAR.AID.ReleaseDefiance);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: WAR", "Planner support for utility actions", "veyn", RotationModuleQuality.WIP, BitMask.Build((int)Class.WAR), 100);
        DefineShared(res, IDLimitBreak3, IDStanceApply, IDStanceRemove);

        DefineSimpleConfig(res, Track.Thrill, "ThrillOfBattle", "Thrill", 450, WAR.AID.ThrillOfBattle, 10);

        res.Define(Track.Vengeance).As<VengOption>("Vengeance", "Veng", 550)
            .AddOption(VengOption.None, "None", "Do not use automatically")
            .AddOption(VengOption.Vengeance, "Use", "Use Vengeance", 120, 15, ActionTargets.Self, 38, 91)
            .AddOption(VengOption.Damnation, "UseEx", "Use Damnation", 120, 15, ActionTargets.Self, 92);

        DefineSimpleConfig(res, Track.Holmgang, "Holmgang", "", 400, WAR.AID.Holmgang, 10);

        res.Define(Track.Bloodwhetting).As<BWOption>("BW", uiPriority: 350)
            .AddOption(BWOption.None, "None", "Do not use automatically")
            .AddOption(BWOption.Bloodwhetting, "BW", "Use Bloodwhetting", 25, 4, ActionTargets.Self, 82) // note: secondary effect duration 8
            .AddOption(BWOption.RawIntuition, "RI", "Use Raw Intuition", 25, 6, ActionTargets.Self, 56, 81)
            .AddOption(BWOption.NascentFlash, "NF", "Use Nascent Flash", 25, 4, ActionTargets.Party, 76) // note: secondary effect duration 8
            .AddAssociatedActions(WAR.AID.Bloodwhetting, WAR.AID.RawIntuition, WAR.AID.NascentFlash);

        DefineSimpleConfig(res, Track.Equilibrium, "Equilibrium", "Equi", 320, WAR.AID.Equilibrium); // note: secondary effect (hot) duration 6
        DefineSimpleConfig(res, Track.ShakeItOff, "ShakeItOff", "SIO", 220, WAR.AID.ShakeItOff, 30); // note: secondary effect duration 15

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget)
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)WAR.SID.Defiance);
        ExecuteSimple(strategy.Option(Track.Thrill), WAR.AID.ThrillOfBattle, Player);
        ExecuteSimple(strategy.Option(Track.Holmgang), WAR.AID.Holmgang, Player);
        ExecuteSimple(strategy.Option(Track.Equilibrium), WAR.AID.Equilibrium, Player);
        ExecuteSimple(strategy.Option(Track.ShakeItOff), WAR.AID.ShakeItOff, Player);

        var veng = strategy.Option(Track.Vengeance);
        var vengAction = veng.As<VengOption>() switch
        {
            VengOption.Vengeance => WAR.AID.Vengeance,
            VengOption.Damnation => WAR.AID.Damnation,
            _ => default
        };
        if (vengAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(vengAction), Player, veng.Priority());

        var bw = strategy.Option(Track.Bloodwhetting);
        var bwAction = bw.As<BWOption>() switch
        {
            BWOption.Bloodwhetting => WAR.AID.Bloodwhetting,
            BWOption.RawIntuition => WAR.AID.RawIntuition,
            BWOption.NascentFlash => WAR.AID.NascentFlash,
            _ => default
        };
        if (bwAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(bwAction), bwAction == WAR.AID.NascentFlash ? ResolveTargetOverride(bw.Value) ?? CoTank() : Player, bw.Priority());
    }
}
