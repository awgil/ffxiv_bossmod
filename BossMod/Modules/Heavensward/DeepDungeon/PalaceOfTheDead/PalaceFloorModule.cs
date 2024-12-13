using System.Data.SQLite;
using System.IO;
using System.Text;

namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.FloorModule;

[ConfigDisplay(Name = "Palace of the Dead", Parent = typeof(HeavenswardConfig))]
class PotDConfig : ConfigNode
{
    [PropertyDisplay("Enable module")]
    public bool Enable = true;
    [PropertyDisplay("Try to avoid traps")]
    public bool TrapHints = true;
    [PropertyDisplay("Automatically navigate to Cairn of Passage")]
    public bool AutoPassage = true;
    [PropertyDisplay("Automatically target mobs until Passage is open")]
    public bool AutoClear = true;
    [PropertyDisplay("Automatically navigate to coffers")]
    public bool AutoMoveTreasure = true;
    [PropertyDisplay("Prioritize opening coffers over Cairn of Passage")]
    public bool OpenChestsFirst = false;
    [PropertyDisplay("Prioritize clearing floor over Cairn of Passage")]
    public bool FullClear = false;
    [PropertyDisplay("Open gold coffers")]
    public bool GoldCoffer = true;
    [PropertyDisplay("Open silver coffers")]
    public bool SilverCoffer = true;
    [PropertyDisplay("Open bronze coffers")]
    public bool BronzeCoffer = true;
}

enum OID : uint
{
    CairnOfPassage = 0x1EA094,
    SilverCoffer = 0x1EA13D,
    GoldCoffer = 0x1EA13E,
    BandedCofferIndicator = 0x1EA1F6,
    BandedCoffer = 0x1EA1F7,
}

enum AID : uint
{
    StoneGaze = 6351, // 3.5s cast, single target, gaze, inflicts petrification
    Chirp = 6365, // 2.5s cast, 21.6 radius, deals no damage, inflicts sleep
    Infatuation = 6397, // 3s cast, applies pox
    IceSpikes = 6943, // 1.5s cast, applies thorns to self
    MysteriousLight = 6953, // 3s cast, 30 radius, gaze, inflicts damage if failed
    Tornado = 7028, // 1s cast, 6 radius, targets player, deals minor damage
}

enum SID : uint
{
    Silence = 7,
    Pacification = 620,
    ItemPenalty = 1094,

    IceSpikes = 198,
}

public abstract class PalaceFloorModule : ZoneModule
{
    private static readonly uint[] RevealedTrapOIDs = [0x1EA08E, 0x1EA08F, 0x1EA090, 0x1EA091, 0x1EA092];

    private readonly PotDConfig _config = Service.Config.Get<PotDConfig>();
    private readonly EventSubscriptions _subscriptions;
    private readonly List<WPos> _trapsCurrentZone = [];

    private readonly Dictionary<ulong, PomanderID> _chestContents = [];
    private PomanderID? _lastChestContents;
    private bool _showTrapHints = true;

    private readonly List<(Actor Source, DateTime Activation)> _gazes = [];
    private readonly List<Actor> _interrupts = [];
    private readonly List<Actor> _forbiddenTargets = [];

    private DeepDungeonState Palace => World.Client.DeepDungeonState;

    public PalaceFloorModule(WorldState ws) : base(ws)
    {
        _subscriptions = new(
            ws.Network.ServerIPCReceived.Subscribe(OnServerIPC),
            ws.Actors.CastStarted.Subscribe(OnCastStarted),
            ws.Actors.CastFinished.Subscribe(OnCastFinished),
            ws.Actors.StatusGain.Subscribe(OnStatusGain),
            ws.Actors.StatusLose.Subscribe(OnStatusLose)
        );

        _trapsCurrentZone = PalacePalInterop.GetTrapLocationsForZone(ws.CurrentZone);
    }

    private void OnCastStarted(Actor actor)
    {
        switch ((AID)actor.CastInfo!.Action.ID)
        {
            case AID.MysteriousLight:
            case AID.StoneGaze:
                _gazes.Add((actor, World.FutureTime(actor.CastInfo.NPCRemainingTime)));
                break;
            case AID.Infatuation:
            case AID.IceSpikes:
                _interrupts.Add(actor);
                break;
        }
    }

    private void OnCastFinished(Actor actor)
    {
        switch ((AID)actor.CastInfo!.Action.ID)
        {
            case AID.MysteriousLight:
            case AID.StoneGaze:
                _gazes.RemoveAll(d => d.Source == actor);
                break;
            case AID.Infatuation:
            case AID.IceSpikes:
                _interrupts.Remove(actor);
                break;
        }
    }

    private void OnStatusGain(Actor actor, int index)
    {
        var status = actor.Statuses[index];
        switch ((SID)status.ID)
        {
            case SID.IceSpikes:
                _forbiddenTargets.Add(actor);
                break;
        }
    }

