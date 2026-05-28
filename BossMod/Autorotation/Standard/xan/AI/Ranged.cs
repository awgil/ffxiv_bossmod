namespace BossMod.Autorotation.xan;

public class RangedAI(RotationModuleManager manager, Actor player) : AIBase<RangedAI.Strategy>(manager, player)
{
    public struct Strategy
    {
        [Track("Head Graze", InternalName = "Head Graze", Action = ClassShared.AID.HeadGraze)]
        public Track<EnabledByDefault> Interrupt;
        [Track("Second Wind", InternalName = "Second Wind", Action = ClassShared.AID.SecondWind)]
        public Track<EnabledByDefault> SecondWind;
        [Track("Limit Break", InternalName = "Limit Break", Actions = [ClassShared.AID.Desperado, ClassShared.AID.BigShot])]
        public Track<EnabledByDefault> LimitBreak;
    }
    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Phys Ranged AI", "Utilities for physical ranged dps - peloton, interrupt, defensive abilities", "AI (xan)", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.ARC, Class.BRD, Class.MCH, Class.DNC), 100).WithStrategies<Strategy>();
    }

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        // interrupt
        if (strategy.Interrupt.IsEnabled() && NextChargeIn(ClassShared.AID.HeadGraze) == 0)
        {
            var interruptibleEnemy = Hints.PotentialTargets.FirstOrDefault(e => ShouldInterrupt(e) && Player.DistanceToHitbox(e.Actor) <= 25);
            if (interruptibleEnemy != null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.HeadGraze), interruptibleEnemy.Actor, ActionQueue.Priority.High);
        }

        // second wind
        if (strategy.SecondWind.IsEnabled() && Player.InCombat && Player.PendingHPRatio <= 0.5)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.SecondWind), Player, ActionQueue.Priority.Medium);

        ExecLB(strategy, primaryTarget);

        if (ActionUnlocked(ActionID.MakeSpell(BossMod.BRD.AID.WardensPaean)) && NextChargeIn(BossMod.BRD.AID.WardensPaean) == 0 && ActionDefinitions.FindEsunaTarget(World) is Actor tar)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.BRD.AID.WardensPaean), tar, ActionQueue.Priority.Low);
    }

    private void ExecLB(in Strategy strategy, Actor? primaryTarget)
    {
        Actor? lbTarget(float halfWidth) => FindBetterTargetBy(primaryTarget, 30, actor => Hints.NumPriorityTargetsInAOERect(Player.Position, Player.DirectionTo(actor), 30, halfWidth)).Target;

        if (!strategy.LimitBreak.IsEnabled() || World.Party.WithoutSlot(includeDead: true).Count(x => x.Type == ActorType.Player) > 1)
            return;

        var bars = World.Party.LimitBreakLevel;
        switch (bars)
        {
            case 1:
                if (lbTarget(2) is Actor a)
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.BigShot), a, ActionQueue.Priority.VeryHigh, castTime: 2);
                break;
            case 2:
                if (lbTarget(2.5f) is Actor b)
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Desperado), b, ActionQueue.Priority.VeryHigh, castTime: 3);
                break;
            case 3:
                var lb3 = Player.Class switch
                {
                    Class.ARC or Class.BRD => ClassBRDUtility.IDLimitBreak3,
                    Class.MCH => ClassMCHUtility.IDLimitBreak3,
                    Class.DNC => ClassDNCUtility.IDLimitBreak3,
                    _ => default
                };
                if (lbTarget(4) is Actor c && lb3 != default)
                    Hints.ActionsToExecute.Push(lb3, c, ActionQueue.Priority.VeryHigh, castTime: 4.5f);
                break;
        }
    }
}
