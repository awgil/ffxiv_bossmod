namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.FloorModule;

[ConfigDisplay(Name = "Palace of the Dead", Parent = typeof(HeavenswardConfig))]
class PotDConfig : ConfigNode
{
    [PropertyDisplay("Open gold coffers")]
    public bool GoldCoffer = true;
    [PropertyDisplay("Open silver coffers")]
    public bool SilverCoffer = true;
    [PropertyDisplay("Open bronze coffers")]
    public bool BronzeCoffer = true;
    [PropertyDisplay("Open normal coffers")]
    public bool NormalCoffer = true;
}

public abstract class PalaceFloorModule : ZoneModule
{
    private static readonly uint[] RevealedTrapOIDs = [0x1EA08E, 0x1EA08F, 0x1EA090, 0x1EA091, 0x1EA092];

    private readonly PotDConfig _config = Service.Config.Get<PotDConfig>();
    private readonly EventSubscriptions _subscriptions;
    private readonly HashSet<ulong> _skipChests = [];
    private bool _skipThisChest;

    private DeepDungeonState Palace => World.Client.DeepDungeonState;

    public PalaceFloorModule(WorldState ws) : base(ws)
    {
        _subscriptions = new(
            ws.Network.ServerIPCReceived.Subscribe(OnServerIPC)
        );
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
        base.Dispose(disposing);
    }

    private void OnServerIPC(NetworkState.OpServerIPC op)
    {
        var ipc = op.Packet;
        if (ipc.ID == Network.ServerIPC.PacketID.SystemLogMessage1)
        {
            var messageId = BitConverter.ToInt32(ipc.Payload, 4);
            if (messageId == 7222) // pomander overcap
                _skipThisChest = true;

            if (messageId == 7248) // transference initiated
                ClearState();
        }
    }

    private void ClearState()
    {
        _skipThisChest = false;
        _skipChests.Clear();
    }

    private bool OpenGold => _config.GoldCoffer && (Palace[PomanderID.Strength].Count < 3 || Palace[PomanderID.Steel].Count < 3 || Palace.Floor > 80 && Palace[PomanderID.Resolution].Count < 3);
    private bool OpenSilver => _config.SilverCoffer && Palace.WeaponLevel + Palace.ArmorLevel < 198;
    private bool OpenBronze => _config.BronzeCoffer;
    private bool OpenNormal => _config.NormalCoffer;

    // public override bool WantToBeDrawn() => true;

    public override List<string> CalculateGlobalHints() => [$"Chests to skip: {string.Join(", ", _skipChests)}"];

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        if (player.InCombat || player.IsTransformed || Palace.Floor % 10 == 0)
            return;

        Actor? coffer = null;
        Actor? hoardLight = null;
        Actor? hoard = null;
        Actor? passage = null;
        List<Func<WPos, float>> revealedTraps = [];

        foreach (var a in World.Actors)
        {
            if (_skipChests.Contains(a.InstanceID))
                continue;

            var oid = (OID)a.OID;
            if (a.IsTargetable && (
                oid == OID.GoldCoffer && OpenGold ||
                oid == OID.SilverCoffer && OpenSilver ||
                oid == OID.BronzeCoffer && OpenBronze ||
                oid == OID.NormalCoffer && OpenNormal ||
                oid == OID.BandedCoffer
            ))
            {
                if (coffer == null || a.DistanceToHitbox(player) < coffer.DistanceToHitbox(player))
                    coffer = a;
            }

            if (a.OID == (uint)OID.BandedCofferIndicator)
                hoardLight = a;

            if (a.OID == (uint)OID.CairnOfPassage)
                passage = a;

            if (RevealedTrapOIDs.Contains(a.OID))
                revealedTraps.Add(ShapeDistance.Circle(a.Position, 2));
        }

        if (coffer != null && _skipThisChest)
        {
            _skipChests.Add(coffer.InstanceID);
            _skipThisChest = false;
            return;
        }

        hints.InteractWithTarget = hoard ?? coffer;
        var haveChest = hints.InteractWithTarget is Actor t && InBounds(hints, t.Position);

        var havePassage = false;
        if (Palace.PassageActive && passage is Actor c)
        {
            havePassage = true;
            hints.GoalZones.Add(hints.GoalSingleTarget(c.Position, 2));
        }

        if (revealedTraps.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.Union(revealedTraps));

        if (hoardLight is Actor h && Palace.ItemInfo[13].Active)
        {
            hints.GoalZones.Add(hints.GoalSingleTarget(h.Position, 2, 10));
            hints.InteractWithTarget = null;
            return;
        }

        if (!(havePassage || haveChest))
            foreach (var p in hints.PotentialTargets)
                p.Priority = 0;
    }

    private bool InBounds(AIHints hints, WPos pos) => hints.PathfindMapBounds.Contains(pos - hints.PathfindMapCenter);
}

enum OID : uint
{
    CairnOfPassage = 0x1EA094,
    SilverCoffer = 0x1EA13D,
    GoldCoffer = 0x1EA13E,
    BronzeCoffer = 0x322,
    NormalCoffer = 0x323,
    BandedCofferIndicator = 0x1EA1F6,
    BandedCoffer = 0x1EA1F7,
}

[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 174)]
public class Palace10(WorldState ws) : PalaceFloorModule(ws);

[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 175)]
public class Palace20(WorldState ws) : PalaceFloorModule(ws);
[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 176)]
public class Palace30(WorldState ws) : PalaceFloorModule(ws);
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
