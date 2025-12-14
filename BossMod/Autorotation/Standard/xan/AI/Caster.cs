namespace BossMod.Autorotation.xan;

public sealed class Caster(RotationModuleManager manager, Actor player) : AIBase<Caster.Strategy>(manager, player)
{
    public struct Strategy
    {
        public Track<RaiseStrategy> Raise;
        [Track("Raise targets")]
        public Track<RaiseUtil.Targets> RaiseTargets;
    }

    public enum Track { Raise, RaiseTarget }
    public enum RaiseStrategy
    {
        [Option("Don't automatically raise")]
        None,
        [Option("Raise using Swiftcast only")]
        Swiftcast,
        [Option("Allow raising without Swiftcast (not applicable for RDM)")]
        Slowcast,
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Caster AI", "Auto-caster", "AI (xan)", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.ACN, Class.SMN, Class.RDM), 100).WithStrategies<Strategy>();
    }

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var raise = strategy.Raise.Value;
        if (raise == RaiseStrategy.None)
            return;

        // set of all statuses called "Resurrection Restricted"
        // TODO maybe this is a flag in sheets somewhere
        if (Player.Statuses.Any(s => s.ID is 1755 or 2449 or 3380))
            return;

        var target = GetRaiseTarget(strategy);
        if (target == null)
            return; // nobody to raise

        var swiftcastCD = NextChargeIn(ClassShared.AID.Swiftcast);
        var swiftcast = StatusDetails(Player, (uint)ClassShared.SID.Swiftcast, Player.InstanceID, 15).Left;

        if (Player.Class is Class.ACN or Class.SMN)
        {
            var res = ActionID.MakeSpell(BossMod.SMN.AID.Resurrection);
            var def = ActionDefinitions.Instance[res];

            if (raise == RaiseStrategy.Slowcast)
            {
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Swiftcast), Player, ActionQueue.Priority.High + 600);
                if (swiftcastCD > 8)
                    Hints.ActionsToExecute.Push(res, target, ActionQueue.Priority.High + 500, castTime: swiftcast > GCD ? 0 : def!.CastTime - 0.5f);
            }
            else // strategy = swiftcast only
            {
                if (swiftcast > GCD)
                    Hints.ActionsToExecute.Push(res, target, ActionQueue.Priority.High + 500);
                else
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Swiftcast), Player, ActionQueue.Priority.High + 600);
            }
        }

        if (Player.Class == Class.RDM)
        {
            var swift2 = MathF.Max(swiftcast, StatusDetails(Player, (uint)BossMod.RDM.SID.Dualcast, Player.InstanceID, 15).Left);
            // instant cast available now
            if (swift2 > GCD)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.RDM.AID.Verraise), target, ActionQueue.Priority.High + 500);

            // swiftcast will be available sooner than vercure would finish casting
            else if (swiftcastCD < 2.5f)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Swiftcast), Player, ActionQueue.Priority.High + 600);

            // use vercure for dualcast
            else
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.RDM.AID.Vercure), Player, ActionQueue.Priority.High + 500, castTime: 1.5f);
        }
    }

    private Actor? GetRaiseTarget(in Strategy strategy) => RaiseUtil.FindRaiseTargets(World, strategy.RaiseTargets.Value).FirstOrDefault();
}
