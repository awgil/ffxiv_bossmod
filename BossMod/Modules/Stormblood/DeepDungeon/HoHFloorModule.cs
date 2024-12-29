using BossMod.Global.DeepDungeon;

namespace BossMod.Stormblood.DeepDungeon;

public enum AID : uint
{
    StoneGaze = 6351, // 22AF->player, 3.5s cast, single-target
    BlindingBurst = 12174, // 22C3->self, 3.0s cast, range 25 circle
    NightmarishLight = 12322, // 22BC->self, 4.0s cast, range 30+R circle
    Malice = 12313, // 2355->player, 3.0s cast, single-target
    ShiftingLight = 12357, // 22DC->self, 3.0s cast, range 30+R circle
    Cry = 12350, // 22D9->self, 5.0s cast, range 12+R circle
    Eyeshine = 12261, // 230E->self, 3.5s cast, range 38+R circle
    AtropineSpore = 12415, // 22FF->self, 4.0s cast, range ?-41 donut
    FrondFatale = 12416, // 22FF->self, 3.0s cast, range 40 circle
    TheDragonsVoice = 12432, // 2311->self, 4.5s cast, range 30 circle
    Hex = 12442, // 2314->self, 4.0s cast, range 30 circle
    HorrisonousBlast = 12258, // 230D->self, 4.0s cast, range 16+R circle
    Northerlies = 12227, // 230C->self, 5.0s cast, range 40+R circle
}

public abstract class HoHFloorModule(WorldState ws) : DeepDungeonAutoClear(ws, 70)
{
    protected override void OnCastStarted(Actor actor)
    {
        switch ((AID)actor.CastInfo!.Action.ID)
        {
            case AID.Cry:
                Stuns.Add(actor);
                break;
            case AID.Malice:
            case AID.HorrisonousBlast:
            case AID.Northerlies:
                Interrupts.Add(actor);
                break;
            case AID.StoneGaze:
            case AID.BlindingBurst:
            case AID.NightmarishLight:
            case AID.ShiftingLight:
            case AID.Eyeshine:
            case AID.FrondFatale:
            case AID.Hex:
                Gazes.Add((actor, World.FutureTime(actor.CastInfo.NPCRemainingTime), null));
                break;
            case AID.AtropineSpore:
                Donuts.Add((actor, World.FutureTime(actor.CastInfo.NPCRemainingTime), 10));
                break;
            case AID.TheDragonsVoice:
                Donuts.Add((actor, World.FutureTime(actor.CastInfo.NPCRemainingTime), 8));
                break;
        }
    }

    protected override void OnCastFinished(Actor actor)
    {
        switch ((AID)actor.CastInfo!.Action.ID)
        {
            case AID.Cry:
                Stuns.Remove(actor);
                break;
            case AID.Malice:
            case AID.HorrisonousBlast:
            case AID.Northerlies:
                Interrupts.Remove(actor);
                break;
            case AID.StoneGaze:
            case AID.BlindingBurst:
            case AID.NightmarishLight:
            case AID.ShiftingLight:
            case AID.Eyeshine:
            case AID.FrondFatale:
            case AID.Hex:
                Gazes.RemoveAll(g => g.Source == actor);
                break;
            case AID.AtropineSpore:
            case AID.TheDragonsVoice:
                Donuts.RemoveAll(g => g.Source == actor);
                break;
        }
    }

    protected override IEnumerable<ActionID> ActionsToIgnore() => [ActionID.MakeSpell(AID.BlindingBurst), ActionID.MakeSpell(AID.AtropineSpore), ActionID.MakeSpell(AID.TheDragonsVoice)];
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
