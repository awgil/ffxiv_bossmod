﻿namespace BossMod.Autorotation;

public sealed class ClassDRKUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { DarkMind = SharedTrack.Count, ShadowWall, LivingDead, TheBlackestNight, Oblation, DarkMissionary, Shadowstride }
    public enum WallOption { None, ShadowWall, ShadowedVigil } //ShadowWall strategy
    public enum TBNStrategy { None, Force } //TheBlackestNight strategy
    public enum OblationStrategy { None, Force, ForceHold1 } //Oblation strategy
    public enum DashStrategy { None, GapClose, GapCloseHold1 } //GapCloser strategy

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(DRK.AID.DarkForce);
    public static readonly ActionID IDStanceApply = ActionID.MakeSpell(DRK.AID.Grit);
    public static readonly ActionID IDStanceRemove = ActionID.MakeSpell(DRK.AID.ReleaseGrit);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: DRK", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility|Tank", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.DRK), 100);
        DefineShared(res, IDLimitBreak3, IDStanceApply, IDStanceRemove);

        DefineSimpleConfig(res, Track.DarkMind, "DarkMind", "DMind", 450, DRK.AID.DarkMind, 10); //120s CD, 15s duration

        res.Define(Track.ShadowWall).As<WallOption>("ShadowWall", "Wall", 550) //120s CD, 15s duration
            .AddOption(WallOption.None, "None", "Do not use automatically")
            .AddOption(WallOption.ShadowWall, "Use", "Use Shadow Wall", 120, 15, ActionTargets.Self, 38, 91)
            .AddOption(WallOption.ShadowedVigil, "UseEx", "Use Shadowed Vigil", 120, 15, ActionTargets.Self, 92)
            .AddAssociatedActions(DRK.AID.ShadowWall, DRK.AID.ShadowedVigil);

        DefineSimpleConfig(res, Track.LivingDead, "LivingDead", "LD", 400, DRK.AID.LivingDead, 10); //300s CD, 10s duration

        res.Define(Track.TheBlackestNight).As<TBNStrategy>("TheBlackestNight", "TBN", 550) //60s (120s total), 10s duration, 2 charges
            .AddOption(TBNStrategy.None, "None", "Do not use automatically")
            .AddOption(TBNStrategy.Force, "Use", "Use The Blackest Night", 15, 7, ActionTargets.Self | ActionTargets.Party, 70)
            .AddAssociatedActions(DRK.AID.TheBlackestNight);

        res.Define(Track.Oblation).As<OblationStrategy>("Oblation", "", 550) //60s (120s total), 10s duration, 2 charges
            .AddOption(OblationStrategy.None, "None", "Do not use automatically")
            .AddOption(OblationStrategy.Force, "Use", "Use Oblation", 60, 10, ActionTargets.Self | ActionTargets.Party, 82)
            .AddOption(OblationStrategy.ForceHold1, "UseHold1", "Use Oblation; Holds 1 charge for manual usage", 60, 10, ActionTargets.Self | ActionTargets.Party, 82)
            .AddAssociatedActions(DRK.AID.Oblation);

        DefineSimpleConfig(res, Track.DarkMissionary, "DarkMissionary", "Mission", 220, DRK.AID.DarkMissionary, 15); //90s CD, 15s duration

        res.Define(Track.Shadowstride).As<DashStrategy>("Shadowstride", "Dash", 20)
            .AddOption(DashStrategy.None, "None", "No use")
            .AddOption(DashStrategy.GapClose, "GapClose", "Use as gapcloser if outside melee range", 30, 0, ActionTargets.Hostile, 56)
            .AddOption(DashStrategy.GapCloseHold1, "GapCloseHold1", "Use as gapcloser if outside melee range; conserves 1 charge for manual usage", 60, 0, ActionTargets.Hostile, 84)
            .AddAssociatedActions(DRK.AID.Shadowstride);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)DRK.SID.Grit, primaryTarget); //Execution of our shared abilities
        ExecuteSimple(strategy.Option(Track.DarkMind), DRK.AID.DarkMind, Player); //Execution of DarkMind
        ExecuteSimple(strategy.Option(Track.LivingDead), DRK.AID.LivingDead, Player); //Execution of LivingDead
        ExecuteSimple(strategy.Option(Track.DarkMissionary), DRK.AID.DarkMissionary, Player); //Execution of DarkMissionary

        //TBN execution
        var canTBN = ActionUnlocked(ActionID.MakeSpell(DRK.AID.TheBlackestNight)) && Player.HPMP.CurMP >= 3000;
        var tbn = strategy.Option(Track.TheBlackestNight);
        var tbnTarget = ResolveTargetOverride(tbn.Value) ?? CoTank() ?? primaryTarget ?? Player; //Smart-Targets Co-Tank if set to Automatic, if no Co-Tank then targets self
        if (canTBN && tbn.As<TBNStrategy>() == TBNStrategy.Force)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(DRK.AID.TheBlackestNight), tbnTarget, tbn.Priority(), tbn.Value.ExpireIn);

        //Oblation execution
        var canObl = ActionUnlocked(ActionID.MakeSpell(DRK.AID.Oblation));
        var oblation = strategy.Option(Track.Oblation);
        var oblationStrat = oblation.As<OblationStrategy>();
        var oblationTarget = ResolveTargetOverride(oblation.Value) ?? primaryTarget ?? Player; //Smart-Targets Co-Tank if set to Automatic, if no Co-Tank then targets self
        var oblationStatus = StatusDetails(oblationTarget, DRK.SID.Oblation, Player.InstanceID).Left > 0.1f;
        var oblationCD = World.Client.Cooldowns[ActionDefinitions.Instance.Spell(DRK.AID.Oblation)!.MainCooldownGroup].Remaining;
        if (canObl && oblationStrat != OblationStrategy.None && !oblationStatus)
        {
            if ((oblationStrat == OblationStrategy.Force) ||
                (oblationStrat == OblationStrategy.ForceHold1 && oblationCD < 0.6f))
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(DRK.AID.Oblation), oblationTarget, oblation.Priority(), oblation.Value.ExpireIn);
        }

        //Shadow Wall / Vigil execution
        var wall = strategy.Option(Track.ShadowWall);
        var wallAction = wall.As<WallOption>() switch
        {
            WallOption.ShadowWall => DRK.AID.ShadowWall,
            WallOption.ShadowedVigil => DRK.AID.ShadowedVigil,
            _ => default
        };
        if (wallAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(wallAction), Player, wall.Priority(), wall.Value.ExpireIn); //Checking proper use of said option

        //Shadowstride execution
        var dash = strategy.Option(Track.Shadowstride);
        var dashStrategy = strategy.Option(Track.Shadowstride).As<DashStrategy>();
        var dashTarget = ResolveTargetOverride(dash.Value) ?? primaryTarget; //Smart-Targeting
        var distance = Player.DistanceToHitbox(dashTarget);
        var dashCD = World.Client.Cooldowns[ActionDefinitions.Instance.Spell(DRK.AID.Shadowstride)!.MainCooldownGroup].Remaining;
        var shouldDash = dashStrategy switch
        {
            DashStrategy.None => false,
            DashStrategy.GapClose => distance is > 3f and <= 20f && dashCD <= 30.5f,
            DashStrategy.GapCloseHold1 => distance is > 3f and <= 20f && dashCD < 0.6f,
            _ => false,
        };
        if (shouldDash)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(DRK.AID.Shadowstride), dashTarget, dash.Priority(), dash.Value.ExpireIn);
    }
}
