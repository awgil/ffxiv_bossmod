using Dalamud;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace BossMod
{
    // extensions and utilities for interacting with game's ActionManager singleton
    // animation lock for instants: on request it is set to 0.5 (or 0.35 for some specific actions) and ticks down, on response updated to packet data (typically 0.6) and continues ticking down, so 'effective' lock is action lock + delay between request and response
    // animation lock for casts: on request is remains 0, on response updated to packet data (typically 0.1) and is 'frozen' until 'cast' ends - and only then starts ticking down (meaning that 'effective' lock is ~0.6 after action-effect or ~0.1 after visual cast end)
    // since for casts 'visual' cast countdown starts immediately on request and is not updated, there is no extra delay
    class ActionManagerEx : IDisposable
    {
        public static ActionManagerEx? Instance;
        public const int NumCooldownGroups = 80;

        public float AnimationLockDelaySmoothing = 0.8f; // TODO tweak
        public float AnimationLockDelayAverage { get; private set; } = 0.1f; // smoothed delay between client request and server response
        public float AnimationLockDelayMax = float.MaxValue; // this caps max delay a-la xivalexander
        public unsafe float AnimationLock => Utils.ReadField<float>(_inst, 8);

        public unsafe uint CastSpellID => Utils.ReadField<uint>(_inst, 0x24);
        public ActionID CastSpell => new(ActionType.Spell, CastActionID);
        public unsafe ActionType CastActionType => (ActionType)Utils.ReadField<uint>(_inst, 0x28);
        public unsafe uint CastActionID => Utils.ReadField<uint>(_inst, 0x2C);
        public ActionID CastAction => new(CastActionType, CastActionID);
        public unsafe float CastTimeElapsed => Utils.ReadField<float>(_inst, 0x30);
        public unsafe float CastTimeTotal => Utils.ReadField<float>(_inst, 0x34);
        public float CastTimeRemaining => CastSpellID != 0 ? CastTimeTotal - CastTimeElapsed : 0;
        public unsafe ulong CastTargetID => Utils.ReadField<ulong>(_inst, 0x38);
        public unsafe Vector3 CastTargetPos => Utils.ReadField<Vector3>(_inst, 0x40);

        public unsafe float ComboTimeLeft => Utils.ReadField<float>(_inst, 0x60);
        public unsafe uint ComboLastMove => Utils.ReadField<uint>(_inst, 0x64);

        public unsafe bool QueueActive => Utils.ReadField<bool>(_inst, 0x68);
        public unsafe ActionType QueueActionType => (ActionType)Utils.ReadField<uint>(_inst, 0x6C);
        public unsafe uint QueueActionID => Utils.ReadField<uint>(_inst, 0x70);
        public ActionID QueueAction => new(QueueActionType, QueueActionID);
        public unsafe ulong QueueTargetID => Utils.ReadField<ulong>(_inst, 0x78);
        public unsafe uint QueueCallType => Utils.ReadField<uint>(_inst, 0x80);
        public unsafe uint QueueComboRouteID => Utils.ReadField<uint>(_inst, 0x84);

        public unsafe uint GTActionID => Utils.ReadField<uint>(_inst, 0x88);
        public unsafe ActionType GTActionType => (ActionType)Utils.ReadField<uint>(_inst, 0x8C);
        public ActionID GTAction => new(GTActionType, GTActionID);
        public unsafe uint GTSpellID => Utils.ReadField<uint>(_inst, 0x90);
        public ActionID GTSpell => new(ActionType.Spell, GTSpellID);
        public unsafe uint GTUnkArg => Utils.ReadField<uint>(_inst, 0x94);
        public unsafe ulong GTUnkObj => Utils.ReadField<ulong>(_inst, 0x98);
        public unsafe byte GT_uA0 => Utils.ReadField<byte>(_inst, 0xA0);
        public unsafe byte GT_uB8 => Utils.ReadField<byte>(_inst, 0xB8);
        public unsafe uint GT_uBC => Utils.ReadField<byte>(_inst, 0xBC);

        public float EffectiveAnimationLock => AnimationLock + CastTimeRemaining; // animation lock starts ticking down only when cast ends
        public float EffectiveAnimationLockDelay => AnimationLockDelayMax <= 0.5f ? AnimationLockDelayMax : MathF.Min(AnimationLockDelayAverage, 0.1f); // this is a conservative estimate

        public event EventHandler? PostUpdate;

        private unsafe ActionManager* _inst;
        private float _lastReqInitialAnimLock;
        private ushort _lastReqSequence;

        private unsafe delegate bool GetGroundTargetPositionDelegate(ActionManager* self, Vector3* outPos);
        private GetGroundTargetPositionDelegate _getGroundTargetPositionFunc;

        private unsafe delegate void FaceTargetDelegate(ActionManager* self, Vector3* position, ulong targetID);
        private FaceTargetDelegate _faceTargetFunc;

        private unsafe delegate void UpdateDelegate(ActionManager* self);
        private Hook<UpdateDelegate> _updateHook;

        private unsafe delegate bool UseActionLocationDelegate(ActionManager* self, ActionType actionType, uint actionID, ulong targetID, Vector3* targetPos, uint itemLocation);
        private Hook<UseActionLocationDelegate> _useActionLocationHook;

        private unsafe delegate void ProcessActionEffectPacketDelegate(uint casterID, void* casterObj, Vector3* targetPos, Protocol.Server_ActionEffectHeader* header, ulong* effects, ulong* targets);
        private Hook<ProcessActionEffectPacketDelegate> _processActionEffectPacketHook;

        private IntPtr _gtQueuePatch; // instruction inside UseAction: conditional jump that disallows queueing for ground-targeted actions
        private bool _gtQueuePatchEnabled;
        public bool AllowGTQueueing
        {
            get => _gtQueuePatchEnabled;
            set
            {
                if (_gtQueuePatchEnabled != value)
                {
                    SafeMemory.WriteBytes(_gtQueuePatch, new byte[] { value ? (byte)0xEB : (byte)0x74 });
                    _gtQueuePatchEnabled = value;
                }
            }
        }

        public unsafe ActionManagerEx()
        {
            _inst = ActionManager.Instance();
            Service.Log($"[AMEx] ActionManager singleton address = 0x{(ulong)_inst:X}");

            var getGroundTargetPositionAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 44 8B 84 24 80 00 00 00 33 C0");
            Service.Log($"[AMEx] GetGroundTargetPosition address = 0x{getGroundTargetPositionAddress:X}");
            _getGroundTargetPositionFunc = Marshal.GetDelegateForFunctionPointer<GetGroundTargetPositionDelegate>(getGroundTargetPositionAddress);

            var faceTargetAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 81 FE FB 1C 00 00 74 ?? 81 FE 53 5F 00 00 74 ?? 81 FE 6F 73 00 00");
            Service.Log($"[AMEx] FaceTarget address = 0x{faceTargetAddress:X}");
            _faceTargetFunc = Marshal.GetDelegateForFunctionPointer<FaceTargetDelegate>(faceTargetAddress);

            var updateAddress = Service.SigScanner.ScanText("48 8B C4 48 89 58 20 57 48 81 EC 90 00 00 00 48 8B 3D ?? ?? ?? ?? 48 8B D9 48 85 FF 0F 84 ?? ?? ?? ?? 48 89 68 08 48 8B CF 48 89 70 10 4C 89 70 18 0F 29 70 E8 44 0F 29 48 B8 44 0F 29 50 A8");
            Service.Log($"[AMEx] Update address = 0x{updateAddress:X}");
            _updateHook = Hook<UpdateDelegate>.FromAddress(updateAddress, UpdateDetour);
            _updateHook.Enable();

            var useActionLocationAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 3C 01 0F 85 ?? ?? ?? ?? EB 46");
            Service.Log($"[AMEx] UseActionLocation address = 0x{useActionLocationAddress:X}");
            _useActionLocationHook = Hook<UseActionLocationDelegate>.FromAddress(useActionLocationAddress, UseActionLocationDetour);
            _useActionLocationHook.Enable();

            var processActionEffectPacketAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B 4C 24 68 48 33 CC E8 ?? ?? ?? ?? 4C 8D 5C 24 70 49 8B 5B 20 49 8B 73 28 49 8B E3 5F C3");
            Service.Log($"[AMEx] ProcessActionEffectPacket address = 0x{processActionEffectPacketAddress:X}");
            _processActionEffectPacketHook = Hook<ProcessActionEffectPacketDelegate>.FromAddress(processActionEffectPacketAddress, ProcessActionEffectPacketDetour);
            _processActionEffectPacketHook.Enable();

            _gtQueuePatch = Service.SigScanner.ScanModule("74 24 41 81 FE F5 0D 00 00");
            Service.Log($"[AMEx] GT queue check address = 0x{_gtQueuePatch:X}");
            AllowGTQueueing = true;
        }

        public void Dispose()
        {
            AllowGTQueueing = false;
            _processActionEffectPacketHook.Dispose();
            _useActionLocationHook.Dispose();
            _updateHook.Dispose();
        }

        public unsafe Vector3? GetWorldPosUnderCursor()
        {
            Vector3 res = new();
            return _getGroundTargetPositionFunc(_inst, &res) ? res : null;
        }

        public unsafe void FaceTarget(Vector3 position, ulong unkObjID = GameObject.InvalidGameObjectId)
        {
            _faceTargetFunc(_inst, &position, unkObjID);
        }
        public void FaceDirection(WDir direction)
        {
            var player = Service.ClientState.LocalPlayer;
            if (player != null)
                FaceTarget(player.Position + new Vector3(direction.X, 0, direction.Z));
        }

        public unsafe void GetCooldowns(float[] cooldowns)
        {
            var rg = _inst->GetRecastGroupDetail(0);
            for (int i = 0; i < NumCooldownGroups; ++i)
            {
                cooldowns[i] = rg->Total - rg->Elapsed;
                ++rg;
            }
        }

        public unsafe float GCD()
        {
            var gcd = _inst->GetRecastGroupDetail(CommonDefinitions.GCDGroup);
            return gcd->Total - gcd->Elapsed;
        }

        public unsafe uint GetAdjustedActionID(uint actionID) => _inst->GetAdjustedActionId(actionID);

        public unsafe uint GetActionStatus(ActionID action, ulong target, bool checkRecastActive = true, bool checkCastingActive = true, uint* outOptExtraInfo = null)
        {
            return _inst->GetActionStatus((FFXIVClientStructs.FFXIV.Client.Game.ActionType)action.Type, action.ID, (long)target, checkRecastActive, checkCastingActive, outOptExtraInfo);
        }

        public unsafe bool UseAction(ActionID action, ulong targetID, uint itemLocation, uint callType, uint comboRouteID, bool* outOptGTModeStarted)
        {
            return _inst->UseAction((FFXIVClientStructs.FFXIV.Client.Game.ActionType)action.Type, action.ID, (long)targetID, itemLocation, callType, comboRouteID, outOptGTModeStarted);
        }

        // skips queueing etc
        public unsafe bool UseActionRaw(ActionID action, ulong targetID = GameObject.InvalidGameObjectId, Vector3 targetPos = new(), uint itemLocation = 0)
        {
            return UseActionLocationDetour(_inst, action.Type, action.ID, targetID, &targetPos, itemLocation);
        }

        private unsafe void UpdateDetour(ActionManager* self)
        {
            _updateHook.Original(self);
            PostUpdate?.Invoke(this, EventArgs.Empty);
        }

        private unsafe bool UseActionLocationDetour(ActionManager* self, ActionType actionType, uint actionID, ulong targetID, Vector3* targetPos, uint itemLocation)
        {
            var prevSeq = Utils.ReadField<ushort>(self, 0x110);
            bool ret = _useActionLocationHook.Original(self, actionType, actionID, targetID, targetPos, itemLocation);
            var currSeq = Utils.ReadField<ushort>(self, 0x110);
            if (currSeq != prevSeq)
            {
                _lastReqInitialAnimLock = AnimationLock;
                _lastReqSequence = currSeq;
                Service.Log($"[AMEx] UAL #{currSeq} ({new ActionID(actionType, actionID)} @ {targetID:X} / {Utils.Vec3String(*targetPos)} {(ret ? "succeeded" : "failed?")}, ALock={_lastReqInitialAnimLock:f3}, CTR={CastTimeRemaining:f3}, GCD={GCD():f3}");
            }
            return ret;
        }

        private unsafe void ProcessActionEffectPacketDetour(uint casterID, void* casterObj, Vector3* targetPos, Protocol.Server_ActionEffectHeader* header, ulong* effects, ulong* targets)
        {
            var prevAnimLock = AnimationLock;
            _processActionEffectPacketHook.Original(casterID, casterObj, targetPos, header, effects, targets);
            var currAnimLock = AnimationLock;
            if (currAnimLock == prevAnimLock)
                return;

            if (_lastReqSequence != header->SourceSequence)
            {
                Service.Log($"[AMEx] Animation lock updated by action with unexpected sequence ID: {prevAnimLock:f3} -> {currAnimLock:f3}");
                return;
            }

            float originalDelay = _lastReqInitialAnimLock - prevAnimLock;
            float reduction = 0;
            if (_lastReqInitialAnimLock > 0)
            {
                float adjDelay = originalDelay;
                if (adjDelay > AnimationLockDelayMax)
                {
                    reduction = Math.Min(adjDelay - AnimationLockDelayMax, currAnimLock);
                    adjDelay -= reduction;
                    Utils.WriteField(_inst, 8, currAnimLock - reduction);
                }
                AnimationLockDelayAverage = adjDelay * (1 - AnimationLockDelaySmoothing) + AnimationLockDelayAverage * AnimationLockDelaySmoothing;
            }
            Service.Log($"[AMEx] AEP #{header->SourceSequence} {prevAnimLock:f3} -> ALock={currAnimLock:f3} (delayed by {originalDelay:f3}-{reduction:f3}), CTR={CastTimeRemaining:f3}, GCD={GCD():f3}");
        }
    }
}
