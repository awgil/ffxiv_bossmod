namespace BossMod.Autorotation.xan;

public class EurekaAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track { Dispel, Feint, Platebearer }

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
        var def = new RotationModuleDefinition("Eureka AI", "Eureka utilities", "AI (xan)", "xan", RotationModuleQuality.WIP, new(~0ul), MaxLevel: 70, Order: RotationModuleOrder.HighLevel);

        def.AbilityTrack(Track.Dispel, "Auto-Dispel L").AddAssociatedActions(EurekaActionID.DispelL);
        def.AbilityTrack(Track.Feint, "Auto-Feint L").AddAssociatedActions(EurekaActionID.FeintL);
        def.Define(Track.Platebearer).As<PBIgnore>("PB", "Ignore all AOEs while Platebearer is active")
            .AddOption(PBIgnore.Disabled, "Disabled")
            .AddOption(PBIgnore.Enabled, "Enabled");

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (strategy.Option(Track.Platebearer).As<PBIgnore>() == PBIgnore.Enabled && Player.Statuses.Any(s => s.ID == (uint)SID.WisdomOfThePlatebearer))
            Hints.ForbiddenZones.Clear();

        if (strategy.Enabled(Track.Feint) && HaveLogos(EurekaActionID.FeintL) && primaryTarget is { } p1 && p1.ForayInfo.Element == 3)
        {
            var feintLeft = p1.FindStatus(SID.EvasionDown, World.FutureTime(60)) is ActorStatus s ? (s.ExpireAt - World.CurrentTime).TotalSeconds : 0;
            if (feintLeft < NextChargeIn(EurekaActionID.FeintL))
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(EurekaActionID.FeintL), p1, ActionQueue.Priority.VeryHigh);
        }

        if (strategy.Enabled(Track.Dispel) && HaveLogos(EurekaActionID.DispelL) && primaryTarget is { } p2 && Hints.FindEnemy(p2)?.ShouldBeDispelled == true && p2.PendingDispels.Count == 0)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(EurekaActionID.DispelL), p2, ActionQueue.Priority.VeryHigh);
    }

    private bool HaveLogos(EurekaActionID id) => World.Client.DutyActions.Any(d => d.Action.ID == (uint)id);
}
