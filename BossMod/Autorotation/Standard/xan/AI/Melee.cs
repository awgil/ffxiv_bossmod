namespace BossMod.Autorotation.xan;

public class MeleeAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track { SecondWind, Bloodbath, Stun, LimitBreak }
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Melee DPS AI", "Utilities for melee - bloodbath, second wind, stun", "AI (xan)", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.PGL, Class.MNK, Class.LNC, Class.DRG, Class.ROG, Class.NIN, Class.SAM, Class.RPR, Class.VPR), 100);

        def.AbilityTrack(Track.SecondWind, "Second Wind");
        def.AbilityTrack(Track.Bloodbath, "Bloodbath");
        def.AbilityTrack(Track.Stun, "Stun");
        def.AbilityTrack(Track.LimitBreak, "Limit Break").AddAssociatedActions(ClassShared.AID.Braver, ClassShared.AID.Bladedance);

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (Player.Statuses.Any(x => x.ID is (uint)BossMod.NIN.SID.TenChiJin or (uint)BossMod.NIN.SID.Mudra or 1092))
            return;

        // second wind
        if (strategy.Enabled(Track.SecondWind) && Player.InCombat && Player.PendingHPRatio <= 0.5)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.SecondWind), Player, ActionQueue.Priority.Medium);

        // bloodbath
        if (strategy.Enabled(Track.Bloodbath) && Player.InCombat && Player.PendingHPRatio <= 0.3)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Bloodbath), Player, ActionQueue.Priority.Medium);

        // low blow
        if (strategy.Enabled(Track.Stun) && NextChargeIn(ClassShared.AID.LegSweep) == 0)
        {
            var stunnableEnemy = Hints.PotentialTargets.FirstOrDefault(e => ShouldStun(e) && Player.DistanceToHitbox(e.Actor) <= 3);
            if (stunnableEnemy != null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.LegSweep), stunnableEnemy.Actor, ActionQueue.Priority.Minimal);
        }

        if (Player.FindStatus(2324) != null && Bossmods.ActiveModule?.Info?.GroupType is BossModuleInfo.GroupType.BozjaDuel)
        {
            var gcdLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);
            var fopLeft = Player.FindStatus(2346) is ActorStatus st ? StatusDuration(st.ExpireAt) : 0;
            if (GCD + gcdLength < fopLeft)
                Hints.ActionsToExecute.Push(BozjaActionID.GetNormal(BozjaHolsterID.LostAssassination), primaryTarget, ActionQueue.Priority.Low);
        }

        ExecLB(strategy, primaryTarget);
    }

    private void ExecLB(StrategyValues strategy, Actor? primaryTarget)
    {
        if (!strategy.Enabled(Track.LimitBreak) || World.Party.WithoutSlot(includeDead: true).Count(x => x.Type == ActorType.Player) > 1 || Bossmods.ActiveModule is null)
            return;

        switch (World.Party.LimitBreakLevel)
        {
            case 1:
                break;
            case 2:
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Bladedance), primaryTarget, ActionQueue.Priority.VeryHigh, castTime: 3);
                break;
            case 3:
                var lb3 = Player.Class switch
                {
                    Class.PGL or Class.MNK => ClassMNKUtility.IDLimitBreak3,
                    Class.LNC or Class.DRG => ClassDRGUtility.IDLimitBreak3,
                    Class.ROG or Class.NIN => ClassNINUtility.IDLimitBreak3,
                    Class.SAM => ClassSAMUtility.IDLimitBreak3,
                    Class.RPR => ActionID.MakeSpell(BossMod.RPR.AID.TheEnd),
                    Class.VPR => ActionID.MakeSpell(BossMod.VPR.AID.WorldSwallower),
                    _ => default
                };
                if (lb3 != default)
                    Hints.ActionsToExecute.Push(lb3, primaryTarget, ActionQueue.Priority.VeryHigh, castTime: 4.5f);
                break;
        }
    }
}
