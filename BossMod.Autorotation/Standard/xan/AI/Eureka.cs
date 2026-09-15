namespace BossMod.Autorotation.xan;

public class EurekaAI(RotationModuleManager manager, Actor player) : AIBase<EurekaAI.Strategy>(manager, player)
{
    public struct Strategy
    {
        [Track("Ignore all AOEs while Platebearer is active", InternalName = "PB")]
        public Track<DisabledByDefault> Platebearer;
        [Track(Item = 22306)]
        public Track<EnabledByDefault> Potion;

        [Track("Auto-Dispel", InternalName = "Auto-Dispel L", Action = EurekaActionID.DispelL)]
        public Track<EnabledByDefault> Dispel;
        [Track("Auto-Feint L", InternalName = "Auto-Feint L", Action = EurekaActionID.FeintL)]
        public Track<EnabledByDefault> Feint;
        [Track("Auto-Bloodbath L", InternalName = "Auto-Bloodbath L", Action = EurekaActionID.BloodbathL)]
        public Track<EnabledByDefault> Bloodbath;
    }

    public enum PBIgnore
    {
        Disabled,
        Enabled
    }

    internal enum SID : uint
    {
        EvasionDown = 32,
        WisdomOfThePlatebearer = 1633
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Eureka AI", "Eureka utilities", "AI (xan)", "xan", RotationModuleQuality.WIP, new(~0ul), MaxLevel: 70).WithStrategies<Strategy>();
    }

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (strategy.Platebearer.IsEnabled() && Player.Statuses.Any(s => s.ID == (uint)SID.WisdomOfThePlatebearer))
            Hints.ForbiddenZones.Clear();

        if (strategy.Feint.IsEnabled() && HaveLogos(EurekaActionID.FeintL) && primaryTarget?.ForayInfo.Element == 3)
        {
            var feintLeft = primaryTarget.FindStatus(SID.EvasionDown, World.FutureTime(60)) is ActorStatus s ? (s.ExpireAt - World.CurrentTime).TotalSeconds : 0;
            if (feintLeft < NextChargeIn(EurekaActionID.FeintL))
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(EurekaActionID.FeintL), primaryTarget, ActionQueue.Priority.VeryHigh);
        }

        if (strategy.Dispel.IsEnabled() && HaveLogos(EurekaActionID.DispelL) && Hints.FindEnemy(primaryTarget)?.ShouldBeDispelled == true && primaryTarget?.PendingDispels.Count == 0)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(EurekaActionID.DispelL), primaryTarget, ActionQueue.Priority.VeryHigh);

        if (strategy.Bloodbath.IsEnabled() && HaveLogos(EurekaActionID.BloodbathL) && Player.InCombat && Player.PendingHPRatio < 0.5f)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(EurekaActionID.BloodbathL), Player, ActionQueue.Priority.Medium);

        if (strategy.Potion.IsEnabled() && InEureka && Player.InCombat && Player.PendingHPRatio < 0.75f)
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionEureka, Player, ActionQueue.Priority.VeryLow);
    }

    private bool HaveLogos(EurekaActionID id) => FindDutyActionSlot(id) >= 0;

    private bool InEureka => World.CurrentCFCID is 283 or 581 or 598 or 639;
}
