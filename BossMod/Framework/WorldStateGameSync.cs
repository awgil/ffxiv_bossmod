using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.Interop;
using System.Runtime.InteropServices;
using System.Text;

namespace BossMod;

// utility that updates a world state to correspond to game state
sealed class WorldStateGameSync : IDisposable
{
    private const int ObjectTableSize = 819; // should match CS; note that different ranges are used for different purposes - consider splitting?..
    private const uint InvalidEntityId = 0xE0000000;

    private readonly WorldState _ws;
    private readonly ActionManagerEx _amex;
    private readonly DateTime _startTime;
    private readonly long _startQPC;

    // list of actors that are present in the user's enemy list
    private readonly List<ulong> _playerEnmity = [];

    private readonly List<WorldState.Operation> _globalOps = [];
    private readonly Dictionary<ulong, List<WorldState.Operation>> _actorOps = [];
    private readonly Dictionary<ulong, Vector3> _lastCastPositions = []; // unfortunately, game only saves cast location for area-targeted spells
    private readonly Actor?[] _actorsByIndex = new Actor?[ObjectTableSize];

    private bool _needInventoryUpdate = true;

    private readonly Network.OpcodeMap _opcodeMap = new();
    private readonly Network.PacketInterceptor _interceptor = new();
    private readonly Network.PacketDecoderGame _decoder = new();

    private readonly ConfigListener<ReplayManagementConfig> _netConfig;
    private readonly EventSubscriptions _subscriptions;

    private unsafe delegate void ProcessPacketActorCastDelegate(uint casterId, Network.ServerIPC.ActorCast* packet);
    private readonly Hook<ProcessPacketActorCastDelegate> _processPacketActorCastHook;

    private unsafe delegate void ProcessPacketEffectResultDelegate(uint targetID, byte* packet, byte replaying);
    private readonly Hook<ProcessPacketEffectResultDelegate> _processPacketEffectResultHook;
    private readonly Hook<ProcessPacketEffectResultDelegate> _processPacketEffectResultBasicHook;

    private delegate void ProcessPacketActorControlDelegate(uint actorID, uint category, uint p1, uint p2, uint p3, uint p4, uint p5, uint p6, ulong targetID, byte replaying);
    private readonly Hook<ProcessPacketActorControlDelegate> _processPacketActorControlHook;

    private unsafe delegate void ProcessPacketNpcYellDelegate(Network.ServerIPC.NpcYell* packet);
    private readonly Hook<ProcessPacketNpcYellDelegate> _processPacketNpcYellHook;

    public unsafe delegate void ProcessMapEffectDelegate(void* self, uint index, ushort s1, ushort s2);
    private readonly Hook<ProcessMapEffectDelegate> _processMapEffectHook;

    private unsafe delegate void ProcessMapEffectNDelegate(ContentDirector* director, byte* packet);
    private readonly Hook<ProcessMapEffectNDelegate> _processMapEffect1Hook;
    private readonly Hook<ProcessMapEffectNDelegate> _processMapEffect2Hook;
    private readonly Hook<ProcessMapEffectNDelegate> _processMapEffect3Hook;

    public unsafe delegate byte ProcessLegacyMapEffectDelegate(EventFramework* fwk, EventId eventId, byte seq, byte unk, void* data, ulong length);
    private readonly Hook<ProcessLegacyMapEffectDelegate> _processLegacyMapEffectHook;

    private unsafe delegate void ProcessPacketRSVDataDelegate(byte* packet);
    private readonly Hook<ProcessPacketRSVDataDelegate> _processPacketRSVDataHook;

    private unsafe delegate void ProcessPacketOpenTreasureDelegate(uint actorID, byte* packet);
    private readonly Hook<ProcessPacketOpenTreasureDelegate> _processPacketOpenTreasureHook;

    private unsafe delegate void* ProcessSystemLogMessageDelegate(uint entityId, uint logMessageId, int* args, byte argCount);
    private readonly Hook<ProcessSystemLogMessageDelegate> _processSystemLogMessageHook;

    private unsafe delegate void* ProcessPacketFateInfoDelegate(ulong fateId, long startTimestamp, ulong durationSecs);
    private readonly Hook<ProcessPacketFateInfoDelegate> _processPacketFateInfoHook;

    private readonly unsafe delegate* unmanaged<ContainerInterface*, float> _calculateMoveSpeedMulti;

    private unsafe delegate void ApplyKnockbackDelegate(Character* thisPtr, float a2, float a3, float a4, byte a5, int a6);
    private readonly Hook<ApplyKnockbackDelegate> _applyKnockbackHook;

    private unsafe delegate void InventoryAckDelegate(uint a1, void* a2);
    private readonly Hook<InventoryAckDelegate> _inventoryAckHook;

