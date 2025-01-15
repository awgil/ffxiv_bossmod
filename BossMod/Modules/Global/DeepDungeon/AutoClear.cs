using BossMod.Pathfinding;
using ImGuiNET;
using System.Data.SQLite;
using System.IO;
using System.Text.Json;
using static FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentDeepDungeon;

namespace BossMod.Global.DeepDungeon;

[Serializable]
public record class Floor<T>(
    uint DungeonId,
    uint Floorset,
    Tileset<T> RoomsA,
    Tileset<T> RoomsB
)
{
    public Floor<M> Map<M>(Func<T, M> Mapping) => new(DungeonId, Floorset, RoomsA.Map(Mapping), RoomsB.Map(Mapping));
}

public record class Tileset<T>(List<RoomData<T>> Rooms)
{
    public Tileset<M> Map<M>(Func<T, M> Mapping) => new(Rooms.Select(m => m.Map(Mapping)).ToList());

    public RoomData<T> this[int index] => Rooms[index];

    public override string ToString() => $"Tileset {{ Rooms = [{string.Join(", ", Rooms)}] }}";
}

public record class RoomData<T>(
    T Center,
    T North,
    T South,
    T West,
    T East
)
{
    public RoomData<M> Map<M>(Func<T, M> F) => new(F(Center), F(North), F(South), F(West), F(East));
}

public record struct Wall(WPos Position, float Depth);

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
    [PropertyDisplay("Enable minimap")]
    public bool EnableMinimap = true;
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

    PhysicalDamageUp = 53,
    DamageUp = 61,
    DreadBeastAura = 2056, // unnamed status, displays red fog vfx on actor
    EvasionUp = 2402, // applied by Peculiar Light from orthos diplocaulus
}

sealed unsafe class DungeonDebugger : IDisposable
{

    public void Dispose()
    {
    }
}

