using Dalamud.Hooking;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Fate;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.Interop;

namespace BossMod;

// utility that updates a world state to correspond to game state
sealed class WorldStateGameSync : IDisposable
{
    private const int ObjectTableSize = 599; // should match CS; note that different ranges are used for different purposes - consider splitting?..
    private const uint InvalidEntityId = 0xE0000000;

    private readonly WorldState _ws;
    private readonly DateTime _startTime;
    private readonly long _startQPC;

    private readonly List<WorldState.Operation> _globalOps = [];
    private readonly Dictionary<ulong, List<WorldState.Operation>> _actorOps = [];
    private readonly Actor?[] _actorsByIndex = new Actor?[ObjectTableSize];

    private readonly List<(ulong Caster, ActorCastEvent Event)> _castEvents = [];
    private readonly List<(uint Seq, ulong Target, int TargetIndex)> _confirms = [];

    private readonly Network.OpcodeMap _opcodeMap = new();
    private readonly Network.PacketInterceptor _interceptor = new();
    private readonly Network.PacketDecoderGame _decoder = new();

    private readonly ConfigListener<ReplayManagementConfig> _netConfig;
    private readonly EventSubscriptions _subscriptions;

    private unsafe delegate void ProcessPacketEffectResultDelegate(uint targetID, byte* packet, byte replaying);
    private readonly Hook<ProcessPacketEffectResultDelegate> _processPacketEffectResultHook;
    private readonly Hook<ProcessPacketEffectResultDelegate> _processPacketEffectResultBasicHook;

    private delegate void ProcessPacketActorControlDelegate(uint actorID, uint category, uint p1, uint p2, uint p3, uint p4, uint p5, uint p6, ulong targetID, byte replaying);
    private readonly Hook<ProcessPacketActorControlDelegate> _processPacketActorControlHook;

    private unsafe delegate void ProcessPacketNpcYellDelegate(Network.ServerIPC.NpcYell* packet);
    private readonly Hook<ProcessPacketNpcYellDelegate> _processPacketNpcYellHook;

    private unsafe delegate void ProcessEnvControlDelegate(void* self, uint index, ushort s1, ushort s2);
    private readonly Hook<ProcessEnvControlDelegate> _processEnvControlHook;

    private unsafe delegate void ProcessPacketRSVDataDelegate(byte* packet);
    private readonly Hook<ProcessPacketRSVDataDelegate> _processPacketRSVDataHook;

    public unsafe WorldStateGameSync(WorldState ws)
    {
        _ws = ws;
        _startTime = DateTime.Now;
        _startQPC = Framework.Instance()->PerformanceCounterValue;
        _interceptor.ServerIPCReceived += ServerIPCReceived;

        _netConfig = Service.Config.GetAndSubscribe<ReplayManagementConfig>(config => _interceptor.Active = config.RecordServerPackets || config.DumpServerPackets);
        _subscriptions = new
        (
            ActionManagerEx.Instance!.ActionRequestExecuted.Subscribe(OnActionRequested),
            ActionManagerEx.Instance!.ActionEffectReceived.Subscribe(OnActionEffect)
        );

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
        _processPacketNpcYellHook = Service.Hook.HookFromSignature<ProcessPacketNpcYellDelegate>("48 83 EC 58 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 ?? 0F 10 41 10", ProcessPacketNpcYellDetour);
        _processPacketNpcYellHook.Enable();
        Service.Log($"[WSG] ProcessPacketNpcYell address = 0x{_processPacketNpcYellHook.Address:X}");

        _processEnvControlHook = Service.Hook.HookFromSignature<ProcessEnvControlDelegate>("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 41 0F B7 E8", ProcessEnvControlDetour);
        _processEnvControlHook.Enable();
        Service.Log($"[WSG] ProcessEnvControl address = 0x{_processEnvControlHook.Address:X}");

        _processPacketRSVDataHook = Service.Hook.HookFromSignature<ProcessPacketRSVDataDelegate>("44 8B 09 4C 8D 41 34", ProcessPacketRSVDataDetour);
        _processPacketRSVDataHook.Enable();
        Service.Log($"[WSG] ProcessPacketRSVData address = 0x{_processPacketRSVDataHook.Address:X}");
    }

