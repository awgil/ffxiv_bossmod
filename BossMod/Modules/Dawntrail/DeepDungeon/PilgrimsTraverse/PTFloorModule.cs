using BossMod.Global.DeepDungeon;

namespace BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse;

public enum AID : uint
{
    Malice = 44852, // 49CB->player, 3.0s cast, single-target

    MagneticShock = 41427, // 4917->self, 3.0s cast, range 15 circle, draw-in
    Plaincracker = 41512, // 4917->self, 4.0s cast, range 15 circle

    SmolderingScales = 42212, // 4922->self, 3.0s cast, spikes
    PainfulGust = 44727, // 4987->self, 5.0s cast, range 20 circle

    EarthenAuger = 42091, // 4920->self, 4.0s cast, range 3-30 270-degree donut
    MasterOfLevin = 44732, // 4988->self, 4.0s cast, range 5-30 donut

    Accelerate = 42516, // 4927->location, 4.0s cast, range 6 circle
    Subduction = 42517, // 4927->self, no cast, range 5-10 donut

    HailOfHeelsCast = 44759, // 4996->self, 3.0s cast, range 8 180-degree cone
    HailOfHeels = 44760, // 4996->self, no cast, range 8 180-degree cone

    PeripheralLasers = 44758, // 4995->self, 5.0s cast, range 5-60 donut
    GrowingCirclesOfAblution1 = 42578, // 492A->self, 5.0s cast, range 10 circle
    GrowingCirclesOfAblution2 = 42749, // 492A->self, no cast, range 10-40 donut
    ShrinkingCirclesOfAblution1 = 42748, // 492A->self, 5.0s cast, range 10-40 donut
    ShrinkingCirclesOfAblution2 = 42746, // 492A->self, no cast, range 10 circle

    RightSidedShockwaveCast = 42214, // 4923->self, 5.0s cast, range 30 180-degree cone
    RightSidedShockwave = 42215, // 4923->self, no cast, range 30 180-degree cone
    LeftSidedShockwaveCast = 42216, // 4923->self, 5.0s cast, range 30 180-degree cone
    LeftSidedShockwave = 42217, // 4923->self, no cast, range 30 180-degree cone

    RollingBarrage = 42523, // 4929->self, 16.0s cast, range 45 circle

    IncineratingLahar = 43133, // 492E->self, 16.0s cast, range 46 circle
}

public enum OID : uint
{
    TraverseTroubadour = 0x491B
}

public enum SID : uint
{
    BlazeSpikes = 4579,
}

public abstract class PTFloorModule(WorldState ws) : AutoClear(ws, 100)
{
    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        base.CalculateAIHints(playerSlot, player, hints);

        if (_config.Enable && !player.InCombat)
        {
            var interactDist = hints.InteractWithTarget is { } t ? player.DistanceToPoint(t.Position) : float.MaxValue;
            if (World.Actors.FirstOrDefault(t => t.OID == 0x1EBE27 && t.IsTargetable) is { } v && player.DistanceToPoint(v.Position) < interactDist)
                hints.InteractWithTarget = v;
        }

        foreach (var tar in World.Actors.Where(o => o.OID == (uint)OID.TraverseTroubadour && !o.IsDead))
        {
            // turtles do 60k autos that apply vuln, but are much slower than the player
            if (tar.TargetID == player.InstanceID && (tar.CastInfo == null || tar.CastInfo.RemainingTime < 1))
                hints.AddForbiddenZone(ShapeContains.Circle(tar.Position, tar.HitboxRadius + 5.5f));

            // TODO: add a separate forbidden zone for sight cone if turtle is out of combat
        }
    }

    protected override void OnCastStarted(Actor actor)
    {
        // TODO:
        // crystalline stingers (LOS, maybe stun?, 61-70)
        // hail of heels -> multiple frontal cones
        // Passions' Heat -> targeted AOE (with marker) and applies pyretic

        // does Tail Drive have a cast bar

        switch ((AID)actor.CastInfo!.Action.ID)
        {
            case AID.EarthenAuger:
                AddDonut(actor, 3, 30, 135.Degrees());
                HintDisabled.Add(actor);
                break;

            case AID.PeripheralLasers:
                AddDonut(actor, 5, 60);
                HintDisabled.Add(actor);
                break;

            case AID.Malice:
                Interrupts.Add(actor);
                Stuns.Add(actor);
                break;

            case AID.MagneticShock:
                KnockbackZones.Add((actor, 15));
                HintDisabled.Add(actor);
                break;

            case AID.RollingBarrage:
                Circles.Add((actor, 45));
                break;

            case AID.IncineratingLahar:
                Circles.Add((actor, 46));
                break;

            // stun for melee uptime
            case AID.Plaincracker:
            case AID.PainfulGust:
                Stuns.Add(actor);
                break;

            case AID.MasterOfLevin:
                AddDonut(actor, 5, 30);
                HintDisabled.Add(actor);
                break;
        }
    }

    protected override void OnEventCast(Actor actor, ActorCastEvent ev)
    {
        switch ((AID)ev.Action.ID)
        {
            case AID.ShrinkingCirclesOfAblution1:
                AddVoidzone(actor, new AOEShapeCircle(10));
                break;
            case AID.GrowingCirclesOfAblution1:
                AddVoidzone(actor, new AOEShapeDonut(10, 40));
                break;
            case AID.Accelerate:
                AddVoidzone(actor, new AOEShapeDonut(5, 10));
                break;

            case AID.RightSidedShockwaveCast:
                AddVoidzone(actor, new AOEShapeCone(30, 90.Degrees(), 90.Degrees()));
                break;
            case AID.LeftSidedShockwaveCast:
                AddVoidzone(actor, new AOEShapeCone(30, 90.Degrees(), -90.Degrees()));
                break;

            case AID.HailOfHeelsCast:
                AddVoidzone(actor, new AOEShapeCone(8, 90.Degrees()), 3);
                break;

            case AID.HailOfHeels:
                var vz = Voidzones.FindIndex(v => v.Source.InstanceID == actor.InstanceID);
                if (vz >= 0)
                {
                    Voidzones.Ref(vz).Counter--;
                    if (Voidzones[vz].Counter <= 0)
                        Voidzones.RemoveAt(vz);
                }
                break;

            case AID.ShrinkingCirclesOfAblution2:
            case AID.GrowingCirclesOfAblution2:
            case AID.RightSidedShockwave:
            case AID.LeftSidedShockwave:
            case AID.Subduction:
                Voidzones.RemoveAll(v => v.Source == actor);
                break;
        }
    }

    protected override void OnCastFinished(Actor actor)
    {
        switch ((AID)actor.CastInfo!.Action.ID)
        {
            case AID.SmolderingScales:
                Spikes.Add((actor, World.FutureTime(10)));
                break;
        }
    }

    protected override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.BlazeSpikes:
                Spikes.RemoveAll(s => s.Actor == actor);
                break;
        }
    }
}

[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 1032)]
public class PT10(WorldState ws) : PTFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 1033)]
public class PT20(WorldState ws) : PTFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 1034)]
public class PT30(WorldState ws) : PTFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 1035)]
public class PT40(WorldState ws) : PTFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 1036)]
public class PT50(WorldState ws) : PTFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 1037)]
public class PT60(WorldState ws) : PTFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 1038)]
public class PT70(WorldState ws) : PTFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 1039)]
public class PT80(WorldState ws) : PTFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 1040)]
public class PT90(WorldState ws) : PTFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 1041)]
public class PT100(WorldState ws) : PTFloorModule(ws);