    public unsafe WorldStateGameSync(WorldState ws, ActionManagerEx amex)
    {
        _ws = ws;
        _amex = amex;
        _startTime = DateTime.Now;
        _startQPC = Framework.Instance()->PerformanceCounterValue;
        _interceptor.ServerIPCReceived += ServerIPCReceived;
        _interceptor.ClientIPCSent += ClientIPCSent;

        _netConfig = Service.Config.GetAndSubscribe<ReplayManagementConfig>(config =>
        {
            _interceptor.ActiveRecv = config.RecordServerPackets || config.DumpServerPackets;
            _interceptor.ActiveSend = config.DumpClientPackets;
        });
        _subscriptions = new
        (
            amex.ActionRequestExecuted.Subscribe(OnActionRequested),
            amex.ActionEffectReceived.Subscribe(OnActionEffect)
        );

        _processPacketActorCastHook = Service.Hook.HookFromSignature<ProcessPacketActorCastDelegate>("40 53 57 48 81 EC ?? ?? ?? ?? 48 8B FA 8B D1", ProcessPacketActorCastDetour);
        _processPacketActorCastHook.Enable();
        Service.Log($"[WSG] ProcessPacketActorCast address = 0x{_processPacketActorCastHook.Address:X}");

        _processPacketEffectResultHook = Service.Hook.HookFromSignature<ProcessPacketEffectResultDelegate>("48 8B C4 44 88 40 18 89 48 08", ProcessPacketEffectResultDetour);
        _processPacketEffectResultHook.Enable();
        Service.Log($"[WSG] ProcessPacketEffectResult address = 0x{_processPacketEffectResultHook.Address:X}");

        _processPacketEffectResultBasicHook = Service.Hook.HookFromSignature<ProcessPacketEffectResultDelegate>("40 53 41 54 41 55 48 83 EC 40", ProcessPacketEffectResultBasicDetour);
        _processPacketEffectResultBasicHook.Enable();
        Service.Log($"[WSG] ProcessPacketEffectResultBasic address = 0x{_processPacketEffectResultBasicHook.Address:X}");

        _processPacketActorControlHook = Service.Hook.HookFromSignature<ProcessPacketActorControlDelegate>("E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64", ProcessPacketActorControlDetour);
        _processPacketActorControlHook.Enable();
        Service.Log($"[WSG] ProcessPacketActorControl address = 0x{_processPacketActorControlHook.Address:X}");

        // alt sig - impl: "45 33 D2 48 8D 41 48"
        _processPacketNpcYellHook = Service.Hook.HookFromSignature<ProcessPacketNpcYellDelegate>("48 83 EC 68 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 ?? 0F 10 41 10", ProcessPacketNpcYellDetour);
        _processPacketNpcYellHook.Enable();
        Service.Log($"[WSG] ProcessPacketNpcYell address = 0x{_processPacketNpcYellHook.Address:X}");

        _processMapEffectHook = Service.Hook.HookFromSignature<ProcessMapEffectDelegate>("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 41 0F B7 E8", ProcessMapEffectDetour);
        _processMapEffectHook.Enable();
        Service.Log($"[WSG] ProcessMapEffect address = 0x{_processMapEffectHook.Address:X}");

        var mapEffectAddrs = Service.SigScanner.ScanAllText("40 55 41 57 48 83 EC ?? 48 83 B9");
        if (mapEffectAddrs.Length != 3)
            throw new InvalidOperationException($"expected 3 matches for multi-MapEffect handlers, but got {mapEffectAddrs.Length}");

        _processMapEffect1Hook = Service.Hook.HookFromAddress<ProcessMapEffectNDelegate>(mapEffectAddrs[0], ProcessMapEffect1Detour);
        _processMapEffect1Hook.Enable();
        _processMapEffect2Hook = Service.Hook.HookFromAddress<ProcessMapEffectNDelegate>(mapEffectAddrs[1], ProcessMapEffect2Detour);
        _processMapEffect2Hook.Enable();
        _processMapEffect3Hook = Service.Hook.HookFromAddress<ProcessMapEffectNDelegate>(mapEffectAddrs[2], ProcessMapEffect3Detour);
        _processMapEffect3Hook.Enable();
        Service.Log($"[WSG] ProcessMapEffectN addresses = 0x{_processMapEffect1Hook.Address:X}, 0x{_processMapEffect2Hook.Address:X}, 0x{_processMapEffect3Hook.Address:X}");

        _processPacketRSVDataHook = Service.Hook.HookFromSignature<ProcessPacketRSVDataDelegate>("44 8B 09 4C 8D 41 34", ProcessPacketRSVDataDetour);
        _processPacketRSVDataHook.Enable();
        Service.Log($"[WSG] ProcessPacketRSVData address = 0x{_processPacketRSVDataHook.Address:X}");

        _processSystemLogMessageHook = Service.Hook.HookFromSignature<ProcessSystemLogMessageDelegate>("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 0F B6 47 28", ProcessSystemLogMessageDetour);
        _processSystemLogMessageHook.Enable();
        Service.Log($"[WSG] ProcessSystemLogMessage address = 0x{_processSystemLogMessageHook.Address:X}");

        _processPacketOpenTreasureHook = Service.Hook.HookFromSignature<ProcessPacketOpenTreasureDelegate>("40 53 48 83 EC 20 48 8B DA 48 8D 0D ?? ?? ?? ?? 8B 52 10 E8 ?? ?? ?? ?? 48 85 C0 74 1B", ProcessPacketOpenTreasureDetour);
        _processPacketOpenTreasureHook.Enable();
        Service.Log($"[WSG] ProcessPacketOpenTreasure address = 0x{_processPacketOpenTreasureHook.Address:X}");

        _processPacketFateInfoHook = Service.Hook.HookFromSignature<ProcessPacketFateInfoDelegate>("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 0F B7 4F 10 48 8D 57 12 41 B8", ProcessPacketFateInfoDetour);
        _processPacketFateInfoHook.Enable();
        Service.Log($"[WSG] ProcessPacketFateInfo address = 0x{_processPacketFateInfoHook.Address:X}");

        _calculateMoveSpeedMulti = (delegate* unmanaged<ContainerInterface*, float>)Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 44 0F 28 D8 45 0F 57 D2");
        Service.Log($"[WSG] CalculateMovementSpeedMultiplier address = 0x{(nint)_calculateMoveSpeedMulti:X}");

        _processLegacyMapEffectHook = Service.Hook.HookFromSignature<ProcessLegacyMapEffectDelegate>("89 54 24 10 48 89 4C 24 ?? 53 56 57 41 55 41 57 48 83 EC 30 48 8B 99 ?? ?? ?? ??", ProcessLegacyMapEffectDetour);
        _processLegacyMapEffectHook.Enable();
        Service.Log($"[WSG] LegacyMapEffect address = {_processLegacyMapEffectHook.Address:X}");

        _applyKnockbackHook = Service.Hook.HookFromSignature<ApplyKnockbackDelegate>("E8 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? FF C6", ApplyKnockbackDetour);
        if (Service.IsDev)
        {
            _applyKnockbackHook.Enable();
            Service.Log($"[WSG] ApplyKnockback address = {_applyKnockbackHook.Address:X}");
        }

        _inventoryAckHook = Service.Hook.HookFromSignature<InventoryAckDelegate>("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8D 57 10 41 8B CE E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 48 8B D7", InventoryAckDetour);
        _inventoryAckHook.Enable();
        Service.Log($"[WSG] InventoryAck address = {_inventoryAckHook.Address:X}");
    }

    public void Dispose()
    {
        _inventoryAckHook.Dispose();
        _applyKnockbackHook.Dispose();
        _processLegacyMapEffectHook.Dispose();
        _processMapEffect1Hook.Dispose();
        _processMapEffect2Hook.Dispose();
        _processMapEffect3Hook.Dispose();
        _processPacketActorCastHook.Dispose();
        _processPacketEffectResultBasicHook.Dispose();
        _processPacketEffectResultHook.Dispose();
        _processPacketActorControlHook.Dispose();
        _processPacketNpcYellHook.Dispose();
        _processMapEffectHook.Dispose();
        _processPacketRSVDataHook.Dispose();
        _processSystemLogMessageHook.Dispose();
        _processPacketOpenTreasureHook.Dispose();
        _processPacketFateInfoHook.Dispose();
        _subscriptions.Dispose();
        _netConfig.Dispose();
        _interceptor.Dispose();
    }

    public unsafe void Update(TimeSpan prevFramePerf)
    {
        var fwk = Framework.Instance();
        _ws.Execute(new WorldState.OpFrameStart
        (
            new(
                _startTime.AddSeconds((double)(fwk->PerformanceCounterValue - _startQPC) / _ws.QPF),
                (ulong)fwk->PerformanceCounterValue,
                fwk->FrameCounter,
                fwk->RealFrameDeltaTime,
                fwk->FrameDeltaTime,
                fwk->GameSpeedMultiplier
            ),
            prevFramePerf,
            GaugeData(),
            Camera.Instance?.CameraAzimuth.Radians() ?? default
        ));
        if (_ws.CurrentZone != Service.ClientState.TerritoryType || _ws.CurrentCFCID != GameMain.Instance()->CurrentContentFinderConditionId)
        {
            _ws.Execute(new WorldState.OpZoneChange(Service.ClientState.TerritoryType, GameMain.Instance()->CurrentContentFinderConditionId));
        }
        var proxy = fwk->NetworkModuleProxy->ReceiverCallback;
        var scramble = Network.IDScramble.Get();
        if (_ws.Network.IDScramble != scramble)
            _ws.Execute(new NetworkState.OpIDScramble(scramble));

        foreach (var op in _globalOps)
        {
            _ws.Execute(op);
        }
        _globalOps.Clear();

        _playerEnmity.Clear();
        var uiState = UIState.Instance();
        for (var i = 0; i < uiState->Hater.HaterCount; i++)
            _playerEnmity.Add(uiState->Hater.Haters[i].EntityId);

        UpdateWaymarks();
        UpdateActors();
        UpdateParty();
        UpdateClient();
        UpdateDeepDungeon();
    }

