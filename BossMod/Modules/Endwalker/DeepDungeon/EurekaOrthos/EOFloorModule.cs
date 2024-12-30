using BossMod.Global.DeepDungeon;

namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos;

public enum AID : uint
{
    BigBurst = 32381, // 3DCA->self, 3.5s cast, range 6 circle
    TerrorTouch = 32389, // 3DD0->player, 4.0s cast, single-target
    Diamondback = 32431, // 3DEC->self, 4.0s cast, single-target, permanent defense up to self
    Tailwind = 33167, // 3DF6->3DF5/3DF6, 3.0s cast, single-target, damage up to ally
    SelfDetonate = 32410, // 3DE0->self, 10.0s cast, range 40 circle, enrage
    DoubleHexEye = 32437, // 3DF1->self, 4.0s cast, range 40 circle, instakill mechanic
    Bombination = 32424, // 3DE9->self, 2.0s cast, range 25 circle
    TheDragonsVoice = 32444, // 3DF4->self, 4.0s cast, range ?-30 donut
    Quake = 32470, // 3E02->self, 5.0s cast, range 30 circle, interruptible
    Infatuation = 32798, // 3E80->player, 3.0s cast, single-target
    AbyssalCry = 32467, // 3E00->self, 6.0s cast, range 30 circle, instakill mechanic
    SprigganHaste = 33175, // 3DFB->self, 1.5s cast
}

public enum SID : uint
{
    IceSpikes = 198
}

public abstract class EOFloorModule(WorldState ws) : DeepDungeonAutoClear(ws, 90)
{
    protected override void OnCastStarted(Actor actor)
    {
        switch ((AID)actor.CastInfo!.Action.ID)
        {
            case AID.BigBurst:
            case AID.Tailwind:
            case AID.SprigganHaste:
                Stuns.Add(actor);
                break;
            case AID.TerrorTouch:
            case AID.Diamondback:
            case AID.Infatuation:
            case AID.Quake:
                Interrupts.Add(actor);
                break;
            case AID.DoubleHexEye:
                Gazes.Add((actor, null));
                break;
            case AID.TheDragonsVoice:
                Donuts.Add((actor, 8));
                break;
            case AID.SelfDetonate:
                Circles.Add((actor, 40));
                break;
            case AID.AbyssalCry:
                Circles.Add((actor, 30));
                break;
        }
    }

    protected override void OnStatusGain(Actor actor, int index)
    {
        switch ((SID)actor.Statuses[index].ID)
        {
            case SID.IceSpikes:
                ForbiddenTargets.Add(actor);
                break;
        }
    }

    protected override void OnStatusLose(Actor actor, int index)
    {
        switch ((SID)actor.Statuses[index].ID)
        {
            case SID.IceSpikes:
                ForbiddenTargets.Remove(actor);
                break;
        }
    }

    protected override IEnumerable<ActionID> ActionsToIgnore() => [
        ActionID.MakeSpell(AID.Bombination),
        ActionID.MakeSpell(AID.TheDragonsVoice)
    ];
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
