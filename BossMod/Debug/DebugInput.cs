using Dalamud.Game.ClientState.Keys;
using Dalamud.Hooking;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BossMod
{
    [StructLayout(LayoutKind.Explicit, Size = 0xB)]
    public unsafe struct InputDataKeybind
    {
        [FieldOffset(0)] public fixed ushort Bindings[5];
        [FieldOffset(10)] public byte Terminator;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x9C8)]
    public unsafe struct InputData
    {
        [FieldOffset(0)] public void** Vtbl;

        [FieldOffset(0x9AC)] public int KeybindCount;
        [FieldOffset(0x9B0)] public InputDataKeybind* Keybinds;
        [FieldOffset(0x9B8)] public fixed byte GamepadAxisRemap[7];
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x140)]
    public unsafe struct PlayerMoveControllerWalk
    {
        [FieldOffset(0x10)] public Vector3 MovementDir;
        [FieldOffset(0x58)] public float BaseMovementSpeed;
        [FieldOffset(0x90)] public float MovementDirRelToCharacterFacing;
        [FieldOffset(0x94)] public byte Forced;
        [FieldOffset(0xA0)] public Vector3 MovementDirWorld;
        [FieldOffset(0xB0)] public float RotationDir;
        [FieldOffset(0x110)] public uint MovementState;
        [FieldOffset(0x114)] public float MovementLeft;
        [FieldOffset(0x118)] public float MovementFwd;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0xB0)]
    public unsafe struct PlayerMoveControllerFly
    {
        [FieldOffset(0x66)] public byte IsFlying;
        [FieldOffset(0x9C)] public float AngularAscent;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct PlayerController
    {
        [FieldOffset(0x10)] public PlayerMoveControllerWalk MoveControllerWalk;
        [FieldOffset(0x150)] public PlayerMoveControllerFly MoveControllerFly;
        [FieldOffset(0x559)] public byte ControlMode;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x18)]
    public unsafe struct PlayerMoveControllerFlyInput
    {
        [FieldOffset(0x0)] public float Forward;
        [FieldOffset(0x4)] public float Left;
        [FieldOffset(0x8)] public float Up;
        [FieldOffset(0xC)] public float Turn;
        [FieldOffset(0x10)] public float u10;
        [FieldOffset(0x14)] public byte DirMode;
        [FieldOffset(0x15)] public byte HaveBackwardOrStrafe;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x2B0)]
    public unsafe struct CameraX
    {
        [FieldOffset(0x140)] public float InputDeltaH;
        [FieldOffset(0x144)] public float InputDeltaV;
    }

    unsafe class DebugInput : IDisposable
    {
        private delegate byte ConvertVirtualKeyDelegate(int vkCode);
        private ConvertVirtualKeyDelegate _convertVirtualKey;

        private delegate ref int GetRefValueDelegate(int vkCode);
        private GetRefValueDelegate _getKeyRef;

        private PlayerController* _playerController;

        private delegate void RMIWalkDelegate(PlayerMoveControllerWalk* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk);
        private Hook<RMIWalkDelegate> _rmiWalkHook;

        private delegate void RMIFlyDelegate(PlayerMoveControllerFly* self, PlayerMoveControllerFlyInput* result);
        private Hook<RMIFlyDelegate> _rmiFlyHook;

        private delegate void RMICameraACDDelegate(CameraX* self, int inputMode, float speedH, float speedV);
        private delegate void RMICameraBDelegate(CameraX* self, int inputMode, float speedH);
        private Hook<RMICameraACDDelegate> _rmiCameraAHook;
        private Hook<RMICameraBDelegate> _rmiCameraBHook;
        private Hook<RMICameraACDDelegate> _rmiCameraCHook;
        private Hook<RMICameraACDDelegate> _rmiCameraDHook;

        private UITree _tree = new();
        private WorldState _ws;
        private AI.AIController _navi;
        private Vector2 _dest;
        private Vector3 _prevPos;
        private bool _jump;
        private bool _gamepadAxisOverrideEnable;
        private float _gamepadAxisOverrideAngle;
        private int _gamepadButtonOverride = -1;
        private int _gamepadButtonValue;
        private bool _gamepadNavigate;
        private bool _pmcOverrideDirEnable;
        private float _pmcOverrideDir;
        private int _pmcOverrideUp;

        public DebugInput(Autorotation autorot)
        {
            _convertVirtualKey = Service.KeyState.GetType().GetMethod("ConvertVirtualKey", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<ConvertVirtualKeyDelegate>(Service.KeyState);
            _getKeyRef = Service.KeyState.GetType().GetMethod("GetRefValue", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<GetRefValueDelegate>(Service.KeyState);
            _ws = autorot.WorldState;
            _navi = new();

            _playerController = (PlayerController*)Service.SigScanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 3C 01 75 1E 48 8D 0D");
            Service.Log($"[DebugInput] playerController addess: 0x{(nint)_playerController:X}");

            var rmiWalkAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 80 7B 3E 00 48 8D 3D");
            Service.Log($"[DebugInput] rmiwalk addess: 0x{rmiWalkAddress:X}");
            _rmiWalkHook = Hook<RMIWalkDelegate>.FromAddress(rmiWalkAddress, RMIWalkDetour);

            var rmiFlyAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 0F B6 0D ?? ?? ?? ?? B8");
            Service.Log($"[DebugInput] rmifly addess: 0x{rmiFlyAddress:X}");
            _rmiFlyHook = Hook<RMIFlyDelegate>.FromAddress(rmiFlyAddress, RMIFlyDetour);

            var rmiCameraA = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? EB 4F 85 F6");
            Service.Log($"[DebugInput] rmicameraA addess: 0x{rmiCameraA:X}");
            _rmiCameraAHook = Hook<RMICameraACDDelegate>.FromAddress(rmiCameraA, RMICameraADetour);

            var rmiCameraB = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? EB 1D 41 83 FF 03");
            Service.Log($"[DebugInput] rmicameraB addess: 0x{rmiCameraB:X}");
            _rmiCameraBHook = Hook<RMICameraBDelegate>.FromAddress(rmiCameraB, RMICameraBDetour);

            var rmiCameraC = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 0F 28 7C 24 ?? 48 8B BC 24");
            Service.Log($"[DebugInput] rmicameraC addess: 0x{rmiCameraC:X}");
            _rmiCameraCHook = Hook<RMICameraACDDelegate>.FromAddress(rmiCameraC, RMICameraCDetour);

            var rmiCameraD = Service.SigScanner.ScanText("40 53 48 83 EC 70 44 0F 29 44 24 ?? 48 8B D9");
            Service.Log($"[DebugInput] rmicameraD addess: 0x{rmiCameraD:X}");
            _rmiCameraDHook = Hook<RMICameraACDDelegate>.FromAddress(rmiCameraD, RMICameraDDetour);
        }

        public void Dispose()
        {
            _rmiWalkHook.Dispose();
            _rmiFlyHook.Dispose();
            _rmiCameraAHook.Dispose();
            _rmiCameraBHook.Dispose();
            _rmiCameraCHook.Dispose();
            _rmiCameraDHook.Dispose();
        }

        public void Draw()
        {
            var dt = Utils.FrameDuration();

            var player = _ws.Party.Player();
            var curPos = player?.PosRot.XYZ() ?? new();
            var speed = (curPos - _prevPos) / dt;
            _prevPos = curPos;
            ImGui.TextUnformatted($"Speed={speed.Length():f3}, SpeedH={speed.XZ().Length():f3}, Azimuth={Angle.FromDirection(new(speed.XZ()))}, Altitude={Angle.FromDirection(new(speed.Y, speed.XZ().Length()))}");

            ImGui.Checkbox("Jump!", ref _jump);
            ImGui.InputFloat2("Destination", ref _dest);
            ImGui.SameLine();
            if (ImGui.Button("Move!"))
            {
                _navi.NaviTargetPos = new(_dest);

                //var toTarget = _navi.NaviTargetPos.Value - curPos;
                //_navi.NaviTargetRot = toTarget.Normalized();

                //var cameraFacing = _navi.CameraFacing;
                //var dot = cameraFacing.Dot(_navi.NaviTargetRot.Value);
                //if (dot < -0.707107f)
                //    _navi.NaviTargetRot = -_navi.NaviTargetRot.Value;
                //else if (dot < 0.707107f)
                //    _navi.NaviTargetRot = cameraFacing.OrthoL().Dot(_navi.NaviTargetRot.Value) > 0 ? _navi.NaviTargetRot.Value.OrthoR() : _navi.NaviTargetRot.Value.OrthoL();

                _navi.WantJump = _jump;
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel move"))
            {
                _navi.Clear();
            }
            _navi.Update(player);

            DrawGamepad();

            foreach (var n in _tree.Node("Input data"))
            {
                var idata = GetInputData();
                foreach (var n2 in _tree.Node($"Keybinds ({idata->KeybindCount} total)"))
                {
                    var mapping = new VirtualKey[256];
                    foreach (var vk in Service.KeyState.GetValidVirtualKeys())
                        mapping[_convertVirtualKey((int)vk)] = vk;

                    Func<byte, string> bindString = v => v switch
                    {
                        0 => "---",
                        < 0xA0 => mapping[v].ToString(),
                        < 0xA7 => $"mouse{v - 0xA0}",
                        _ => $"gamepad{v-0xA7}"
                    };
                    Func<ushort, string> printBinding = v => $"{((v & 0x100) != 0 ? "shift+" : "")}{((v & 0x200) != 0 ? "ctrl+" : "")}{((v & 0x400) != 0 ? "alt+" : "")}{((v & 0xF800) != 0 ? "?+" : "")}{bindString((byte)v)} ({v:X4})";
                    for (int i = 0; i < idata->KeybindCount; ++i)
                    {
                        _tree.LeafNode($"{i} = {string.Join(", ", Enumerable.Range(0, 5).Select(j => printBinding(idata->Keybinds[i].Bindings[j])))}");
                    }
                }
            }

            foreach (var n in _tree.Node("Virtual keys"))
            {
                _tree.LeafNodes(Service.KeyState.GetValidVirtualKeys(), vk => $"{vk} ({(int)vk}): internal code={_convertVirtualKey((int)vk)}, state={_getKeyRef((int)vk)}");
            }

            foreach (var n in _tree.Node("PMC"))
            {
                _tree.LeafNode($"Instance: 0x{(nint)_playerController:X}");
                _tree.LeafNode($"Flying: {_playerController->MoveControllerFly.IsFlying != 0}");
                _tree.LeafNode($"Control mode: {_playerController->ControlMode}");
                if (_playerController->MoveControllerFly.IsFlying == 0)
                {
                    _tree.LeafNode($"Movement dir 0x10: {Utils.Vec3String(_playerController->MoveControllerWalk.MovementDir)}");
                    _tree.LeafNode($"Base movement speed: {_playerController->MoveControllerWalk.BaseMovementSpeed:f3}");
                    _tree.LeafNode($"Relative movement dir: {_playerController->MoveControllerWalk.MovementDirRelToCharacterFacing.Radians()}");
                    _tree.LeafNode($"Forced: {_playerController->MoveControllerWalk.Forced}");
                    _tree.LeafNode($"Movement dir 0xA0: {Utils.Vec3String(_playerController->MoveControllerWalk.MovementDirWorld)}");
                    _tree.LeafNode($"Rotation dir: {_playerController->MoveControllerWalk.RotationDir}");
                    _tree.LeafNode($"Movement state: {_playerController->MoveControllerWalk.MovementState}");
                    _tree.LeafNode($"Movement left: {_playerController->MoveControllerWalk.MovementLeft}");
                    _tree.LeafNode($"Movement forward: {_playerController->MoveControllerWalk.MovementFwd}");
                }
                else
                {
                    _tree.LeafNode($"Angular ascent: {_playerController->MoveControllerFly.AngularAscent.Radians()}");
                }

                bool walkOverride = _rmiWalkHook.IsEnabled;
                if (ImGui.Checkbox("Override walk", ref walkOverride))
                {
                    if (walkOverride)
                        _rmiWalkHook.Enable();
                    else
                        _rmiWalkHook.Disable();
                }
                bool flyOverride = _rmiFlyHook.IsEnabled;
                if (ImGui.Checkbox("Override fly", ref flyOverride))
                {
                    if (flyOverride)
                        _rmiFlyHook.Enable();
                    else
                        _rmiFlyHook.Disable();
                }
                bool cameraOverride = _rmiCameraAHook.IsEnabled;
                if (ImGui.Checkbox("Override camera", ref cameraOverride))
                {
                    if (cameraOverride)
                    {
                        _rmiCameraAHook.Enable();
                        _rmiCameraBHook.Enable();
                        _rmiCameraCHook.Enable();
                        _rmiCameraDHook.Enable();
                    }
                    else
                    {
                        _rmiCameraAHook.Disable();
                        _rmiCameraBHook.Disable();
                        _rmiCameraCHook.Disable();
                        _rmiCameraDHook.Disable();
                    }
                }

                if (walkOverride || flyOverride)
                {
                    ImGui.Checkbox("Override move direction", ref _pmcOverrideDirEnable);
                    if (_pmcOverrideDirEnable)
                        ImGui.DragFloat("Override move direction", ref _pmcOverrideDir, 1, -180, 180);
                    ImGui.DragInt("Override vertical", ref _pmcOverrideUp, 0.5f, -1, 1);
                }
            }
        }

        private void DrawGamepad()
        {
            ImGui.SliderFloat("Gamepad angle", ref _gamepadAxisOverrideAngle, -180, 180);
            ImGui.SameLine();
            ImGui.Checkbox("Gamepad move", ref _gamepadAxisOverrideEnable);

            ImGui.SliderInt("Gamepad value", ref _gamepadButtonValue, -100, 100);
            ImGui.SameLine();
            ImGui.InputInt("Gamepad button", ref _gamepadButtonOverride);

            ImGui.Checkbox("Gamepad navigate", ref _gamepadNavigate);

            var input = ActionManagerEx.Instance!.InputOverride;
            Array.Fill(input.GamepadOverrides, 0);
            if (_gamepadButtonOverride >= 0 && _gamepadButtonOverride < input.GamepadOverrides.Length)
            {
                input.GamepadOverridesEnabled = true;
                input.GamepadOverrides[_gamepadButtonOverride] = _gamepadButtonValue;
            }
            else if (_gamepadAxisOverrideEnable)
            {
                input.GamepadOverridesEnabled = true;
                input.GamepadOverrides[3] = (int)(100 * _gamepadAxisOverrideAngle.Degrees().Sin());
                input.GamepadOverrides[4] = (int)(100 * _gamepadAxisOverrideAngle.Degrees().Cos());
            }
            else if (_gamepadNavigate)
            {
                var dest = new WPos(_dest);
                var dir = (Camera.Instance?.CameraAzimuth ?? 0).Radians() - Angle.FromDirection(dest - (_ws.Party.Player()?.Position ?? dest)) + 180.Degrees();
                input.GamepadOverridesEnabled = true;
                input.GamepadOverrides[3] = (int)(100 * dir.Sin());
                input.GamepadOverrides[4] = (int)(100 * dir.Cos());
            }
            else
            {
                input.GamepadOverridesEnabled = false;
            }
        }

        private InputData* GetInputData() => (InputData*)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->GetUIInputData();

        private void RMIWalkDetour(PlayerMoveControllerWalk* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk)
        {
            _rmiWalkHook.Original(self, sumLeft, sumForward, sumTurnLeft, haveBackwardOrStrafe, a6, bAdditiveUnk);
            if (_pmcOverrideDirEnable)
            {
                var dir = _pmcOverrideDir.Degrees().ToDirection();
                *sumLeft = dir.X;
                *sumForward = dir.Z;
            }
            else
            {
                *sumLeft = 0;
                *sumForward = 0;
            }
            //Service.Log($"RMIWalk: l={*sumLeft:f3}, f={*sumForward:f3}, t={*sumTurnLeft:f3}, bs={*haveBackwardOrStrafe}, a6={*a6}, a7={bAdditiveUnk}");
        }

        private void RMIFlyDetour(PlayerMoveControllerFly* self, PlayerMoveControllerFlyInput* result)
        {
            _rmiFlyHook.Original(self, result);
            if (_pmcOverrideDirEnable)
            {
                var dir = _pmcOverrideDir.Degrees().ToDirection();
                result->Left = dir.X;
                result->Forward = dir.Z;
            }
            else
            {
                result->Left = 0;
                result->Forward = 0;
            }
            result->Up = _pmcOverrideUp;
            //Service.Log($"RMIFly: f={result->Forward:f3}, l={result->Left:f3}, up={result->Up:f3}, t={result->Turn:f3}, f10={result->u10:f3}, dmode={result->DirMode}, bs={result->HaveBackwardOrStrafe}");
        }

        private void RMICameraADetour(CameraX* self, int inputMode, float speedH, float speedV)
        {
            _rmiCameraAHook.Original(self, inputMode, speedH, speedV);
            Service.Log($"RMICameraA: mode={inputMode}, dh={self->InputDeltaH}, dv={self->InputDeltaV}");
        }

        private void RMICameraBDetour(CameraX* self, int inputMode, float speedH)
        {
            _rmiCameraBHook.Original(self, inputMode, speedH);
            Service.Log($"RMICameraB: mode={inputMode}, dh={self->InputDeltaH}, dv={self->InputDeltaV}");
        }

        private void RMICameraCDetour(CameraX* self, int inputMode, float speedH, float speedV)
        {
            _rmiCameraCHook.Original(self, inputMode, speedH, speedV);
            Service.Log($"RMICameraC: mode={inputMode}, dh={self->InputDeltaH}, dv={self->InputDeltaV}");
        }

        private void RMICameraDDetour(CameraX* self, int inputMode, float speedH, float speedV)
        {
            _rmiCameraDHook.Original(self, inputMode, speedH, speedV);
            Service.Log($"RMICameraD: mode={inputMode}, dh={self->InputDeltaH}, dv={self->InputDeltaV}");
        }
    }
}