    private unsafe void UpdateWaymarks()
    {
        var wm = Waymark.A;
        foreach (ref var marker in MarkingController.Instance()->FieldMarkers)
        {
            Vector3? pos = marker.Active ? new(marker.X / 1000.0f, marker.Y / 1000.0f, marker.Z / 1000.0f) : null;
            if (_ws.Waymarks[wm] != pos)
                _ws.Execute(new WaymarkState.OpWaymarkChange(wm, pos));
            ++wm;
        }

        var sgn = Sign.Attack1;
        foreach (ref var marker in MarkingController.Instance()->Markers)
        {
            var id = SanitizedObjectID(marker.Id);
            if (_ws.Waymarks[sgn] != id)
                _ws.Execute(new WaymarkState.OpSignChange(sgn, id));
            ++sgn;
        }
    }

    private unsafe void UpdateActors()
    {
        var mgr = GameObjectManager.Instance();
        for (int i = 0; i < _actorsByIndex.Length; ++i)
        {
            var actor = _actorsByIndex[i];
            var obj = mgr->Objects.IndexSorted[i].Value;

            if (obj != null && obj->EntityId == InvalidEntityId)
                obj = null; // ignore non-networked objects (really?..)

            if (obj != null && (obj->EntityId & 0xFF000000) == 0xFF000000)
            {
                Service.LogVerbose($"[WorldState] Skipping bad object #{i} with id {obj->EntityId:X}");
                obj = null;
            }

            var existing = obj != null ? _ws.Actors.Find(obj->EntityId) : null;

            if (actor != null && (obj == null || existing == null || actor.InstanceID != obj->EntityId))
            {
                _actorsByIndex[i] = null;
                RemoveActor(actor);
                actor = null;
            }

            if (obj != null)
            {
                if (actor != existing)
                    Service.Log($"[WorldState] Actor position mismatch for #{i} {actor}");

                UpdateActor(obj, i, actor);
            }
        }

        foreach (var (id, ops) in _actorOps)
            Service.Log($"[WorldState] {ops.Count} actor events for unknown entity {id:X}");
        _actorOps.Clear();
    }

    private void RemoveActor(Actor actor)
    {
        DispatchActorEvents(actor.InstanceID);
        _ws.Execute(new ActorState.OpDestroy(actor.InstanceID));
    }

    private unsafe void UpdateActor(GameObject* obj, int index, Actor? act)
    {
        var chr = obj->IsCharacter() ? (Character*)obj : null;
        var name = obj->NameString;
        var nameID = chr != null ? chr->NameId : 0;
        var classID = chr != null ? (Class)chr->ClassJob : Class.None;
        var level = chr != null ? chr->Level : 0;
        var posRot = new Vector4(obj->Position, obj->Rotation);
        var hpmp = new ActorHPMP();
        bool inCombat = false;
        if (chr != null)
        {
            hpmp.CurHP = chr->Health;
            hpmp.MaxHP = chr->MaxHealth;
            hpmp.Shield = (uint)(chr->ShieldValue * 0.01f * hpmp.MaxHP);
            hpmp.CurMP = chr->Mana;
            hpmp.MaxMP = chr->MaxMana;
            inCombat = chr->InCombat;
        }
        var targetable = obj->GetIsTargetable();
        var friendly = chr == null || ActionManager.ClassifyTarget(chr) != ActionManager.TargetCategory.Enemy;
        var isDead = obj->IsDead();
        var hasAggro = _playerEnmity.IndexOf(obj->EntityId) >= 0;
        var target = chr != null ? SanitizedObjectID(chr->GetTargetId()) : 0; // note: when changing targets, we want to see changes immediately rather than wait for server response
        var modelState = chr != null ? new ActorModelState(chr->Timeline.ModelState, chr->Timeline.AnimationState[0], chr->Timeline.AnimationState[1]) : default;
        var eventState = obj->EventState;
        var radius = obj->GetRadius();
        var mountId = chr != null ? chr->Mount.MountId : 0u;
        var forayInfoPtr = chr != null ? chr->GetForayInfo() : null;
        var forayInfo = forayInfoPtr == null ? default : new ActorForayInfo(forayInfoPtr->Level, forayInfoPtr->Element);

        if (act == null)
        {
            var type = (ActorType)(((int)obj->ObjectKind << 8) + obj->SubKind);
            _ws.Execute(new ActorState.OpCreate(obj->EntityId, obj->BaseId, index, obj->LayoutId, name, nameID, type, classID, level, posRot, radius, hpmp, targetable, friendly, SanitizedObjectID(obj->OwnerId), obj->FateId));
            act = _actorsByIndex[index] = _ws.Actors.Find(obj->EntityId)!;

            // note: for now, we continue relying on network messages for tether changes, since sometimes multiple changes can happen in a single frame, and some components rely on seeing all of them...
            var tether = chr != null ? new ActorTetherInfo(chr->Vfx.Tethers[0].Id, chr->Vfx.Tethers[0].TargetId) : default;
            if (tether.ID != 0)
                _ws.Execute(new ActorState.OpTether(act.InstanceID, tether));
        }
        else
        {
            if (act.NameID != nameID || act.Name != name)
                _ws.Execute(new ActorState.OpRename(act.InstanceID, name, nameID));
            if (act.Class != classID || act.Level != level)
                _ws.Execute(new ActorState.OpClassChange(act.InstanceID, classID, level));
            if (act.PosRot != posRot)
                _ws.Execute(new ActorState.OpMove(act.InstanceID, posRot));
            if (act.HitboxRadius != radius)
                _ws.Execute(new ActorState.OpSizeChange(act.InstanceID, radius));
            if (act.HPMP != hpmp)
                _ws.Execute(new ActorState.OpHPMP(act.InstanceID, hpmp));
            if (act.IsTargetable != targetable)
                _ws.Execute(new ActorState.OpTargetable(act.InstanceID, targetable));
            if (act.IsAlly != friendly)
                _ws.Execute(new ActorState.OpAlly(act.InstanceID, friendly));
        }

        if (act.IsDead != isDead)
            _ws.Execute(new ActorState.OpDead(act.InstanceID, isDead));
        if (act.InCombat != inCombat)
            _ws.Execute(new ActorState.OpCombat(act.InstanceID, inCombat));
        if (act.AggroPlayer != hasAggro)
            _ws.Execute(new ActorState.OpAggroPlayer(act.InstanceID, hasAggro));
        if (act.ModelState != modelState)
            _ws.Execute(new ActorState.OpModelState(act.InstanceID, modelState));
        if (act.EventState != eventState)
            _ws.Execute(new ActorState.OpEventState(act.InstanceID, eventState));
        if (act.TargetID != target)
            _ws.Execute(new ActorState.OpTarget(act.InstanceID, target));
        if (act.MountId != mountId)
            _ws.Execute(new ActorState.OpMount(act.InstanceID, mountId));
        if (act.ForayInfo != forayInfo)
            _ws.Execute(new ActorState.OpForayInfo(act.InstanceID, forayInfo));

        DispatchActorEvents(act.InstanceID);

        var castInfo = chr != null ? chr->GetCastInfo() : null;
        if (castInfo != null)
        {
            var curCast = castInfo->IsCasting
                ? new ActorCastInfo
                {
                    Action = new((ActionType)castInfo->ActionType, castInfo->ActionId),
                    TargetID = SanitizedObjectID(castInfo->TargetId),
                    Rotation = chr->CastRotation.Radians(),
                    Location = _lastCastPositions.GetValueOrDefault(act.InstanceID, castInfo->TargetLocation),
                    ElapsedTime = castInfo->CurrentCastTime,
                    TotalTime = castInfo->BaseCastTime,
                    Interruptible = castInfo->Interruptible
                } : null;
            UpdateActorCastInfo(act, curCast);
        }

        var sm = chr != null ? chr->GetStatusManager() : null;
        if (sm != null)
        {
            for (int i = 0; i < sm->NumValidStatuses; ++i)
            {
                // note: sometimes (Ocean Fishing) remaining-time is weird (I assume too large?) and causes exception in AddSeconds - so we just clamp it to some reasonable range
                // note: self-cast buffs with duration X will have duration -X until EffectResult (~0.6s later); see autorotation for more details
                ActorStatus curStatus = new();
                ref var s = ref sm->Status[i];
                if (s.StatusId != 0)
                {
                    var dur = Math.Min(Math.Abs(s.RemainingTime), 100000);
                    curStatus.ID = s.StatusId;
                    curStatus.SourceID = SanitizedObjectID(s.SourceObject);
                    curStatus.Extra = s.Param;
                    curStatus.ExpireAt = _ws.CurrentTime.AddSeconds(dur);
                }
                UpdateActorStatus(act, i, curStatus);
            }
        }

        var aeh = chr != null ? chr->GetActionEffectHandler() : null;
        if (aeh != null)
        {
            for (int i = 0; i < aeh->IncomingEffects.Length; ++i)
            {
                ref var eff = ref aeh->IncomingEffects[i];
                ref var prev = ref act.IncomingEffects[i];
                if ((prev.GlobalSequence, prev.TargetIndex) != (eff.GlobalSequence != 0 ? (eff.GlobalSequence, eff.TargetIndex) : (0, 0)))
                {
                    var effects = new ActionEffects();
                    for (int j = 0; j < ActionEffects.MaxCount; ++j)
                        effects[j] = *(ulong*)eff.Effects.Effects.GetPointer(j);
                    _ws.Execute(new ActorState.OpIncomingEffect(act.InstanceID, i, new(eff.GlobalSequence, eff.TargetIndex, eff.Source, new((ActionType)eff.ActionType, eff.ActionId), effects)));
                }
            }
        }
    }

