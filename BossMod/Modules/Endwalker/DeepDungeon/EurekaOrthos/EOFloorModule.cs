using BossMod.Global.DeepDungeon;

namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos;

public enum AID : uint
{
    BigBurst = 32381, // 3DCA->self, 3.5s cast, range 6 circle
    TerrorTouch = 32389, // 3DD0->player, 4.0s cast, single-target
    Diamondback = 32431, // 3DEC->self, 4.0s cast, single-target, permanent defense up to self
    Tailwind = 33167, // 3DF6->enemy, 3.0s cast, single-target, damage up to ally
    SelfDetonate = 32410, // 3DE0->self, 10.0s cast, range 40 circle, enrage
    Electromagnetism = 32413, // 3DE1->self, 5.0s cast, range 15 circle
    Headspin = 32412, // 3DE1->self, 0.0s cast, range 6 circle, instant after Electromagnetism
    DoubleHexEye = 32437, // 3DF1->self, 4.0s cast, range 40 circle, instakill mechanic
    Bombination = 32424, // 3DE9->self, 2.0s cast, range 25 circle
    Bombination2 = 32461, // 3DFF->self, 2.0s cast, range 25 circle
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
    AquaSpear = 32500, // 3E12->self, 5.5s cast, width 5 charge, must be LOSed
    Sucker = 32477, // 3E08->self, 3.5s cast, range 25 circle, draw-in
    WaterIII = 32478, // 3E08->self, 2.5s cast, range 8 circle, used after sucker
    RipeBanana = 33195, // 3E52->self, 2s cast, single-target, damage buff
    ChestThump = 32680, // 3E52->self, 2s cast, range 52 circle, used after banana

    KillingPaw = 33193, // 3E50->self, 3.0s cast, range 9 120-degree cone
    SavageSwipe = 32654, // 3E50->self, instant cast, range 6 120-degree cone

    SewerWaterCastFront = 32491, // 3E0F->self, 3.0s cast, range 12 180-degree cone
    SewerWaterCastBack = 32492, // 3E0F->self, 3.0s cast, range 12 180-degree cone
    SewerWaterInstantFront = 32493, // 3E0F->self, instant cast, range 12 180-degree cone
    SewerWaterInstantBack = 32494, // 3E0F->self, instant cast, range 12 180-degree cone

    GoobInhale = 33178, // 3E04->self, instant cast, range 40 90-degree cone
    GoobSneeze = 32473, // 3E04->self, 1.0s cast, range 7 90-degree cone

    GourmInhale = 32748, // 3E5B->self, instant cast, range 40 90-degree cone
    GourmSneeze = 32749, // 3E5B->self, 1.0s cast, range 6 90-degree cone
}

public enum SID : uint
{
    BlazeSpikes = 197,
    IceSpikes = 198,
}

public abstract class EOFloorModule(WorldState ws) : AutoClear(ws, 90)
{
    protected override void OnCastStarted(Actor actor)
    {
        switch ((AID)actor.CastInfo!.Action.ID)
        {
            // stunnable actions, either self buffs or large point blank AOEs that prevent going into melee range
            case AID.BigBurst:
            case AID.Tailwind:
            case AID.SprigganHaste:
                Stuns.Add(actor);
                break;

            // interruptible casts
            case AID.TerrorTouch:
            case AID.Diamondback:
                Interrupts.Add(actor);
                break;
            case AID.Infatuation:
                Interrupts.Add(actor);
                if (Palace.Floor < 60)
                    Stuns.Add(actor);
                break;

            // gazes
            case AID.DoubleHexEye:
            case AID.EyeOfTheFierce:
                AddGaze(actor, 40);
                break;
            case AID.AllaganFear:
                AddGaze(actor, 30);
                break;
            case AID.Hypnotize:
                AddGaze(actor, 20);
                HintDisabled.Add(actor);
                break;
            case AID.DemonEye1:
            case AID.DemonEye2:
                AddGaze(actor, 33);
                break;
            case AID.HexEye:
                AddGaze(actor, 5);
                break;

            // donut AOEs
            case AID.TheDragonsVoice:
            case AID.TheDragonsVoice2:
                AddDonut(actor, 8, 30);
                HintDisabled.Add(actor);
                break;
            case AID.ElectricCachexia:
                AddDonut(actor, 8, 44);
                HintDisabled.Add(actor);
                break;
            case AID.ElectricWhorl:
                AddDonut(actor, 8, 60);
                HintDisabled.Add(actor);
                break;

            // very large circle AOEs that trigger autohints' "this is a raidwide" check but are actually avoidable
            case AID.Catharsis:
                Circles.Add((actor, 40));
                break;

            // LOS attacks
            case AID.Quake:
            case AID.HighVoltage:
                Interrupts.Add(actor);
                AddLOS(actor, 30);
                break;
            case AID.EclipticMeteor:
            case AID.AquaSpear:
                AddLOS(actor, 50);
                break;
            case AID.Explosion:
                AddLOS(actor, 60);
                break;
            case AID.AbyssalCry:
                AddLOS(actor, 30);
                break;
            case AID.SelfDetonate:
                AddLOS(actor, 40);
                break;

            // knockbacks (can be ignored on kb penalty floors or if arms length is up)
            case AID.Electromagnetism:
                KnockbackZones.Add((actor, 15));
                HintDisabled.Add(actor);
                break;
            case AID.Sucker:
                KnockbackZones.Add((actor, 25));
                HintDisabled.Add(actor);
                break;

            // large out of combat AOEs that are fast and generally nonthreatening, we want to ignore these so we don't interfere with pathfinding
            case AID.Bombination:
            case AID.Bombination2:
                HintDisabled.Add(actor);
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
                Spikes.Add((actor, World.FutureTime(10)));
                break;
        }
    }

    protected override void OnEventCast(Actor actor, ActorCastEvent ev)
    {
        switch ((AID)ev.Action.ID)
        {
            case AID.GoobInhale:
                AddVoidzone(actor, new AOEShapeCone(7, 45.Degrees()));
                break;
            case AID.GourmInhale:
                AddVoidzone(actor, new AOEShapeCone(6, 45.Degrees()));
                break;
            case AID.KillingPaw:
                AddVoidzone(actor, new AOEShapeCone(6, 60.Degrees()));
                break;
            case AID.SewerWaterCastFront:
                AddVoidzone(actor, new AOEShapeCone(12, 90.Degrees(), 180.Degrees()));
                break;
            case AID.SewerWaterCastBack:
                AddVoidzone(actor, new AOEShapeCone(12, 90.Degrees()));
                break;
            case AID.Electromagnetism:
                AddVoidzone(actor, new AOEShapeCircle(6));
                break;
            case AID.RipeBanana:
                AddVoidzone(actor, new AOEShapeCircle(52));
                break;

            case AID.GoobSneeze:
            case AID.GourmSneeze:
            case AID.Headspin:
            case AID.SavageSwipe:
            case AID.SewerWaterInstantFront:
            case AID.SewerWaterInstantBack:
            case AID.ChestThump:
                Voidzones.RemoveAll(v => v.Source == actor);
                break;
        }
    }

    protected override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.IceSpikes:
            case (uint)SID.BlazeSpikes:
                Spikes.RemoveAll(t => t.Actor == actor);
                break;
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
public class EO80(WorldState ws) : EOFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 905)]
public class EO90(WorldState ws) : EOFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 906)]
public class EO100(WorldState ws) : EOFloorModule(ws);
