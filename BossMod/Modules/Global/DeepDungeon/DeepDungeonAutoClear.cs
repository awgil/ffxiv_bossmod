using System.Data.SQLite;
using System.IO;
using System.Text;

namespace BossMod.Global.DeepDungeon;

[ConfigDisplay(Name = "Auto-DeepDungeon", Parent = typeof(ModuleConfig))]
public class AutoDDConfig : ConfigNode
{
    public enum ClearBehavior
    {
        [PropertyDisplay("Do not auto target")]
        None,
        [PropertyDisplay("Stop when passage opens")]
        Passage,
        [PropertyDisplay("Target everything if not at level cap, otherwise stop when passage opens")]
        Leveling,
        [PropertyDisplay("Target everything")]
        All,
    }

    [PropertyDisplay("Enable module")]
    public bool Enable = true;
    [PropertyDisplay("Try to avoid traps", tooltip: "Avoid known trap locations sourced from PalacePal data. (Traps revealed by a Pomander of Sight will always be avoided regardless of this setting.)")]
    public bool TrapHints = true;
    [PropertyDisplay("Automatically navigate to Cairn of Passage")]
    public bool AutoPassage = true;

    [PropertyDisplay("Automatic mob targeting behavior")]
    public ClearBehavior AutoClear = ClearBehavior.Leveling;

    [PropertyDisplay("Automatically navigate to coffers")]
    public bool AutoMoveTreasure = true;
    [PropertyDisplay("Prioritize opening coffers over Cairn of Passage")]
    public bool OpenChestsFirst = false;
    [PropertyDisplay("Open gold coffers")]
    public bool GoldCoffer = true;
    [PropertyDisplay("Open silver coffers")]
    public bool SilverCoffer = true;
    [PropertyDisplay("Open bronze coffers")]
    public bool BronzeCoffer = true;
}

enum OID : uint
{
    CairnPalace = 0x1EA094,
    BeaconHoH = 0x1EA9A3,
    PylonEO = 0x1EB867,
    SilverCoffer = 0x1EA13D,
    GoldCoffer = 0x1EA13E,
    BandedCofferIndicator = 0x1EA1F6,
    BandedCoffer = 0x1EA1F7,
}

enum SID : uint
{
    Silence = 7,
    Pacification = 620,
    ItemPenalty = 1094,
}

public abstract class DeepDungeonAutoClear : ZoneModule
{
    public readonly int LevelCap;

    public static readonly HashSet<uint> BronzeChestIDs = [
        // PotD
        782, 783, 784, 785, 786, 787, 788, 789, 790, 802, 803, 804, 805,
        // HoH
        1036, 1037, 1038, 1039, 1040, 1041, 1042, 1043, 1044, 1045, 1046, 1047, 1048, 1049,
        // EO
        1541, 1542, 1543, 1544, 1545, 1546, 1547, 1548, 1549, 1550, 1551, 1552, 1553, 1554
    ];
    public static readonly HashSet<uint> RevealedTrapOIDs = [0x1EA08E, 0x1EA08F, 0x1EA090, 0x1EA091, 0x1EA092, 0x1EA9A0];

    protected readonly List<(Actor Source, float Radius)> Donuts = [];
    protected readonly List<(Actor Source, float Radius)> Circles = [];
    protected readonly List<(Actor Source, AOEShape? Shape)> Gazes = [];
    protected readonly List<Actor> Interrupts = [];
    protected readonly List<Actor> Stuns = [];
    protected readonly List<Actor> ForbiddenTargets = [];

    protected readonly AutoDDConfig _config = Service.Config.Get<AutoDDConfig>();
    private readonly EventSubscriptions _subscriptions;
    private readonly List<WPos> _trapsCurrentZone = [];

    private readonly Dictionary<ulong, PomanderID> _chestContents = [];
    private readonly HashSet<ulong> _openedChests = [];
    private readonly HashSet<ulong> _fakeExits = [];
    private PomanderID? _lastChestContents;
    private bool _showTrapHints = true;

    protected DeepDungeonState Palace => World.DeepDungeon;

