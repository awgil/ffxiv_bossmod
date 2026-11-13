using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using static FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.InstanceContentDeepDungeon;

namespace BossMod.Global.DeepDungeon;

enum OID : uint
{
    CairnPalace = 0x1EA094,
    BeaconHoH = 0x1EA9A3,
    PylonEO = 0x1EB867,
    PylonPT = 0x1EBE24,
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

    StoneCurse = 437, // petrification (on enemies)
    AutoHealPenalty = 1097,
}

public abstract partial class AutoClear : ZoneModule
{
    public record struct GoalObject(Actor Actor, GoalType Type, float PlayerDistSq, int Tier = 0)
    {
        public readonly bool IsTreasure => Type is GoalType.Treasure or GoalType.ChestHoard;
    }

    public enum GoalType
    {
        ChestHoard,
        Hoard,
        Passage,
        Treasure,
    }

    public readonly int LevelCap;

    public static readonly HashSet<uint> BronzeChestIDs = [
        // PotD
        782, 783, 784, 785, 786, 787, 788, 789, 790, 802, 803, 804, 805,
        // HoH
        1036, 1037, 1038, 1039, 1040, 1041, 1042, 1043, 1044, 1045, 1046, 1047, 1048, 1049,
        // EO
        1541, 1542, 1543, 1544, 1545, 1546, 1547, 1548, 1549, 1550, 1551, 1552, 1553, 1554,
        // PT
        1881, 1882, 1883, 1884, 1885, 1886, 1887, 1888, 1889, 1890, 1891, 1892, 1893, 1906, 1907, 1908
    ];
    public static readonly HashSet<uint> RevealedTrapOIDs = [0x1EA08E, 0x1EA08F, 0x1EA090, 0x1EA091, 0x1EA092, 0x1EA9A0, 0x1EB864, 0x1EBEDB];

    protected readonly List<(Actor Source, float Inner, float Outer, Angle HalfAngle)> Donuts = [];
    protected readonly List<(Actor Source, float Radius)> Circles = [];
    protected readonly List<(Actor Source, float Radius)> KnockbackZones = [];
    protected readonly List<(Actor Source, AOEShape Zone, DateTime Activation, int Counter)> Voidzones = [];
    private readonly List<Gaze> Gazes = [];
    protected readonly List<Actor> Interrupts = [];
    protected readonly List<Actor> Stuns = [];
    protected readonly List<(Actor Actor, DateTime Timeout)> Spikes = [];
    protected readonly List<Actor> HintDisabled = [];
    private readonly List<Actor> LOS = [];

    private readonly Dictionary<ulong, RelSimplifiedComplexPolygon> _losCache = [];

    public record class Gaze(Actor Source, AOEShape Shape);

    protected readonly AutoDDConfig _config = Service.Config.Get<AutoDDConfig>();
    private readonly EventSubscriptions _subscriptions;

    private readonly Dictionary<ulong, PomanderID> _chestContentsGold = [];
    private readonly Dictionary<ulong, int> _chestContentsSilver = [];
    private readonly HashSet<ulong> _fakeExits = [];
    private PomanderID? _lastChestContentsGold;
    private bool _lastChestMagicite;

    private readonly List<GoalObject> InterestingObjects = [];
    private GoalObject? MostInterestingObject;

    private readonly List<PointTest> _pathfindZones = [];

    private int Kills;
    private bool BetweenFloors = true;

    private bool PendingInitialize;

    protected struct PlayerImmuneState
    {
        public DateTime RoleBuffExpire; // 0 if not active
        public DateTime JobBuffExpire; // 0 if not active
        public bool KnockbackPenalty;

        public readonly bool ImmuneAt(DateTime time) => KnockbackPenalty || RoleBuffExpire > time || JobBuffExpire > time;
    }

    private readonly PlayerImmuneState[] _playerImmunes = new PlayerImmuneState[4];

    protected DeepDungeonState Palace => World.DeepDungeon;

