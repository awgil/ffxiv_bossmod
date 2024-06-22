namespace BossMod.Autorotation;

public sealed class ClassWARUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { Thrill = SharedTrack.Count, Vengeance, Holmgang, Bloodwhetting, Equilibrium, ShakeItOff }
    public enum BWOption { None, Bloodwhetting, RawIntuition, NascentFlash }

    public static ActionID IDLimitBreak3 = ActionID.MakeSpell(WAR.AID.LandWaker);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: WAR", "Planner support for utility actions", "veyn", RotationModuleQuality.WIP, BitMask.Build((int)Class.WAR), 90);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.Thrill, "ThrillOfBattle", "Thrill", 450, WAR.AID.ThrillOfBattle, 10);
        DefineSimpleConfig(res, Track.Vengeance, "Vengeance", "Veng", 550, WAR.AID.Vengeance, 15);
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
        ExecuteShared(strategy, IDLimitBreak3);
        ExecuteSimple(strategy.Option(Track.Thrill), WAR.AID.ThrillOfBattle, Player);
        ExecuteSimple(strategy.Option(Track.Vengeance), WAR.AID.Vengeance, Player);
        ExecuteSimple(strategy.Option(Track.Holmgang), WAR.AID.Holmgang, Player);
        ExecuteSimple(strategy.Option(Track.Equilibrium), WAR.AID.Equilibrium, Player);
        ExecuteSimple(strategy.Option(Track.ShakeItOff), WAR.AID.ShakeItOff, Player);

        var bw = strategy.Option(Track.Bloodwhetting);
        var aid = bw.As<BWOption>() switch
        {
            BWOption.Bloodwhetting => WAR.AID.Bloodwhetting,
            BWOption.RawIntuition => WAR.AID.RawIntuition,
            BWOption.NascentFlash => WAR.AID.NascentFlash,
            _ => default
        };
        if (aid != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), bw.As<BWOption>() == BWOption.NascentFlash ? ResolveTargetOverride(bw.Value) ?? CoTank() : Player, bw.Priority());
    }
}