    public DeepDungeonAutoClear(WorldState ws, int LevelCap) : base(ws)
    {
        this.LevelCap = LevelCap;

        _subscriptions = new(
            ws.SystemLogMessage.Subscribe(OnSystemLogMessage),
            ws.Actors.CastStarted.Subscribe(OnCastStarted),
            ws.Actors.CastFinished.Subscribe(OnCastFinished),
            ws.Actors.StatusGain.Subscribe(OnStatusGain),
            ws.Actors.StatusLose.Subscribe(OnStatusLose),
            ws.Actors.EventOpenTreasure.Subscribe(OnOpenTreasure),
            ws.Actors.EventObjectAnimation.Subscribe(OnEObjAnim)
        );

        _trapsCurrentZone = PalacePalInterop.GetTrapLocationsForZone(ws.CurrentZone);
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
        base.Dispose(disposing);
    }

    protected virtual void OnCastStarted(Actor actor) { }

    protected virtual void OnCastFinished(Actor actor) { }

    protected virtual void OnStatusGain(Actor actor, int index) { }

    protected virtual void OnStatusLose(Actor actor, int index) { }

    private void OnSystemLogMessage(WorldState.OpSystemLogMessage op)
    {
        switch (op.MessageId)
        {
            case 7222: // pomander overcap
                _lastChestContents = (PomanderID)op.Args[0];
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

    private void OnOpenTreasure(Actor chest) => _openedChests.Add(chest.InstanceID);

    private void OnEObjAnim(Actor actor, ushort p1, ushort p2)
    {
        // fake beacon deactivation; accompanied by system log #9217 but it does not indicate a specific actor
        if (actor.OID == (uint)OID.BeaconHoH && p1 == 0x0400 && p2 == 0x0800)
            _fakeExits.Add(actor.InstanceID);
    }

    private void ClearState()
    {
        Gazes.Clear();
        Interrupts.Clear();
        ForbiddenTargets.Clear();
        _lastChestContents = null;
        _showTrapHints = true;
        _chestContents.Clear();
        _openedChests.Clear();
        _fakeExits.Clear();
    }

    private bool OpenGold => _config.GoldCoffer;
    private bool OpenSilver
    {
        get
        {
            // disabled
            if (!_config.SilverCoffer)
                return false;

            // sanity check
            if (World.Party.Player() is not { } player)
                return false;

            // explosive silver chests deal 70% max hp damage
            if (player.HPMP.CurHP <= player.HPMP.MaxHP * 0.7f)
                return false;

            // upgrade weapon if desired
            if (Palace.Progress.WeaponLevel + Palace.Progress.ArmorLevel < 198)
                return true;

            return Palace.Type switch
            {
                DeepDungeonState.DungeonType.HOH or DeepDungeonState.DungeonType.EO => Palace.Progress.Floor >= 7,// magicite/demiclones start dropping on floor 7
                _ => false,
            };
        }
    }

    private bool OpenBronze => _config.BronzeCoffer;

    public override bool WantToBeDrawn() => true;

    public override List<string> CalculateGlobalHints()
    {
        StringBuilder sb = new();
        for (var i = 0; i < 25; i++)
        {
            sb.Append($"{Palace.MapData[i]:X2} ");
            if (i % 5 == 4)
                sb.Append('\n');
        }
        return [sb.ToString()];
    }

    private bool CanAutoUse(PomanderID p) => p
        is PomanderID.Steel or PomanderID.Strength or PomanderID.Sight or PomanderID.Raising
        or PomanderID.ProtoSteel or PomanderID.ProtoStrength or PomanderID.ProtoSight or PomanderID.ProtoRaising
        or PomanderID.ProtoLethargy;

    protected virtual IEnumerable<ActionID> ActionsToIgnore() => [];

    public override void BeforeCalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        hints.HintedActions.UnionWith(ActionsToIgnore());
    }

    private bool OnBeacon(WPos pos) => pos.AlmostEqual(new WPos(259.7f, 307.14f), 1);

    private void IterAndExpire<T>(List<T> items, Func<T, bool> expire, Action<T> action)
    {
        for (var i = items.Count - 1; i >= 0; i--)
        {
            var item = items[i];
            if (expire(item))
                items.RemoveAt(i);
            else
                action(item);
        }
    }

    private DateTime CastFinishAt(Actor c) => World.FutureTime(c.CastInfo!.NPCRemainingTime);

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        if (!_config.Enable || Palace.Progress.Floor % 10 == 0)
            return;

        IterAndExpire(Gazes, g => g.Source.CastInfo == null, d =>
        {
            if (d.Shape == null || d.Shape.Check(player.Position, d.Source))
                hints.ForbiddenDirections.Add((player.AngleTo(d.Source), 45.Degrees(), CastFinishAt(d.Source)));
        });

        IterAndExpire(Donuts, d => d.Source.CastInfo == null, d =>
        {
            hints.AddForbiddenZone(new AOEShapeDonut(d.Radius, 100), d.Source.Position, default, CastFinishAt(d.Source));
        });

        IterAndExpire(Circles, d => d.Source.CastInfo == null, d =>
        {
            hints.AddForbiddenZone(new AOEShapeCircle(d.Radius), d.Source.Position, default, CastFinishAt(d.Source));
        });

        IterAndExpire(Interrupts, d => d.CastInfo == null, d =>
        {
            if (hints.FindEnemy(d) is { } e)
                e.ShouldBeInterrupted = true;
        });

        IterAndExpire(Stuns, d => d.CastInfo == null, d =>
        {
            if (hints.FindEnemy(d) is { } e)
                e.ShouldBeStunned = true;
        });

        foreach (var d in ForbiddenTargets)
            if (hints.FindEnemy(d) is { } e)
                e.Priority = AIHints.Enemy.PriorityForbidFully;

        if (_config.TrapHints && _showTrapHints)
        {
            var traps = _trapsCurrentZone.Where(t => t.InCircle(player.Position, 30) && !OnBeacon(t)).Select(t => ShapeDistance.Circle(t, 2)).ToList();
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
            if (_chestContents.TryGetValue(a.InstanceID, out var pid) && Palace.GetItem(pid).Count == 3 && a.IsTargetable)
            {
                if (CanAutoUse(pid))
                    pomanderToUseHere ??= pid;
                continue;
            }

            if (_openedChests.Contains(a.InstanceID) || _fakeExits.Contains(a.InstanceID))
                continue;

            var oid = (OID)a.OID;
            if (a.IsTargetable && (
                oid == OID.GoldCoffer && OpenGold ||
                oid == OID.SilverCoffer && OpenSilver && player.HPMP.CurHP > player.HPMP.MaxHP * 0.7f ||
                BronzeChestIDs.Contains(a.OID) && OpenBronze ||
                oid == OID.BandedCoffer
            ))
            {
                if ((coffer?.DistanceToHitbox(player) ?? float.MaxValue) > a.DistanceToHitbox(player))
                    coffer = a;
            }

            if (a.OID == (uint)OID.BandedCofferIndicator)
                hoardLight = a;

            if ((OID)a.OID is OID.CairnPalace or OID.BeaconHoH or OID.PylonEO && (passage?.DistanceToHitbox(player) ?? float.MaxValue) > a.DistanceToHitbox(player))
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
        if (coffer is Actor t && InBounds(hints, t.Position))
        {
            if (_config.AutoMoveTreasure && !isOccupied || player.DistanceToHitbox(t) < 3.5f && !isStunned)
            {
                hints.InteractWithTarget = coffer;
                haveChest = true;
            }
        }

        if (!player.InCombat && _config.AutoPassage && Palace.PassageActive && passage is Actor c)
        {
            hints.GoalZones.Add(hints.GoalSingleTarget(c.Position, 2, 0.5f));
            if (haveChest && player.DistanceToHitbox(c) < player.DistanceToHitbox(coffer) && !_config.OpenChestsFirst)
                hints.InteractWithTarget = null;
        }

        if (revealedTraps.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.Union(revealedTraps));

        if (!isOccupied && _config.AutoMoveTreasure && hoardLight is Actor h && Palace.GetItem(PomanderID.Intuition).Active && InBounds(hints, h.Position))
            hints.GoalZones.Add(hints.GoalSingleTarget(h.Position, 2, 10));

        var shouldTargetMobs = !isOccupied && _config.AutoClear switch
        {
            AutoDDConfig.ClearBehavior.Passage => !Palace.PassageActive,
            AutoDDConfig.ClearBehavior.Leveling => player.Level < LevelCap || !Palace.PassageActive,
            AutoDDConfig.ClearBehavior.All => true,
            _ => false
        };

        if (shouldTargetMobs)
            foreach (var pp in hints.PotentialTargets.Where(t => t.Actor.FindStatus(2056) == null))
                pp.Priority = 0;
    }

    private bool InBounds(AIHints hints, WPos pos) => hints.PathfindMapBounds.Contains(pos - hints.PathfindMapCenter);

}

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
