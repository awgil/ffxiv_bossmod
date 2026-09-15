using BossMod.AI;
using BossMod.Autorotation;

namespace BossMod.Dawntrail.Savage.RM02SHoneyBLovely.AI;

sealed class AIExperiment(RotationModuleManager manager, Actor player) : AIRotationModule(manager, player)
{
    public enum Track { DragBoss, DropSplash, StageCombo, Pheromones2, DefamationsTowers }
    public enum DragBossStrategy { None, SCenterFaceN, NorthFaceS }
    public enum DropSplashStrategy { None, NWMeleeCW }
    public enum StageComboStrategy { None, NNW, NNWOrHeart }
    public enum Pheromones2Strategy { None, Nearest }
    public enum DefamationsTowersStrategy { None, NNW }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("AI Experiment", "Experimental encounter-specific rotation", "Encounter AI", "veyn", RotationModuleQuality.WIP, new(~1ul), 100, 1, RotationModuleOrder.Movement, typeof(RM02SHoneyBLovely), DevMode: true);
        res.Define(Track.DragBoss).As<DragBossStrategy>("DragBoss", "Drag")
            .AddOption(DragBossStrategy.None, "Do nothing")
            .AddOption(DragBossStrategy.SCenterFaceN, "Position boss in center or slightly to the south, then face north")
            .AddOption(DragBossStrategy.NorthFaceS, "Position boss in the north, then face center");
        res.Define(Track.DropSplash).As<DropSplashStrategy>("DropSplash")
            .AddOption(DropSplashStrategy.None, "Do nothing")
            .AddOption(DropSplashStrategy.NWMeleeCW, "NW, melee/CW");
        res.Define(Track.StageCombo).As<StageComboStrategy>("StageCombo")
            .AddOption(StageComboStrategy.None, "Do nothing")
            .AddOption(StageComboStrategy.NNW, "N/NW")
            .AddOption(StageComboStrategy.NNWOrHeart, "N/NW, keep uptime if <3 hearts");
        res.Define(Track.Pheromones2).As<Pheromones2Strategy>("Pheromones2", "Phero2")
            .AddOption(Pheromones2Strategy.None, "Do nothing")
            .AddOption(Pheromones2Strategy.Nearest, "Go to nearest safespot");
        res.Define(Track.DefamationsTowers).As<DefamationsTowersStrategy>("DefamationsTowers", "D/T")
            .AddOption(DefamationsTowersStrategy.None, "Do nothing")
            .AddOption(DefamationsTowersStrategy.NNW, "Defamations N, towers NW");
        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (Bossmods.ActiveModule is not RM02SHoneyBLovely module)
            return;

        var drag = strategy.Option(Track.DragBoss).As<DragBossStrategy>();
        if (drag != default)
        {
            SetForcedMovement(GetDragBossPosition(module, drag));
        }

        var dropSplash = strategy.Option(Track.DropSplash).As<DropSplashStrategy>();
        if (dropSplash != default)
        {
            SetForcedMovement(GetDropSplashPosition(module, dropSplash));
        }

        var stageCombo = strategy.Option(Track.StageCombo).As<StageComboStrategy>();
        if (stageCombo != default)
        {
            SetForcedMovement(GetStageComboPosition(module, stageCombo));
        }

