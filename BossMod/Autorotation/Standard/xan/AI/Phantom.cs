using BossMod.Data;

namespace BossMod.Autorotation.xan;

public class PhantomAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track
    {
        Cannoneer,
        Ranger,
        TimeMage,
        Chemist
    }

    public enum RaiseStrategy
    {
        Never,
        OutOfCombat,
        InCombat
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Phantom Job AI", "Basic phantom job action automation", "AI (xan)", "xan", RotationModuleQuality.WIP, new(~0ul), MaxLevel: 100);

        def.AbilityTrack(Track.Cannoneer, "Cannoneer", "Cannoneer: Use cannon actions on best AOE target as soon as they become available")
            .AddAssociatedActions(PhantomID.DarkCannon, PhantomID.HolyCannon, PhantomID.ShockCannon, PhantomID.SilverCannon, PhantomID.PhantomFire);
        def.AbilityTrack(Track.Ranger, "Ranger", "Ranger: Use Phantom Aim on cooldown")
            .AddAssociatedActions(PhantomID.PhantomAim);
        def.AbilityTrack(Track.TimeMage, "TimeMage", "Time Mage: Use Comet ASAP if it will be instant")
            .AddAssociatedActions(PhantomID.OccultComet);
        def.Define(Track.Chemist).As<RaiseStrategy>("Chemist", "Chemist raise")
            .AddOption(RaiseStrategy.Never, "Never", "Disabled")
            .AddOption(RaiseStrategy.OutOfCombat, "OutOfCombat", "Out of combat")
            .AddOption(RaiseStrategy.InCombat, "InCombat", "Always")
            .AddAssociatedActions(PhantomID.Revive);

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (strategy.Enabled(Track.Cannoneer) && !CheckMidCombo())
        {
            var prio = strategy.Option(Track.Cannoneer).Priority(ActionQueue.Priority.High + 500);

            var bestTarget = primaryTarget;
            var bestCount = bestTarget == null ? 0 : Hints.NumPriorityTargetsInAOECircle(bestTarget.Position, 5);
            foreach (var tar in Hints.PriorityTargets.Where(x => Player.DistanceToHitbox(x.Actor) <= 30))
            {
                if (tar.Actor == bestTarget)
                    continue;

                var cnt = Hints.NumPriorityTargetsInAOECircle(tar.Actor.Position, 5);
                if (cnt > bestCount)
                {
                    bestTarget = tar.Actor;
                    bestCount = cnt;
                }
            }

            if (bestTarget != null)
            {
                UseAction(PhantomID.SilverCannon, bestTarget, prio); // shares CD with holy
                UseAction(PhantomID.ShockCannon, bestTarget, prio); // shares CD with dark

                // use after silver for the extra damage from silver debuff
                UseAction(PhantomID.PhantomFire, bestTarget, prio);

                UseAction(PhantomID.HolyCannon, bestTarget, prio);
                UseAction(PhantomID.DarkCannon, bestTarget, prio);
            }
        }

        if (strategy.Enabled(Track.Ranger) && primaryTarget?.IsAlly == false && Player.InCombat)
        {
            var prio = strategy.Option(Track.Ranger).Priority(ActionQueue.Priority.Low);

            UseAction(PhantomID.PhantomAim, Player, prio);
        }

        if (strategy.Enabled(Track.TimeMage) && primaryTarget != null)
        {
            var prio = strategy.Option(Track.TimeMage).Priority(ActionQueue.Priority.High + 500);

            var nextGCD = World.FutureTime(GCD);
            var haveSwift = Player.Statuses.Any(s => InstantCastStatus.Contains(s.ID) && s.ExpireAt > nextGCD);
            if (haveSwift)
                UseAction(PhantomID.OccultComet, primaryTarget, prio);
        }

        var option = strategy.Option(Track.Chemist);
        var canRaise = option.As<RaiseStrategy>() switch
        {
            RaiseStrategy.InCombat => true,
            RaiseStrategy.OutOfCombat => !Player.InCombat,
            _ => false
        };

        // check that we have Revive to avoid pointlessly scanning the object table again
        if (canRaise && World.Client.DutyActions.Any(d => d.Action.ID == (uint)PhantomID.Revive))
        {
            var prio = option.Priority(ActionQueue.Priority.High + 500);
            if (RaiseUtil.FindRaiseTargets(World, RaiseUtil.Targets.Everyone).FirstOrDefault() is { } tar)
                UseAction(PhantomID.Revive, tar, prio);
        }
    }

    public static readonly uint[] InstantCastStatus = [
        (uint)ClassShared.SID.Swiftcast,
        (uint)BossMod.RDM.SID.Dualcast,
        (uint)BossMod.BLM.SID.Triplecast,
        (uint)BossMod.PLD.SID.Requiescat
    ];

    private void UseAction(PhantomID pid, Actor target, float prio)
    {
        if (World.Client.DutyActions.Any(d => d.Action.ID == (uint)pid) && NextChargeIn(pid) <= GCD)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(pid), target, prio);
    }

    private bool CheckMidCombo()
    {
        // cannons break ninjutsu, TCJ prevents most GCDs entirely
        if (Player.Statuses.Any(s => (BossMod.NIN.SID)s.ID is BossMod.NIN.SID.Mudra or BossMod.NIN.SID.TenChiJin))
            return true;

        // TODO: check reaper soul reaver or whatever it's called
        return false;
    }
}