    private readonly ICallGateSubscriber<ushort, List<Vector3>> _palantirGetTraps = Service.PluginInterface.GetIpcSubscriber<ushort, List<Vector3>>("Palantir.GetTraps");
    private readonly ICallGateSubscriber<List<Vector3>, object> _palantirWatchTraps = Service.PluginInterface.GetIpcSubscriber<List<Vector3>, object>("Palantir.WatchTraps");

    const float TrapRadius = 1.75f;

    protected AutoClear(WorldState ws, int LevelCap) : base(ws)
    {
        this.LevelCap = LevelCap;

        _subscriptions = new(
            ws.SystemLogMessage.Subscribe(OnSystemLogMessage),
            ws.Actors.CastStarted.Subscribe(OnCastStarted),
            ws.Actors.CastFinished.Subscribe(OnCastFinished),
            ws.Actors.CastEvent.Subscribe(HandleCastEvent),
            ws.Actors.Added.Subscribe(HandleActorAdd),
            ws.Actors.InCombatChanged.Subscribe(OnActorCombatChanged),
            ws.Actors.StatusGain.Subscribe(OnActorStatusGain),
            ws.Actors.StatusLose.Subscribe(OnActorStatusLose),
            ws.Actors.IsDeadChanged.Subscribe(op =>
            {
                if (!op.IsAlly && op.IsDead)
                    Kills++;
            }),
            ws.Actors.EventObjectAnimation.Subscribe(OnEObjAnim),
            ws.DeepDungeon.MapDataChanged.Subscribe(op =>
            {
                if (BetweenFloors && !PendingInitialize && op.Rooms.Any(r => r > 0))
                    OnEnterFloorInternal();
            })
        );

        LoadedFloors = Utils.LoadResource<Dictionary<string, Floor<Wall>>>("BossMod.Global.DeepDungeon.Walls.json");
        ProblematicTrapLocations = Utils.LoadResource<List<WPos>>("BossMod.Global.DeepDungeon.BadTraps.json");

        _palantirWatchTraps?.Subscribe(UpdateTrapsFloorset);

        PendingInitialize = true;
    }

    protected override void Dispose(bool disposing)
    {
        _palantirWatchTraps?.Unsubscribe(UpdateTrapsFloorset);
        _subscriptions.Dispose();
        base.Dispose(disposing);
    }

    public override void OnWindowClose()
    {
        _config.Enable = false;
        _config.EnableMinimap = false;
        _config.Modified.Fire();
    }

    protected virtual void OnCastStarted(Actor actor) { }

    protected virtual void OnCastFinished(Actor actor) { }
    private void HandleCastEvent(Actor actor, ActorCastEvent ev)
    {
        HandleTrapTriggered(actor, ev);
        OnEventCast(actor, ev);
    }
    protected virtual void OnEventCast(Actor actor, ActorCastEvent ev) { }

    private void OnActorStatusGain(Actor actor, int index)
    {
        var status = actor.Statuses[index];

        switch (status.ID)
        {
            case (uint)WHM.SID.Surecast:
            case (uint)WAR.SID.ArmsLength:
                if (World.Party.TryFindSlot(actor.InstanceID, out var slot1))
                    _playerImmunes[slot1].RoleBuffExpire = status.ExpireAt;
                break;
            case (uint)WAR.SID.InnerStrength:
                if (World.Party.TryFindSlot(actor.InstanceID, out var slot2))
                    _playerImmunes[slot2].JobBuffExpire = status.ExpireAt;
                break;
            // Knockback Penalty floor effect
            case 1096:
            case 1512:
                if (World.Party.TryFindSlot(actor.InstanceID, out var slot3))
                    _playerImmunes[slot3].KnockbackPenalty = true;
                break;
        }

        OnStatusGain(actor, status);
    }

    protected virtual void OnStatusGain(Actor actor, ActorStatus status) { }

    private void OnActorStatusLose(Actor actor, int index)
    {
        var status = actor.Statuses[index];
        OnStatusLose(actor, status);
    }

    protected virtual void OnStatusLose(Actor actor, ActorStatus status) { }

