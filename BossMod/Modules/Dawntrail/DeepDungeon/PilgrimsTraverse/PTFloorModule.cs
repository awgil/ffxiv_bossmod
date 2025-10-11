using BossMod.Global.DeepDungeon;

namespace BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse;

public enum AID : uint
{
    EarthenAuger = 42091, // 4920->self, 4.0s cast, range 3-30 270-degree donut
}

public abstract class PTFloorModule(WorldState ws) : AutoClear(ws, 100)
{
    protected override void CalculateExtraHints(int playerSlot, Actor player, AIHints hints)
    {
        if (!player.InCombat && hints.InteractWithTarget == null)
            hints.InteractWithOID(World, 0x1EBE27);
    }

    protected override void OnCastStarted(Actor actor)
    {
        switch ((AID)actor.CastInfo!.Action.ID)
        {
            case AID.EarthenAuger:
                AddDonut(actor, 3, 30, 135.Degrees());
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
