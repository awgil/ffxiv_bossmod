using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public class HealerAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track { Raise, RaiseTarget, Heal }
    public enum RaiseStrategy
    {
        None,
        Swiftcast,
        Slowcast,
        Hardcast,
    }
    public enum RaiseTarget
    {
        Party,
        Alliance,
        Everyone
    }

    public ActionID RaiseAction => Player.Class switch
    {
        Class.CNJ or Class.WHM => ActionID.MakeSpell(BossMod.WHM.AID.Raise),
        Class.ACN or Class.SCH => ActionID.MakeSpell(BossMod.SCH.AID.Resurrection),
        Class.AST => ActionID.MakeSpell(BossMod.AST.AID.Ascend),
        Class.SGE => ActionID.MakeSpell(BossMod.SGE.AID.Egeiro),
        _ => default
    };

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Healer AI", "Auto-healer", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.CNJ, Class.WHM, Class.ACN, Class.SCH, Class.SGE, Class.AST), 100);

        def.Define(Track.Raise).As<RaiseStrategy>("Raise")
            .AddOption(RaiseStrategy.None, "Don't automatically raise")
            .AddOption(RaiseStrategy.Swiftcast, "Raise using Swiftcast only")
            .AddOption(RaiseStrategy.Slowcast, "Raise without requiring Swiftcast to be available")
            .AddOption(RaiseStrategy.Hardcast, "Never use Swiftcast to raise");

        def.Define(Track.RaiseTarget).As<RaiseTarget>("RaiseTargets")
            .AddOption(RaiseTarget.Party, "Party members")
            .AddOption(RaiseTarget.Alliance, "Alliance raid members")
            .AddOption(RaiseTarget.Everyone, "Any dead player");

        def.AbilityTrack(Track.Heal, "Heal");

        return def;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        AutoRaise(strategy);

        if (strategy.Enabled(Track.Heal))
            switch (Player.Class)
            {
                case Class.WHM:
                    AutoWHM(strategy);
                    break;
            }
    }

    private void AutoRaise(StrategyValues strategy)
    {
        var swiftcast = StatusDetails(Player, (uint)BossMod.WHM.SID.Swiftcast, Player.InstanceID, 15).Left;
        var thinair = StatusDetails(Player, (uint)BossMod.WHM.SID.ThinAir, Player.InstanceID, 12).Left;
        var swiftcastCD = NextChargeIn(BossMod.WHM.AID.Swiftcast);
        var raise = strategy.Option(Track.Raise).As<RaiseStrategy>();

        void UseThinAir()
        {
            if (thinair == 0 && Player.Class == Class.WHM)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.WHM.AID.ThinAir), Player, ActionQueue.Priority.VeryHigh + 3);
        }

        switch (raise)
        {
            case RaiseStrategy.None:
                break;
            case RaiseStrategy.Hardcast:
                if (swiftcast == 0 && GetRaiseTarget(strategy) is Actor tar)
                {
                    UseThinAir();
                    Hints.ActionsToExecute.Push(RaiseAction, tar, ActionQueue.Priority.VeryHigh);
                }
                break;
            case RaiseStrategy.Swiftcast:
                if (GetRaiseTarget(strategy) is Actor tar2)
                {
                    if (swiftcast > GCD)
                    {
                        UseThinAir();
                        Hints.ActionsToExecute.Push(RaiseAction, tar2, ActionQueue.Priority.VeryHigh);
                    }
                    else
                        Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.WHM.AID.Swiftcast), Player, ActionQueue.Priority.VeryHigh);
                }
                break;
            case RaiseStrategy.Slowcast:
                if (GetRaiseTarget(strategy) is Actor tar3)
                {
                    UseThinAir();
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.WHM.AID.Swiftcast), Player, ActionQueue.Priority.VeryHigh + 2);
                    if (swiftcastCD > 8)
                        Hints.ActionsToExecute.Push(RaiseAction, tar3, ActionQueue.Priority.VeryHigh + 1);
                }
                break;
        }
    }

    private Actor? GetRaiseTarget(StrategyValues strategy)
    {
        var candidates = strategy.Option(Track.RaiseTarget).As<RaiseTarget>() switch
        {
            RaiseTarget.Everyone => World.Actors.Where(x => x.Type is ActorType.Player or ActorType.DutySupport && x.IsAlly),
            RaiseTarget.Alliance => World.Party.WithoutSlot(true, false),
            _ => World.Party.WithoutSlot(true, true)
        };

        return candidates.Where(x => x.IsDead && Player.DistanceToHitbox(x) <= 30 && !BeingRaised(x)).MaxBy(actor => actor.Class.GetRole() switch
        {
            Role.Healer => 5,
            Role.Tank => 4,
            _ => actor.Class is Class.RDM or Class.SMN or Class.ACN ? 3 : 2
        });
    }

    private static bool BeingRaised(Actor actor) => actor.Statuses.Any(s => s.ID is 148 or 1140);

    private void AutoWHM(StrategyValues strategy)
    {
        var bestSTHealTarget = World.Party.WithoutSlot(false, true).MinBy(PredictedHPRatio)!;
        if (PredictedHPRatio(bestSTHealTarget) < 0.25)
        {
            if (World.Client.GetGauge<WhiteMageGauge>().Lily > 0)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.WHM.AID.AfflatusSolace), bestSTHealTarget, ActionQueue.Priority.VeryHigh);

            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.WHM.AID.Tetragrammaton), bestSTHealTarget, ActionQueue.Priority.Medium);
        }
    }
}
