using Dalamud.Game.Network;
using Dalamud.Hooking;
using Dalamud.Memory;
using System;
using System.IO;
using System.Numerics;

namespace BossMod
{
    class Network : IDisposable
    {
        public event EventHandler<(ulong actorID, ActorCastEvent cast)>? EventActionEffect;
        public event EventHandler<(ulong actorID, uint seq, int targetIndex)>? EventEffectResult;
        public event EventHandler<(ulong actorID, ActionID action, float castTime, ulong targetID)>? EventActorCast;
        public event EventHandler<(ulong actorID, uint actionID)>? EventActorControlCancelCast;
        public event EventHandler<(ulong actorID, uint iconID)>? EventActorControlTargetIcon;
        public event EventHandler<(ulong actorID, ulong targetID, uint tetherID)>? EventActorControlTether;
        public event EventHandler<ulong>? EventActorControlTetherCancel;
        public event EventHandler<(ulong actorID, ushort state)>? EventActorControlEObjSetState;
        public event EventHandler<(ulong actorID, ushort p1, ushort p2)>? EventActorControlEObjAnimation;
        public event EventHandler<(ulong actorID, ushort actionTimelineID)>? EventActorControlPlayActionTimeline;
        public event EventHandler<ClientActionReject>? EventActorControlSelfActionRejected;
        public event EventHandler<(uint directorID, uint updateID, uint p1, uint p2, uint p3, uint p4)>? EventActorControlSelfDirectorUpdate;
        public event EventHandler<(uint directorID, byte index, uint state)>? EventEnvControl;
        public event EventHandler<(Waymark waymark, Vector3? pos)>? EventWaymark;
        public event EventHandler<(string key, string value)>? EventRSVData;

        private GeneralConfig _config;
        //private Logger _logger;

        private unsafe delegate void ProcessZonePacketDownDelegate(void* a, uint targetId, void* dataPtr);
        private Hook<ProcessZonePacketDownDelegate> _processZonePacketDownHook;

        private unsafe delegate byte ProcessZonePacketUpDelegate(void* a1, void* dataPtr, void* a3, byte a4);
        private Hook<ProcessZonePacketUpDelegate> _processZonePacketUpHook;

        // this is a mega weird thing - apparently some IDs sent over network have some extra delta added to them (e.g. action ids, icon ids, etc.)
        // they change on relogs or zone changes or something...
        // we have one simple way of detecting them - by looking at casts, since they contain both offset id and real ('animation') id
        private int _unkDelta = 0;

        public unsafe Network(DirectoryInfo logDir)
        {
            _config = Service.Config.Get<GeneralConfig>();
            _config.Modified += ApplyConfig;
            //_logger = new("Network", logDir);

            // this is lifted from dalamud - for some reason they stopped dispatching client messages :(
            //Service.GameNetwork.NetworkMessage += HandleMessage;
            var processZonePacketDownAddress = Service.SigScanner.ScanText("48 89 5C 24 ?? 56 48 83 EC 50 8B F2");
            _processZonePacketDownHook = Hook<ProcessZonePacketDownDelegate>.FromAddress(processZonePacketDownAddress, ProcessZonePacketDownDetour);
            _processZonePacketDownHook.Enable();

            var processZonePacketUpAddress = Service.SigScanner.ScanText("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC 70 8B 81 ?? ?? ?? ??");
            _processZonePacketUpHook = Hook<ProcessZonePacketUpDelegate>.FromAddress(processZonePacketUpAddress, ProcessZonePacketUpDetour);
            _processZonePacketUpHook.Enable();
        }

        public void Dispose()
        {
            _config.Modified -= ApplyConfig;
            //_logger.Deactivate();

            //Service.GameNetwork.NetworkMessage -= HandleMessage;
            _processZonePacketDownHook.Dispose();
            _processZonePacketUpHook.Dispose();
        }

        private void ApplyConfig(object? sender, EventArgs args)
        {
            //if (_config.DumpServerPackets)
            //{
            //    if (!_logger.Active)
            //        _logger.Activate(0);
            //}
            //else
            //{
            //    _logger.Deactivate();
            //}
        }

        private unsafe void ProcessZonePacketDownDetour(void* self, uint targetId, void* dataPtr)
        {
            HandleMessage((IntPtr)dataPtr + sizeof(Protocol.Server_IPCHeader), ((Protocol.Server_IPCHeader*)dataPtr)->MessageType, 0, targetId, NetworkMessageDirection.ZoneDown);
            _processZonePacketDownHook.Original(self, targetId, dataPtr);
        }