    protected virtual void OnActorCombatChanged(Actor actor) { }

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
                _trapsCurrentFloor.Clear();
                break;
            case 9208: // magicite overcap
            case 10287: // demiclone overcap
                _lastChestMagicite = true;
                break;
            case 11251:
                if (op.Args[1] == 4) // mazeroot balm used, reveals map and traps
                    _trapsCurrentFloor.Clear();
                break;
        }
    }

    private void OnEObjAnim(Actor actor, ushort p1, ushort p2)
    {
        // fake beacon deactivation; accompanied by system log #9217 but it does not indicate a specific actor
        if (actor.OID == (uint)OID.BeaconHoH && p1 == 0x0400 && p2 == 0x0800)
            _fakeExits.Add(actor.InstanceID);
    }

    protected virtual void OnLeaveFloor() { }
    protected virtual void OnEnterFloor() { }

    private void OnEnterFloorInternal()
    {
        LoadWalls();
        LoadGeometry();
        UpdateTrapsFloor();
        BetweenFloors = false;
        OnEnterFloor();
    }

    private void ClearState()
    {
        Donuts.Clear();
        Circles.Clear();
        Gazes.Clear();
        Interrupts.Clear();
        Stuns.Clear();
        Spikes.Clear();
        HintDisabled.Clear();
        LOS.Clear();
        Walls.Clear();
        DesiredRoom = default;
        Kills = 0;
        Array.Fill(_playerImmunes, default);
        _lastChestContentsGold = null;
        _lastChestMagicite = false;
        _chestContentsGold.Clear();
        _chestContentsSilver.Clear();
        _trapsHidden = true;
        _fakeExits.Clear();
        OnLeaveFloor();
        BetweenFloors = true;
    }

    public override void Update()
    {
        base.Update();

        if (PendingInitialize && Palace.Rooms.Any(r => r > 0) && Palace.DungeonId != DeepDungeonState.DungeonType.None)
        {
            if (_config.TrapHints)
            {
                try
                {
                    if (_palantirGetTraps != null)
                        UpdateTrapsFloorset(_palantirGetTraps.InvokeFunc(World.CurrentZone));
                }
                catch (IpcNotReadyError)
                {
                    Service.ShowNotification("Trap locations will not be shown. Please make sure Palantir is installed.", "Unable to communicate with Palantir", Dalamud.Interface.ImGuiNotification.NotificationType.Warning);
                }
            }
            PendingInitialize = false;
            OnEnterFloorInternal();
        }
    }

    protected void AddGaze(Actor Source, AOEShape Shape) => Gazes.Add(new(Source, Shape));
    protected void AddGaze(Actor Source, float Radius) => AddGaze(Source, new AOEShapeCircle(Radius));
    protected void AddDonut(Actor Source, float Inner, float Outer, Angle? HalfAngle = null) => Donuts.Add((Source, Inner, Outer, HalfAngle ?? 180.Degrees()));
    protected void AddVoidzone(Actor Source, AOEShape Shape, DateTime Activation = default, int Counter = 0) => Voidzones.Add((Source, Shape, Activation, Counter));

    protected void AddLOS(Actor Source, float Range)
    {
        if (_config.AutoLOS)
            AddLOSFromTerrain(Source, Range);
        else
            Circles.Add((Source, Range));
    }

    private bool OpenSilver
    {
        get
        {
            // sanity check
            if (World.Party.Player() is not { } player)
                return false;

            // explosive silver chests deal 70% max hp damage
            if (player.HPMP.CurHP <= player.HPMP.MaxHP * 0.7f)
                return false;

            // upgrade weapon if desired
            if (Palace.Progress.WeaponLevel + Palace.Progress.ArmorLevel < 198)
                return true;

            return Palace.DungeonId switch
            {
                DeepDungeonState.DungeonType.PT => true,
                DeepDungeonState.DungeonType.HOH or DeepDungeonState.DungeonType.EO => Palace.Floor >= 7, // per-dungeon gimmick items start dropping on floor 7
                _ => false,
            };
        }
    }

    private bool CanAutoUse(PomanderID p, Actor player)
    {
        if (Palace.Party.Count(p => p.EntityId > 0) > 1)
            return false;

        if (player.FindStatus(SID.ItemPenalty) != null)
            return false;

        if (!_config.AutoPoms[(int)p])
            return false;

        if (p is PomanderID.Purity or PomanderID.ProtoPurity)
            return player.FindStatus(1087) != null;

        return true;
    }

    private void ProcessAll<T>(List<T> items, Func<T, bool> expire, Action<T> action, Action<T>? onRemove = null)
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

    private void HandleActorAdd(Actor c)
    {
        HandleBeaconCreated(c);
        OnActorCreated(c);
    }
    protected virtual void OnActorCreated(Actor c) { }

    private DateTime CastFinishAt(Actor c) => World.FutureTime(c.CastInfo!.NPCRemainingTime);

    protected virtual void CalculateExtraHints(int playerSlot, Actor player, AIHints hints) { }

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        if (Palace.IsBossFloor || BetweenFloors)
            return;

        foreach (var (w, rot) in Walls)
            hints.TemporaryObstacles.Add(ShapeDistance.Rect(w.Position, (rot ? 90f : 0f).Degrees(), w.Depth, w.Depth, 20));

        if (_config.TrapHints && _trapsHidden)
        {
            var tshape = _trapsCurrentFloor.Select(t => new WPos(t.X, t.Z)).Where(f => f.AlmostEqual(player.Position, 30)).Select(f => ShapeDistance.Circle(f, TrapRadius)).ToList();
            if (tshape.Count > 0)
                hints.AddForbiddenZone(ShapeDistance.Union(tshape));
        }

        DrawAOEs(playerSlot, player, hints);

        var numAggro = hints.PotentialTargets.Count(t => t.Actor.AggroPlayer && !t.Actor.IsDeadOrDestroyed);

        (PlayerRoom, PlayerRoomBox) = FindClosestRoom(player.Position);

        // allow navigating when at max pull, i.e. so we can walk our dog to the nearest chest
        var canNavigate = numAggro <= _config.MaxPull;

        if (canNavigate)
            HandleFloorPathfind(player, hints);

        if (_config.ForbidDOTs)
            foreach (var hpt in hints.PotentialTargets)
                hpt.ForbidDOTs = true;

        if (!_config.Enable)
            return;

        CalculateExtraHints(playerSlot, player, hints);

        var isTransformed = IsPlayerTransformed(player);
        var isStunned = isTransformed || player.Statuses.Any(s => (SID)s.ID is SID.Silence or SID.Pacification);
        var isOccupied = player.InCombat || isStunned;

        InterestingObjects.Clear();

        foreach (var a in World.Actors)
        {
            if (_chestContentsGold.TryGetValue(a.InstanceID, out var pid) && Palace.GetPomanderState(pid).Count == 3 && a.IsTargetable)
            {
                if (CanAutoUse(pid, player))
                    hints.ActionsToExecute.Push(new ActionID(ActionType.Pomander, (uint)pid), null, ActionQueue.Priority.VeryHigh);
                continue;
            }

            if (_chestContentsSilver.ContainsKey(a.InstanceID) && Palace.Magicite.All(m => m > 0))
                // TODO use magicite/demiclone to prevent overcap
                continue;

            if (_fakeExits.Contains(a.InstanceID))
                continue;

            var dist = (player.Position - a.Position).LengthSq();

            var oid = (OID)a.OID;
            if (IsInThisRoomOrAdjacent(a))
            {
                if (!isTransformed)
                {
                    if (oid == OID.BandedCoffer && a.IsTargetable)
                        InterestingObjects.Add(new(a, GoalType.ChestHoard, dist));

                    if (_config.OpenTreasure)
                    {
                        if (oid == OID.GoldCoffer && a.IsTargetable)
                            InterestingObjects.Add(new(a, GoalType.Treasure, dist, 3));

                        if (oid == OID.SilverCoffer && OpenSilver && a.IsTargetable)
                            InterestingObjects.Add(new(a, GoalType.Treasure, dist, 2));

                        if (BronzeChestIDs.Contains(a.OID) && !a.IsOpenTreasure)
                            InterestingObjects.Add(new(a, GoalType.Treasure, dist, 1));
                    }
                }

                if (oid == OID.BandedCofferIndicator && Palace.Progress.HoardCurrentFloor)
                    InterestingObjects.Add(new(a, GoalType.Hoard, dist));

                if (oid is OID.CairnPalace or OID.BeaconHoH or OID.PylonEO or OID.PylonPT && Palace.PassageActive && !player.InCombat && !Palace.Progress.HoardCurrentFloor)
                {
                    if (!(_config.FullClear && Palace.Rooms.Any(r => r > 0 && !r.HasFlag(RoomFlags.Revealed))))
                        InterestingObjects.Add(new(a, GoalType.Passage, dist));
                }
            }

            if (RevealedTrapOIDs.Contains(a.OID))
                hints.AddForbiddenZone(ShapeDistance.Circle(a.Position, TrapRadius));
        }

        PickARoom(player, hints);

        InterestingObjects.SortBy(o => (o.Type, o.PlayerDistSq));

        if (_lastChestContentsGold is PomanderID p && InterestingObjects.FirstOrNull(p => p.Type == GoalType.Treasure && p.Tier == 3) is { } goldOvercap)
        {
            _chestContentsGold[goldOvercap.Actor.InstanceID] = p;
            _lastChestContentsGold = null;
            return;
        }

        if (_lastChestMagicite && InterestingObjects.FirstOrNull(p => p.Type == GoalType.Treasure && p.Tier == 2) is { } silverOvercap)
        {
            _chestContentsSilver[silverOvercap.Actor.InstanceID] = 1;
            _lastChestMagicite = false;
            return;
        }

        MostInterestingObject = InterestingObjects.FirstOrNull();

        var goalWeight = player.InCombat ? 1 : 0.01f;

        if (canNavigate)
        {
            switch (MostInterestingObject)
            {
                case { Type: GoalType.Passage, Actor: var c }:
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(c.Position, 2, goalWeight));
                    var (_, room) = FindClosestRoom(c.Position);
                    var roomShape = ShapeDistance.Rect(room.Pos, default(Angle), room.Size.Z, room.Size.Z, room.Size.X);
                    _pathfindZones.Add((ShapeDistance.Circle(c.Position, 25) & roomShape).Discretize());
                    break;

                case { Type: GoalType.Hoard, Actor: var h }:
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(h.Position, 2, 10));
                    break;

                case { IsTreasure: true, Actor: var d }:
                    _pathfindZones.Add(ShapeDistance.Circle(d.Position, 25).Discretize());
                    hints.InteractWithTarget = d;
                    break;
            }
        }

        var passageRevealed = Palace.Rooms.Any(r => r.HasFlag(RoomFlags.Passage) && r.HasFlag(RoomFlags.Revealed));

        var canTarget = _config.AutoClear switch
        {
            AutoDDConfig.ClearBehavior.Passage => passageRevealed && !Palace.PassageActive,
            AutoDDConfig.ClearBehavior.Leveling => player.Level < LevelCap || passageRevealed && !Palace.PassageActive,
            AutoDDConfig.ClearBehavior.All => true,
            _ => false
        };

        foreach (var pp in hints.PotentialTargets)
        {
            // enemy is petrified, any damage will kill
            if (pp.Actor.FindStatus(SID.StoneCurse)?.ExpireAt > World.FutureTime(1.5f))
                pp.ForcePriority(0);

            // pomander of storms was used, enemy can't autoheal; any damage will kill
            else if (pp.Actor.FindStatus(SID.AutoHealPenalty) != null && pp.Actor.PendingHPRaw is > 0 and < 10)
                pp.ForcePriority(0);

            // if not at max pull size, prioritize mobs
            else if (canTarget && (numAggro < _config.MaxPull || _config.MaxPull == 0 && numAggro == 0) && !pp.Actor.Statuses.Any(s => IsDangerousOutOfCombatStatus(s.ID)) && IsInThisRoomOrAdjacent(pp.Actor) && !pp.Actor.AggroPlayer)
            {
                // passive mobs shouldn't be prioritized over aggro'd ones
                if (player.InCombat)
                    pp.Priority = 0;
                else
                    pp.ShouldBeTargeted = true;
            }
        }

        if (_pathfindZones.Count > 0)
            hints.GoalZones.AddSimple(ShapeDistance.UnionPoint([.. _pathfindZones]), player.InCombat ? 1 : 0.01f);

        _pathfindZones.Clear();
    }

    private void DrawAOEs(int playerSlot, Actor player, AIHints hints)
    {
        ProcessAll(HintDisabled, g => g.CastInfo == null, g =>
        {
            hints.ForbiddenZones.RemoveAll(z => z.tag == g.InstanceID);
        });

        ProcessAll(Gazes, g => g.Source.CastInfo == null, d =>
        {
            if (d.Shape.Check(player.Position, d.Source))
                hints.ForbiddenDirections.Add((player.AngleTo(d.Source), 45.Degrees(), CastFinishAt(d.Source)));
        });

        ProcessAll(Donuts, d => d.Source.CastInfo == null, d =>
        {
            hints.AddForbiddenZone(new AOEShapeDonutSector(d.Inner, d.Outer, d.HalfAngle), d.Source.Position, d.Source.CastInfo!.Rotation, CastFinishAt(d.Source));
        });

        ProcessAll(Circles, d => d.Source.CastInfo == null, d =>
        {
            hints.AddForbiddenZone(new AOEShapeCircle(d.Radius), d.Source.Position, default, CastFinishAt(d.Source));

            // some enrages are way bigger than pathfinding map size (e.g. slime explosion is 60y)
            // in these cases, if the player is inside the aoe, add a goal zone telling it to GTFO as far as possible
            if (d.Radius >= 30)
            {
                var distToSource = (player.Position - d.Source.Position).Length();
                if (distToSource <= d.Radius)
                {
                    var desiredDistance = distToSource + 10;
                    hints.GoalZones.AddSimple(p => (p - d.Source.Position).Length() >= desiredDistance, 100);
                }
            }
        });

        ProcessAll(Interrupts, d => d.CastInfo == null, d =>
        {
            if (hints.FindEnemy(d) is { } e)
                e.ShouldBeInterrupted = true;
        });

        ProcessAll(Stuns, d => d.CastInfo == null, d =>
        {
            if (hints.FindEnemy(d) is { } e)
                e.ShouldBeStunned = true;
        });

        ProcessAll(LOS, d => d.CastInfo == null, caster =>
        {
            if (!_losCache.TryGetValue(caster.InstanceID, out var dangermap))
                return;

            hints.AddForbiddenZone(new AOEShapeCustom(dangermap), caster.Position, default, CastFinishAt(caster));
        }, d => _losCache.Remove(d.InstanceID));

        ProcessAll(Voidzones, d => d.Source.IsDeadOrDestroyed, d =>
        {
            hints.AddForbiddenZone(d.Zone, d.Source.Position, d.Source.Rotation, d.Activation);
        });

        ProcessAll(KnockbackZones, d => d.Source.CastInfo == null, kb =>
        {
            var castFinish = CastFinishAt(kb.Source);
            if (_playerImmunes[playerSlot].ImmuneAt(castFinish))
                return;

            hints.AddForbiddenZone(new AOEShapeCircle(kb.Radius), kb.Source.Position, default, castFinish);
        });

        ProcessAll(Spikes, t => t.Timeout <= World.CurrentTime, t =>
        {
            if (hints.FindEnemy(t.Actor) is { } enemy)
                enemy.Spikes = true;
        });
    }

    private static bool IsPlayerTransformed(Actor player) => player.Statuses.Any(Autorotation.RotationModuleManager.IsTransformStatus);
    private static bool IsDangerousOutOfCombatStatus(uint statusRaw) => (SID)statusRaw is SID.DamageUp or SID.DreadBeastAura or SID.PhysicalDamageUp;

    protected void AddLOSFromTerrain(Actor Source, float Range)
    {
        var src = Source.Position;
        var polys = Service.Shadowcast.ComputeVisibility2D(Source.PosRot.XYZ() with { Y = 6 }, new(1, 0, 0), Range);
        _losCache[Source.InstanceID] = new RelSimplifiedComplexPolygon(polys.Select(p => p - src));
        LOS.Add(Source);
    }
}
