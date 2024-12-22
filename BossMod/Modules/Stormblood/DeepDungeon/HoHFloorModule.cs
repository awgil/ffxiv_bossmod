using BossMod.Global.DeepDungeon;

namespace BossMod.Stormblood.DeepDungeon;

public enum AID : uint
{
    StoneGaze = 6351, // 22AF->player, 3.5s cast, single-target
    BlindingBurst = 12174, // 22C3->self, 3.0s cast, range 25 circle
    NightmarishLight = 12322, // 22BC->self, 4.0s cast, range 30+R circle
}

public abstract class HoHFloorModule(WorldState ws) : DeepDungeonAutoClear(ws, 70)
{
    protected override void OnCastStarted(Actor actor)
    {
        switch ((AID)actor.CastInfo!.Action.ID)
        {
            case AID.StoneGaze:
            case AID.BlindingBurst:
            case AID.NightmarishLight:
                Gazes.Add((actor, World.FutureTime(actor.CastInfo.NPCRemainingTime), null));
                break;
        }
    }

    protected override void OnCastFinished(Actor actor)
    {
        switch ((AID)actor.CastInfo!.Action.ID)
        {
            case AID.StoneGaze:
            case AID.BlindingBurst:
            case AID.NightmarishLight:
                Gazes.RemoveAll(g => g.Source == actor);
                break;
        }
    }

    protected override IEnumerable<ActionID> ActionsToIgnore() => [ActionID.MakeSpell(AID.BlindingBurst)];
}

[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 540)]
public class HoH10(WorldState ws) : HoHFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 541)]
public class HoH20(WorldState ws) : HoHFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 542)]
public class HoH30(WorldState ws) : HoHFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 543)]
public class HoH40(WorldState ws) : HoHFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 544)]
public class HoH50(WorldState ws) : HoHFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 545)]
public class HoH60(WorldState ws) : HoHFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 546)]
public class HoH70(WorldState ws) : HoHFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 547)]
public class HoH80(WorldState ws) : HoHFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 548)]
public class HoH90(WorldState ws) : HoHFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 549)]
public class HoH100(WorldState ws) : HoHFloorModule(ws);
