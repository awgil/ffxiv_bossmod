#if DEBUG
using BossMod.Autorotation;

namespace BossMod.Dawntrail.Savage.RM04SWickedThunder.AI;

sealed class AIExperiment(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { ElectrifyingWitchHunt, WNWitchHunt, IonClusterPlatform }
    public enum ElectrifyingWitchHuntStrategy { None, NWNear }
    public enum WNWitchHuntStrategy { None, BaitFirstAny, BaitFirstNear }
    public enum IonClusterPlatformStrategy { None, MaxUptime }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("AI Experiment", "Experimental encounter-specific rotation", "veyn", RotationModuleQuality.WIP, new(~1ul), 100, 1, typeof(RM04SWickedThunder));
        res.Define(Track.ElectrifyingWitchHunt).As<ElectrifyingWitchHuntStrategy>("ElectrifyingWitchHunt", "EWH")
            .AddOption(ElectrifyingWitchHuntStrategy.None, "None", "Do nothing")
            .AddOption(ElectrifyingWitchHuntStrategy.NWNear, "NWNear", "NW prefer near (MT spot)");
        res.Define(Track.WNWitchHunt).As<WNWitchHuntStrategy>("WNWitchHuntStrategy", "WNWH")
            .AddOption(WNWitchHuntStrategy.None, "None", "Do nothing")
            .AddOption(WNWitchHuntStrategy.BaitFirstAny, "BaitFirstAny", "Bait first any")
            .AddOption(WNWitchHuntStrategy.BaitFirstNear, "BaitFirstNear", "Bait first near (first or second)");
        res.Define(Track.IonClusterPlatform).As<IonClusterPlatformStrategy>("IonClusterPlatformStrategy", "IonPlatform")
            .AddOption(IonClusterPlatformStrategy.None, "None", "Do nothing")
            .AddOption(IonClusterPlatformStrategy.MaxUptime, "MaxUptime", "Max uptime");
        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        if (Manager.Bossmods.ActiveModule is not RM04SWickedThunder module)
            return;

        var ewh = strategy.Option(Track.ElectrifyingWitchHunt).As<ElectrifyingWitchHuntStrategy>();
        if (ewh != ElectrifyingWitchHuntStrategy.None)
        {
            SetForcedMovement((module.StateMachine.ActiveState?.ID ?? 0) switch
            {
                0x00020000 => ElectrifyingWitchHuntPreTeleport(module, ewh),
                0x00020011 => ElectrifyingWitchHuntFirstDodge(module, ewh),
                0x00020111 => ElectrifyingWitchHuntSecondDodge(module, ewh),
                _ => ElectrifyingWitchHuntInitialPosition(module, ewh)
            });
        }

        var wnwh = strategy.Option(Track.WNWitchHunt).As<WNWitchHuntStrategy>();
        if (wnwh != WNWitchHuntStrategy.None)
        {
            SetForcedMovement((module.StateMachine.ActiveState?.ID ?? 0) switch
            {
                0x00030001 => WNWitchHuntDodge(module, wnwh, 0),
                0x00030010 => WNWitchHuntDodge(module, wnwh, 1),
                0x00030020 => WNWitchHuntDodge(module, wnwh, 2),
                0x00030030 => WNWitchHuntDodge(module, wnwh, 3),
                0x00030040 => WNWitchHuntDodge(module, wnwh, 4),
                _ => WNWitchHuntInitialPosition(module)
            });
        }

        var ionPlatform = strategy.Option(Track.IonClusterPlatform).As<IonClusterPlatformStrategy>();
        if (ionPlatform != IonClusterPlatformStrategy.None)
        {
            SetForcedMovement((module.StateMachine.ActiveState?.ID ?? 0) switch
            {
                < 0x00090020 => IonPlatformAOEs(module, ionPlatform, false),
                0x00090020 => IonPlatformAOEs(module, ionPlatform, true),
                _ => IonPlatformMid(module)
            });
        }
    }

    private void SetForcedMovement(WPos pos, float tolerance = 0.1f)
    {
        var dir = pos - Player.Position;
        Hints.ForcedMovement = dir.LengthSq() > tolerance * tolerance ? new(dir.X, Player.PosRot.Y, dir.Z) : default;
    }

    private float Speed() => Player.FindStatus(50) != null ? 7.8f : 6;

    // default spot: max melee at z=-5
    private WPos ElectrifyingWitchHuntInitialPosition(RM04SWickedThunder module, ElectrifyingWitchHuntStrategy strategy) => module.Center - new WDir(5.9f, 5);

    private WPos ElectrifyingWitchHuntPreTeleport(RM04SWickedThunder module, ElectrifyingWitchHuntStrategy strategy)
    {
        var off = Player.Position - module.Center;
        var distSq = off.LengthSq();
        return distSq > 7.9 * 7.9 ? module.Center + 7.5f * off / MathF.Sqrt(distSq) : Player.Position;
    }

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
            var timeToSafety = (uptimePos - downtimePos).Length() / Speed();
            return module.StateMachine.TimeSinceTransition + GCD + timeToSafety < (module.StateMachine.ActiveState?.Duration ?? 0) ? uptimePos : downtimePos;
        }
        return ElectrifyingWitchHuntInitialPosition(module, strategy);
    }

    private WPos ElectrifyingWitchHuntSecondDodge(RM04SWickedThunder module, ElectrifyingWitchHuntStrategy strategy)
    {
        var burst = module.FindComponent<ElectrifyingWitchHuntBurst>();
        var resolve = module.FindComponent<ElectrifyingWitchHuntResolve>();
        var defaultPos = ElectrifyingWitchHuntInitialPosition(module, strategy);
        if (burst?.Casters.Count() > 0 && resolve?.CurMechanic > ElectrifyingWitchHuntResolve.Mechanic.None)
        {
            var wantBait = !resolve.ForbidBait[Manager.PlayerSlot];
            var baitFar = resolve.CurMechanic == ElectrifyingWitchHuntResolve.Mechanic.Far;
            var goFar = wantBait == baitFar;
            var centerUnsafe = burst.Casters.Any(c => c.Position.X is > 98 and < 102);
            if (!centerUnsafe && !goFar)
                return defaultPos; // default is good enough for uptime...
            var vertShiftW = burst.Casters.Sum(c => c.Position.X - module.Center.X) < 0;
            var safeX = !centerUnsafe ? -5 : -16.2f + (vertShiftW ? -1.5f : +1.5f);
            var safeSpot = new WPos(module.Center.X + safeX, module.Center.Z - (goFar ? 12 : 3));
            var timeToSafety = (defaultPos - safeSpot).Length() / Speed();
            if (module.StateMachine.TimeSinceTransition + GCD + timeToSafety >= (module.StateMachine.ActiveState?.Duration ?? 0))
                return safeSpot;
        }
        return defaultPos;
    }

    private WPos WNWitchHuntInitialPosition(RM04SWickedThunder module) => module.Center - new WDir(0, 7);

    private WPos WNWitchHuntDodge(RM04SWickedThunder module, WNWitchHuntStrategy strategy, int nextBait)
    {
        var bait = module.FindComponent<WideningNarrowingWitchHuntBait>()?.CurMechanic ?? WideningNarrowingWitchHuntBait.Mechanic.None;
        var aoe = module.FindComponent<WideningNarrowingWitchHunt>();
        var defaultPos = WNWitchHuntInitialPosition(module);
        if (bait == default || aoe == null || aoe.NumCasts >= aoe.AOEs.Count)
            return defaultPos;

        var timeToDodge = (module.StateMachine.ActiveState?.Duration ?? 0) - module.StateMachine.TimeSinceTransition;
        if (nextBait == 0)
        {
            timeToDodge += 1.1f;
            ++nextBait;
        }

        var baitNear = bait == WideningNarrowingWitchHuntBait.Mechanic.Near;
        var wantBait = nextBait switch
        {
            1 => strategy == WNWitchHuntStrategy.BaitFirstAny || strategy == WNWitchHuntStrategy.BaitFirstNear && baitNear,
            2 => strategy == WNWitchHuntStrategy.BaitFirstNear && baitNear,
            _ => false
        };
        var wantNear = wantBait == baitNear;
        var wantIn = aoe.AOEs[aoe.NumCasts].Shape is AOEShapeDonut;

        var safeDist = wantIn ? (wantNear ? 4 : 7.7f) : (wantNear ? 10.3f : 12.5f);
        var angle = wantIn && !wantBait ? nextBait switch
        {
            2 => 135.Degrees(),
            4 => baitNear ? 180.Degrees() : -135.Degrees(),
            _ => 180.Degrees()
        } : 180.Degrees();
        var safePos = module.Center + safeDist * angle.ToDirection();
        if (wantIn)
            return safePos;
        var timeToSafety = (defaultPos - safePos).Length() / Speed();
        return GCD + timeToSafety >= timeToDodge ? safePos : defaultPos;
    }

    private WPos IonPlatformAOEs(RM04SWickedThunder module, IonClusterPlatformStrategy strategy, bool nextIsDeadly)
    {
        var thunder = module.FindComponent<StampedingThunder>();
        if (thunder?.AOE == null)
            return new(module.Center.X, module.Center.Z - 15);

        var offset = thunder.AOE.Value.Origin.X > module.Center.X ? -1 : 1;
        var uptimePos = module.PrimaryActor.Position + new WDir(offset * 7.8f, 0.1f);
        var downtimePos = module.PrimaryActor.Position + new WDir(offset * 10.2f, 0.1f);

        var timeToDodge = (module.StateMachine.ActiveState?.Duration ?? 0) - module.StateMachine.TimeSinceTransition;
        var timeToSafety = 2.4f / Speed();
        var goToDowntime = nextIsDeadly ? (GCD + timeToSafety >= timeToDodge) : (GCD > timeToDodge + timeToSafety);
        return goToDowntime ? downtimePos : uptimePos;
    }

    private WPos IonPlatformMid(RM04SWickedThunder module)
    {
        var thunder = module.FindComponent<StampedingThunder>();
        if (thunder?.SmallArena != true)
            return Player.Position;

        var offset = module.Arena.Bounds == RM04SWickedThunder.P1IonClusterRBounds ? 1 : -1;
        return module.Center + new WDir(offset * 10.4f, -6.2f);
    }
}
#endif