public abstract class AutoClear : ZoneModule
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
    public static readonly HashSet<uint> RevealedTrapOIDs = [0x1EA08E, 0x1EA08F, 0x1EA090, 0x1EA091, 0x1EA092, 0x1EA9A0, 0x1EB864];

    protected readonly List<(Actor Source, float Inner, float Outer)> Donuts = [];
    protected readonly List<(Actor Source, float Radius)> Circles = [];
    private readonly List<Gaze> Gazes = [];
    protected readonly List<Actor> Interrupts = [];
    protected readonly List<Actor> Stuns = [];
    protected readonly List<Actor> ForbiddenTargets = [];
    private readonly List<Actor> LOS = [];

    private readonly Dictionary<ulong, (WPos, Bitmap)> _losCache = [];

    public record class Gaze(Actor Source, AOEShape Shape);

    protected void AddGaze(Actor Source, AOEShape Shape) => Gazes.Add(new(Source, Shape));
    protected void AddGaze(Actor Source, float Radius) => AddGaze(Source, new AOEShapeCircle(Radius));

    protected void AddLOS(Actor Source, float Range)
    {
        var (entry, data) = _obstacles.Find(Source.PosRot.XYZ());
        if (entry == null || data == null)
        {
            Service.Log($"no bitmap found for {Source}, not adding LOS hints");
            return;
        }

        var pixelRange = (int)(Range / data.PixelSize);
        var casterOff = Source.Position - entry.Origin;
        var casterCell = casterOff / data.PixelSize;
        var casterX = (int)casterCell.X;
        var casterZ = (int)casterCell.Z;

        var bm = new Bitmap(data.Width, data.Height, data.Color0, data.Color1, data.Resolution);
        for (var i = Math.Max(0, casterX - pixelRange); i <= Math.Min(data.Width, casterX + pixelRange); i++)
        {
            for (var j = Math.Max(0, casterZ - pixelRange); j <= Math.Min(data.Height, casterZ + pixelRange); j++)
            {
                var pt = new Vector2(i, j);
                var cc = new Vector2(casterX, casterZ);
                if (!IsBlocked(data, pt, cc, pixelRange))
                    bm[i, j] = true;
            }
        }

        _losCache[Source.InstanceID] = (entry.Origin, bm);
        LOS.Add(Source);
    }

    private static bool IsBlocked(Bitmap map, Vector2 point, Vector2 origin, float maxRange)
    {
        var dir = origin - point;
        var dist = dir.Length();
        if (dist >= maxRange)
            return true;

        dir /= dist;

        var ox = point.X;
        var oy = point.Y;
        var vx = dir.X;
        var vy = dir.Y;

        for (var i = 0; i < (int)dist; i++)
        {
            if (map[(int)ox, (int)oy])
                return true;
            ox += vx;
            oy += vy;
        }

        return false;
    }

    protected readonly AutoDDConfig _config = Service.Config.Get<AutoDDConfig>();
    private readonly EventSubscriptions _subscriptions;
    private readonly List<WPos> _trapsCurrentZone = [];

    private readonly Dictionary<ulong, PomanderID> _chestContentsGold = [];
    private readonly Dictionary<ulong, int> _chestContentsSilver = [];
    private readonly HashSet<ulong> _openedChests = [];
    private readonly HashSet<ulong> _fakeExits = [];
    private PomanderID? _lastChestContentsGold;
    private bool _lastChestMagicite;
    private bool _trapsHidden = true;
    private readonly DungeonDebugger? _dbg = null;

    private readonly Dictionary<string, Floor<Wall>> LoadedFloors;
    private readonly List<(Wall Wall, bool Rotated)> Walls = [];

    private int Kills;
    private int DesiredRoom;
    private bool BetweenFloors;

    private ObstacleMapManager _obstacles;

    protected DeepDungeonState Palace => World.DeepDungeon;

    public static readonly string WallsFile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher", "pluginConfigs", "BossMod", "walls.json");

    protected AutoClear(WorldState ws, int LevelCap) : base(ws)
    {
        this.LevelCap = LevelCap;
        _obstacles = new(ws);

        _subscriptions = new(
            ws.SystemLogMessage.Subscribe(OnSystemLogMessage),
            ws.Actors.CastStarted.Subscribe(OnCastStarted),
            ws.Actors.CastFinished.Subscribe(OnCastFinished),
            ws.Actors.StatusGain.Subscribe(OnStatusGain),
            ws.Actors.StatusLose.Subscribe(OnStatusLose),
            ws.Actors.IsDeadChanged.Subscribe(op =>
            {
                if (!op.IsAlly && op.IsDead)
                    Kills++;
            }),
            ws.Actors.EventOpenTreasure.Subscribe(OnOpenTreasure),
            ws.Actors.EventObjectAnimation.Subscribe(OnEObjAnim),
            ws.DeepDungeon.MapDataChanged.Subscribe(_ =>
            {
                BetweenFloors = false;
                if (Walls.Count == 0)
                    LoadWalls();
            })
        );

        _trapsCurrentZone = PalacePalInterop.GetTrapLocationsForZone(ws.CurrentZone);

        using (var fstream = new FileStream(WallsFile, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
        {
            LoadedFloors = JsonSerializer.Deserialize<Dictionary<string, Floor<Wall>>>(fstream)!;
        }

#if DEBUG
        if (Service.SigScanner != null)
            _dbg = new();
#endif
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
        _obstacles.Dispose();
        _dbg?.Dispose();
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
                _lastChestContentsGold = (PomanderID)op.Args[0];
                break;
            case 7248: // transference initiated
                ClearState();
                break;
            case 7255: // safety used
            case 7256: // sight used
                _trapsHidden = false;
                break;
            case 10287: // demiclone overcap
                _lastChestMagicite = true;
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
        Walls.Clear();
        Interrupts.Clear();
        ForbiddenTargets.Clear();
        DesiredRoom = 0;
        Kills = 0;
        _lastChestContentsGold = null;
        _lastChestMagicite = false;
        _chestContentsGold.Clear();
        _chestContentsSilver.Clear();
        _trapsHidden = true;
        _openedChests.Clear();
        _fakeExits.Clear();
        BetweenFloors = true;
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
                DeepDungeonState.DungeonType.HOH or DeepDungeonState.DungeonType.EO => Palace.Floor >= 7,// magicite/demiclones start dropping on floor 7
                _ => false,
            };
        }
    }

    private bool OpenBronze => _config.BronzeCoffer;

    public override bool WantDrawExtra() => _config.EnableMinimap && !Palace.Progress.IsBossFloor;

    private bool _allowNavigationInCombat;

    public override void DrawExtra()
    {
        var player = World.Party.Player();
        var targetRoom = new Minimap(Palace, player?.Rotation ?? default, DesiredRoom).Draw();
        if (targetRoom >= 0)
            DesiredRoom = targetRoom;

        ImGui.Text($"Kills: {Kills}");
        ImGui.Checkbox("Allow navigation in combat", ref _allowNavigationInCombat);
        if (ImGui.Button("Reload obstacles"))
        {
            _obstacles.Dispose();
            _obstacles = new(World);
        }
        if (player != null)
        {
            var (entry, data) = _obstacles.Find(player.PosRot.XYZ());
            if (entry == null)
            {
                ImGui.SameLine();
                UIMisc.HelpMarker(() => "Obstacle map missing for floor!", Dalamud.Interface.FontAwesomeIcon.ExclamationTriangle);
            }

            if (data != null && data.PixelSize != 0.5f)
            {
                ImGui.SameLine();
                UIMisc.HelpMarker(() => $"Wrong resolution for map; should be 0.5, got {data.PixelSize}", Dalamud.Interface.FontAwesomeIcon.ExclamationTriangle);
            }
        }
    }

    private bool CanAutoUse(PomanderID p) => p
        is PomanderID.Steel or PomanderID.Strength or PomanderID.Sight or PomanderID.Raising
        or PomanderID.ProtoSteel or PomanderID.ProtoStrength or PomanderID.ProtoSight or PomanderID.ProtoRaising
        or PomanderID.ProtoLethargy;

    protected virtual IEnumerable<ActionID> AutohintDisabledActions() => [];

    public override void BeforeCalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        hints.AutohintDisabledActions.UnionWith(AutohintDisabledActions());
    }

    private void IterAndExpire<T>(List<T> items, Func<T, bool> expire, Action<T> action, Action<T>? onRemove = null)
    {
        for (var i = items.Count - 1; i >= 0; i--)
        {
            var item = items[i];
            if (expire(item))
            {
                items.RemoveAt(i);
                onRemove?.Invoke(item);
            }
            else
                action(item);
        }
    }

    private DateTime CastFinishAt(Actor c) => World.FutureTime(c.CastInfo!.NPCRemainingTime);

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        if (!_config.Enable || Palace.Progress.IsBossFloor || BetweenFloors)
            return;

        foreach (var (w, rot) in Walls)
            hints.AddForbiddenZone(new AOEShapeRect(w.Depth, 20, w.Depth), w.Position, (rot ? 90f : 0f).Degrees());

        if (_obstacles.Find(player.PosRot.XYZ()).entry == null)
            hints.ForcedMovement = new(0);

        HandleFloorPathfind(player, hints);

        IterAndExpire(Gazes, g => g.Source.CastInfo == null, d =>
        {
            if (d.Shape.Check(player.Position, d.Source))
                hints.ForbiddenDirections.Add((player.AngleTo(d.Source), 45.Degrees(), CastFinishAt(d.Source)));
        });

        IterAndExpire(Donuts, d => d.Source.CastInfo == null, d =>
        {
            hints.AddForbiddenZone(new AOEShapeDonut(d.Inner, d.Outer), d.Source.Position, default, CastFinishAt(d.Source));
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

        IterAndExpire(LOS, d => d.CastInfo == null, caster =>
        {
            if (!_losCache.TryGetValue(caster.InstanceID, out var dangermap))
                return;

            var origin = dangermap.Item1;
            var map = dangermap.Item2;

            hints.AddForbiddenZone(p =>
            {
                var offset = (p - origin) / map.PixelSize;
                return map[(int)offset.X, (int)offset.Z] ? -10 : 10;
            }, CastFinishAt(caster));
        }, d => _losCache.Remove(d.InstanceID));

        foreach (var d in ForbiddenTargets)
            if (hints.FindEnemy(d) is { } e)
                e.Priority = AIHints.Enemy.PriorityForbidden;

        if (_config.TrapHints && _trapsHidden)
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
            if (_chestContentsGold.TryGetValue(a.InstanceID, out var pid) && Palace.GetItem(pid).Count == 3 && a.IsTargetable)
            {
                if (CanAutoUse(pid))
                    pomanderToUseHere ??= pid;
                continue;
            }

            if (_chestContentsSilver.ContainsKey(a.InstanceID) && Palace.Magicite.All(m => m > 0))
                // TODO use magicite/demiclone to prevent overcap
                continue;

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

        if (coffer != null)
        {
            if (_lastChestContentsGold is PomanderID p)
            {
                _chestContentsGold[coffer.InstanceID] = p;
                _lastChestContentsGold = null;
                return;
            }

            if (_lastChestMagicite)
            {
                // TODO figure out why the system log args arent working
                _chestContentsSilver[coffer.InstanceID] = 1;
                _lastChestMagicite = false;
                return;
            }
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

        if (!player.InCombat && _config.AutoPassage && Palace.PassageActive)
        {
            DesiredRoom = Array.FindIndex(Palace.MapData, d => ((RoomFlags)d).HasFlag(RoomFlags.Passage));

            if (passage is Actor c)
            {
                hints.GoalZones.Add(hints.GoalSingleTarget(c.Position, 2, 0.5f));
                // give pathfinder a little help lmao
                hints.GoalZones.Add(hints.GoalSingleTarget(c.Position, 25, 0.25f));
                if (haveChest && player.DistanceToHitbox(c) < player.DistanceToHitbox(coffer) && !_config.OpenChestsFirst)
                    hints.InteractWithTarget = null;
            }
        }

        if (revealedTraps.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.Union(revealedTraps));

        if (!isOccupied && _config.AutoMoveTreasure && hoardLight is Actor h && Palace.GetItem(PomanderID.Intuition).Active && InBounds(hints, h.Position))
            hints.GoalZones.Add(hints.GoalSingleTarget(h.Position, 2, 10));

        var shouldTargetMobs = _config.AutoClear switch
        {
            AutoDDConfig.ClearBehavior.Passage => !Palace.PassageActive,
            AutoDDConfig.ClearBehavior.Leveling => player.Level < LevelCap || !Palace.PassageActive,
            AutoDDConfig.ClearBehavior.All => true,
            _ => false
        };

        if (shouldTargetMobs && !player.InCombat && player.TargetID == 0)
            foreach (var pp in hints.PotentialTargets.Where(t => !t.Actor.Statuses.Any(s => IsDangerousOutOfCombatStatus(s.ID))))
                pp.Priority = 0;
    }

    private bool InBounds(AIHints hints, WPos pos) => hints.PathfindMapBounds.Contains(pos - hints.PathfindMapCenter);

    private static bool IsDangerousOutOfCombatStatus(uint statusRaw) => (SID)statusRaw is SID.DamageUp or SID.DreadBeastAura or SID.PhysicalDamageUp;

    private void HandleFloorPathfind(Actor player, AIHints hints)
    {
        if (player.InCombat && !_allowNavigationInCombat)
            return;

        var playerRoom = Palace.Party[0].Room;

        if (DesiredRoom == playerRoom || DesiredRoom == 0)
        {
            DesiredRoom = 0;
            return;
        }

        var path = new FloorPathfind(Palace.Map).Pathfind(playerRoom, DesiredRoom);
        if (path.Count == 0)
        {
            Service.Log($"uh-oh, no path from {playerRoom} to {DesiredRoom}");
            return;
        }
        var next = path[0];
        Direction d;
        if (next == playerRoom + 1)
            d = Direction.East;
        else if (next == playerRoom - 1)
            d = Direction.West;
        else if (next == playerRoom + 5)
            d = Direction.South;
        else if (next == playerRoom - 5)
            d = Direction.North;
        else
        {
            Service.Log($"pathfinding instructions are nonsense: {string.Join(", ", path)}");
            DesiredRoom = 0;
            return;
        }

        hints.GoalZones.Add(p =>
        {
            var pp = player.Position;
            return d switch
            {
                Direction.North => pp.Z - p.Z,
                Direction.South => p.Z - pp.Z,
                Direction.East => p.X - pp.X,
                Direction.West => pp.X - p.X,
                _ => 0,
            } * 0.001f;
        });
    }

    private void LoadWalls()
    {
        Service.Log($"loading walls for current floor...");
        Walls.Clear();
        var floorset = Palace.Floor / 10;
        var key = $"{Palace.DungeonId}.{floorset + 1}";
        if (!LoadedFloors.TryGetValue(key, out var floor))
        {
            Service.Log($"unable to load floorset {key}");
            return;
        }
        Tileset<Wall> tileset;
        switch (Palace.Progress.Tileset)
        {
            case 0:
                tileset = floor.RoomsA;
                break;
            case 1:
                tileset = floor.RoomsB;
                break;
            case 2:
                Service.Log($"hall of fallacies - nothing to do");
                return;
            default:
                Service.Log($"unrecognized tileset number {Palace.Progress.Tileset}");
                return;
        }
        foreach (var (room, i) in Palace.Map.ToArray().Select((m, i) => (m, i)))
        {
            if (room > 0)
            {
                var roomdata = tileset[i];
                if (roomdata.North != default && !room.HasFlag(RoomFlags.ConnectionN))
                    Walls.Add((roomdata.North, false));
                if (roomdata.South != default && !room.HasFlag(RoomFlags.ConnectionS))
                    Walls.Add((roomdata.South, false));
                if (roomdata.East != default && !room.HasFlag(RoomFlags.ConnectionE))
                    Walls.Add((roomdata.East, true));
                if (roomdata.West != default && !room.HasFlag(RoomFlags.ConnectionW))
                    Walls.Add((roomdata.West, true));
            }
        }

    }
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

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var x = reader.GetFloat(0);
                var z = reader.GetFloat(1);
                locations.Add(new(x, z));
            }
        }

        return locations;
    }
}
