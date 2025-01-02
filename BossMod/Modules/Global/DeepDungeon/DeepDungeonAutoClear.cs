using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using ImGuiNET;
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

sealed unsafe class DungeonDebugger : IDisposable
{
    private unsafe delegate char* DoLayoutDelegate(InstanceContentDeepDungeon* thisPtr, char a2, ulong a3);
    private readonly HookAddress<DoLayoutDelegate> _h1;

    private unsafe delegate char DoLayoutSingle(InstanceContentDeepDungeon* thisPtr, ulong a2, byte a3);
    private readonly HookAddress<DoLayoutSingle> _h2;

    private readonly LayoutManager* _layout;

    public DungeonDebugger()
    {
        _h1 = new("E8 ?? ?? ?? ?? 0F B6 8B ?? ?? ?? ?? 48 89 B3 ?? ?? ?? ??", F1);
        _h2 = new("E8 ?? ?? ?? ?? 48 81 C3 ?? ?? ?? ?? 48 8D 3D ?? ?? ?? ??", F2);

        _layout = LayoutWorld.Instance()->ActiveLayout;
    }

    private char* F1(InstanceContentDeepDungeon* thisPtr, char a2, ulong a3)
    {
        var orig = _h1.Original(thisPtr, a2, a3);
        Service.Log($"DoLayoutDetour: {(nint)thisPtr:X}, {(int)a2}, {a3:X} = {(nint)orig:X}");
        Service.Log($"offset into self: {(nint)orig - (nint)thisPtr:X}");
        return orig;
    }

    // second argument is the ID of a collisionbox in the layout instance
    // third argument: only two observed values
    //   - 0: collider should be left on
    //   - 2: collider should be disabled
    // there are at least two different sets of colliders that are updated here - for example, in PotD 1-10, the walls are group 256, layer E3F5
    // the other set is group 259 layer E446 and it seems to just be small structures in the middle of rooms - i can't figure out what these are for
    //
    // all of the collider IDs are stored in a datastructure in InstanceContentDeepDungeon starting at offset 0x1F20, but this includes both active and inactive ones, we have to cross reference with MapData to figure out which
    private char F2(InstanceContentDeepDungeon* thisPtr, ulong a2, byte a3)
    {
        var orig = _h2.Original(thisPtr, a2, a3);
        Service.Log($"DoLayoutSingle: {(nint)thisPtr:X}, {a2:X}, {a3:X} = {(byte)orig:X}");
        return orig;
    }

    public void Dispose()
    {
        _h1.Dispose();
        _h2.Dispose();
    }

    internal void FloorChanged()
    {
        var dd = EventFramework.Instance()->GetInstanceContentDeepDungeon();
        if (dd == null)
            return;

        var roomKey = dd->LayoutInfos[dd->ActiveLayoutIndex].RoomStartIndex;

        List<uint> ids = [];

        for (var k = roomKey; k < roomKey + 21; k++)
        {
            var row = Service.LuminaRow<Lumina.Excel.Sheets.DeepDungeonRoom>(k);
            // Level array, from left to right
            // center (id of SharedGroup, i think translation can be used to calculate room center?)
            // North wall
            // south wall
            // west wall
            // east wall
            ids.AddRange(row!.Value.Level.Skip(1).Select(l => l.RowId).Where(r => r > 1));
        }

        foreach (var (_, cbox) in *_layout->InstancesByType[InstanceType.CollisionBox].Value)
        {
            var key = cbox.Value->Id.InstanceKey;
            if (ids.Contains(key))
            {
                var tra = *cbox.Value->GetTranslationImpl();
                var rot = *cbox.Value->GetRotationImpl();
                var sc = *cbox.Value->GetScaleImpl();

                var pos = new Vector3(tra.X, tra.Y, tra.Z);
                var yrot = (2 * MathF.Acos(rot.W)).Radians();

                Service.Log($"{key:X} = {tra.XZ()} {yrot}");
            }
        }
    }
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

    private readonly Dictionary<ulong, PomanderID> _chestContentsGold = [];
    private readonly Dictionary<ulong, int> _chestContentsSilver = [];
    private readonly HashSet<ulong> _openedChests = [];
    private readonly HashSet<ulong> _fakeExits = [];
    private PomanderID? _lastChestContentsGold;
    private bool _lastChestMagicite;
    private bool _trapsHidden = true;
    private readonly DungeonDebugger? _dbg = null;

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

#if DEBUG
        if (Service.SigScanner != null)
            _dbg = new();
#endif
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
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
            case 7270: // "Floor #"
                _dbg?.FloorChanged();
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
        Interrupts.Clear();
        ForbiddenTargets.Clear();
        _lastChestContentsGold = null;
        _lastChestMagicite = false;
        _chestContentsGold.Clear();
        _chestContentsSilver.Clear();
        _trapsHidden = true;
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

    public override bool WantToBeDrawn() => false; // _dbg != null;

    public override void DrawExtra()
    {
        if (ImGui.Button("Check colliders"))
        {
            _dbg?.FloorChanged();
        }
    }

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

        if (_config.TrapHints && _trapsHidden)
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
