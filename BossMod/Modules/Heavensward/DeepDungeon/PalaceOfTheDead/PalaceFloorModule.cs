using BossMod.Global.DeepDungeon;

namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead;

enum AID : uint
{
    // gazes
    StoneGazeSingle = 6351, // 3.5s cast, single target, inflicts petrification
    StoneGazeCone = 6356, // 2.5s cast, range 8.2 90-degree cone, inflicts petrification
    MysteriousLight = 6953, // 3s cast, 30 radius, inflicts damage

    // interruptible casts
    ParalyzeIII = 6386, // 5s cast, 6 radius on target (player), applies paralysis
    ParalyzeIII2 = 6388, // exactly the same as above but different action for some reason
    Infatuation = 6397, // 3s cast, applies pox
    VoidBlizzard = 7049, // 2.5s cast, applies slow to target, interruptible
    Mucin = 7014, // 3s cast, applies stoneskin to self
    BladeOfSuffering = 7029, // 3s cast, applies drain touch to self
    HorroisonousBlast = 7058, // 4s cast, 20 radius, interruptible, can't be dodged due to the heavy

    // should be ignored
    Chirp = 6365, // 2.5s cast, 21.6 radius, deals no damage, inflicts sleep
}

enum SID : uint
{
    BlazeSpikes = 197,
    IceSpikes = 198,
}

public abstract class PalaceFloorModule(WorldState ws) : AutoClear(ws, 60)
{
    protected override void OnCastStarted(Actor actor)
    {
        switch ((AID)actor.CastInfo!.Action.ID)
        {
            case AID.MysteriousLight:
                AddGaze(actor, 30);
                break;
            case AID.StoneGazeSingle:
                AddGaze(actor, 100);
                break;
            case AID.StoneGazeCone:
                AddGaze(actor, new AOEShapeCone(8.2f, 45.Degrees()));
                HintDisabled.Add(actor);
                break;
            case AID.Chirp:
                HintDisabled.Add(actor);
                break;
            case AID.Infatuation:
            case AID.VoidBlizzard:
            case AID.HorroisonousBlast:
            case AID.Mucin:
            case AID.BladeOfSuffering:
            case AID.ParalyzeIII:
            case AID.ParalyzeIII2:
                Interrupts.Add(actor);
                break;
        }
    }

    protected override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.BlazeSpikes:
            case SID.IceSpikes:
                Spikes.Add((actor, World.FutureTime(10)));
                break;
        }
    }

    protected override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.BlazeSpikes:
            case SID.IceSpikes:
                Spikes.RemoveAll(t => t.Actor == actor);
                break;
        }
    }

    protected override void CalculateExtraHints(int playerSlot, Actor player, AIHints hints)
    {
        foreach (var p in hints.PotentialTargets)
        {
            // this mob will enrage after some time
            if (p.Actor.OID == 0x1842 && p.Actor.InCombat && p.Actor.TargetID == player.InstanceID)
                p.Priority = 10;
        }
    }
}

[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 174)]
public class Palace10(WorldState ws) : PalaceFloorModule(ws);

[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 175)]
public class Palace20(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 176)]
public class Palace30(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 177)]
public class Palace40(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 178)]
public class Palace50(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 204)]
public class Palace60(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 205)]
public class Palace70(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 206)]
public class Palace80(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 207)]
public class Palace90(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 208)]
public class Palace100(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 209)]
public class Palace110(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 210)]
public class Palace120(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 211)]
public class Palace130(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 212)]
public class Palace140(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 213)]
public class Palace150(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 214)]
public class Palace160(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 215)]
public class Palace170(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 216)]
public class Palace180(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 217)]
public class Palace190(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 218)]
public class Palace200(WorldState ws) : PalaceFloorModule(ws);
