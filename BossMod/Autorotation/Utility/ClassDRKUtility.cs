﻿namespace BossMod.Autorotation;

public sealed class ClassDRKUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { DarkMind = SharedTrack.Count, ShadowWall, LivingDead, TheBlackestNight, Oblation, DarkMissionary, Shadowstride }
    public enum WallOption { None, ShadowWall, ShadowedVigil }
    public enum OblationStrategy { None, Force, Delay }
    public enum DashStrategy { None, GapClose } //GapCloser strategy
    public bool InMeleeRange(Actor? target) => Player.DistanceToHitbox(target) <= 3; //Checks if we're inside melee range

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(DRK.AID.DarkForce);
    public static readonly ActionID IDStanceApply = ActionID.MakeSpell(DRK.AID.Grit);
    public static readonly ActionID IDStanceRemove = ActionID.MakeSpell(DRK.AID.ReleaseGrit);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: DRK", "Planner support for utility actions", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.DRK), 100);
        DefineShared(res, IDLimitBreak3, IDStanceApply, IDStanceRemove);

        DefineSimpleConfig(res, Track.DarkMind, "DarkMind", "DMind", 450, DRK.AID.DarkMind, 10); //120s CD, 15s duration

        res.Define(Track.ShadowWall).As<WallOption>("ShadowWall", "Wall", 550) //120s CD, 15s duration
            .AddOption(WallOption.None, "None", "Do not use automatically")
            .AddOption(WallOption.ShadowWall, "Use", "Use Shadow Wall", 120, 15, ActionTargets.Self, 38, 91)
            .AddOption(WallOption.ShadowedVigil, "UseEx", "Use Shadowed Vigil", 120, 15, ActionTargets.Self, 92)
            .AddAssociatedActions(DRK.AID.ShadowWall, DRK.AID.ShadowedVigil);

        DefineSimpleConfig(res, Track.LivingDead, "LivingDead", "LD", 400, DRK.AID.LivingDead, 10); //300s CD, 10s duration
        DefineSimpleConfig(res, Track.TheBlackestNight, "The Blackest Night", "TBN", 400, DRK.AID.TheBlackestNight, 7); //15s CD, 7s duration, 3000MP cost

        res.Define(Track.Oblation).As<OblationStrategy>("Oblation", "", 550) //60s (120s total), 10s duration, 2 charges
            .AddOption(OblationStrategy.None, "None", "Do not use automatically")
            .AddOption(OblationStrategy.Force, "Use", "Use Oblation", 60, 10, ActionTargets.Self | ActionTargets.Party, 82)
            .AddOption(OblationStrategy.Delay, "Don't use", "Delay Oblation")
            .AddAssociatedActions(DRK.AID.Oblation);

        DefineSimpleConfig(res, Track.DarkMissionary, "DarkMissionary", "Mission", 220, DRK.AID.DarkMissionary, 15); //90s CD, 15s duration

        res.Define(Track.Shadowstride).As<DashStrategy>("Shadowstride", "Dash", 20)
            .AddOption(DashStrategy.None, "None", "No use")
            .AddOption(DashStrategy.GapClose, "GapClose", "Use as gapcloser if outside melee range", 30, 0, ActionTargets.Hostile, 56)
            .AddAssociatedActions(DRK.AID.Shadowstride);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)DRK.SID.Grit, primaryTarget); //Execution of our shared abilities
        ExecuteSimple(strategy.Option(Track.DarkMind), DRK.AID.DarkMind, Player); //Execution of DarkMind
        ExecuteSimple(strategy.Option(Track.LivingDead), DRK.AID.LivingDead, Player); //Execution of LivingDead
        ExecuteSimple(strategy.Option(Track.TheBlackestNight), DRK.AID.TheBlackestNight, null); //Execution of TheBlackestNight
        ExecuteSimple(strategy.Option(Track.DarkMissionary), DRK.AID.DarkMissionary, Player); //Execution of DarkMissionary

        var obl = strategy.Option(Track.Oblation);
        var oblAction = obl.As<OblationStrategy>() switch
        {
            OblationStrategy.Force => DRK.AID.Oblation,
            OblationStrategy.Delay => DRK.AID.None,
            _ => default
        };
        if (oblAction != default && SelfStatusLeft(DRK.SID.Oblation) <= 3f)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(oblAction), Player, obl.Priority(), obl.Value.ExpireIn); //Checking proper use of said option

        var wall = strategy.Option(Track.ShadowWall);
        var wallAction = wall.As<WallOption>() switch
        {
            WallOption.ShadowWall => DRK.AID.ShadowWall,
            WallOption.ShadowedVigil => DRK.AID.ShadowedVigil,
            _ => default
        };
        if (wallAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(wallAction), Player, wall.Priority(), wall.Value.ExpireIn); //Checking proper use of said option

        var dashStrategy = strategy.Option(Track.Shadowstride).As<DashStrategy>();
        if (ShouldUseDash(dashStrategy, primaryTarget))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(DRK.AID.Shadowstride), Player, obl.Priority());
    }
    private bool ShouldUseDash(DashStrategy strategy, Actor? primaryTarget) => strategy switch
    {
        DashStrategy.None => false,
        DashStrategy.GapClose => !InMeleeRange(primaryTarget),
        _ => false,
    };
}