    private void OnStatusLose(Actor actor, int index)
    {
        var status = actor.Statuses[index];
        switch ((SID)status.ID)
        {
            case SID.IceSpikes:
                _forbiddenTargets.Remove(actor);
                break;
        }
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
            switch (messageId)
            {
                case 7222: // pomander overcap
                    _lastChestContents = (PomanderID)ipc.Payload[12];
                    break;
                case 7248: // transference initiated
                    ClearState();
                    break;
                case 7255: // safety used
                case 7256: // sight used
                    _showTrapHints = false;
                    break;
            }
        }
    }

    private void ClearState()
    {
        _lastChestContents = null;
        _showTrapHints = true;
        _interrupts.Clear();
        _forbiddenTargets.Clear();
        _gazes.Clear();
        _chestContents.Clear();
    }

    private bool OpenGold => _config.GoldCoffer && Palace.Items.Any(i => i.Count < 3);
    private bool OpenSilver => _config.SilverCoffer && Palace.WeaponLevel + Palace.ArmorLevel < 198;
    private bool OpenBronze => _config.BronzeCoffer;

    public override bool WantToBeDrawn() => true;

    public override List<string> CalculateGlobalHints()
    {
        StringBuilder sb = new();
        for (var i = 0; i < 25; i++)
        {
            var open = Palace.MapData[i] > 0 ? "X" : " ";
            sb.Append($"{open} ");
            if (i % 5 == 4)
                sb.Append('\n');
        }
        return [sb.ToString()];
    }

    private bool CanAutoUse(PomanderID p) => p is PomanderID.Steel or PomanderID.Strength or PomanderID.Sight;

    public override void BeforeCalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        hints.HintedActions.Add(ActionID.MakeSpell(AID.Chirp));
        hints.HintedActions.Add(ActionID.MakeSpell(AID.Tornado));
    }

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        if (!_config.Enable || Palace.Floor % 10 == 0)
            return;

        foreach (var d in _gazes)
            hints.ForbiddenDirections.Add((player.AngleTo(d.Source), 45.Degrees(), d.Activation));

        foreach (var d in _interrupts)
            if (hints.FindEnemy(d) is { } e)
                e.ShouldBeInterrupted = true;

        foreach (var d in _forbiddenTargets)
            if (hints.FindEnemy(d) is { } e)
                e.Priority = AIHints.Enemy.PriorityForbidFully;

        if (_config.TrapHints && _showTrapHints)
        {
            var traps = _trapsCurrentZone.Where(t => t.InCircle(player.Position, 30)).Select(t => ShapeDistance.Circle(t, 2)).ToList();
            if (traps.Count > 0)
                hints.AddForbiddenZone(ShapeDistance.Union(traps));
        }

        var isStunned = player.IsTransformed || player.Statuses.Any(s => (SID)s.ID is SID.Silence or SID.Pacification);
        var isOccupied = player.InCombat || isStunned;

        Actor? coffer = null;
        Actor? hoardLight = null;
        Actor? passage = null;
        List<Func<WPos, float>> revealedTraps = [];

        PomanderID? pomanderToUseHere = null;

        foreach (var a in World.Actors)
        {
            if (_chestContents.TryGetValue(a.InstanceID, out var pid) && Palace.Items[pid].Count == 3 && a.IsTargetable)
            {
                if (CanAutoUse(pid))
                    pomanderToUseHere ??= pid;
                continue;
            }

            var oid = (OID)a.OID;
            if (a.IsTargetable && (
                oid == OID.GoldCoffer && OpenGold ||
                oid == OID.SilverCoffer && OpenSilver ||
                (a.OID is >= 0x310 and <= 0x316 || a.OID is >= 0x322 and <= 0x325) && OpenBronze ||
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

        if (coffer != null && _lastChestContents is PomanderID p)
        {
            _chestContents[coffer.InstanceID] = p;
            _lastChestContents = null;
            return;
        }

        if (!isOccupied && pomanderToUseHere is PomanderID p2 && player.FindStatus(SID.ItemPenalty) == null)
            hints.ActionsToExecute.Push(new ActionID(ActionType.Pomander, (uint)p2), null, ActionQueue.Priority.Low);

        var haveChest = false;
        if (!isStunned && coffer is Actor t && InBounds(hints, t.Position) && (_config.AutoMoveTreasure || player.DistanceToHitbox(t) < 3.5f))
        {
            hints.InteractWithTarget = coffer;
            haveChest = true;
        }

        if (!player.InCombat && _config.AutoPassage && Palace.PassageActive && passage is Actor c)
        {
            hints.GoalZones.Add(hints.GoalSingleTarget(c.Position, 2, 0.5f));
            if (haveChest && player.DistanceToHitbox(c) < player.DistanceToHitbox(coffer) && !_config.OpenChestsFirst)
                hints.InteractWithTarget = null;
        }

        if (revealedTraps.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.Union(revealedTraps));

        if (!isOccupied && _config.AutoMoveTreasure && hoardLight is Actor h && Palace.Items[PomanderID.Intuition].Active && InBounds(hints, h.Position))
            hints.GoalZones.Add(hints.GoalSingleTarget(h.Position, 2, 10));

        if (!isOccupied && _config.AutoClear && (_config.FullClear || !Palace.PassageActive))
            foreach (var pp in hints.PotentialTargets)
                pp.Priority = 0;
    }

    private bool InBounds(AIHints hints, WPos pos) => hints.PathfindMapBounds.Contains(pos - hints.PathfindMapCenter);
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

static class PalacePalInterop
{
    private static readonly string PalacePalDbFile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher", "pluginConfigs", "PalacePal", "palace-pal.data.sqlite3");

    public static List<WPos> GetTrapLocationsForZone(uint zone)
    {
        List<WPos> locations = [];

        using (var connection = new SQLiteConnection($"Data Source={PalacePalDbFile}"))
        {
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                select X,Z from Locations where Type = 1 and TerritoryType = $tt
            ";
            command.Parameters.AddWithValue("$tt", zone);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var x = reader.GetFloat(0);
                    var z = reader.GetFloat(1);
                    locations.Add(new(x, z));
                }
            }
        }

        return locations;
    }
}