    public void Dispose()
    {
        _processPacketEffectResultBasicHook.Dispose();
        _processPacketEffectResultHook.Dispose();
        _processPacketActorControlHook.Dispose();
        _processPacketNpcYellHook.Dispose();
        _processEnvControlHook.Dispose();
        _processPacketRSVDataHook.Dispose();
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
            GaugeData()
        ));
        if (_ws.CurrentZone != Service.ClientState.TerritoryType || _ws.CurrentCFCID != GameMain.Instance()->CurrentContentFinderConditionId)
        {
            _ws.Execute(new WorldState.OpZoneChange(Service.ClientState.TerritoryType, GameMain.Instance()->CurrentContentFinderConditionId));
        }
        if (_ws.Network.IDScramble != Network.IDScramble.Delta)
        {
            _ws.Execute(new NetworkState.OpIDScramble(Network.IDScramble.Delta));
        }

        foreach (var c in _confirms)
            _ws.PendingEffects.Confirm(_ws.CurrentTime, c.Seq, c.Target, c.TargetIndex);
        _confirms.Clear();
        _ws.PendingEffects.RemoveExpired(_ws.CurrentTime);
        foreach (var c in _castEvents)
            _ws.PendingEffects.AddEntry(_ws.CurrentTime, c.Caster, c.Event);
        _castEvents.Clear();

        foreach (var op in _globalOps)
        {
            _ws.Execute(op);
        }
        _globalOps.Clear();

        UpdateWaymarks();
        UpdateActors();
        UpdateParty();
        UpdateClient();
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
                Service.Log($"[WorldState] Skipping bad object #{i} with id {obj->EntityId:X}");
                obj = null;
            }

            if (actor != null && (obj == null || actor.InstanceID != obj->EntityId))
            {
                _actorsByIndex[i] = null;
                RemoveActor(actor);
                actor = null;
            }

            if (obj != null)
            {
                if (actor != _ws.Actors.Find(obj->EntityId))
                {
                    Service.Log($"[WorldState] Actor position mismatch for #{i} {actor}");
                }
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
            inCombat = chr->InCombat;
        }
        var targetable = obj->GetIsTargetable();
        var friendly = Utils.GameObjectIsFriendly(obj) != 0;
        var isDead = obj->IsDead();
        var target = chr != null ? SanitizedObjectID(chr->GetTargetId()) : 0; // note: when changing targets, we want to see changes immediately rather than wait for server response
        var modelState = chr != null ? new ActorModelState(chr->Timeline.ModelState, chr->Timeline.AnimationState[0], chr->Timeline.AnimationState[1]) : default;
        var eventState = obj->EventState;
        var radius = obj->GetRadius();

        if (act == null)
        {
            var type = (ActorType)(((int)obj->ObjectKind << 8) + obj->SubKind);
            _ws.Execute(new ActorState.OpCreate(obj->EntityId, obj->BaseId, index, name, nameID, type, classID, level, posRot, radius, hpmp, targetable, friendly, SanitizedObjectID(obj->OwnerId), obj->FateId));
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
        if (act.ModelState != modelState)
            _ws.Execute(new ActorState.OpModelState(act.InstanceID, modelState));
        if (act.EventState != eventState)
            _ws.Execute(new ActorState.OpEventState(act.InstanceID, eventState));
        if (act.TargetID != target)
            _ws.Execute(new ActorState.OpTarget(act.InstanceID, target));
        DispatchActorEvents(act.InstanceID);

        var castInfo = chr != null ? chr->GetCastInfo() : null;
        if (castInfo != null)
        {
            var curCast = castInfo->IsCasting != 0
                ? new ActorCastInfo
                {
                    Action = new((ActionType)castInfo->ActionType, castInfo->ActionId),
                    TargetID = SanitizedObjectID(castInfo->TargetId),
                    Rotation = chr->CastRotation.Radians(),
                    Location = castInfo->TargetLocation,
                    TotalTime = castInfo->TotalCastTime, // TODO: should it use adjusted here?..
                    FinishAt = _ws.CurrentTime.AddSeconds(Math.Clamp(castInfo->TotalCastTime - castInfo->CurrentCastTime, 0, 100000)),
                    Interruptible = castInfo->Interruptible != 0,
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
                    curStatus.SourceID = SanitizedObjectID(s.SourceId);
                    curStatus.Extra = s.Param;
                    curStatus.ExpireAt = _ws.CurrentTime.AddSeconds(dur);
                }
                UpdateActorStatus(act, i, curStatus);
            }
        }
    }

    private void UpdateActorCastInfo(Actor act, ActorCastInfo? cast)
    {
        if (cast == null && act.CastInfo == null)
            return; // was not casting and is not casting

        // note: ignore small finish-at differences, assume these are due to frame time irregularities
        if (cast != null && act.CastInfo != null && cast.Action == act.CastInfo.Action && cast.TargetID == act.CastInfo.TargetID && Math.Abs((cast.FinishAt - act.CastInfo.FinishAt).TotalSeconds) < 0.2)
        {
            // continuing casting same spell
            act.CastInfo.TotalTime = cast.TotalTime;
            act.CastInfo.FinishAt = cast.FinishAt;
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
        var gm = GroupManager.Instance();
        var ui = UIState.Instance();
        var pc = GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        var pcContentId = UIState.Instance()->PlayerState.ContentId;
        var pcEntityId = UIState.Instance()->PlayerState.EntityId;
        if (Service.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.DutyRecorderPlayback])
        {
            // when doing replay playback, game uses independent group manager
            gm += 1;
            pcEntityId = pc != null ? pc->EntityId : 0;
            var member = pc != null ? gm->GetPartyMemberByObjectId(pcEntityId) : null;
            pcContentId = member != null ? member->ContentId : 0;
        }
        else if (pc != null && pc->EntityId != pcEntityId)
        {
            Service.Log($"[WSG] Player entity id mismatch: {pcEntityId:X} vs {pc->EntityId:X}");
        }

        // update player slot
        UpdatePartySlot(PartyState.PlayerSlot, pcContentId, pcEntityId);

        // update normal party slots: first update/remove existing members, then add new ones
        for (int i = PartyState.PlayerSlot + 1; i < PartyState.MaxPartySize; ++i)
        {
            var contentID = _ws.Party.ContentIDs[i];
            var instanceID = _ws.Party.ActorIDs[i];
            if (contentID != 0)
            {
                // slot was occupied by player => see if it's still in party
                var member = gm->GetPartyMemberByContentId(contentID);
                if (member != null)
                    UpdatePartySlot(i, contentID, member->EntityId); // slot is still occupied by player; update in case instance-id changed
                else
                    UpdatePartySlot(i, 0, 0); // player is no longer in party => clear slot
            }
            else if (instanceID != 0)
            {
                // slot was occupied by trust => see if it's still in party
                if (!HasBuddy(instanceID))
                    UpdatePartySlot(i, 0, 0); // buddy is no longer in party => clear slot
                // else: no reason to update...
            }
            // else: slot was empty, skip
        }
        for (int i = 0; i < gm->MemberCount; ++i)
        {
            ref var member = ref gm->PartyMembers[i];
            if (_ws.Party.ContentIDs.IndexOf(member.ContentId) == -1)
                AddPartyMember(member.ContentId, member.EntityId);
            // else: already added, updated in previous loop
        }
        for (int i = 0; i < ui->Buddy.DutyHelperInfo.ENpcIds.Length; ++i)
        {
            var instanceID = ui->Buddy.DutyHelperInfo.DutyHelpers[i].EntityId;
            if (instanceID != InvalidEntityId && _ws.Party.ActorIDs[1..PartyState.MaxPartySize].IndexOf(instanceID) == -1)
                AddPartyMember(0, instanceID);
            // else: buddy is non-existent or already updated, skip
        }

        // update alliance members
        var isNormalAlliance = gm->IsAlliance && !gm->IsSmallGroupAlliance;
        for (int i = PartyState.MaxPartySize; i < PartyState.MaxAllianceSize; ++i)
        {
            var member = isNormalAlliance ? gm->AllianceMembers.GetPointer(i - PartyState.MaxPartySize) : null;
            if (member != null && !member->IsValidAllianceMember)
                member = null;
            UpdatePartySlot(i, 0, member != null ? member->EntityId : 0);
        }

        // update limit break
        var lb = LimitBreakController.Instance();
        if (_ws.Party.LimitBreakCur != lb->CurrentUnits || _ws.Party.LimitBreakMax != lb->BarUnits)
            _ws.Execute(new PartyState.OpLimitBreakChange(lb->CurrentUnits, lb->BarUnits));
    }

    private unsafe bool HasBuddy(ulong instanceID)
    {
        var ui = UIState.Instance();
        for (int i = 0; i < ui->Buddy.DutyHelperInfo.ENpcIds.Length; ++i)
            if (ui->Buddy.DutyHelperInfo.DutyHelpers[i].EntityId == instanceID)
                return true;
        return false;
    }

    private int FindFreePartySlot()
    {
        for (int i = 1; i < PartyState.MaxPartySize; ++i)
            if (_ws.Party.ContentIDs[i] == 0 && _ws.Party.ActorIDs[i] == 0)
                return i;
        return -1;
    }

    private void AddPartyMember(ulong contentID, ulong instanceID)
    {
        var freeSlot = FindFreePartySlot();
        if (freeSlot >= 0)
            _ws.Execute(new PartyState.OpModify(freeSlot, contentID, instanceID));
        else
            Service.Log($"[WorldState] Failed to find empty slot for party member {contentID:X}:{instanceID:X}");
    }

    private void UpdatePartySlot(int slot, ulong contentID, ulong instanceID)
    {
        if (contentID != (slot < PartyState.MaxPartySize ? _ws.Party.ContentIDs[slot] : 0) || instanceID != _ws.Party.ActorIDs[slot])
            _ws.Execute(new PartyState.OpModify(slot, contentID, instanceID));
    }

    private unsafe void UpdateClient()
    {
        var countdownAgent = AgentCountDownSettingDialog.Instance();
        float? countdown = countdownAgent != null && countdownAgent->Active ? countdownAgent->TimeRemaining : null;
        if (_ws.Client.CountdownRemaining != countdown)
            _ws.Execute(new ClientState.OpCountdownChange(countdown));

        Span<Cooldown> cooldowns = stackalloc Cooldown[_ws.Client.Cooldowns.Length];
        ActionManagerEx.Instance!.GetCooldowns(cooldowns);
        if (!MemoryExtensions.SequenceEqual(_ws.Client.Cooldowns.AsSpan(), cooldowns))
        {
            if (cooldowns.IndexOfAnyExcept(default(Cooldown)) < 0)
                _ws.Execute(new ClientState.OpCooldown(true, []));
            else
                _ws.Execute(new ClientState.OpCooldown(false, CalcCooldownDifference(cooldowns, _ws.Client.Cooldowns.AsSpan())));
        }

        var (dutyAction0, dutyAction1) = ActionManagerEx.Instance!.GetDutyActions();
        if (_ws.Client.DutyActions[0] != dutyAction0 || _ws.Client.DutyActions[1] != dutyAction1)
            _ws.Execute(new ClientState.OpDutyActionsChange(dutyAction0, dutyAction1));

        Span<byte> bozjaHolster = stackalloc byte[_ws.Client.BozjaHolster.Length];
        bozjaHolster.Clear();
        var bozjaState = PublicContentBozja.GetState();
        if (bozjaState != null)
            foreach (var action in bozjaState->HolsterActions)
                if (action != 0)
                    ++bozjaHolster[action];
        if (!MemoryExtensions.SequenceEqual(_ws.Client.BozjaHolster.AsSpan(), bozjaHolster))
            _ws.Execute(new ClientState.OpBozjaHolsterChange(CalcBozjaHolster(bozjaHolster)));

        var curFate = FateManager.Instance()->CurrentFate;
        ClientState.Fate activeFate = curFate != null ? new(curFate->FateId, curFate->Location, curFate->Radius) : default;
        if (_ws.Client.ActiveFate != activeFate)
            _ws.Execute(new ClientState.OpActiveFateChange(activeFate));
    }

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

    private unsafe ulong GaugeData()
    {
        var curGauge = JobGaugeManager.Instance()->CurrentGauge;
        return curGauge != null ? Utils.ReadField<ulong>(curGauge, 8) : 0;
    }

    private unsafe void ServerIPCReceived(DateTime sendTimestamp, uint sourceServerActor, uint targetServerActor, ushort opcode, uint epoch, Span<byte> payload)
    {
        var id = _opcodeMap.ID(opcode);
        // targetServerActor is always a player?..
        var ipc = new NetworkState.ServerIPC(id, opcode, epoch, sourceServerActor, sendTimestamp, [.. payload]);
        if (_netConfig.Data.RecordServerPackets)
            _globalOps.Add(new NetworkState.OpServerIPC(ipc));
        if (_netConfig.Data.DumpServerPackets)
            _decoder.LogNode(_decoder.Decode(ipc, DateTime.UtcNow), "");
    }

    private void OnActionRequested(ClientActionRequest arg)
    {
        _globalOps.Add(new ClientState.OpActionRequest(arg));
    }

    private void OnActionEffect(ulong casterID, ActorCastEvent info)
    {
        _actorOps.GetOrAdd(casterID).Add(new ActorState.OpCastEvent(casterID, info));
        _castEvents.Add((casterID, info));
    }

    private void OnEffectResult(ulong targetID, uint seq, int targetIndex)
    {
        _actorOps.GetOrAdd(targetID).Add(new ActorState.OpEffectResult(targetID, seq, targetIndex));
        _confirms.Add((seq, targetID, targetIndex));
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
                _actorOps.GetOrAdd(actorID).Add(new ActorState.OpIcon(actorID, p1 - Network.IDScramble.Delta));
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
        }
    }

    private unsafe void ProcessPacketNpcYellDetour(Network.ServerIPC.NpcYell* packet)
    {
        _processPacketNpcYellHook.Original(packet);
        _actorOps.GetOrAdd(packet->SourceID).Add(new ActorState.OpEventNpcYell(packet->SourceID, packet->Message));
    }

    private unsafe void ProcessEnvControlDetour(void* self, uint index, ushort s1, ushort s2)
    {
        // note: this function is only executed for incoming packets that pass some checks (validation that currently active director is what is expected) - don't think it's a big deal?
        _processEnvControlHook.Original(self, index, s1, s2);
        _globalOps.Add(new WorldState.OpEnvControl((byte)index, s1 | ((uint)s2 << 16)));
    }

    private unsafe void ProcessPacketRSVDataDetour(byte* packet)
    {
        _processPacketRSVDataHook.Original(packet);
        _globalOps.Add(new WorldState.OpRSVData(MemoryHelper.ReadStringNullTerminated((nint)(packet + 4)), MemoryHelper.ReadString((nint)(packet + 0x34), *(int*)packet)));
    }
}
