namespace BossMod.Autorotation;

public sealed class ClassDRKUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { DarkMind = SharedTrack.Count, ShadowWall, LivingDead, TheBlackestNight, Oblation, DarkMissionary, Shadowstride }
    public enum WallOption { None, ShadowWall, ShadowedVigil }
    public enum TBNStrategy { None, Force }
    public enum OblationStrategy { None, Force }
    public enum DashStrategy { None, GapClose, GapCloseHold1 }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(DRK.AID.DarkForce);
    public static readonly ActionID IDStanceApply = ActionID.MakeSpell(DRK.AID.Grit);
    public static readonly ActionID IDStanceRemove = ActionID.MakeSpell(DRK.AID.ReleaseGrit);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: DRK", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.DRK), 100);
        DefineShared(res, IDLimitBreak3, IDStanceApply, IDStanceRemove);

        DefineSimpleConfig(res, Track.DarkMind, "DarkMind", "DMind", 450, DRK.AID.DarkMind, 10);

        res.Define(Track.ShadowWall).As<WallOption>("ShadowWall", "Wall", 550)
            .AddOption(WallOption.None, "Do not use automatically")
            .AddOption(WallOption.ShadowWall, "Use Shadow Wall", 120, 15, ActionTargets.Self, 38, 91)
            .AddOption(WallOption.ShadowedVigil, "Use Shadowed Vigil", 120, 15, ActionTargets.Self, 92)
            .AddAssociatedActions(DRK.AID.ShadowWall, DRK.AID.ShadowedVigil);

        DefineSimpleConfig(res, Track.LivingDead, "LivingDead", "LD", 400, DRK.AID.LivingDead, 10);

        res.Define(Track.TheBlackestNight).As<TBNStrategy>("TheBlackestNight", "TBN", 550)
            .AddOption(TBNStrategy.None, "Do not use automatically")
            .AddOption(TBNStrategy.Force, "Use The Blackest Night", 15, 7, ActionTargets.Self | ActionTargets.Party, 70)
            .AddAssociatedActions(DRK.AID.TheBlackestNight);

        res.Define(Track.Oblation).As<OblationStrategy>("Oblation", "", 550)
            .AddOption(OblationStrategy.None, "Do not use automatically")
            .AddOption(OblationStrategy.Force, "Use Oblation", 60, 10, ActionTargets.Self | ActionTargets.Party, 82)
            .AddAssociatedActions(DRK.AID.Oblation);

        DefineSimpleConfig(res, Track.DarkMissionary, "DarkMissionary", "Mission", 220, DRK.AID.DarkMissionary, 15);

        res.Define(Track.Shadowstride).As<DashStrategy>("Shadowstride", "Dash", 20)
            .AddOption(DashStrategy.None, "No use")
            .AddOption(DashStrategy.GapClose, "Use as gapcloser if outside melee range", 30, 0, ActionTargets.Hostile, 56)
            .AddOption(DashStrategy.GapCloseHold1, "Use as gapcloser if outside melee range; conserves 1 charge for manual usage", 60, 0, ActionTargets.Hostile, 84)
            .AddAssociatedActions(DRK.AID.Shadowstride);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)DRK.SID.Grit, primaryTarget);
        ExecuteSimple(strategy.Option(Track.DarkMind), DRK.AID.DarkMind, Player);
        ExecuteSimple(strategy.Option(Track.LivingDead), DRK.AID.LivingDead, Player);
        ExecuteSimple(strategy.Option(Track.DarkMissionary), DRK.AID.DarkMissionary, Player);

        //TBN execution
        var tbn = strategy.Option(Track.TheBlackestNight);
        var tbnTarget = ResolveTargetOverride(tbn.Value) ?? CoTank() ?? primaryTarget ?? Player; //smart-target -> CoTank -> target (if current target is party member) -> self
        if (ActionUnlocked(ActionID.MakeSpell(DRK.AID.TheBlackestNight)) && Player.HPMP.CurMP >= 3000 && tbn.As<TBNStrategy>() == TBNStrategy.Force)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(DRK.AID.TheBlackestNight), tbnTarget, tbn.Priority(), tbn.Value.ExpireIn);

        //Oblation execution
        var oblation = strategy.Option(Track.Oblation);
        var oblationTarget = ResolveTargetOverride(oblation.Value) ?? primaryTarget ?? Player; //smart-target -> target (if current target is party member) -> self
        if (ActionUnlocked(ActionID.MakeSpell(DRK.AID.Oblation)) &&
            World.Client.Cooldowns[ActionDefinitions.Instance.Spell(DRK.AID.Oblation)!.MainCooldownGroup].Remaining <= 60.5f &&
            oblationTarget?.FindStatus(DRK.SID.Oblation) == null && oblation.As<OblationStrategy>() == OblationStrategy.Force)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(DRK.AID.Oblation), oblationTarget, oblation.Priority(), oblation.Value.ExpireIn);

        //Shadow Wall / Vigil execution
        var wall = strategy.Option(Track.ShadowWall);
        var wallAction = wall.As<WallOption>() switch
        {
            WallOption.ShadowWall => DRK.AID.ShadowWall,
            WallOption.ShadowedVigil => DRK.AID.ShadowedVigil,
            _ => default
        };
        if (wallAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(wallAction), Player, wall.Priority(), wall.Value.ExpireIn);

        //Shadowstride execution
        var dash = strategy.Option(Track.Shadowstride);
        var dashStrategy = strategy.Option(Track.Shadowstride).As<DashStrategy>();
        var dashTarget = ResolveTargetOverride(dash.Value) ?? primaryTarget;
        var distance = Player.DistanceToHitbox(dashTarget);
        var dashCD = World.Client.Cooldowns[ActionDefinitions.Instance.Spell(DRK.AID.Shadowstride)!.MainCooldownGroup].Remaining;
        if (dashStrategy switch
        {
            DashStrategy.GapClose => distance is > 3f and <= 20f && dashCD <= 30.5f,
            DashStrategy.GapCloseHold1 => distance is > 3f and <= 20f && dashCD < 0.6f,
            _ => false,
        })
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(DRK.AID.Shadowstride), dashTarget, dash.Priority(), dash.Value.ExpireIn);
    }
}
