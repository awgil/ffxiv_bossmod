using BossMod.Global.DeepDungeon;

namespace BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse;

public abstract class PTFloorModule(WorldState ws) : AutoClear(ws, 100)
{
    protected override void CalculateExtraHints(int playerSlot, Actor player, AIHints hints)
    {
        if (!player.InCombat)
            hints.InteractWithOID(World, 0x1EBE27);
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
