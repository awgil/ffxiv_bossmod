#if DEBUG
using BossMod.Autorotation;

namespace BossMod.Dawntrail.Savage.RM04SWickedThunder.AI;

sealed class AIExperiment(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { ElectrifyingWitchHunt }
    public enum ElectrifyingWitchHuntStrategy { None, NWNear }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("AI Experiment", "Experimental encounter-specific rotation", "veyn", RotationModuleQuality.WIP, new(~1ul), 100, 1, typeof(RM04SWickedThunder));
        res.Define(Track.ElectrifyingWitchHunt).As<ElectrifyingWitchHuntStrategy>("ElectrifyingWitchHunt", "EWH")
            .AddOption(ElectrifyingWitchHuntStrategy.None, "None", "Do nothing")
            .AddOption(ElectrifyingWitchHuntStrategy.NWNear, "NWNear", "NW prefer near (MT spot)");
        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        if (Manager.Bossmods.ActiveModule is not RM04SWickedThunder module)
            return;

        var ewh = strategy.Option(Track.ElectrifyingWitchHunt).As<ElectrifyingWitchHuntStrategy>();
        if (ewh != ElectrifyingWitchHuntStrategy.None)
        {
            var pos = CalcElectrifyingWitchHuntPosition(module, ewh);
            var dir = pos - Player.Position;
            Hints.ForcedMovement = dir.LengthSq() > 0.01f ? dir.ToVec3() : default;
        }
    }

    private WPos CalcElectrifyingWitchHuntPosition(RM04SWickedThunder module, ElectrifyingWitchHuntStrategy strategy)
    {
        return (module.StateMachine.ActiveState?.ID ?? 0) switch
        {
            0x00020011 => ElectrifyingWitchHuntFirstDodge(module, strategy),
            0x00020111 => ElectrifyingWitchHuntSecondDodge(module, strategy),
            _ => ElectrifyingWitchHuntInitialPosition(module, strategy)
        };
    }

    // default spot: max melee at z=-5
    private WPos ElectrifyingWitchHuntInitialPosition(RM04SWickedThunder module, ElectrifyingWitchHuntStrategy strategy) => module.Center - new WDir(5.9f, 5);

    private WPos ElectrifyingWitchHuntFirstDodge(RM04SWickedThunder module, ElectrifyingWitchHuntStrategy strategy)
    {
        var burst = module.FindComponent<ElectrifyingWitchHuntBurst>();
        var electray = module.FindComponent<Electray>();
        var electrayH = electray?.AOEs.FirstOrDefault(aoe => aoe.Origin.Z is > 95 and < 105) ?? default;
        if (burst?.Casters.Count > 0 && electrayH.Shape != null)
        {
            var horizNSafe = electrayH.Origin.Z < 100;
            var uptimePos = ElectrifyingWitchHuntInitialPosition(module, strategy) + new WDir(0, horizNSafe ? -0.2f : +0.2f);
            var centerUnsafe = burst.Casters.Any(c => c.Position.X is > 98 and < 102);
            if (!centerUnsafe)
                return uptimePos;
            var vertShiftW = burst.Casters.Sum(c => c.Position.X - module.Center.X) < 0;
            var downtimePos = uptimePos with { X = module.Center.X - 16.2f + (vertShiftW ? -1.5f : +1.5f) };
            var timeToSafety = (Player.Position - downtimePos).Length() / 6;
            return module.StateMachine.TimeSinceTransition + GCD + timeToSafety < (module.StateMachine.ActiveState?.Duration ?? 0) ? uptimePos : downtimePos;
        }
        return ElectrifyingWitchHuntInitialPosition(module, strategy);
    }

    private WPos ElectrifyingWitchHuntSecondDodge(RM04SWickedThunder module, ElectrifyingWitchHuntStrategy strategy)
    {
        var burst = module.FindComponent<ElectrifyingWitchHuntBurst>();
        var resolve = module.FindComponent<ElectrifyingWitchHuntResolve>();
        if (burst?.Casters.Count() > 0 && resolve?.CurMechanic > ElectrifyingWitchHuntResolve.Mechanic.None)
        {
            var wantBait = !resolve.ForbidBait[Manager.PlayerSlot];
            var baitFar = resolve.CurMechanic == ElectrifyingWitchHuntResolve.Mechanic.Far;
            var goFar = wantBait == baitFar;
            var centerUnsafe = burst.Casters.Any(c => c.Position.X is > 98 and < 102);
            var vertShiftW = burst.Casters.Sum(c => c.Position.X - module.Center.X) < 0;
            var safeX = !centerUnsafe ? -5 : -16.2f + (vertShiftW ? -1.5f : +1.5f);
            var safeSpot = new WPos(module.Center.X + safeX, module.Center.Z - (goFar ? 12 : 3));
            var timeToSafety = (Player.Position - safeSpot).Length() / 6;
            if (module.StateMachine.TimeSinceTransition + GCD + timeToSafety >= (module.StateMachine.ActiveState?.Duration ?? 0))
                return safeSpot;
        }
        return ElectrifyingWitchHuntInitialPosition(module, strategy);
    }
}
#endif
