namespace BossMod.Autorotation;

public sealed record class ClassWARUtility(WorldState World, Actor Player, AIHints Hints) : RoleTankUtility(World, Player, Hints)
{
    public enum Track { Thrill = SharedTrack.Count, Vengeance, Holmgang, Bloodwhetting, Equilibrium, ShakeItOff }
    public enum BWOption { None, Bloodwhetting, RawIntuition, NascentFlash }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: WAR", "Planner support for utility actions", BitMask.Build((int)Class.WAR), 90);
        DefineShared(res);

        DefineSimpleConfig(res, Track.Thrill, "Thrill", "", 450, WAR.AID.ThrillOfBattle, 10);
        DefineSimpleConfig(res, Track.Vengeance, "Veng", "", 550, WAR.AID.Vengeance, 15);
        DefineSimpleConfig(res, Track.Holmgang, "Holmgang", "", 400, WAR.AID.Holmgang, 10);

        var bw = res.AddConfig(Track.Bloodwhetting, new("BW", UIPriority: 350));
        bw.AddOption(BWOption.None, new(0x80ffffff, ActionTargets.None, "None", "Do not use automatically"));
        bw.AddOption(BWOption.Bloodwhetting, new(0x8000ffff, ActionTargets.Self, "BW", "Use Bloodwhetting", 25, 4, 82)); // note: secondary effect duration 8
        bw.AddOption(BWOption.RawIntuition, new(0x8000ffff, ActionTargets.Self, "RI", "Use Raw Intuition", 25, 6, 56, 81));
        bw.AddOption(BWOption.NascentFlash, new(0x80ff00ff, ActionTargets.Party, "NF", "Use Nascent Flash", 25, 4, 76)); // note: secondary effect duration 8

        DefineSimpleConfig(res, Track.Equilibrium, "Equi", "", 320, WAR.AID.Equilibrium); // note: secondary effect (hot) duration 6
        DefineSimpleConfig(res, Track.ShakeItOff, "SIO", "", 220, WAR.AID.ShakeItOff, 30); // note: secondary effect duration 15

        return res;
    }

    public override void Execute(ReadOnlySpan<StrategyValue> strategy, Actor? primaryTarget, ActionQueue actions)
    {
        ExecuteShared(strategy, actions, ActionID.MakeSpell(WAR.AID.LandWaker));
        ExecuteSimple(strategy[(int)Track.Thrill], WAR.AID.ThrillOfBattle, Player, actions);
        ExecuteSimple(strategy[(int)Track.Vengeance], WAR.AID.Vengeance, Player, actions);
        ExecuteSimple(strategy[(int)Track.Holmgang], WAR.AID.Holmgang, Player, actions);
        ExecuteSimple(strategy[(int)Track.Equilibrium], WAR.AID.Equilibrium, Player, actions);
        ExecuteSimple(strategy[(int)Track.ShakeItOff], WAR.AID.ShakeItOff, Player, actions);

        var bw = strategy[(int)Track.Bloodwhetting];
        var aid = (BWOption)bw.Option switch
        {
            BWOption.Bloodwhetting => WAR.AID.Bloodwhetting,
            BWOption.RawIntuition => WAR.AID.RawIntuition,
            BWOption.NascentFlash => WAR.AID.NascentFlash,
            _ => default
        };
        if (aid != default)
            actions.Push(ActionID.MakeSpell(aid), (BWOption)bw.Option == BWOption.NascentFlash ? CoTank() : Player, bw.Priority(ActionQueue.Priority.Low));
    }
}
