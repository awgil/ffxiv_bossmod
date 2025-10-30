namespace BossMod.Autorotation.xan;

public sealed class Caster(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track { Raise, RaiseTarget }
    public enum RaiseStrategy
    {
        None,
        Swiftcast,
        Slowcast,
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Caster AI", "Auto-caster", "AI (xan)", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.ACN, Class.SMN, Class.RDM), 100);

        def.Define(Track.Raise).As<RaiseStrategy>("Raise")
            .AddOption(RaiseStrategy.None, "Don't automatically raise")
            .AddOption(RaiseStrategy.Swiftcast, "Raise using Swiftcast only")
            .AddOption(RaiseStrategy.Slowcast, "Allow raising without Swiftcast (not applicable to RDM)");

        def.Define(Track.RaiseTarget).As<RaiseUtil.Targets>("RaiseTargets", "Raise targets")
            .AddOption(RaiseUtil.Targets.Party, "Party members")
            .AddOption(RaiseUtil.Targets.Alliance, "Alliance raid members")
            .AddOption(RaiseUtil.Targets.Everyone, "Any dead player");

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var raise = strategy.Option(Track.Raise).As<RaiseStrategy>();
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

    private Actor? GetRaiseTarget(StrategyValues strategy) => RaiseUtil.FindRaiseTargets(World, strategy.Option(Track.RaiseTarget).As<RaiseUtil.Targets>()).FirstOrDefault();
}