    private void UpdateActorCastInfo(Actor act, ActorCastInfo? cast)
    {
        if (cast == null && act.CastInfo == null)
            return; // was not casting and is not casting

        if (cast != null && act.CastInfo != null && cast.Action == act.CastInfo.Action && cast.TargetID == act.CastInfo.TargetID && cast.TotalTime == act.CastInfo.TotalTime && Math.Abs(cast.ElapsedTime - act.CastInfo.ElapsedTime) < 0.2)
        {
            // continuing casting same spell
            // TODO: consider *not* ignoring elapsed differences, these probably mean we're doing something wrong...
            act.CastInfo.ElapsedTime = cast.ElapsedTime;
            return;
        }

        // update cast info
        _ws.Execute(new ActorState.OpCastInfo(act.InstanceID, cast));
    }

    private void UpdateActorStatus(Actor act, int index, ActorStatus value)
    {
        // note: some statuses have non-zero remaining time but never tick down (e.g. FC buffs); currently we ignore that fact, to avoid log spam...
        // note: RemainingTime is not monotonously decreasing (I assume because it is really calculated by game and frametime fluctuates...), we ignore 'slight' duration increases (<1 sec)
        var prev = act.Statuses[index];
        if (prev.ID == value.ID && prev.SourceID == value.SourceID && prev.Extra == value.Extra && (value.ExpireAt - prev.ExpireAt).TotalSeconds <= 1)
        {
            act.Statuses[index].ExpireAt = value.ExpireAt;
            return;
        }

        // update status info
        _ws.Execute(new ActorState.OpStatus(act.InstanceID, index, value));
    }

    private unsafe void UpdateParty()
    {
        var replay = Service.Condition[ConditionFlag.DutyRecorderPlayback];
        var group = GroupManager.Instance()->GetGroup(replay);

        // update party members
        var playerMember = UpdatePartyPlayer(replay, group);
        UpdatePartyNormal(group, playerMember);
        UpdatePartyAlliance(group);
        UpdatePartyNPCs();

        // update limit break
        var lb = LimitBreakController.Instance();
        if (_ws.Party.LimitBreakCur != lb->CurrentUnits || _ws.Party.LimitBreakMax != lb->BarUnits)
            _ws.Execute(new PartyState.OpLimitBreakChange(lb->CurrentUnits, lb->BarUnits));
    }

    // returns player entry in game's group
    private unsafe PartyMember* UpdatePartyPlayer(bool recorderPlaybackMode, GroupManager.Group* group)
    {
        // in worldstate, player is always in slot #0
        // in game, there are several considerations:
        // - PlayerState contains character data as long as player is logged in; in playback mode, it contains actual logged-in player rather than replay's POV
        // - objecttable entry #0 is always a player; in playback mode, it contains POV object; however, sometimes that object can be non-existent (eg during zone transitions)
        // - group manager contains player's entry at arbitrary position; it can be set before player's object is created, and it's not present while solo
        var player = PartyState.EmptySlot;

        var pc = (Character*)GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        if (pc != null && !pc->IsCharacter())
        {
            Service.Log($"[WSG] Object #0 is not a character, this should never happen");
            pc = null;
        }

        if (!recorderPlaybackMode)
        {
            // in normal mode, the primary data source is playerstate
            var ui = UIState.Instance();
            if (ui->PlayerState.IsLoaded)
            {
                var inCutscene = Service.Condition[ConditionFlag.OccupiedInCutSceneEvent] || Service.Condition[ConditionFlag.WatchingCutscene78] || Service.Condition[ConditionFlag.Occupied33] || Service.Condition[ConditionFlag.BetweenAreas] || Service.Condition[ConditionFlag.OccupiedInQuestEvent];
                player = new(ui->PlayerState.ContentId, ui->PlayerState.EntityId, inCutscene, ui->PlayerState.CharacterNameString);
                if (pc != null && (pc->ContentId != player.ContentId || pc->EntityId != player.InstanceId))
                    Service.Log($"[WSG] Object #0 is valid ({pc->AccountId:X}.{pc->ContentId:X}, {pc->EntityId:X8} '{pc->NameString}') but different from playerstate ({player})");
            }
            else
            {
                // player not logged in, just do some sanity checks
                if (pc != null)
                    Service.Log($"[WSG] Object #0 is valid ({pc->AccountId:X}.{pc->ContentId:X}, {pc->EntityId:X8} '{pc->NameString}') while player is not logged in");
                if (group->MemberCount > 0)
                    Service.Log($"[WGS] Group is non-empty while player is not logged in");
            }
        }
        else
        {
            // in playback mode, the primary data source is object #0
            if (pc != null)
            {
                player = new(pc->ContentId, pc->EntityId, false, pc->NameString);
            }
            // else: just assume there's no player for now...
        }

        var member = player.InstanceId != 0 ? group->GetPartyMemberByEntityId((uint)player.InstanceId) : null;
        if (member != null)
            player.InCutscene |= (member->Flags & 0x10) != 0;
        UpdatePartySlot(PartyState.PlayerSlot, player);
        return member;
    }

