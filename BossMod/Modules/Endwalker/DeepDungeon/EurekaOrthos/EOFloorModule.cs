using BossMod.Global.DeepDungeon;

namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos;

public enum AID : uint
{
    BigBurst = 32381, // 3DCA->self, 3.5s cast, range 6 circle
    TerrorTouch = 32389, // 3DD0->player, 4.0s cast, single-target
    Diamondback = 32431, // 3DEC->self, 4.0s cast, single-target, permanent defense up to self
    Tailwind = 33167, // 3DF6->enemy, 3.0s cast, single-target, damage up to ally
    SelfDetonate = 32410, // 3DE0->self, 10.0s cast, range 40 circle, enrage
    DoubleHexEye = 32437, // 3DF1->self, 4.0s cast, range 40 circle, instakill mechanic
    Bombination = 32424, // 3DE9->self, 2.0s cast, range 25 circle
    TheDragonsVoice = 32444, // 3DF4->self, 4.0s cast, range ?-30 donut
    Quake = 32470, // 3E02->self, 5.0s cast, range 30 circle, interruptible
    Explosion = 32452, // 3DFA->self, 7.0s cast, range 60 circle
    Infatuation = 32798, // 3E80->player, 3.0s cast, single-target
    AbyssalCry = 32467, // 3E00->self, 6.0s cast, range 30 circle, instakill mechanic
    SprigganHaste = 33175, // 3DFB->self, 1.5s cast
    GelidCharge = 33180, // 3E13->self, 2.0s cast, single-target
    SmolderingScales = 32952, // 3E3D->self, 2.5s cast, single-target
    ElectricCachexia = 32979, // 3E45->self, 4.0s cast, range ?-44 donut
    EyeOfTheFierce = 32667, // 3E53->self, 3.0s cast, range 40 circle
    DemonEye1 = 32762, // 3E5E->self, 5.0s cast, range 33 circle
    DemonEye2 = 32761, // 3E5E->self, 5.0s cast, range 33 circle
    Hypnotize = 32737, // 3E58->self, 3.0s cast, range 20 circle
    Catharsis = 32732, // 3E56->self, 10.0s cast, range 40 circle
    ElectricWhorl = 33186, // 3E09->self, 4.0s cast, range 60 circle
    HexEye = 32731, // 3E56->self, 3.0s cast, range 5 circle
    EclipticMeteor = 33043, // 3DC9->self, 12.0s cast, range 50 circle
    TheDragonsVoice2 = 32910,
    AllaganFear = 32896,
    HighVoltage = 32878, // 3E64->self, 7.5s cast, range 30 circle
}

public enum SID : uint
{
    BlazeSpikes = 197,
    IceSpikes = 198,
}

public abstract class EOFloorModule(WorldState ws, bool autoRaiseOnEnter = false) : AutoClear(ws, 90)
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
                Interrupts.Add(actor);
                break;
            case AID.Quake:
            case AID.HighVoltage:
                Interrupts.Add(actor);
                AddLOS(actor, 30);
                break;
            case AID.EclipticMeteor:
                AddLOS(actor, 50);
                break;
            case AID.DoubleHexEye:
            case AID.EyeOfTheFierce:
                AddGaze(actor, 40);
                break;
            case AID.Explosion:
                AddLOS(actor, 60);
                break;
            case AID.AllaganFear:
                AddGaze(actor, 30);
                break;
            case AID.Hypnotize:
                AddGaze(actor, 20);
                break;
            case AID.DemonEye1:
            case AID.DemonEye2:
                AddGaze(actor, 33);
                break;
            case AID.HexEye:
                AddGaze(actor, 5);
                break;
            case AID.TheDragonsVoice:
            case AID.TheDragonsVoice2:
                Donuts.Add((actor, 8, 30));
                break;
            case AID.ElectricCachexia:
                Donuts.Add((actor, 8, 44));
                break;
            case AID.ElectricWhorl:
                Donuts.Add((actor, 8, 60));
                break;
            case AID.Catharsis:
                Circles.Add((actor, 40));
                break;
            case AID.AbyssalCry:
                AddLOS(actor, 30);
                break;
            case AID.SelfDetonate:
                AddLOS(actor, 40);
                break;
        }
    }

    protected override void OnCastFinished(Actor actor)
    {
        switch ((AID)actor.CastInfo!.Action.ID)
        {
            // setting target to forbidden when it gains the spikes status is too late
            case AID.GelidCharge:
            case AID.SmolderingScales:
                ForbiddenTargets.Add(actor);
                break;
        }
    }

    protected override void OnStatusLose(Actor actor, int index)
    {
        switch ((SID)actor.Statuses[index].ID)
        {
            case SID.IceSpikes:
            case SID.BlazeSpikes:
                ForbiddenTargets.Remove(actor);
                break;
        }
    }

    protected override IEnumerable<ActionID> AutohintDisabledActions() => [
        ActionID.MakeSpell(AID.TheDragonsVoice),
        ActionID.MakeSpell(AID.TheDragonsVoice2),
        ActionID.MakeSpell(AID.ElectricCachexia),
        ActionID.MakeSpell(AID.ElectricWhorl),
        ActionID.MakeSpell(AID.Hypnotize)
    ];

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        base.CalculateAIHints(playerSlot, player, hints);

        if (autoRaiseOnEnter && Palace.Floor % 10 == 1)
        {
            var raising = Palace.GetItem(PomanderID.ProtoRaising);
            if (!raising.Active && raising.Count > 0)
                hints.ActionsToExecute.Push(new ActionID(ActionType.Pomander, (uint)PomanderID.ProtoRaising), player, ActionQueue.Priority.VeryHigh);
        }
    }
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
public class EO80(WorldState ws) : EOFloorModule(ws, true);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 905)]
public class EO90(WorldState ws) : EOFloorModule(ws, true);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 906)]
public class EO100(WorldState ws) : EOFloorModule(ws, true);
