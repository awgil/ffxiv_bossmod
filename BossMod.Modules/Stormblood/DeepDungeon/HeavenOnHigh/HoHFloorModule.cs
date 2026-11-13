using BossMod.Global.DeepDungeon;

namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh;

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
    WoolyInspiration = 12218, // 2309->player, 2.5s cast, range 70.8 60-degree cone
    TuskButt = 12219, // 2309->self, instant, range 10.8 90?-degree cone
    GoldenTongue = 12222, // 230A->self, 2.5s cast, damage buff
    TheDragonsVoice = 12432, // 2311->self, 4.5s cast, range 30 circle
    Hex = 12442, // 2314->self, 4.0s cast, range 30 circle
    HorrisonousBlast = 12258, // 230D->self, 4.0s cast, range 16+R circle
    Northerlies = 12227, // 230C->self, 5.0s cast, range 40+R circle
}

public enum SID : uint
{
    // Stun = 2, // from mandragora, 2s duration
    // Windburn = 269, // from 22D1, negligible damage
    Silence = 7, // from trap
    Concussion = 266, // 22F9->player
    Frostbite = 268, // 2307->player
    Poison = 560, // 22F7/22FE->player
    Pacification = 620, // from trap
}

public abstract class HoHFloorModule(WorldState ws) : AutoClear(ws, 70)
{
    static readonly uint[] BadStatuses = [
        (uint)SID.Silence,
        (uint)SID.Concussion,
        (uint)SID.Frostbite,
        (uint)SID.Poison,
        (uint)SID.Pacification
    ];

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
            case AID.GoldenTongue:
                Interrupts.Add(actor);
                break;
            case AID.StoneGaze:
                AddGaze(actor, 100); // actually a single target cast
                break;
            case AID.BlindingBurst:
                AddGaze(actor, 25);
                HintDisabled.Add(actor);
                break;
            case AID.NightmarishLight:
            case AID.ShiftingLight:
                AddGaze(actor, 35);
                break;
            case AID.Eyeshine:
            case AID.FrondFatale:
                AddGaze(actor, 40);
                break;
            case AID.Hex:
                AddGaze(actor, 30);
                break;
            case AID.AtropineSpore:
                AddDonut(actor, 9, 41);
                HintDisabled.Add(actor);
                break;
            case AID.TheDragonsVoice:
                AddDonut(actor, 8, 30);
                HintDisabled.Add(actor);
                break;

            case AID.WoolyInspiration:
                AddVoidzone(actor, new AOEShapeCone(10.8f, 45.Degrees()), World.FutureTime(actor.CastInfo.NPCRemainingTime));
                break;
            case AID.TuskButt:
                Voidzones.RemoveAll(v => v.Source == actor);
                break;
        }
    }

    BitMask _shouldCleanse;

    protected override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (BadStatuses.Contains(status.ID) && World.Party.TryFindSlot(actor.InstanceID, out var slot))
            _shouldCleanse.Set(slot);
    }

    protected override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (BadStatuses.Contains(status.ID) && World.Party.TryFindSlot(actor.InstanceID, out var slot))
        {
            // actor still has the status during this event
            if (actor.Statuses.Count(s => BadStatuses.Contains(s.ID)) == 1)
                _shouldCleanse.Clear(slot);
        }
    }

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        base.CalculateAIHints(playerSlot, player, hints);

        hints.ShouldCleanse |= _shouldCleanse;
    }
}

[ZoneModuleInfo(540)]
public class HoH10(WorldState ws) : HoHFloorModule(ws);
[ZoneModuleInfo(541)]
public class HoH20(WorldState ws) : HoHFloorModule(ws);
[ZoneModuleInfo(542)]
public class HoH30(WorldState ws) : HoHFloorModule(ws);
[ZoneModuleInfo(543)]
public class HoH40(WorldState ws) : HoHFloorModule(ws);
[ZoneModuleInfo(544)]
public class HoH50(WorldState ws) : HoHFloorModule(ws);
[ZoneModuleInfo(545)]
public class HoH60(WorldState ws) : HoHFloorModule(ws);
[ZoneModuleInfo(546)]
public class HoH70(WorldState ws) : HoHFloorModule(ws);
[ZoneModuleInfo(547)]
public class HoH80(WorldState ws) : HoHFloorModule(ws);
[ZoneModuleInfo(548)]
public class HoH90(WorldState ws) : HoHFloorModule(ws);
[ZoneModuleInfo(549)]
public class HoH100(WorldState ws) : HoHFloorModule(ws);
