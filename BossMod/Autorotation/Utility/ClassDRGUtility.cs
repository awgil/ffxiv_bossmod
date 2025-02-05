namespace BossMod.Autorotation;

public sealed class ClassDRGUtility(RotationModuleManager manager, Actor player) : RoleMeleeUtility(manager, player)
{
    public enum Track { WingedGlide = SharedTrack.Count, ElusiveJump }
    public enum DashStrategy { None, GapClose, GapCloseHold1 }
    public enum ElusiveStrategy { None, CharacterForward, CharacterBackward, CameraForward, CameraBackward }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(DRG.AID.DragonsongDive);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: DRG", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.DRG), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.WingedGlide).As<DashStrategy>("Winged Glide", "W.Glide", 20)
            .AddOption(DashStrategy.None, "None", "No use.")
            .AddOption(DashStrategy.GapClose, "GapClose", "Use as gapcloser if outside melee range", 60, 0, ActionTargets.Hostile, 45)
            .AddOption(DashStrategy.GapCloseHold1, "GapCloseHold1", "Use as gapcloser if outside melee range; conserves 1 charge for manual usage", 60, 0, ActionTargets.Hostile, 84)
            .AddAssociatedActions(DRG.AID.WingedGlide);

        res.Define(Track.ElusiveJump).As<ElusiveStrategy>("Elusive Jump", "E.Jump", 30)
            .AddOption(ElusiveStrategy.None, "None", "No use.", 0, 0, ActionTargets.Self, 35)
            .AddOption(ElusiveStrategy.CharacterForward, "CharacterForward", "Dashes in the Forward direction relative to the Character", 30, 15, ActionTargets.Self, 35)
            .AddOption(ElusiveStrategy.CharacterBackward, "CharacterBackward", "Dashes in the Backward direction relative to the Character", 30, 15, ActionTargets.Self, 35)
            .AddOption(ElusiveStrategy.CameraForward, "CameraForward", "Dashes in the Forward direction relative to the Camera", 30, 15, ActionTargets.Self, 35)
            .AddOption(ElusiveStrategy.CameraBackward, "CameraBackward", "Dashes in the Backward direction relative to the Camera", 30, 15, ActionTargets.Self, 35)
            .AddAssociatedActions(DRG.AID.ElusiveJump);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);

        var ej = strategy.Option(Track.ElusiveJump);
        if (ej.As<ElusiveStrategy>() != ElusiveStrategy.None)
        {
            var angle = ej.As<ElusiveStrategy>() switch
            {
                ElusiveStrategy.CharacterForward => Player.Rotation + 180.Degrees(),
                ElusiveStrategy.CameraBackward => World.Client.CameraAzimuth + 180.Degrees(),
                ElusiveStrategy.CameraForward => World.Client.CameraAzimuth,
                _ => Player.Rotation
            };
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(DRG.AID.ElusiveJump), Player, ej.Priority(), ej.Value.ExpireIn, facingAngle: angle);
        }

        var dash = strategy.Option(Track.WingedGlide);
        var dashStrategy = strategy.Option(Track.WingedGlide).As<DashStrategy>();
        var dashTarget = ResolveTargetOverride(dash.Value) ?? primaryTarget; //Smart-Targeting
        var distance = Player.DistanceToHitbox(dashTarget);
        var cd = World.Client.Cooldowns[ActionDefinitions.Instance.Spell(DRG.AID.WingedGlide)!.MainCooldownGroup].Remaining;
        var shouldDash = dashStrategy switch
        {
            DashStrategy.None => false,
            DashStrategy.GapClose => distance is > 3 and <= 20 && cd <= 60.5f,
            DashStrategy.GapCloseHold1 => distance is > 3 and <= 20 && cd < 0.6f,
            _ => true,
        };
        if (shouldDash)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(DRG.AID.WingedGlide), dashTarget, dash.Priority(), dash.Value.ExpireIn);
    }
}