        private unsafe byte ProcessZonePacketUpDetour(void* self, void* dataPtr, void* a3, byte a4)
        {
            HandleMessage((IntPtr)dataPtr + 0x20, Utils.ReadField<ushort>(dataPtr, 0), 0, 0, NetworkMessageDirection.ZoneUp);
            return _processZonePacketUpHook.Original(self, dataPtr, a3, a4);
        }

        private unsafe void HandleMessage(IntPtr dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
        {
            if (direction == NetworkMessageDirection.ZoneDown)
            {
                // server->client
                if (_config.DumpServerPackets)
                {
                    DumpServerMessage(dataPtr, opCode, targetActorId);
                }

                switch ((Protocol.Opcode)opCode)
                {
                    case Protocol.Opcode.ActionEffect1:
                        HandleActionEffect1((Protocol.Server_ActionEffect1*)dataPtr, targetActorId);
                        break;
                    case Protocol.Opcode.ActionEffect8:
                        HandleActionEffect8((Protocol.Server_ActionEffect8*)dataPtr, targetActorId);
                        break;
                    case Protocol.Opcode.ActionEffect16:
                        HandleActionEffect16((Protocol.Server_ActionEffect16*)dataPtr, targetActorId);
                        break;
                    case Protocol.Opcode.ActionEffect24:
                        HandleActionEffect24((Protocol.Server_ActionEffect24*)dataPtr, targetActorId);
                        break;
                    case Protocol.Opcode.ActionEffect32:
                        HandleActionEffect32((Protocol.Server_ActionEffect32*)dataPtr, targetActorId);
                        break;
                    case Protocol.Opcode.EffectResultBasic1:
                        HandleEffectResultBasic(Math.Min((byte)1, *(byte*)dataPtr), (Protocol.Server_EffectResultBasicEntry*)(dataPtr + 4), targetActorId);
                        break;
                    case Protocol.Opcode.EffectResultBasic4:
                        HandleEffectResultBasic(Math.Min((byte)4, *(byte*)dataPtr), (Protocol.Server_EffectResultBasicEntry*)(dataPtr + 4), targetActorId);
                        break;
                    case Protocol.Opcode.EffectResultBasic8:
                        HandleEffectResultBasic(Math.Min((byte)8, *(byte*)dataPtr), (Protocol.Server_EffectResultBasicEntry*)(dataPtr + 4), targetActorId);
                        break;
                    case Protocol.Opcode.EffectResultBasic16:
                        HandleEffectResultBasic(Math.Min((byte)16, *(byte*)dataPtr), (Protocol.Server_EffectResultBasicEntry*)(dataPtr + 4), targetActorId);
                        break;
                    case Protocol.Opcode.EffectResultBasic32:
                        HandleEffectResultBasic(Math.Min((byte)32, *(byte*)dataPtr), (Protocol.Server_EffectResultBasicEntry*)(dataPtr + 4), targetActorId);
                        break;
                    case Protocol.Opcode.EffectResultBasic64:
                        HandleEffectResultBasic(Math.Min((byte)64, *(byte*)dataPtr), (Protocol.Server_EffectResultBasicEntry*)(dataPtr + 4), targetActorId);
                        break;
                    case Protocol.Opcode.EffectResult1:
                        HandleEffectResult(Math.Min((byte)1, *(byte*)dataPtr), (Protocol.Server_EffectResultEntry*)(dataPtr + 4), targetActorId);
                        break;
                    case Protocol.Opcode.EffectResult4:
                        HandleEffectResult(Math.Min((byte)4, *(byte*)dataPtr), (Protocol.Server_EffectResultEntry*)(dataPtr + 4), targetActorId);
                        break;
                    case Protocol.Opcode.EffectResult8:
                        HandleEffectResult(Math.Min((byte)8, *(byte*)dataPtr), (Protocol.Server_EffectResultEntry*)(dataPtr + 4), targetActorId);
                        break;
                    case Protocol.Opcode.EffectResult16:
                        HandleEffectResult(Math.Min((byte)16, *(byte*)dataPtr), (Protocol.Server_EffectResultEntry*)(dataPtr + 4), targetActorId);
                        break;
                    case Protocol.Opcode.ActorCast:
                        HandleActorCast((Protocol.Server_ActorCast*)dataPtr, targetActorId);
                        break;
                    case Protocol.Opcode.ActorControl:
                        HandleActorControl((Protocol.Server_ActorControl*)dataPtr, targetActorId);
                        break;
                    case Protocol.Opcode.ActorControlSelf:
                        HandleActorControlSelf((Protocol.Server_ActorControlSelf*)dataPtr, targetActorId);
                        break;
                    case Protocol.Opcode.EnvControl:
                        HandleEnvControl((Protocol.Server_EnvControl*)dataPtr, targetActorId);
                        break;
                    case Protocol.Opcode.Waymark:
                        HandleWaymark((Protocol.Server_Waymark*)dataPtr);
                        break;
                    case Protocol.Opcode.WaymarkPreset:
                        HandleWaymarkPreset((Protocol.Server_WaymarkPreset*)dataPtr);
                        break;
                    case Protocol.Opcode.RSVData:
                        HandleRSVData(MemoryHelper.ReadStringNullTerminated(dataPtr + 4), MemoryHelper.ReadString(dataPtr + 0x34, *(int*)dataPtr));
                        break;
                }
            }
            else
            {
                // client->server
                if (_config.DumpClientPackets)
                {
                    DumpClientMessage(dataPtr, opCode);
                }
            }
        }

        private unsafe void HandleActionEffect1(Protocol.Server_ActionEffect1* p, uint actorID)
        {
            HandleActionEffect(actorID, &p->Header, (ActionEffect*)p->Effects, p->TargetID, 1, new());
        }

        private unsafe void HandleActionEffect8(Protocol.Server_ActionEffect8* p, uint actorID)
        {
            HandleActionEffect(actorID, &p->Header, (ActionEffect*)p->Effects, p->TargetID, 8, IntToFloatCoords(p->TargetX, p->TargetY, p->TargetZ));
        }

        private unsafe void HandleActionEffect16(Protocol.Server_ActionEffect16* p, uint actorID)
        {
            HandleActionEffect(actorID, &p->Header, (ActionEffect*)p->Effects, p->TargetID, 16, IntToFloatCoords(p->TargetX, p->TargetY, p->TargetZ));
        }

        private unsafe void HandleActionEffect24(Protocol.Server_ActionEffect24* p, uint actorID)
        {
            HandleActionEffect(actorID, &p->Header, (ActionEffect*)p->Effects, p->TargetID, 24, IntToFloatCoords(p->TargetX, p->TargetY, p->TargetZ));
        }

        private unsafe void HandleActionEffect32(Protocol.Server_ActionEffect32* p, uint actorID)
        {
            HandleActionEffect(actorID, &p->Header, (ActionEffect*)p->Effects, p->TargetID, 32, IntToFloatCoords(p->TargetX, p->TargetY, p->TargetZ));
        }

        private unsafe void HandleActionEffect(uint casterID, Protocol.Server_ActionEffectHeader* header, ActionEffect* effects, ulong* targetIDs, uint maxTargets, Vector3 targetPos)
        {
            if (header->actionType == ActionType.Spell)
            {
                int newDelta = (int)header->actionId - (int)header->actionAnimationId;
                if (_unkDelta != newDelta)
                {
                    Service.Log($"Updating network delta: {_unkDelta} -> {newDelta}");
                    _unkDelta = newDelta;
                }
            }

            var info = new ActorCastEvent
            {
                Action = new(header->actionType, (uint)(header->actionId - _unkDelta)), // note: see _unkDelta comment
                MainTargetID = header->animationTargetId,
                AnimationLockTime = header->animationLockTime,
                MaxTargets = maxTargets,
                TargetPos = targetPos,
                SourceSequence = header->SourceSequence,
                GlobalSequence = header->globalEffectCounter,
            };

            var targets = Math.Min(header->NumTargets, maxTargets);
            for (int i = 0; i < targets; ++i)
            {
                var target = new ActorCastEvent.Target();
                target.ID = targetIDs[i];
                for (int j = 0; j < 8; ++j)
                    target.Effects[j] = *(ulong*)(effects + (i * 8) + j);
                info.Targets.Add(target);
            }

            EventActionEffect?.Invoke(this, (casterID, info));
        }

        private unsafe void HandleEffectResultBasic(int count, Protocol.Server_EffectResultBasicEntry* p, uint actorID)
        {
            for (int i = 0; i < count; ++i)
            {
                EventEffectResult?.Invoke(this, (actorID, p->RelatedActionSequence, p->RelatedTargetIndex));
                ++p;
            }
        }

        private unsafe void HandleEffectResult(int count, Protocol.Server_EffectResultEntry* p, uint actorID)
        {
            for (int i = 0; i < count; ++i)
            {
                EventEffectResult?.Invoke(this, (actorID, p->RelatedActionSequence, p->RelatedTargetIndex));
                ++p;
            }
        }

        private unsafe void HandleActorCast(Protocol.Server_ActorCast* p, uint actorID)
        {
            EventActorCast?.Invoke(this, (actorID, new(p->ActionType, p->SpellID), p->CastTime, p->TargetID));
        }

        private unsafe void HandleActorControl(Protocol.Server_ActorControl* p, uint actorID)
        {
            switch (p->category)
            {
                case Protocol.Server_ActorControlCategory.CancelCast:
                    EventActorControlCancelCast?.Invoke(this, (actorID, p->param3));
                    break;
                case Protocol.Server_ActorControlCategory.TargetIcon:
                    EventActorControlTargetIcon?.Invoke(this, (actorID, (uint)(p->param1 - _unkDelta)));
                    break;
                case Protocol.Server_ActorControlCategory.Tether:
                    EventActorControlTether?.Invoke(this, (actorID, p->param3, p->param2));
                    break;
                case Protocol.Server_ActorControlCategory.TetherCancel:
                    EventActorControlTetherCancel?.Invoke(this, actorID);
                    break;
                case Protocol.Server_ActorControlCategory.EObjSetState:
                    // p2 is unused (seems to be director id?), p3==1 means housing (?) item instead of event obj, p4 is housing item id
                    EventActorControlEObjSetState?.Invoke(this, (actorID, (ushort)p->param1));
                    break;
                case Protocol.Server_ActorControlCategory.EObjAnimation:
                    EventActorControlEObjAnimation?.Invoke(this, (actorID, (ushort)p->param1, (ushort)p->param2));
                    break;
                case Protocol.Server_ActorControlCategory.PlayActionTimeline:
                    EventActorControlPlayActionTimeline?.Invoke(this, (actorID, (ushort)p->param1));
                    break;
            }
        }

        private unsafe void HandleActorControlSelf(Protocol.Server_ActorControlSelf* p, uint actorID)
        {
            switch (p->category)
            {
                case Protocol.Server_ActorControlCategory.ActionRejected:
                    EventActorControlSelfActionRejected?.Invoke(this, new ClientActionReject() { Action = new((ActionType)p->param2, p->param3), SourceSequence = p->param6, RecastElapsed = p->param4 * 0.01f, RecastTotal = p->param5 * 0.01f, LogMessageID = p->param1 });
                    break;
                case Protocol.Server_ActorControlCategory.DirectorUpdate:
                    EventActorControlSelfDirectorUpdate?.Invoke(this, (p->param1, p->param2, p->param3, p->param4, p->param5, p->param6));
                    break;
            }
        }

        private unsafe void HandleEnvControl(Protocol.Server_EnvControl* p, uint actorID)
        {
            EventEnvControl?.Invoke(this, (p->FeatureID, p->Index, p->State));
        }

        private unsafe void HandleWaymark(Protocol.Server_Waymark* p)
        {
            if (p->Waymark < Waymark.Count)
                EventWaymark?.Invoke(this, (p->Waymark, p->Active != 0 ? new Vector3(p->PosX / 1000.0f, p->PosY / 1000.0f, p->PosZ / 1000.0f) : null));
        }

        private unsafe void HandleWaymarkPreset(Protocol.Server_WaymarkPreset* p)
        {
            byte mask = 1;
            for (var i = Waymark.A; i < Waymark.Count; ++i)
            {
                EventWaymark?.Invoke(this, (i, (p->WaymarkMask & mask) != 0 ? new Vector3(p->PosX[(byte)i] / 1000.0f, p->PosY[(byte)i] / 1000.0f, p->PosZ[(byte)i] / 1000.0f) : null));
                mask <<= 1;
            }
        }

        private unsafe void HandleRSVData(string key, string value)
        {
            EventRSVData?.Invoke(this, (key, value));
        }

        private unsafe void DumpClientMessage(IntPtr dataPtr, ushort opCode)
        {
            //Service.Log($"[Network] Client message {(Protocol.Opcode)opCode}");
            //switch ((Protocol.Opcode)opCode)
            //{
            //    case Protocol.Opcode.ActionRequest:
            //        {
            //            var p = (Protocol.Client_ActionRequest*)dataPtr;
            //            Service.Log($"[Network] - AID={new ActionID(p->Type, p->ActionID)}, proc={p->ActionProcState}, target={Utils.ObjectString(p->TargetID)}, seq={p->Sequence}, itemsrc={p->ItemSourceContainer}:{p->ItemSourceSlot}, casterrot={(p->IntCasterRot * 2 * MathF.PI / 65535 - MathF.PI).Radians()}, dirtotarget={(p->IntDirToTarget * 2 * MathF.PI / 65535 - MathF.PI).Radians()}, u={p->u1:X4} {p->u3:X4} {p->u4:X8} {p->u5:X16}");
            //            break;
            //        }
            //    case Protocol.Opcode.ActionRequestGroundTargeted:
            //        {
            //            var p = (Protocol.Client_ActionRequestGroundTargeted*)dataPtr;
            //            Service.Log($"[Network] - AID={new ActionID(p->Type, p->ActionID)}, proc={p->ActionProcState}, target={Utils.Vec3String(new(p->LocX, p->LocY, p->LocZ))}, seq={p->Sequence}, casterrot={(p->IntCasterRot * 2 * MathF.PI / 65535 - MathF.PI).Radians()}, dirtotarget={(p->IntDirToTarget * 2 * MathF.PI / 65535 - MathF.PI).Radians()}, u={p->u1:X4} {p->u3:X4} {p->u4:X8} {p->u5:X16}");
            //            break;
            //        }
            //}
        }

        private unsafe void DumpServerMessage(IntPtr dataPtr, ushort opCode, uint targetActorId)
        {
            var header = (Protocol.Server_IPCHeader*)(dataPtr - 0x10);
            Service.Log($"[Network] Server message {(Protocol.Opcode)opCode} -> {Utils.ObjectString(targetActorId)} (seq={header->Epoch}): {((ulong*)dataPtr)[0]:X16} {((ulong*)dataPtr)[1]:X16}...");
            switch ((Protocol.Opcode)opCode)
            {
                case Protocol.Opcode.ActionEffect1:
                    {
                        var p = (Protocol.Server_ActionEffect1*)dataPtr;
                        DumpActionEffect(&p->Header, (ActionEffect*)p->Effects, p->TargetID, 1, new());
                        break;
                    }
                case Protocol.Opcode.ActionEffect8:
                    {
                        var p = (Protocol.Server_ActionEffect8*)dataPtr;
                        DumpActionEffect(&p->Header, (ActionEffect*)p->Effects, p->TargetID, 8, IntToFloatCoords(p->TargetX, p->TargetY, p->TargetZ));
                        break;
                    }
                case Protocol.Opcode.ActionEffect16:
                    {
                        var p = (Protocol.Server_ActionEffect16*)dataPtr;
                        DumpActionEffect(&p->Header, (ActionEffect*)p->Effects, p->TargetID, 16, IntToFloatCoords(p->TargetX, p->TargetY, p->TargetZ));
                        break;
                    }
                case Protocol.Opcode.ActionEffect24:
                    {
                        var p = (Protocol.Server_ActionEffect24*)dataPtr;
                        DumpActionEffect(&p->Header, (ActionEffect*)p->Effects, p->TargetID, 24, IntToFloatCoords(p->TargetX, p->TargetY, p->TargetZ));
                        break;
                    }
                case Protocol.Opcode.ActionEffect32:
                    {
                        var p = (Protocol.Server_ActionEffect32*)dataPtr;
                        DumpActionEffect(&p->Header, (ActionEffect*)p->Effects, p->TargetID, 32, IntToFloatCoords(p->TargetX, p->TargetY, p->TargetZ));
                        break;
                    }
                case Protocol.Opcode.ActorCast:
                    {
                        var p = (Protocol.Server_ActorCast*)dataPtr;
                        uint aid = (uint)(p->ActionID - _unkDelta);
                        Service.Log($"[Network] - AID={new ActionID(p->ActionType, aid)} ({new ActionID(ActionType.Spell, p->SpellID)}), target={Utils.ObjectString(p->TargetID)}, time={p->CastTime:f2} ({p->BaseCastTime100ms * 0.1f:f1}), rot={IntToFloatAngle(p->Rotation)}, targetpos={Utils.Vec3String(IntToFloatCoords(p->PosX, p->PosY, p->PosZ))}, interruptible={p->Interruptible}, u1={p->u1:X2}, u2={Utils.ObjectString(p->u2_objID)}, u3={p->u3:X4}");
                        break;
                    }
                case Protocol.Opcode.ActorControl:
                    {
                        var p = (Protocol.Server_ActorControl*)dataPtr;
                        Service.Log($"[Network] - cat={p->category}, params={p->param1:X8} {p->param2:X8} {p->param3:X8} {p->param4:X8} {p->param5:X8}, unk={p->unk0:X4}");
                        switch (p->category)
                        {
                            case Protocol.Server_ActorControlCategory.CancelCast: // note: some successful boss casts have this message on completion, seen param1=param4=0, param2=1; param1 is related to cast time?..
                                Service.Log($"[Network] -- cancelled {new ActionID((ActionType)p->param2, p->param3)}, interrupted={p->param4 == 1}");
                                break;
                            case Protocol.Server_ActorControlCategory.GainEffect: // gain status effect, seen param3=param4=0
                                Service.Log($"[Network] -- gained {Utils.StatusString(p->param1)}, extra={p->param2:X4}");
                                break;
                            case Protocol.Server_ActorControlCategory.LoseEffect: // lose status effect, seen param2=param4=0, param3=invalid-oid
                                Service.Log($"[Network] -- lost {Utils.StatusString(p->param1)}");
                                break;
                        }
                        break;
                    }
                case Protocol.Opcode.ActorControlSelf:
                    {
                        var p = (Protocol.Server_ActorControlSelf*)dataPtr;
                        Service.Log($"[Network] - cat={p->category}, params={p->param1:X8} {p->param2:X8} {p->param3:X8} {p->param4:X8} {p->param5:X8} {p->param6:X8} {p->param7:X8}, unk={p->unk0:X4}");
                        switch (p->category)
                        {
                            case Protocol.Server_ActorControlCategory.RecastDetails:
                                Service.Log($"[Network] -- group={p->param1}, elapsed={p->param2 / 100.0f:f2}s, total={p->param3 / 100.0f:f2}s");
                                break;
                            case Protocol.Server_ActorControlCategory.Cooldown:
                                Service.Log($"[Network] -- group={p->param1}, action={new ActionID(ActionType.Spell, p->param2)}, time={p->param3 / 100.0f:f2}s");
                                break;
                            case Protocol.Server_ActorControlCategory.IncrementRecast:
                                Service.Log($"[Network] -- group={p->param1}, time={p->param2 / 100.0f:f2}s");
                                break;
                        }
                        break;
                    }
                case Protocol.Opcode.ActorControlTarget:
                    {
                        var p = (Protocol.Server_ActorControlTarget*)dataPtr;
                        Service.Log($"[Network] - cat={p->category}, target={Utils.ObjectString(p->TargetID)}, params={p->param1:X8} {p->param2:X8} {p->param3:X8} {p->param4:X8} {p->param5:X8}, unk={p->unk0:X4} {p->unk1:X8}");
                        break;
                    }
                case Protocol.Opcode.ActorGauge:
                    {
                        var p = (Protocol.Server_ActorGauge*)dataPtr;
                        Service.Log($"[Network] - {p->ClassJobID} = {p->Payload:X16}, u={p->u5:X2} {p->u6:X4} {p->u8:X16}");
                        break;
                    }
                //case Protocol.Opcode.ActorMove:
                //    {
                //        var p = (Protocol.Server_ActorMove*)dataPtr;
                //        Service.Log($"[Network] - {Utils.Vec3String(IntToFloatCoords(p->X, p->Y, p->Z))}, {IntToFloatAngle(p->Rotation)}, anim={p->AnimationFlags:X4}/{p->AnimationSpeed}, u={p->UnknownRotation:X2} {p->Unknown:X8}");
                //        break;
                //    }
                case Protocol.Opcode.EffectResult1:
                    DumpEffectResult(Math.Min((byte)1, *(byte*)dataPtr), (Protocol.Server_EffectResultEntry*)(dataPtr + 4));
                    break;
                case Protocol.Opcode.EffectResult4:
                    DumpEffectResult(Math.Min((byte)4, *(byte*)dataPtr), (Protocol.Server_EffectResultEntry*)(dataPtr + 4));
                    break;
                case Protocol.Opcode.EffectResult8:
                    DumpEffectResult(Math.Min((byte)8, *(byte*)dataPtr), (Protocol.Server_EffectResultEntry*)(dataPtr + 4));
                    break;
                case Protocol.Opcode.EffectResult16:
                    DumpEffectResult(Math.Min((byte)16, *(byte*)dataPtr), (Protocol.Server_EffectResultEntry*)(dataPtr + 4));
                    break;
                case Protocol.Opcode.EffectResultBasic1:
                    DumpEffectResultBasic(Math.Min((byte)1, *(byte*)dataPtr), (Protocol.Server_EffectResultBasicEntry*)(dataPtr + 4));
                    break;
                case Protocol.Opcode.EffectResultBasic4:
                    DumpEffectResultBasic(Math.Min((byte)4, *(byte*)dataPtr), (Protocol.Server_EffectResultBasicEntry*)(dataPtr + 4));
                    break;
                case Protocol.Opcode.EffectResultBasic8:
                    DumpEffectResultBasic(Math.Min((byte)8, *(byte*)dataPtr), (Protocol.Server_EffectResultBasicEntry*)(dataPtr + 4));
                    break;
                case Protocol.Opcode.EffectResultBasic16:
                    DumpEffectResultBasic(Math.Min((byte)16, *(byte*)dataPtr), (Protocol.Server_EffectResultBasicEntry*)(dataPtr + 4));
                    break;
                case Protocol.Opcode.EffectResultBasic32:
                    DumpEffectResultBasic(Math.Min((byte)32, *(byte*)dataPtr), (Protocol.Server_EffectResultBasicEntry*)(dataPtr + 4));
                    break;
                case Protocol.Opcode.EffectResultBasic64:
                    DumpEffectResultBasic(Math.Min((byte)64, *(byte*)dataPtr), (Protocol.Server_EffectResultBasicEntry*)(dataPtr + 4));
                    break;
                case Protocol.Opcode.Waymark:
                    {
                        var p = (Protocol.Server_Waymark*)dataPtr;
                        Service.Log($"[Network] - {p->Waymark}: {p->Active} at {p->PosX / 1000.0f:f3} {p->PosY / 1000.0f:f3} {p->PosZ / 1000.0f:f3}");
                        break;
                    }
                case Protocol.Opcode.WaymarkPreset:
                    {
                        var p = (Protocol.Server_WaymarkPreset*)dataPtr;
                        for (int i = 0; i < 8; ++i)
                        {
                            Service.Log($"[Network] - {(Waymark)i}: {(p->WaymarkMask & (1 << i)) != 0} at {p->PosX[i] / 1000.0f:f3} {p->PosY[i] / 1000.0f:f3} {p->PosZ[i] / 1000.0f:f3}");
                        }
                        break;
                    }
                case Protocol.Opcode.EnvControl:
                    {
                        var p = (Protocol.Server_EnvControl*)dataPtr;
                        Service.Log($"[Network] - {p->FeatureID:X8}.{p->Index:X2}: {p->State:X8}, u={p->u0:X2} {p->u1:X4} {p->u2:X8}");
                        break;
                    }
                case Protocol.Opcode.UpdateRecastTimes:
                    {
                        var p = (Protocol.Server_UpdateRecastTimes*)dataPtr;
                        Service.Log($"[Network] - {p->Elapsed[0]:f1}/{p->Total[0]:f1}, ..., {p->Elapsed[21]:f1}/{p->Total[21]:f1}");
                        break;
                    }
                case Protocol.Opcode.UpdateHate:
                case Protocol.Opcode.UpdateHater:
                    {
                        ulong* p = (ulong*)(dataPtr + 4);
                        Service.Log($"[Network] - {*(byte*)dataPtr} entries: [{*(uint*)p:X}={*((byte*)p+4)}, ...]");
                        break;
                    }
                case Protocol.Opcode.Countdown:
                    {
                        void* p = (void*)dataPtr;
                        uint senderID = Utils.ReadField<uint>(p, 0);
                        ushort time = Utils.ReadField<ushort>(p, 6);
                        var text = MemoryHelper.ReadStringNullTerminated(dataPtr + 11);
                        Service.Log($"[Network] - {time}s from {Utils.ObjectString(senderID)} {(Utils.ReadField<byte>(p, 8) != 0 ? "fail-in-combat" : "")} '{text}' {Utils.ReadField<ushort>(p, 4):X4} {Utils.ReadField<byte>(p, 9):X2} {Utils.ReadField<byte>(p, 10):X2}");
                        break;
                    }
                case Protocol.Opcode.CountdownCancel:
                    {
                        void* p = (void*)dataPtr;
                        uint senderID = Utils.ReadField<uint>(p, 0);
                        var text = MemoryHelper.ReadStringNullTerminated(dataPtr + 8);
                        Service.Log($"[Network] - from {Utils.ObjectString(senderID)} '{text}' {Utils.ReadField<ushort>(p, 4):X4}");
                        break;
                    }
                case Protocol.Opcode.RSVData:
                    {
                        int valueLen = *(int*)dataPtr;
                        var key = MemoryHelper.ReadStringNullTerminated(dataPtr + 4);
                        var value = MemoryHelper.ReadString(dataPtr + 0x34, valueLen);
                        Service.Log($"[Network] - {key} = {value} [{valueLen}]");
                        break;
                    }
            }
        }

        private unsafe void DumpActionEffect(Protocol.Server_ActionEffectHeader* data, ActionEffect* effects, ulong* targetIDs, uint maxTargets, Vector3 targetPos)
        {
            // rotation: 0 -> -180, 65535 -> +180
            var rot = IntToFloatAngle(data->rotation);
            uint aid = (uint)(data->actionId - _unkDelta);
            Service.Log($"[Network] - AID={new ActionID(data->actionType, aid)} (real={data->actionId}, anim={data->actionAnimationId}), animTarget={Utils.ObjectString(data->animationTargetId)}, animLock={data->animationLockTime:f2}, seq={data->SourceSequence}, cntr={data->globalEffectCounter}, rot={rot}, pos={Utils.Vec3String(targetPos)}, var={data->variation}, someTarget={Utils.ObjectString(data->SomeTargetID)}, u={data->unknown20:X2} {data->padding21:X4}");
            var targets = Math.Min(data->NumTargets, maxTargets);
            for (int i = 0; i < targets; ++i)
            {
                ulong targetId = targetIDs[i];
                if (targetId == 0)
                    continue;

                Service.Log($"[Network] -- target {i} == {Utils.ObjectString(targetId)}");
                for (int j = 0; j < 8; ++j)
                {
                    ActionEffect* eff = effects + (i * 8) + j;
                    if (eff->Type == ActionEffectType.Nothing)
                        continue;

                    Service.Log($"[Network] --- effect {j} == {eff->Type}, params={eff->Param0:X2} {eff->Param1:X2} {eff->Param2:X2} {eff->Param3:X2} {eff->Param4:X2} {eff->Value:X4}");
                }
            }
        }

        private unsafe void DumpEffectResult(int count, Protocol.Server_EffectResultEntry* entries)
        {
            var p = entries;
            for (int i = 0; i < count; ++i)
            {
                Service.Log($"[Network] - [{i}] seq={p->RelatedActionSequence}/{p->RelatedTargetIndex}, actor={Utils.ObjectString(p->ActorID)}, hp={p->CurrentHP}/{p->MaxHP}, class={p->ClassJob} mp={p->CurrentMP}, shield={p->DamageShield}");
                var cnt = Math.Min(4, (int)p->EffectCount);
                var eff = (Protocol.Server_EffectResultEffectEntry*)p->Effects;
                for (int j = 0; j < cnt; ++j)
                {
                    Service.Log($"[Network] -- eff #{eff->EffectIndex}: id={Utils.StatusString(eff->EffectID)}, extra={eff->Extra:X2}, dur={eff->Duration:f2}, src={Utils.ObjectString(eff->SourceActorID)}, pad={eff->padding1:X2} {eff->padding2:X4}");
                    ++eff;
                }
                ++p;
            }
        }

        private unsafe void DumpEffectResultBasic(int count, Protocol.Server_EffectResultBasicEntry* entries)
        {
            var p = entries;
            for (int i = 0; i < count; ++i)
                Service.Log($"[Network] - [{i}] seq={p->RelatedActionSequence}/{p->RelatedTargetIndex}, actor={Utils.ObjectString(p->ActorID)}, hp={p->CurrentHP}");
        }

        private static Vector3 IntToFloatCoords(ushort x, ushort y, ushort z)
        {
            float fx = x * (2000.0f / 65535) - 1000;
            float fy = y * (2000.0f / 65535) - 1000;
            float fz = z * (2000.0f / 65535) - 1000;
            return new(fx, fy, fz);
        }

        private static Angle IntToFloatAngle(ushort rot)
        {
            return (rot / 65535.0f * (2 * MathF.PI) - MathF.PI).Radians();
        }
    }
}
