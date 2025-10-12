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

        if (!player.InCombat)
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

        switch ((AID)actor.CastInfo!.Action.ID)
        {
            case AID.EarthenAuger:
                AddDonut(actor, 3, 30, 135.Degrees());
                break;

            case AID.Malice:
                Interrupts.Add(actor);
                Stuns.Add(actor);
                break;

            case AID.MagneticShock:
                KnockbackZones.Add((actor, 15));
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
