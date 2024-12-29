using BossMod.Global.DeepDungeon;

namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos;
public enum AID : uint
{
    BigBurst = 32381, // 3DCA->self, 3.5s cast, range 6 circle
    TerrorTouch = 32389, // 3DD0->player, 4.0s cast, single-target
    Diamondback = 32431, // 3DEC->self, 4.0s cast, single-target
    Tailwind = 33167, // 3DF6->3DF5/3DF6, 3.0s cast, single-target
    DoubleHexEye = 32437, // 3DF1->self, 4.0s cast, range 40 circle
    TheDragonsVoice = 32444, // 3DF4->self, 4.0s cast, range ?-30 donut
}

public abstract class EOFloorModule(WorldState ws) : DeepDungeonAutoClear(ws, 90)
{
    protected override void OnCastStarted(Actor actor)
    {
        switch ((AID)actor.CastInfo!.Action.ID)
        {
            case AID.BigBurst:
            case AID.Tailwind:
                Stuns.Add(actor);
                break;
            case AID.TerrorTouch:
            case AID.Diamondback:
                Interrupts.Add(actor);
                break;
            case AID.DoubleHexEye:
                Gazes.Add((actor, World.FutureTime(actor.CastInfo.NPCRemainingTime), null));
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
            case AID.BigBurst:
            case AID.Tailwind:
                Stuns.Remove(actor);
                break;
            case AID.TerrorTouch:
            case AID.Diamondback:
                Interrupts.Remove(actor);
                break;
            case AID.DoubleHexEye:
                Gazes.RemoveAll(g => g.Source == actor);
                break;
            case AID.TheDragonsVoice:
                Donuts.RemoveAll(g => g.Source == actor);
                break;
        }
    }

    protected override IEnumerable<ActionID> ActionsToIgnore() => [ActionID.MakeSpell(AID.TheDragonsVoice)];
}

[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 897)]
public class EO10(WorldState ws) : EOFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 898)]
public class EO20(WorldState ws) : EOFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 899)]
public class EO30(WorldState ws) : EOFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 900)]
public class EO40(WorldState ws) : EOFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 901)]
public class EO50(WorldState ws) : EOFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 902)]
public class EO60(WorldState ws) : EOFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 903)]
public class EO70(WorldState ws) : EOFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 904)]
public class EO80(WorldState ws) : EOFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 905)]
public class EO90(WorldState ws) : EOFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 906)]
public class EO100(WorldState ws) : EOFloorModule(ws);
