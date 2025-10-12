using BossMod.Global.DeepDungeon;

namespace BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse;

public enum AID : uint
{
    Malice = 44852, // 49CB->player, 3.0s cast, single-target
    MagneticShock = 41427, // 4917->self, 3.0s cast, range 15 circle, draw-in
    Plaincracker = 41512, // 4917->self, 4.0s cast, range 15 circle
    EarthenAuger = 42091, // 4920->self, 4.0s cast, range 3-30 270-degree donut
}

public enum OID : uint
{
    TraverseTroubadour = 0x491B
}

public abstract class PTFloorModule(WorldState ws) : AutoClear(ws, 100)
{
    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        base.CalculateAIHints(playerSlot, player, hints);

        if (!player.InCombat && hints.InteractWithTarget == null)
            hints.InteractWithOID(World, 0x1EBE27);

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
        // TODO: one-two march (double rect)
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
                Stuns.Add(actor);
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
