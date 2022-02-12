using Dalamud.Game.Network;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    class Network : IDisposable
    {
        public struct PendingAction
        {
            public ActionID Action;
            public uint TargetID;
            public uint Sequence;
        }

        public event EventHandler<WorldState.CastResult>? EventActionEffect;
        public event EventHandler<(uint actorID, uint actionID)>? EventActorControlCancelCast;
        public event EventHandler<(uint actorID, uint iconID)>? EventActorControlTargetIcon;
        public event EventHandler<(uint actorID, uint targetID, uint tetherID)>? EventActorControlTether;
        public event EventHandler<uint>? EventActorControlTetherCancel;
        public event EventHandler<(uint actorID, uint actionID, uint sourceSequence)>? EventActorControlSelfActionRejected;
        public event EventHandler<(uint featureID, byte index, uint state)>? EventEnvControl;
        public event EventHandler<(WorldState.Waymark waymark, Vector3? pos)>? EventWaymark;
        public event EventHandler<PendingAction>? EventActionRequest;

        private GeneralConfig _config;

        // this is a mega weird thing - apparently some IDs sent over network have some extra delta added to them (e.g. action ids, icon ids, etc.)
        // they change on relogs or zone changes or something...
        // we have one simple way of detecting them - by looking at casts, since they contain both offset id and real ('animation') id
        private int _unkDelta = 0;

        public Network(GeneralConfig config)
        {
            _config = config;
            Service.GameNetwork.NetworkMessage += HandleMessage;
        }

        public void Dispose()
        {
            Service.GameNetwork.NetworkMessage -= HandleMessage;
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
                    case Protocol.Opcode.ActorControl:
                        HandleActorControl((Protocol.Server_ActorControl*)dataPtr, targetActorId);
                        break;
                    case Protocol.Opcode.ActorControlSelf:
                        HandleActorControlSelf((Protocol.Server_ActorControlSelf*)dataPtr, targetActorId);
                        break;
                    case Protocol.Opcode.EnvironmentControl:
                        HandleEnvironmentControl((Protocol.Server_EnvironmentControl*)dataPtr, targetActorId);
                        break;
                    case Protocol.Opcode.Waymark:
                        HandleWaymark((Protocol.Server_Waymark*)dataPtr);
                        break;
                    case Protocol.Opcode.PresetWaymark:
                        HandlePresetWaymark((Protocol.Server_PresetWaymark*)dataPtr);
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

                switch ((Protocol.Opcode)opCode)
                {
                    case Protocol.Opcode.ActionRequest:
                        HandleActionRequest((Protocol.Client_ActionRequest*)dataPtr);
                        break;
                }
            }
        }

        private unsafe void HandleActionEffect1(Protocol.Server_ActionEffect1* p, uint actorID)
        {
            HandleActionEffect(actorID, & p->Header, (Protocol.Server_ActionEffect_EffectEntry*)p->Effects, p->TargetID, 1);
        }

        private unsafe void HandleActionEffect8(Protocol.Server_ActionEffect8* p, uint actorID)
        {
            HandleActionEffect(actorID, &p->Header, (Protocol.Server_ActionEffect_EffectEntry*)p->Effects, p->TargetID, 8);
        }

        private unsafe void HandleActionEffect16(Protocol.Server_ActionEffect16* p, uint actorID)
        {
            HandleActionEffect(actorID, &p->Header, (Protocol.Server_ActionEffect_EffectEntry*)p->Effects, p->TargetID, 16);
        }

        private unsafe void HandleActionEffect24(Protocol.Server_ActionEffect24* p, uint actorID)
        {
            HandleActionEffect(actorID, &p->Header, (Protocol.Server_ActionEffect_EffectEntry*)p->Effects, p->TargetID, 24);
        }

        private unsafe void HandleActionEffect32(Protocol.Server_ActionEffect32* p, uint actorID)
        {
            HandleActionEffect(actorID, &p->Header, (Protocol.Server_ActionEffect_EffectEntry*)p->Effects, p->TargetID, 32);
        }

        private unsafe void HandleActionEffect(uint casterID, Protocol.Server_ActionEffectHeader* header, Protocol.Server_ActionEffect_EffectEntry* effects, ulong* targetIDs, uint maxTargets)
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

            var info = new WorldState.CastResult
            {
                CasterID = casterID,
                MainTargetID = header->animationTargetId,
                Action = new(header->actionType, (uint)(header->actionId - _unkDelta)), // note: see _unkDelta comment
                AnimationLockTime = header->animationLockTime,
                MaxTargets = maxTargets,
                SourceSequence = header->SourceSequence
            };

            var targets = Math.Min(header->NumTargets, maxTargets);
            for (int i = 0; i < targets; ++i)
            {
                uint targetID = (uint)targetIDs[i];
                if (targetID != 0)
                {
                    var target = new WorldState.CastResult.Target();
                    target.ID = targetID;
                    for (int j = 0; j < 8; ++j)
                        target.Effects[j] = *(ulong*)(effects + (i * 8) + j);
                    info.Targets.Add(target);
                }
            }

            EventActionEffect?.Invoke(this, info);
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
            }
        }

        private unsafe void HandleActorControlSelf(Protocol.Server_ActorControlSelf* p, uint actorID)
        {
            switch (p->category)
            {
                case Protocol.Server_ActorControlCategory.ActionRejected:
                    EventActorControlSelfActionRejected?.Invoke(this, (actorID, p->param3, p->param6));
                    break;
            }
        }

        private unsafe void HandleEnvironmentControl(Protocol.Server_EnvironmentControl* p, uint actorID)
        {
            EventEnvControl?.Invoke(this, (p->FeatureID, p->Index, p->State));
        }

        private unsafe void HandleWaymark(Protocol.Server_Waymark* p)
        {
            if (p->Waymark < WorldState.Waymark.Count)
                EventWaymark?.Invoke(this, (p->Waymark, p->Active != 0 ? new Vector3(p->PosX / 1000.0f, p->PosY / 1000.0f, p->PosZ / 1000.0f) : null));
        }

        private unsafe void HandlePresetWaymark(Protocol.Server_PresetWaymark* p)
        {
            byte mask = 1;
            for (var i = WorldState.Waymark.A; i < WorldState.Waymark.Count; ++i)
            {
                EventWaymark?.Invoke(this, (i, (p->WaymarkMask & mask) != 0 ? new Vector3(p->PosX[(byte)i] / 1000.0f, p->PosY[(byte)i] / 1000.0f, p->PosZ[(byte)i] / 1000.0f) : null));
                mask <<= 1;
            }
        }

        private unsafe void HandleActionRequest(Protocol.Client_ActionRequest* p)
        {
            EventActionRequest?.Invoke(this, new() { Action = new(p->Type, p->ActionID), TargetID = p->TargetID, Sequence = p->Sequence });
        }

        private unsafe void DumpClientMessage(IntPtr dataPtr, ushort opCode)
        {
            Service.Log($"[Network] Client message {(Protocol.Opcode)opCode}");
            switch ((Protocol.Opcode)opCode)
            {
                case Protocol.Opcode.ActionRequest:
                    {
                        var p = (Protocol.Client_ActionRequest*)dataPtr;
                        Service.Log($"[Network] - AID={new ActionID(p->Type, p->ActionID)}, target={Utils.ObjectString(p->TargetID)}, seq={p->Sequence}, itemsrc={p->ItemSourceContainer}:{p->ItemSourceSlot}, u={p->u0:X2} {p->u1:X4} {p->u2:X4} {p->u3:X8} {p->u4:X8} {p->u5:X8}");
                        break;
                    }
            }
        }

        private unsafe void DumpServerMessage(IntPtr dataPtr, ushort opCode, uint targetActorId)
        {
            var header = (Protocol.Server_IPCHeader*)(dataPtr - 0x10);
            Service.Log($"[Network] Server message {(Protocol.Opcode)opCode} -> {Utils.ObjectString(targetActorId)} (seq={header->Epoch})");
            switch ((Protocol.Opcode)opCode)
            {
                case Protocol.Opcode.ActionEffect1:
                    {
                        var p = (Protocol.Server_ActionEffect1*)dataPtr;
                        DumpActionEffect(&p->Header, (Protocol.Server_ActionEffect_EffectEntry*)p->Effects, p->TargetID, 1, 0, 0);
                        break;
                    }
                case Protocol.Opcode.ActionEffect8:
                    {
                        var p = (Protocol.Server_ActionEffect8*)dataPtr;
                        DumpActionEffect(&p->Header, (Protocol.Server_ActionEffect_EffectEntry*)p->Effects, p->TargetID, 8, p->effectflags1, p->effectflags2);
                        break;
                    }
                case Protocol.Opcode.ActionEffect16:
                    {
                        var p = (Protocol.Server_ActionEffect16*)dataPtr;
                        DumpActionEffect(&p->Header, (Protocol.Server_ActionEffect_EffectEntry*)p->Effects, p->TargetID, 16, p->effectflags1, p->effectflags2);
                        break;
                    }
                case Protocol.Opcode.ActionEffect24:
                    {
                        var p = (Protocol.Server_ActionEffect24*)dataPtr;
                        DumpActionEffect(&p->Header, (Protocol.Server_ActionEffect_EffectEntry*)p->Effects, p->TargetID, 24, p->effectflags1, p->effectflags2);
                        break;
                    }
                case Protocol.Opcode.ActionEffect32:
                    {
                        var p = (Protocol.Server_ActionEffect32*)dataPtr;
                        DumpActionEffect(&p->Header, (Protocol.Server_ActionEffect_EffectEntry*)p->Effects, p->TargetID, 32, p->effectflags1, p->effectflags2);
                        break;
                    }
                case Protocol.Opcode.ActorCast:
                    {
                        var p = (Protocol.Server_ActorCast*)dataPtr;
                        Service.Log($"[Network] - AID={new ActionID(p->SkillType, p->ActionID)}, target={Utils.ObjectString(p->TargetID)}, time={p->CastTime:f2}, rot={p->Rotation:f2}, x={p->PosX}, y={p->PosY}, z={p->PosZ}, u={p->Unknown:X2}, u1={new ActionID(ActionType.Spell, p->Unknown1)}, u2={Utils.ObjectString(p->Unknown2)}, u3={p->Unknown3:X4}");
                        break;
                    }
                case Protocol.Opcode.ActorControl:
                    {
                        var p = (Protocol.Server_ActorControl*)dataPtr;
                        Service.Log($"[Network] - cat={p->category}, params={p->param1:X8} {p->param2:X8} {p->param3:X8} {p->param4:X8} {p->param5:X8}, unk={p->unk0:X4}");
                        switch (p->category)
                        {
                            case Protocol.Server_ActorControlCategory.CancelCast: // note: some successful boss casts have this message on completion, seen param1=param4=0, param2=1; param1 is related to cast time?..
                                Service.Log($"[Network] -- cancelled {new ActionID(ActionType.Spell, p->param3)}, interrupted={p->param4 == 1}");
                                break;
                            case Protocol.Server_ActorControlCategory.GainEffect: // gain status effect, seen param2=param3=param4=0
                                Service.Log($"[Network] -- gained {Utils.StatusString(p->param1)}");
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
                            case Protocol.Server_ActorControlCategory.Cooldown:
                                Service.Log($"[Network] -- group={p->param1}, action={new ActionID(ActionType.Spell, p->param2)}, time={p->param3 / 100.0f:f2}s");
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
                        Service.Log($"[Network] - params={p->param1:X8} {p->param2:X8} {p->param3:X8} {p->param4:X8}");
                        break;
                    }
                case Protocol.Opcode.EffectResult:
                    {
                        var p = (Protocol.Server_EffectResult*)dataPtr;
                        Service.Log($"[Network] - seq={p->RelatedActionSequence}, actor={Utils.ObjectString(p->ActorID)}, hp={p->CurrentHP}/{p->MaxHP}, mp={p->CurrentMP}, shield={p->DamageShield}, u={p->Unknown1:X8} {p->Unknown3:X4} {p->Unknown6:X4}");
                        var cnt = Math.Min(4, (int)p->EffectCount);
                        for (int i = 0; i < cnt; ++i)
                        {
                            var eff = ((Protocol.Server_EffectResultEntry*)p->Effects) + i;
                            Service.Log($"[Network] -- idx={eff->EffectIndex}, id={Utils.StatusString(eff->EffectID)}, dur={eff->duration:f2}, src={Utils.ObjectString(eff->SourceActorID)}, u={eff->unknown1:X2} {eff->unknown2:X4} {eff->unknown3:X4}");
                        }
                        break;
                    }
                case Protocol.Opcode.EffectResultBasic:
                    {
                        var p = (Protocol.Server_EffectResultBasic*)dataPtr;
                        Service.Log($"[Network] - seq={p->RelatedActionSequence}, actor={Utils.ObjectString(p->ActorID)}, hp={p->CurrentHP}, u={p->Unknown1:X8} {p->Unknown2:X8} {p->Unknown3:X4} {p->Unknown4:X4}");
                        break;
                    }
                case Protocol.Opcode.Waymark:
                    {
                        var p = (Protocol.Server_Waymark*)dataPtr;
                        Service.Log($"[Network] - {p->Waymark}: {p->Active} at {p->PosX / 1000.0f:f3} {p->PosY / 1000.0f:f3} {p->PosZ / 1000.0f:f3}");
                        break;
                    }
                case Protocol.Opcode.PresetWaymark:
                    {
                        var p = (Protocol.Server_PresetWaymark*)dataPtr;
                        for (int i = 0; i < 8; ++i)
                        {
                            Service.Log($"[Network] - {(WorldState.Waymark)i}: {(p->WaymarkMask & (1 << i)) != 0} at {p->PosX[i] / 1000.0f:f3} {p->PosY[i] / 1000.0f:f3} {p->PosZ[i] / 1000.0f:f3}");
                        }
                        break;
                    }
                case Protocol.Opcode.EnvironmentControl:
                    {
                        var p = (Protocol.Server_EnvironmentControl*)dataPtr;
                        Service.Log($"[Network] - {p->FeatureID:X8}.{p->Index:X2}: {p->State:X8}, u={p->u0:X2} {p->u1:X4} {p->u2:X8}");
                        break;
                    }
            }
        }

        private unsafe void DumpActionEffect(Protocol.Server_ActionEffectHeader* data, Protocol.Server_ActionEffect_EffectEntry* effects, ulong* targetIDs, uint maxTargets, uint flags1, ushort flags2)
        {
            // rotation: 0 -> -180, 65535 -> +180
            float rot = (data->rotation / 65535.0f * 360.0f) - 180.0f;
            uint aid = (uint)(data->actionId - _unkDelta);
            Service.Log($"[Network] - AID={new ActionID(data->actionType, aid)} (real={data->actionId}, anim={data->actionAnimationId}), animTarget={Utils.ObjectString(data->animationTargetId)}, animLock={data->animationLockTime:f2}, seq={data->SourceSequence}, cntr={data->globalEffectCounter}, rot={rot:f0}, var={data->variation}, flags={flags1:X8} {flags2:X4}, someTarget={Utils.ObjectString(data->SomeTargetID)}, u={data->unknown:X8} {data->unknown20:X2} {data->padding21:X4}");
            var targets = Math.Min(data->NumTargets, maxTargets);
            for (int i = 0; i < targets; ++i)
            {
                uint targetId = (uint)targetIDs[i];
                if (targetId == 0)
                    continue;

                Service.Log($"[Network] -- target {i} == {Utils.ObjectString(targetId)}, hiword = {targetIDs[i] >> 32:X8}");
                for (int j = 0; j < 8; ++j)
                {
                    Protocol.Server_ActionEffect_EffectEntry* eff = effects + (i * 8) + j;
                    if (eff->effectType == Protocol.Server_ActionEffectType.Nothing)
                        continue;

                    Service.Log($"[Network] --- effect {j} == {eff->effectType}, params={eff->hitSeverity:X2} {eff->param:X2} {eff->bonusPercent:X2} {eff->valueMultiplier:X2} {eff->flag:X2} {eff->value:X4}");
                }
            }
        }
    }
}
