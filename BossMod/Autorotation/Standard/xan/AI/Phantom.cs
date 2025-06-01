using BossMod.Data;

namespace BossMod.Autorotation.xan;

public class PhantomAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track
    {
        Cannoneer,
        Ranger
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Phantom Job AI", "Basic phantom job action automation", "AI (xan)", "xan", RotationModuleQuality.WIP, new(~0ul), MaxLevel: 100);

        def.AbilityTrack(Track.Cannoneer, "CAN", "Cannoneer: Use cannon actions on best AOE target as soon as they become available")
            .AddAssociatedActions(PhantomID.DarkCannon, PhantomID.HolyCannon, PhantomID.ShockCannon, PhantomID.SilverCannon, PhantomID.PhantomFire);
        def.AbilityTrack(Track.Ranger, "RAN", "Ranger: Use Phantom Aim on cooldown")
            .AddAssociatedActions(PhantomID.PhantomAim);

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (strategy.Enabled(Track.Cannoneer))
        {
            var prio = strategy.Option(Track.Cannoneer).Priority(ActionQueue.Priority.High + 500);

            var best = Hints.PriorityTargets.Where(x => Player.DistanceToHitbox(x.Actor) <= 30).MaxBy(t => Hints.NumPriorityTargetsInAOECircle(t.Actor.Position, 5))?.Actor ?? primaryTarget;
            if (best != null)
            {
                UseAction(PhantomID.PhantomFire, best, prio);

                UseAction(PhantomID.SilverCannon, best, prio); // shares CD with holy
                UseAction(PhantomID.ShockCannon, best, prio); // shares CD with dark

                UseAction(PhantomID.HolyCannon, best, prio);
                UseAction(PhantomID.DarkCannon, best, prio);
            }
        }

        if (strategy.Enabled(Track.Ranger) && primaryTarget?.IsAlly == false && Player.InCombat)
        {
            var prio = strategy.Option(Track.Ranger).Priority(ActionQueue.Priority.Low);

            UseAction(PhantomID.PhantomAim, Player, prio);
        }
    }

    //private void UseGCD(PhantomID pid, Actor target, float prio = ActionQueue.Priority.High) => UseAction(pid, target, prio);
    //private void UseOGCD(PhantomID pid, Actor target, float prio = ActionQueue.Priority.Low) => UseAction(pid, target, prio);

    private void UseAction(PhantomID pid, Actor target, float prio)
    {
        if (World.Client.DutyActions.Any(d => d.Action.ID == (uint)pid) && NextChargeIn(pid) <= GCD)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(pid), target, prio);
    }
}
