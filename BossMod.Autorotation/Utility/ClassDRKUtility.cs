namespace BossMod.Autorotation;

public sealed class ClassDRKUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { DarkMind = SharedTrack.Count, ShadowWall, LivingDead, TheBlackestNight, Oblation, DarkMissionary, Shadowstride }
    public enum WallOption { None, ShadowWall, ShadowedVigil }
    public enum TBNStrategy { None, Use }
    public enum OblationStrategy { None, Use, Both }
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
            .AddOption(TBNStrategy.Use, "Use The Blackest Night", 15, 7, ActionTargets.Self | ActionTargets.Party, 70, internalNameOverride: "Force")
            .AddAssociatedActions(DRK.AID.TheBlackestNight);

        res.Define(Track.Oblation).As<OblationStrategy>("Oblation", "", 550)
            .AddOption(OblationStrategy.None, "Do not use automatically")
            .AddOption(OblationStrategy.Use, "Use Oblation", 60, 10, ActionTargets.Self | ActionTargets.Party, 82, internalNameOverride: "Force")
            .AddOption(OblationStrategy.Both, "Use Oblation on self and cotank", 60, 10, minLevel: 82)
            .AddAssociatedActions(DRK.AID.Oblation);

        DefineSimpleConfig(res, Track.DarkMissionary, "DarkMissionary", "Mission", 220, DRK.AID.DarkMissionary, 15);

        res.Define(Track.Shadowstride).As<DashStrategy>("Shadowstride", "Dash", 20)
            .AddOption(DashStrategy.None, "Don't use")
            .AddOption(DashStrategy.GapClose, "Use while outside melee range", 30, 0, ActionTargets.Hostile, 56)
            .AddOption(DashStrategy.GapCloseHold1, "Use while outside melee range; conserve one charge", 60, 0, ActionTargets.Hostile, 84)
            .AddAssociatedActions(DRK.AID.Shadowstride);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)DRK.SID.Grit, primaryTarget);
        ExecuteSimple(strategy.Option(Track.DarkMind), DRK.AID.DarkMind, Player);
        ExecuteSimple(strategy.Option(Track.LivingDead), DRK.AID.LivingDead, Player);
        ExecuteSimple(strategy.Option(Track.DarkMissionary), DRK.AID.DarkMissionary, Player);

        // TBN
        var tbn = strategy.Option(Track.TheBlackestNight);
        var tbnTarget = ResolveTarget(tbn.Value) ?? Player; // principle of least astonishment, if target isn't specified then we assume the player needs the mit, not cotank
        if (Player.HPMP.CurMP >= 3000 && tbn.As<TBNStrategy>() == TBNStrategy.Use)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(DRK.AID.TheBlackestNight), tbnTarget, tbn.Priority(), tbn.Value.ExpireIn);

        // Oblation
        var oblation = strategy.Option(Track.Oblation);
        Actor[] oblationTargets = oblation.As<OblationStrategy>() switch
        {
            OblationStrategy.Use => [ResolveTarget(oblation.Value) ?? Player],
            OblationStrategy.Both => CoTank() is { } tank ? [Player, tank] : [Player],
            _ => []
        };
        foreach (var target in oblationTargets)
            if (target.FindStatus(DRK.SID.Oblation, DateTime.MaxValue) == null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(DRK.AID.Oblation), target, oblation.Priority(), oblation.Value.ExpireIn);

        // 40% mit
        var wall = strategy.Option(Track.ShadowWall);
        var wallAction = wall.As<WallOption>() switch
        {
            WallOption.ShadowWall => DRK.AID.ShadowWall,
            WallOption.ShadowedVigil => DRK.AID.ShadowedVigil,
            _ => default
        };
        if (wallAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(wallAction), Player, wall.Priority(), wall.Value.ExpireIn);

        // Dash
        var dash = strategy.Option(Track.Shadowstride);
        var dashStrategy = strategy.Option(Track.Shadowstride).As<DashStrategy>();
        var dashTarget = ResolveTarget(dash.Value) ?? primaryTarget;

        var useDash = Player.DistanceToHitbox(dashTarget) is > 3 and <= 20 && dashStrategy switch
        {
            DashStrategy.GapClose => true,
            DashStrategy.GapCloseHold1 => MaxChargesIn(DRK.AID.Shadowstride) <= World.Client.AnimationLock,
            _ => false
        };
        if (useDash)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(DRK.AID.Shadowstride), dashTarget, dash.Priority(), dash.Value.ExpireIn, forced: true);
    }
}