    private unsafe void UpdatePartyNormal(GroupManager.Group* group, PartyMember* player)
    {
        // first iterate over previous members, search for match in game state, and reconcile differences - update or remove
        for (int i = PartyState.PlayerSlot + 1; i < PartyState.MaxPartySize; ++i)
        {
            ref var m = ref _ws.Party.Members[i];
            if (m.ContentId != 0)
            {
                // slot was occupied by player => see if it's still in party; either update to current state or clear if it's no longer in party
                var member = group->GetPartyMemberByContentId(m.ContentId);
                UpdatePartySlot(i, BuildPartyMember(member));
            }
            else if (m.InstanceId != 0)
            {
                // slot was occupied by trust => see if it's still in party
                if (!HasBuddy(m.InstanceId))
                    UpdatePartySlot(i, PartyState.EmptySlot); // buddy is no longer in party => clear slot
                // else: no reason to update...
            }
            // else: slot was empty, skip
        }

        // now iterate through game state and add new members; note that there's no need to update existing, it was done in the previous loop
        for (int i = 0; i < group->MemberCount; ++i)
        {
            var member = group->PartyMembers.GetPointer(i);
            if (member->ContentId != player->ContentId && Array.FindIndex(_ws.Party.Members, m => m.ContentId == member->ContentId) < 0)
                AddPartyMember(BuildPartyMember(member));
            // else: member is either a player (it was handled by a different function) or already exists in party state
        }
        // consider buddies as party members too
        var ui = UIState.Instance();
        for (int i = 0; i < ui->Buddy.DutyHelperInfo.ENpcIds.Length; ++i)
        {
            var instanceID = ui->Buddy.DutyHelperInfo.DutyHelpers[i].EntityId;
            if (instanceID != InvalidEntityId && _ws.Party.FindSlot(instanceID) < 0)
            {
                var obj = GameObjectManager.Instance()->Objects.GetObjectByEntityId(instanceID);
                AddPartyMember(new(0, instanceID, false, obj != null ? obj->NameString : ""));
            }
            // else: buddy is non-existent or already updated, skip
        }
    }

    private unsafe void UpdatePartyAlliance(GroupManager.Group* group)
    {
        // note: we don't support small-group alliance (should we?)
        // unlike normal party, game's alliance slots never change, so we just keep 1:1 mapping
        var isNormalAlliance = group->IsAlliance && !group->IsSmallGroupAlliance;
        for (int i = PartyState.MaxPartySize; i < PartyState.MaxAllianceSize; ++i)
        {
            var member = isNormalAlliance ? group->AllianceMembers.GetPointer(i - PartyState.MaxPartySize) : null;
            if (member != null && !member->IsValidAllianceMember())
                member = null;
            UpdatePartySlot(i, BuildPartyMember(member));
        }
    }

    private unsafe void UpdatePartyNPCs()
    {
        var treatAlliesAsParty = _ws.CurrentCFCID != 0; // TODO: think more about it, do we ever care about allies in overworld?..
        for (int i = PartyState.MaxAllianceSize; i < PartyState.MaxAllies; ++i)
        {
            ref var m = ref _ws.Party.Members[i];
            if (m.InstanceId != 0)
            {
                var actor = treatAlliesAsParty ? _ws.Actors.Find(m.InstanceId) : null;
                if (actor == null || !actor.IsFriendlyNPC)
                    UpdatePartySlot(i, PartyState.EmptySlot);
            }
        }
        if (!treatAlliesAsParty)
            return;
        foreach (var actor in _ws.Actors)
        {
            if (!actor.IsFriendlyNPC)
                continue;
            if (_ws.Party.FindSlot(actor.InstanceID) == -1)
            {
                var slot = FindFreePartySlot(PartyState.MaxAllianceSize, PartyState.MaxAllies);
                if (slot > 0)
                    UpdatePartySlot(slot, new PartyState.Member(0, actor.InstanceID, false, actor.Name));
                else
                    Service.Log($"[WorldState] Failed to find empty slot for allied NPC {actor.InstanceID:X}");
            }
        }
    }

    private unsafe bool HasBuddy(ulong instanceID)
    {
        var ui = UIState.Instance();
        for (int i = 0; i < ui->Buddy.DutyHelperInfo.ENpcIds.Length; ++i)
            if (ui->Buddy.DutyHelperInfo.DutyHelpers[i].EntityId == instanceID)
                return true;
        return false;
    }

    private int FindFreePartySlot(int firstSlot, int lastSlot)
    {
        for (int i = firstSlot; i < lastSlot; ++i)
            if (!_ws.Party.Members[i].IsValid())
                return i;
        return -1;
    }

    private unsafe PartyState.Member BuildPartyMember(PartyMember* m) => m != null ? new(m->ContentId, m->EntityId, (m->Flags & 0x10) != 0, m->NameString) : PartyState.EmptySlot;

    private void AddPartyMember(PartyState.Member m)
    {
        var freeSlot = FindFreePartySlot(1, PartyState.MaxPartySize);
        if (freeSlot >= 0)
            _ws.Execute(new PartyState.OpModify(freeSlot, m));
        else
        {
            Service.Log($"[WorldState] Failed to find empty slot for party member {m.ContentId:X}:{m.InstanceId:X} ({_ws.Actors.Find(m.InstanceId)})");
            Service.Log($"[WorldState] Current slots: {string.Join(", ", _ws.Party.Members.Select(m => _ws.Actors.Find(m.InstanceId)?.ToString() ?? "<unknown>"))}");
        }
    }

    private void UpdatePartySlot(int slot, PartyState.Member m)
    {
        if (_ws.Party.Members[slot] != m)
            _ws.Execute(new PartyState.OpModify(slot, m));
    }

    [StructLayout(LayoutKind.Explicit)]
    private unsafe struct CharacterContainer
    {
        [FieldOffset(0x8)] public Character* Character;
    }

