using BossMod.Data;

namespace BossMod.Autorotation.xan;

public class PhantomAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track
    {
        Cannoneer,
        Ranger,
        TimeMage,
        Chemist,
        Samurai,
        Bard,
        Monk
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
        def.AbilityTrack(Track.Samurai, "Samurai", "Samurai: Use Iainuki on best AOE target")
            .AddAssociatedActions(PhantomID.Iainuki);
        def.AbilityTrack(Track.Bard, "Bard", "Bard: Use Aria/Rime in combat")
            .AddAssociatedActions(PhantomID.OffensiveAria, PhantomID.HerosRime);
        def.AbilityTrack(Track.Monk, "Monk", "Monk: Use Kick to maintain buff; use Chakra at low HP/MP")
            .AddAssociatedActions(PhantomID.PhantomKick, PhantomID.OccultChakra);

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var isMidCombo = CheckMidCombo();

        if (strategy.Enabled(Track.Cannoneer) && !isMidCombo)
        {
            var prio = strategy.Option(Track.Cannoneer).Priority(ActionQueue.Priority.High + 500);

            var bestTarget = primaryTarget?.IsAlly == false ? primaryTarget : null;
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

        if (strategy.Enabled(Track.TimeMage) && primaryTarget?.IsAlly == false)
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
        if (canRaise && !isMidCombo && World.Client.DutyActions.Any(d => d.Action.ID == (uint)PhantomID.Revive))
        {
            var prio = option.Priority(ActionQueue.Priority.High + 500);
            if (RaiseUtil.FindRaiseTargets(World, RaiseUtil.Targets.Everyone).FirstOrDefault() is { } tar)
                UseAction(PhantomID.Revive, tar, prio);
        }

        if (strategy.Enabled(Track.Samurai) && primaryTarget?.IsAlly == false && !isMidCombo)
        {
            var prio = strategy.Option(Track.Samurai).Priority(ActionQueue.Priority.High + 500);
            if (UseAction(PhantomID.Iainuki, primaryTarget, prio, 0.8f))
                Hints.GoalZones.Add(Hints.GoalAOECone(primaryTarget, 8, 60.Degrees()));
        }

        if (strategy.Enabled(Track.Bard) && primaryTarget?.IsAlly == false && Player.InCombat)
        {
            var ariaLeft = SelfStatusDetails(4247, 70).Left;
            var rimeLeft = SelfStatusDetails(4249, 20).Left;
            var prio = strategy.Option(Track.Bard).Priority(ActionQueue.Priority.Low);

            UseAction(PhantomID.HerosRime, Player, prio);

            if (ariaLeft < 10 && rimeLeft < World.Client.AnimationLock)
                UseAction(PhantomID.OffensiveAria, Player, prio);
        }

        if (strategy.Enabled(Track.Monk) && primaryTarget?.IsAlly == false && Player.InCombat)
        {
            var prio = strategy.Option(Track.Monk).Priority(ActionQueue.Priority.Low);

            if (UseAction(PhantomID.PhantomKick, primaryTarget, prio))
                Hints.GoalZones.Add(Hints.GoalAOERect(primaryTarget, 15, 3));

            if (Player.HPMP.MaxHP * 0.3f >= Player.HPMP.CurHP || Player.HPMP.MaxMP * 0.3f >= Player.HPMP.CurMP)
                UseAction(PhantomID.OccultChakra, Player, prio);
        }
    }

    public static readonly uint[] InstantCastStatus = [
        (uint)ClassShared.SID.Swiftcast,
        (uint)BossMod.RDM.SID.Dualcast,
        (uint)BossMod.BLM.SID.Triplecast,
        (uint)BossMod.PLD.SID.Requiescat
    ];

    private bool UseAction(PhantomID pid, Actor target, float prio, float castTime = 0)
    {
        if (World.Client.DutyActions.Any(d => d.Action.ID == (uint)pid) && NextChargeIn(pid) <= GCD)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(pid), target, prio, castTime: castTime);
            return true;
        }
        return false;
    }

    public static readonly uint[] BreakableComboStatus = [
        (uint)BossMod.NIN.SID.Mudra,
        (uint)BossMod.NIN.SID.TenChiJin,
        (uint)BossMod.RDM.SID.Dualcast,
        (uint)BossMod.DRG.SID.DraconianFire,
        (uint)BossMod.RPR.SID.SoulReaver
    ];

    private bool CheckMidCombo()
    {
        return Player.Statuses.Any(s => BreakableComboStatus.Contains(s.ID));
    }
}