        var pheromones2 = strategy.Option(Track.Pheromones2).As<Pheromones2Strategy>();
        if (pheromones2 != default)
        {
            SetForcedMovement(GetPheromones2Position(module, pheromones2, out var wantSprint));
            if (wantSprint)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), Player, ActionQueue.Priority.Low);
        }

        var defamationsTowers = strategy.Option(Track.DefamationsTowers).As<DefamationsTowersStrategy>();
        if (defamationsTowers != default)
        {
            SetForcedMovement(GetDefamationsTowersPosition(module, defamationsTowers, out var wantSprint));
            if (wantSprint)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), Player, ActionQueue.Priority.Low);
        }
    }

    private WPos? GetDragBossPosition(RM02SHoneyBLovely module, DragBossStrategy strategy) => strategy switch
    {
        DragBossStrategy.SCenterFaceN => module.PrimaryActor.Position.Z < module.Center.Z ? MoveTarget(module.PrimaryActor, module.Center, GCD) : module.PrimaryActor.Position + new WDir(0, -5),
        DragBossStrategy.NorthFaceS => module.PrimaryActor.Position.Z > module.Center.Z - 10 ? MoveTarget(module.PrimaryActor, module.Center - new WDir(0, 10), GCD) : module.Center,
        _ => null
    };

    private WPos? GetDropSplashPosition(RM02SHoneyBLovely module, DropSplashStrategy strategy)
    {
        var aoes = module.FindComponent<PoisonCloudSplinter>()?.Casters ?? module.FindComponent<SweetheartSplinter>()?.Casters;
        var dropSplash = module.FindComponent<DropSplashOfVenom>();
        if (aoes == null || dropSplash == null)
            return ClosestInMelee(Player.Position, module.PrimaryActor);

        if (aoes.Any(c => (c.Position - module.Center).LengthSq() < 8 * 8))
        {
            // center unsafe
            var angle = -135.Degrees(); // TODO: intercard
            if (dropSplash.NextMechanic == DropSplashOfVenom.Mechanic.Spread)
                angle -= 13.Degrees(); // TODO: cw/ccw
            var safespot = module.Center + 13.5f * angle.ToDirection();
            var deadline = dropSplash.Activation != default ? Deadline(dropSplash.Activation) : 100;
            return UptimeDowntimePos(ClosestInMelee(safespot, module.PrimaryActor), safespot, GCD, deadline);
        }
        else
        {
            // center safe
            // TODO: intercard changes sign
            // TODO: ranged + spread z=15, melee or stack z=5
            return module.Center - new WDir(5, 5);
        }
    }

    private WPos? GetStageComboPosition(RM02SHoneyBLovely module, StageComboStrategy strategy)
    {
        var comp = module.FindComponent<StageCombo>();
        if (comp == null)
            return ClosestInMelee(Player.Position, module.PrimaryActor);

        foreach (var aoe in comp.ActiveAOEs(Manager.PlayerSlot, Player))
        {
            if (aoe.Shape is AOEShapeCircle)
            {
                // out + cardinal, but get to melee first (in case we're moving after cross and gcd is imminent)
                return InMeleeRange(module.PrimaryActor) ? module.Center + new WDir(0, -7.5f) : ClosestInMelee(Player.Position, module.PrimaryActor);
            }
            else if (aoe.Shape is AOEShapeDonut)
            {
                // in + intercardinal, but get to melee first (in case we're moving after cross and gcd is imminent)
                return InMeleeRange(module.PrimaryActor) ? module.Center + new WDir(-4.8f, -4.8f) : ClosestInMelee(Player.Position, module.PrimaryActor);
            }
            else if (aoe.Shape is AOEShapeCross)
            {
                var safespot = module.Center + new WDir(-7.2f, -7.2f);
                return strategy == StageComboStrategy.NNWOrHeart && Player.FindStatus(SID.Hearts3) == null
                    ? module.Center + new WDir(0, -7.5f)
                    : UptimeDowntimePos(ClosestInMelee(safespot, module.PrimaryActor), safespot, GCD, Deadline(aoe.Activation) - 0.5f);
            }
        }
        return ClosestInMelee(Player.Position, module.PrimaryActor);
    }

    private WPos? GetPheromones2Position(RM02SHoneyBLovely module, Pheromones2Strategy strategy, out bool wantSprint)
    {
        wantSprint = false;
        var bait = module.FindComponent<PoisonStingBait>();
        var aoe1 = module.FindComponent<BlindingLoveCharge1>();
        var aoe2 = module.FindComponent<BlindingLoveCharge2>();
        if (bait == null || aoe1 == null || aoe2 == null)
            return null;

        foreach (var b in bait.ActiveBaitsOn(Player))
        {
            wantSprint = true;
            var delay = Deadline(b.Activation);
            if (delay > 4)
                break; // too early, we might have a previous set of aoes active

            // find the orientation of the next set of aoes; each set has 4 casters with 90 degrees between them
            var nextCaster = aoe1.Casters.Concat(aoe2.Casters).MinBy(c => c.CastInfo?.RemainingTime);
            if (nextCaster == null)
                break;

            var orientation = nextCaster.CastInfo?.Rotation ?? nextCaster.Rotation;
            var castDir = orientation.ToDirection();
            var casterOffset = Math.Abs(castDir.OrthoL().Dot(nextCaster.Position - module.Center));
            var playerOffset = Player.Position - module.Center;
            var dirToPlayer = Angle.FromDirection(playerOffset);
            var relDir = (dirToPlayer - orientation).Normalized();
            var sectorMidpoint = relDir.Abs().Deg switch
            {
                < 45 => orientation,
                > 135 => orientation + 180.Degrees(),
                _ => relDir.Rad < 0 ? orientation - 90.Degrees() : orientation + 90.Degrees()
            };
            var sectorMidDir = sectorMidpoint.ToDirection();
            var playerOrthoOffset = sectorMidDir.OrthoL().Dot(playerOffset);
            var maxOrthoOffset = casterOffset - 5.2f;
            playerOrthoOffset = Math.Clamp(playerOrthoOffset, -maxOrthoOffset, maxOrthoOffset);
            var playerParrOffset = sectorMidDir.Dot(playerOffset);
            if (playerParrOffset >= casterOffset + 5.5f)
                return module.Center + playerParrOffset * sectorMidDir + playerOrthoOffset * sectorMidDir.OrthoL(); // already in downtime position, don't adjust other than to avoid orthogonal aoes
            var projectedPos = module.Center + casterOffset * sectorMidDir + playerOrthoOffset * sectorMidDir.OrthoL();
            var uptimePos = projectedPos - 5.2f * sectorMidDir;
            var downtimePos = projectedPos + 5.5f * sectorMidDir;
            return UptimeDowntimePos(uptimePos, downtimePos, GCD, delay);
        }
        return null;// ClosestInRange(Player.Position, module.Center, 4); // no baits on player - but we don't want to go in too early, not to clip aoes...
    }

    private WPos? GetDefamationsTowersPosition(RM02SHoneyBLovely module, DefamationsTowersStrategy strategy, out bool wantSprint)
    {
        wantSprint = false;
        var defams = module.FindComponent<HoneyBLiveBeat3BigBurst>();
        var towers = module.FindComponent<Fracture3>();
        if (defams == null || towers == null)
            return null;

        if (defams.Spreads.Count > 0)
        {
            // resolve defamations
            var playerOrder = defams.Order[Manager.PlayerSlot];
            var defamOrder = defams.NumCasts > 0 ? 2 : 1;
            if (defamOrder == playerOrder)
            {
                wantSprint = true;
                var safespot = module.Center + new WDir(0, -17);
                return UptimeDowntimePos(ClosestInMelee(safespot, module.PrimaryActor), safespot, GCD, Deadline(defams.Activation[defamOrder - 1]));
            }
            else
            {
                // just go mid
                return module.Center;
            }
        }
        else
        {
            // resolve towers
            var tower = towers.Towers.FirstOrDefault(t => t.Position.X < module.Center.X && t.Position.Z < module.Center.Z);
            if (tower.Position != default && !tower.ForbiddenSoakers[Manager.PlayerSlot])
            {
                return ClosestInMelee(tower.Position, module.PrimaryActor);
            }
            else
            {
                // just go mid
                return module.Center;
            }
        }
    }
}