    private unsafe void UpdateClient()
    {
        var countdownAgent = AgentCountDownSettingDialog.Instance();
        float? countdown = countdownAgent != null && countdownAgent->Active ? countdownAgent->TimeRemaining : null;
        if (_ws.Client.CountdownRemaining != countdown)
            _ws.Execute(new ClientState.OpCountdownChange(countdown));

        var actionManager = ActionManager.Instance();
        if (_ws.Client.AnimationLock != actionManager->AnimationLock)
            _ws.Execute(new ClientState.OpAnimationLockChange(actionManager->AnimationLock));

        var combo = new ClientState.Combo(actionManager->Combo.Action, actionManager->Combo.Timer);
        if (_ws.Client.ComboState != combo)
            _ws.Execute(new ClientState.OpComboChange(combo));

        var uiState = UIState.Instance();
        var stats = new ClientState.Stats(uiState->PlayerState.Attributes[45], uiState->PlayerState.Attributes[46], uiState->PlayerState.Attributes[47]);
        if (_ws.Client.PlayerStats != stats)
            _ws.Execute(new ClientState.OpPlayerStatsChange(stats));

        var pc = (Character*)GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        if (pc != null)
        {
            var baseSpeed = ControlEx.Instance()->BaseMoveSpeed;
            var c8 = new CharacterContainer() { Character = pc };
            var factor = _calculateMoveSpeedMulti((ContainerInterface*)&c8);
            var speed = baseSpeed * factor;
            if (_ws.Client.MoveSpeed != speed)
                _ws.Execute(new ClientState.OpMoveSpeedChange(speed));
        }

        Span<Cooldown> cooldowns = stackalloc Cooldown[_ws.Client.Cooldowns.Length];
        _amex.GetCooldowns(cooldowns);
        if (!MemoryExtensions.SequenceEqual(_ws.Client.Cooldowns.AsSpan(), cooldowns))
        {
            if (cooldowns.IndexOfAnyExcept(default(Cooldown)) < 0)
                _ws.Execute(new ClientState.OpCooldown(true, []));
            else
                _ws.Execute(new ClientState.OpCooldown(false, CalcCooldownDifference(cooldowns, _ws.Client.Cooldowns.AsSpan())));
        }

        var dutyActions = _amex.GetDutyActions();
        if (!MemoryExtensions.SequenceEqual(_ws.Client.DutyActions.AsSpan(), dutyActions))
            _ws.Execute(new ClientState.OpDutyActionsChange(dutyActions));

        Span<byte> bozjaHolster = stackalloc byte[_ws.Client.BozjaHolster.Length];
        bozjaHolster.Clear();
        var bozjaState = PublicContentBozja.GetState();
        if (bozjaState != null)
            foreach (var action in bozjaState->HolsterActions)
                if (action != 0)
                    ++bozjaHolster[action];
        if (!MemoryExtensions.SequenceEqual(_ws.Client.BozjaHolster.AsSpan(), bozjaHolster))
            _ws.Execute(new ClientState.OpBozjaHolsterChange(CalcBozjaHolster(bozjaHolster)));

        if (!MemoryExtensions.SequenceEqual(_ws.Client.BlueMageSpells.AsSpan(), actionManager->BlueMageActions))
            _ws.Execute(new ClientState.OpBlueMageSpellsChange(actionManager->BlueMageActions.ToArray()));

        var levels = uiState->PlayerState.ClassJobLevels;
        if (!MemoryExtensions.SequenceEqual(_ws.Client.ClassJobLevels.AsSpan(), levels))
            _ws.Execute(new ClientState.OpClassJobLevelsChange(levels.ToArray()));

        var curFate = FateManager.Instance()->CurrentFate;
        ClientState.Fate activeFate = curFate != null ? new(curFate->FateId, curFate->Location, curFate->Radius, curFate->Progress, curFate->HandInCount, Utils.ReadField<uint>(curFate, 0x14)) : default;
        if (_ws.Client.ActiveFate != activeFate)
            _ws.Execute(new ClientState.OpActiveFateChange(activeFate));

        var petinfo = uiState->Buddy.PetInfo;
        var pet = new ClientState.Pet(petinfo.Pet->EntityId, petinfo.Order, petinfo.Stance);
        if (_ws.Client.ActivePet != pet)
            _ws.Execute(new ClientState.OpActivePetChange(pet));

        var focusTarget = TargetSystem.Instance()->FocusTarget;
        var focusTargetId = focusTarget != null ? SanitizedObjectID(focusTarget->GetGameObjectId()) : 0;
        if (_ws.Client.FocusTargetId != focusTargetId)
            _ws.Execute(new ClientState.OpFocusTargetChange(focusTargetId));

        var forcedMovementDir = MovementOverride.ForcedMovementDirection->Radians();
        if (_ws.Client.ForcedMovementDirection != forcedMovementDir)
            _ws.Execute(new ClientState.OpForcedMovementDirectionChange(forcedMovementDir));

        var contentKeyValue = uiState->PlayerState.ContentKeyValueData;
        var ckArray = new uint[]
        {
            contentKeyValue[0].Item1,
            contentKeyValue[0].Item2,
            contentKeyValue[1].Item1,
            contentKeyValue[1].Item2,
            contentKeyValue[2].Item1,
            contentKeyValue[2].Item2
        };
        if (!MemoryExtensions.SequenceEqual(ckArray, _ws.Client.ContentKeyValueData))
            _ws.Execute(new ClientState.OpContentKVDataChange(ckArray));

        var hate = uiState->Hate;
        var hatePrimary = hate.HateTargetId;
        var hateTargets = new ClientState.Hate[32];
        for (var i = 0; i < hate.HateArrayLength; i++)
            hateTargets[i] = new(hate.HateInfo[i].EntityId, hate.HateInfo[i].Enmity);

        if (hatePrimary != _ws.Client.CurrentTargetHate.InstanceID || !MemoryExtensions.SequenceEqual(hateTargets, _ws.Client.CurrentTargetHate.Targets))
            _ws.Execute(new ClientState.OpHateChange(hatePrimary, hateTargets));

        var timers = actionManager->ProcTimers[1..];
        if (!MemoryExtensions.SequenceEqual(timers, _ws.Client.ProcTimers))
            _ws.Execute(new ClientState.OpProcTimersChange(timers.ToArray()));

        void updateQuantity(uint itemId, uint count)
        {
            if (itemId == 0)
                return;
            if (count != _ws.Client.GetItemQuantity(itemId))
                _ws.Execute(new ClientState.OpInventoryChange(itemId, count));
        }

        if (_needInventoryUpdate)
        {
            var im = InventoryManager.Instance();
            // update tracked items
            foreach (var id in ActionDefinitions.Instance.SupportedItems)
            {
                var count = im->GetInventoryItemCount(id % 500000, id > 1000000, checkEquipped: false, checkArmory: false);
                updateQuantity(id, (uint)count);
            }

            // update all key items (smaller set)
            var ic = im->GetInventoryContainer(InventoryType.KeyItems);
            if (ic->IsLoaded)
            {
                for (var i = 0; i < ic->Size; i++)
                {
                    var keyItem = ic->GetInventorySlot(i);
                    if (keyItem != null)
                        updateQuantity(keyItem->GetItemId(), keyItem->GetQuantity());
                }
            }
            _needInventoryUpdate = false;
        }
    }

