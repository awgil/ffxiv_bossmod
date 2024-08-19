namespace BossMod.Autorotation;

public sealed class ClassDRKUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { DarkMind = SharedTrack.Count, ShadowWall, LivingDead, TheBlackestNight, Oblation, DarkMissionary, Stance }
    public enum WallOption { None, ShadowWall, ShadowedVigil }
    public enum StanceOption { None, StanceOn, StanceOff }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(DRK.AID.DarkForce);
    public static readonly ActionID IDStanceApply = ActionID.MakeSpell(DRK.AID.Grit);
    public static readonly ActionID IDStanceRemove = ActionID.MakeSpell(DRK.AID.ReleaseGrit);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: DRK", "Planner support for utility actions", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.DRK), 100);
        DefineShared(res, IDLimitBreak3, IDStanceApply, IDStanceRemove);

        DefineSimpleConfig(res, Track.DarkMind, "DarkMind", "DMind", 450, DRK.AID.DarkMind, 10);

        res.Define(Track.ShadowWall).As<WallOption>("ShadowWall", "Wall", 550)
            .AddOption(WallOption.None, "None", "Do not use automatically")
            .AddOption(WallOption.ShadowWall, "Use", "Use ShadowWall", 120, 15, ActionTargets.Self, 38, 91)
            .AddOption(WallOption.ShadowedVigil, "UseEx", "Use ShadowedVigil", 120, 15, ActionTargets.Self, 92)
            .AddAssociatedActions(DRK.AID.ShadowWall, DRK.AID.ShadowedVigil);

        DefineSimpleConfig(res, Track.LivingDead, "LivingDead", "LD", 400, DRK.AID.LivingDead, 10);
        DefineSimpleConfig(res, Track.TheBlackestNight, "The Blackest Night", "TBN", 400, DRK.AID.TheBlackestNight, 7);
        DefineSimpleConfig(res, Track.Oblation, "Oblation", "Obl", 320, DRK.AID.Oblation); // note: secondary effect (hot) duration 6
        DefineSimpleConfig(res, Track.DarkMissionary, "DarkMissionary", "Mission", 220, DRK.AID.DarkMissionary, 30); // note: secondary effect duration 15

        res.Define(Track.Stance).As<StanceOption>("Stance", "", 200)
            .AddOption(StanceOption.None, "None", "Do not use automatically")
            .AddOption(StanceOption.StanceOn, "", "Force Stance On", 0, 0, ActionTargets.Self)
            .AddOption(StanceOption.StanceOff, "", "Force Stance Off", 0, 0, ActionTargets.Self)
            .AddAssociatedActions(DRK.AID.Grit, DRK.AID.ReleaseGrit);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)DRK.SID.Grit, primaryTarget);
        ExecuteSimple(strategy.Option(Track.DarkMind), DRK.AID.DarkMind, Player);
        ExecuteSimple(strategy.Option(Track.LivingDead), DRK.AID.LivingDead, Player);
        ExecuteSimple(strategy.Option(Track.TheBlackestNight), DRK.AID.Oblation, Player);
        ExecuteSimple(strategy.Option(Track.Oblation), DRK.AID.Oblation, Player);
        ExecuteSimple(strategy.Option(Track.DarkMissionary), DRK.AID.DarkMissionary, Player);

        var wall = strategy.Option(Track.ShadowWall);
        var wallAction = wall.As<WallOption>() switch
        {
            WallOption.ShadowWall => DRK.AID.ShadowWall,
            WallOption.ShadowedVigil => DRK.AID.ShadowedVigil,
            _ => default
        };
        if (wallAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(wallAction), Player, wall.Priority(), wall.Value.ExpireIn);

        var stance = strategy.Option(Track.Stance);
        var stanceOption = stance.As<StanceOption>() switch
        {
            StanceOption.StanceOn => DRK.AID.Grit,
            StanceOption.StanceOff => DRK.AID.ReleaseGrit,
            _ => default
        };
        if (stanceOption != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(stanceOption), Player, stance.Priority());
    }
}
