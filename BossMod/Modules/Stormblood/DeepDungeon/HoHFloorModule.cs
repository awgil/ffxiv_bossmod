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
}

public abstract class HoHFloorModule(WorldState ws) : DeepDungeonAutoClear(ws, 70)
{
    private readonly List<(Actor Source, DateTime Activation)> Donuts = [];

    protected override void OnCastStarted(Actor actor)
    {
        switch ((AID)actor.CastInfo!.Action.ID)
        {
            case AID.StoneGaze:
            case AID.BlindingBurst:
            case AID.NightmarishLight:
            case AID.ShiftingLight:
            case AID.Eyeshine:
            case AID.FrondFatale:
                Gazes.Add((actor, World.FutureTime(actor.CastInfo.NPCRemainingTime), null));
                break;
            case AID.Cry:
                Stuns.Add(actor);
                break;
            case AID.Malice:
                Interrupts.Add(actor);
                break;
            case AID.AtropineSpore:
                Donuts.Add((actor, World.FutureTime(actor.CastInfo.NPCRemainingTime)));
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
            case AID.ShiftingLight:
            case AID.Eyeshine:
            case AID.FrondFatale:
                Gazes.RemoveAll(g => g.Source == actor);
                break;
            case AID.Cry:
                Stuns.Remove(actor);
                break;
            case AID.Malice:
                Interrupts.Remove(actor);
                break;
            case AID.AtropineSpore:
                Donuts.RemoveAll(g => g.Source == actor);
                break;
        }
    }

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        base.CalculateAIHints(playerSlot, player, hints);

        if (!_config.Enable)
            return;

        foreach (var d in Donuts)
            hints.AddForbiddenZone(new AOEShapeDonut(10, 100), d.Source.Position, default, d.Activation);
    }

    protected override IEnumerable<ActionID> ActionsToIgnore() => [ActionID.MakeSpell(AID.BlindingBurst), ActionID.MakeSpell(AID.AtropineSpore)];
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