    private unsafe void UpdateDeepDungeon()
    {
        var dd = EventFramework.Instance()->GetInstanceContentDeepDungeon();
        if (dd != null)
        {
            var currentId = (DeepDungeonState.DungeonType)dd->DeepDungeonId;
            var fullUpdate = currentId != _ws.DeepDungeon.DungeonId;

            var progress = new DeepDungeonState.DungeonProgress(dd->Floor, dd->ActiveLayoutIndex, dd->WeaponLevel, dd->ArmorLevel, dd->SyncedGearLevel, dd->HoardCount, dd->ReturnProgress, dd->PassageProgress);
            if (fullUpdate || progress != _ws.DeepDungeon.Progress)
                _ws.Execute(new DeepDungeonState.OpProgressChange(currentId, progress));

            if (fullUpdate || !MemoryExtensions.SequenceEqual(_ws.DeepDungeon.Rooms.AsSpan(), dd->MapData))
                _ws.Execute(new DeepDungeonState.OpMapDataChange(dd->MapData.ToArray()));

            Span<DeepDungeonState.PartyMember> party = stackalloc DeepDungeonState.PartyMember[DeepDungeonState.NumPartyMembers];
            for (var i = 0; i < DeepDungeonState.NumPartyMembers; ++i)
            {
                ref var p = ref dd->Party[i];
                party[i] = new(SanitizedObjectID(p.EntityId), SanitizeDeepDungeonRoom(p.RoomIndex));
            }
            if (fullUpdate || !MemoryExtensions.SequenceEqual(_ws.DeepDungeon.Party.AsSpan(), party))
                _ws.Execute(new DeepDungeonState.OpPartyStateChange(party.ToArray()));

            Span<DeepDungeonState.PomanderState> pomanders = stackalloc DeepDungeonState.PomanderState[DeepDungeonState.NumPomanderSlots];
            for (var i = 0; i < DeepDungeonState.NumPomanderSlots; ++i)
            {
                ref var item = ref dd->Items[i];
                pomanders[i] = new(item.Count, item.Flags);
            }
            if (fullUpdate || !MemoryExtensions.SequenceEqual(_ws.DeepDungeon.Pomanders.AsSpan(), pomanders))
                _ws.Execute(new DeepDungeonState.OpPomandersChange(pomanders.ToArray()));

            Span<DeepDungeonState.Chest> chests = stackalloc DeepDungeonState.Chest[DeepDungeonState.NumChests];
            for (var i = 0; i < DeepDungeonState.NumChests; ++i)
            {
                ref var c = ref dd->Chests[i];
                chests[i] = new(c.ChestType, SanitizeDeepDungeonRoom(c.RoomIndex));
            }
            if (fullUpdate || !MemoryExtensions.SequenceEqual(_ws.DeepDungeon.Chests.AsSpan(), chests))
                _ws.Execute(new DeepDungeonState.OpChestsChange(chests.ToArray()));

            if (fullUpdate || !MemoryExtensions.SequenceEqual(_ws.DeepDungeon.Magicite.AsSpan(), dd->Magicite))
                _ws.Execute(new DeepDungeonState.OpMagiciteChange(dd->Magicite.ToArray()));
        }
        else if (_ws.DeepDungeon.DungeonId != DeepDungeonState.DungeonType.None)
        {
            // exiting deep dungeon, clean up all state
            _ws.Execute(new DeepDungeonState.OpProgressChange(DeepDungeonState.DungeonType.None, default));
        }
        // else: we were and still are outside deep dungeon, nothing to do
    }

    private byte SanitizeDeepDungeonRoom(sbyte room) => room < 0 ? (byte)0 : (byte)room;
    private ulong SanitizedObjectID(ulong raw) => raw != InvalidEntityId ? raw : 0;

    private void DispatchActorEvents(ulong instanceID)
    {
        var ops = _actorOps.GetValueOrDefault(instanceID);
        if (ops == null)
            return;

        foreach (var op in ops)
            _ws.Execute(op);
        _actorOps.Remove(instanceID);
    }

    private List<(int, Cooldown)> CalcCooldownDifference(Span<Cooldown> values, ReadOnlySpan<Cooldown> reference)
    {
        var res = new List<(int, Cooldown)>();
        for (int i = 0, cnt = Math.Min(values.Length, reference.Length); i < cnt; ++i)
            if (values[i] != reference[i])
                res.Add((i, values[i]));
        return res;
    }

    private List<(BozjaHolsterID, byte)> CalcBozjaHolster(Span<byte> contents)
    {
        var res = new List<(BozjaHolsterID, byte)>();
        for (int i = 0; i < contents.Length; ++i)
            if (contents[i] != 0)
                res.Add(((BozjaHolsterID)i, contents[i]));
        return res;
    }

    private unsafe ClientState.Gauge GaugeData()
    {
        var curGauge = JobGaugeManager.Instance()->CurrentGauge;
        return curGauge != null ? new(Utils.ReadField<ulong>(curGauge, 8), Utils.ReadField<ulong>(curGauge, 16)) : default;
    }

    private unsafe void ServerIPCReceived(DateTime sendTimestamp, uint sourceServerActor, uint targetServerActor, ushort opcode, uint epoch, Span<byte> payload)
    {
        var id = _opcodeMap.ID(opcode);
        // targetServerActor is always a player?..
        var ipc = new NetworkState.ServerIPC(id, opcode, epoch, sourceServerActor, sendTimestamp, [.. payload]);
        if (_netConfig.Data.RecordServerPackets)
            _globalOps.Add(new NetworkState.OpServerIPC(ipc));
        if (_netConfig.Data.DumpServerPackets && (!_netConfig.Data.DumpServerPacketsPlayerOnly || sourceServerActor == UIState.Instance()->PlayerState.EntityId))
            _decoder.LogNode(_decoder.Decode(ipc, DateTime.UtcNow), "");
    }

    private unsafe void ClientIPCSent(uint opcode, Span<byte> payload)
    {
        if (_netConfig.Data.DumpClientPackets)
        {
            var sb = new StringBuilder($"Client IPC [0x{opcode:X4}]: data=");
            foreach (byte b in payload)
                sb.Append($"{b:X2}");
            _decoder.LogNode(new(sb.ToString()), "");
        }
    }

    private void OnActionRequested(ClientActionRequest arg)
    {
        _globalOps.Add(new ClientState.OpActionRequest(arg));
    }

    private void OnActionEffect(ulong casterID, ActorCastEvent info)
    {
        _actorOps.GetOrAdd(casterID).Add(new ActorState.OpCastEvent(casterID, info));
    }

    private void OnEffectResult(ulong targetID, uint seq, int targetIndex)
    {
        _actorOps.GetOrAdd(targetID).Add(new ActorState.OpEffectResult(targetID, seq, targetIndex));
    }

    private unsafe void ProcessPacketActorCastDetour(uint casterId, Network.ServerIPC.ActorCast* packet)
    {
        _lastCastPositions[casterId] = Network.PacketDecoder.IntToFloatCoords(packet->PosX, packet->PosY, packet->PosZ);
        _processPacketActorCastHook.Original(casterId, packet);
    }

    private unsafe void ProcessPacketEffectResultDetour(uint targetID, byte* packet, byte replaying)
    {
        var count = packet[0];
        var p = (Network.ServerIPC.EffectResultEntry*)(packet + 4);
        for (int i = 0; i < count; ++i)
        {
            OnEffectResult(targetID, p->RelatedActionSequence, p->RelatedTargetIndex);
            ++p;
        }
        _processPacketEffectResultHook.Original(targetID, packet, replaying);
    }

    private unsafe void ProcessPacketEffectResultBasicDetour(uint targetID, byte* packet, byte replaying)
    {
        var count = packet[0];
        var p = (Network.ServerIPC.EffectResultBasicEntry*)(packet + 4);
        for (int i = 0; i < count; ++i)
        {
            OnEffectResult(targetID, p->RelatedActionSequence, p->RelatedTargetIndex);
            ++p;
        }
        _processPacketEffectResultBasicHook.Original(targetID, packet, replaying);
    }

    private void ProcessPacketActorControlDetour(uint actorID, uint category, uint p1, uint p2, uint p3, uint p4, uint p5, uint p6, ulong targetID, byte replaying)
    {
        _processPacketActorControlHook.Original(actorID, category, p1, p2, p3, p4, p5, p6, targetID, replaying);
        switch ((Network.ServerIPC.ActorControlCategory)category)
        {
            case Network.ServerIPC.ActorControlCategory.TargetIcon:
                _actorOps.GetOrAdd(actorID).Add(new ActorState.OpIcon(actorID, p1 - Network.IDScramble.Delta, p2));
                break;
            case Network.ServerIPC.ActorControlCategory.TargetVFX:
                _actorOps.GetOrAdd(actorID).Add(new ActorState.OpVFX(actorID, p1, p2));
                break;
            case Network.ServerIPC.ActorControlCategory.Tether:
                _actorOps.GetOrAdd(actorID).Add(new ActorState.OpTether(actorID, new(p2, p3)));
                break;
            case Network.ServerIPC.ActorControlCategory.TetherCancel:
                // note: this seems to clear tether only if existing matches p2
                _actorOps.GetOrAdd(actorID).Add(new ActorState.OpTether(actorID, default));
                break;
            case Network.ServerIPC.ActorControlCategory.EObjSetState:
                // p2 is unused (seems to be director id?), p3==1 means housing (?) item instead of event obj, p4 is housing item id
                _actorOps.GetOrAdd(actorID).Add(new ActorState.OpEventObjectStateChange(actorID, (ushort)p1));
                break;
            case Network.ServerIPC.ActorControlCategory.EObjAnimation:
                _actorOps.GetOrAdd(actorID).Add(new ActorState.OpEventObjectAnimation(actorID, (ushort)p1, (ushort)p2));
                break;
            case Network.ServerIPC.ActorControlCategory.PlayActionTimeline:
                _actorOps.GetOrAdd(actorID).Add(new ActorState.OpPlayActionTimelineEvent(actorID, (ushort)p1));
                break;
            case Network.ServerIPC.ActorControlCategory.ActionRejected:
                _globalOps.Add(new ClientState.OpActionReject(new(new((ActionType)p2, p3), p6, p4 * 0.01f, p5 * 0.01f, p1)));
                break;
            case Network.ServerIPC.ActorControlCategory.DirectorUpdate:
                _globalOps.Add(new WorldState.OpDirectorUpdate(p1, p2, p3, p4, p5, p6));
                break;
            case Network.ServerIPC.ActorControlCategory.FateReceiveItem:
                _needInventoryUpdate = true;
                break;
        }
    }

    private unsafe void ProcessPacketNpcYellDetour(Network.ServerIPC.NpcYell* packet)
    {
        _processPacketNpcYellHook.Original(packet);
        _actorOps.GetOrAdd(packet->SourceID).Add(new ActorState.OpEventNpcYell(packet->SourceID, packet->Message));
    }

    private unsafe void ProcessMapEffectDetour(void* self, uint index, ushort s1, ushort s2)
    {
        // note: this function is only executed for incoming packets that pass some checks (validation that currently active director is what is expected) - don't think it's a big deal?
        _processMapEffectHook.Original(self, index, s1, s2);
        _globalOps.Add(new WorldState.OpMapEffect((byte)index, s1 | ((uint)s2 << 16)));
    }

    private unsafe void ProcessPacketRSVDataDetour(byte* packet)
    {
        _processPacketRSVDataHook.Original(packet);
        _globalOps.Add(new WorldState.OpRSVData(MemoryHelper.ReadStringNullTerminated((nint)(packet + 4)), MemoryHelper.ReadString((nint)(packet + 0x34), *(int*)packet)));
    }

    private unsafe void ProcessPacketOpenTreasureDetour(uint playerID, byte* packet)
    {
        _processPacketOpenTreasureHook.Original(playerID, packet);
        var actorID = *(uint*)(packet + 16);
        _actorOps.GetOrAdd(actorID).Add(new ActorState.OpEventOpenTreasure(actorID));
    }

    private unsafe void* ProcessSystemLogMessageDetour(uint entityId, uint messageId, int* args, byte argCount)
    {
        var res = _processSystemLogMessageHook.Original(entityId, messageId, args, argCount);
        _globalOps.Add(new WorldState.OpSystemLogMessage(messageId, new Span<int>(args, argCount).ToArray()));
        return res;
    }

    private unsafe void* ProcessPacketFateInfoDetour(ulong fateId, long startTimestamp, ulong durationSecs)
    {
        var res = _processPacketFateInfoHook.Original(fateId, startTimestamp, durationSecs);
        _globalOps.Add(new ClientState.OpFateInfo((uint)fateId, DateTimeOffset.FromUnixTimeSeconds(startTimestamp).UtcDateTime));
        return res;
    }

    private unsafe void ProcessMapEffect1Detour(ContentDirector* director, byte* packet)
    {
        _processMapEffect1Hook.Original(director, packet);
        ProcessMapEffect(packet, 10, 18);
    }

    private unsafe void ProcessMapEffect2Detour(ContentDirector* director, byte* packet)
    {
        _processMapEffect2Hook.Original(director, packet);
        ProcessMapEffect(packet, 18, 34);
    }

    private unsafe void ProcessMapEffect3Detour(ContentDirector* director, byte* packet)
    {
        _processMapEffect3Hook.Original(director, packet);
        ProcessMapEffect(packet, 26, 50);
    }

    private unsafe void ProcessMapEffect(byte* data, byte offLow, byte offIndex)
    {
        for (var i = 0; i < *data; i++)
        {
            var low = *(ushort*)(data + 2 * i + offLow);
            var high = *(ushort*)(data + 2 * i + 2);
            var index = data[i + offIndex];
            _globalOps.Add(new WorldState.OpMapEffect(index, low | ((uint)high << 16)));
        }
    }

    private unsafe void ApplyKnockbackDetour(Character* thisPtr, float a2, float a3, float a4, byte a5, int a6)
    {
        Service.Log("applying knockback to player");
        _applyKnockbackHook.Original(thisPtr, a2, a3, a4, a5, a6);
    }

    private unsafe byte ProcessLegacyMapEffectDetour(EventFramework* fwk, EventId eventId, byte seq, byte unk, void* data, ulong length)
    {
        var res = _processLegacyMapEffectHook.Original(fwk, eventId, seq, unk, data, length);

        _globalOps.Add(new WorldState.OpLegacyMapEffect(seq, unk, new Span<byte>(data, (int)length).ToArray()));

        return res;
    }

    private unsafe void InventoryAckDetour(uint a1, void* a2)
    {
        _inventoryAckHook.Original(a1, a2);
        _needInventoryUpdate = true;
    }
}
