using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BossMod;

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

[StructLayout(LayoutKind.Explicit, Size = 0x2B0)]
public unsafe struct CameraX
{
    [FieldOffset(0x130)] public float DirH;
    [FieldOffset(0x134)] public float DirV;
    [FieldOffset(0x138)] public float InputDeltaHAdjusted;
    [FieldOffset(0x13C)] public float InputDeltaVAdjusted;
    [FieldOffset(0x140)] public float InputDeltaH;
    [FieldOffset(0x144)] public float InputDeltaV;
    [FieldOffset(0x148)] public float DirVMin;
    [FieldOffset(0x14C)] public float DirVMax;
}

internal sealed unsafe class DebugInput : IDisposable
{
    private delegate byte ConvertVirtualKeyDelegate(int vkCode);
    private readonly ConvertVirtualKeyDelegate _convertVirtualKey;

    private delegate ref int GetRefValueDelegate(int vkCode);
    private readonly GetRefValueDelegate _getKeyRef;

    //private readonly PlayerController* _playerController;

    //private delegate void RMIWalkDelegate(PlayerMoveControllerWalk* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk);
    //private readonly Hook<RMIWalkDelegate> _rmiWalkHook;

    //private delegate void RMIFlyDelegate(PlayerMoveControllerFly* self, PlayerMoveControllerFlyInput* result);
    //private readonly Hook<RMIFlyDelegate> _rmiFlyHook;

    //private delegate void RMICameraDelegate(CameraX* self, int inputMode, float speedH, float speedV);
    //private readonly Hook<RMICameraDelegate> _rmiCameraHook;

    private readonly UITree _tree = new();
    private readonly WorldState _ws;
    private readonly AIHints _hints;
    private readonly MovementOverride _move;
    //private readonly AI.AIController _navi;
    private Vector4 _prevPosRot;
    private float _prevSpeed;
    private bool _wannaMove;
    private float _moveDir;
    private bool _gamepadAxisOverrideEnable;
    private float _gamepadAxisOverrideAngle;
    private int _gamepadButtonOverride = -1;
    private int _gamepadButtonValue;
    private bool _gamepadNavigate;
    //private bool _pmcOverrideDirEnable;
    //private float _pmcOverrideDir;
    //private float _pmcOverrideVertical;
    //private float _pmcDesiredAzimuth;
    //private float _pmcDesiredAltitude;
    //private float _pmcCameraSpeedH;
    //private float _pmcCameraSpeedV;

    public DebugInput(RotationModuleManager autorot, MovementOverride move)
    {
        _convertVirtualKey = Service.KeyState.GetType().GetMethod("ConvertVirtualKey", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<ConvertVirtualKeyDelegate>(Service.KeyState);
        _getKeyRef = Service.KeyState.GetType().GetMethod("GetRefValue", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<GetRefValueDelegate>(Service.KeyState);
        _ws = autorot.Bossmods.WorldState;
        _hints = autorot.Hints;
        _move = move;
        //_amex = autorot.ActionManager;
        //_navi = new(_amex);

        //_playerController = (PlayerController*)Service.SigScanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 3C 01 75 1E 48 8D 0D");
        //Service.Log($"[DebugInput] playerController addess: 0x{(nint)_playerController:X}");
        Service.Log("[DebugInput] fix me");

        //_rmiWalkHook = Service.Hook.HookFromSignature<RMIWalkDelegate>("E8 ?? ?? ?? ?? 80 7B 3E 00 48 8D 3D", RMIWalkDetour);
        //Service.Log($"[DebugInput] rmiwalk addess: 0x{_rmiWalkHook.Address:X}");

        //_rmiFlyHook = Service.Hook.HookFromSignature<RMIFlyDelegate>("E8 ?? ?? ?? ?? 0F B6 0D ?? ?? ?? ?? B8", RMIFlyDetour);
        //Service.Log($"[DebugInput] rmifly addess: 0x{_rmiFlyHook.Address:X}");

        //_rmiCameraHook = Service.Hook.HookFromSignature<RMICameraDelegate>("40 53 48 83 EC 70 44 0F 29 44 24 ?? 48 8B D9", RMICameraDetour);
        //Service.Log($"[DebugInput] rmicamera addess: 0x{_rmiCameraHook.Address:X}");
    }

    public void Dispose()
    {
        //_rmiWalkHook.Dispose();
        //_rmiFlyHook.Dispose();
        //_rmiCameraHook.Dispose();
    }

    public void Draw()
    {
        var dt = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->FrameDeltaTime;

        var player = _ws.Party.Player();
        var curPosRot = player?.PosRot ?? new();
        var speed = (curPosRot.XYZ() - _prevPosRot.XYZ()) / dt;
        var speedAbs = speed.Length();
        var accel = (speedAbs - _prevSpeed) / dt;
        var rotSpeed = (curPosRot.W - _prevPosRot.W).Radians().Normalized() / dt;
        //if (curPosRot.W != _prevPosRot.W)
        //    Service.Log($"ROT: {_prevPosRot.W.Radians()} -> {curPosRot.W.Radians()} over {dt} (s={rotSpeed})");
        _prevPosRot = curPosRot;
        _prevSpeed = speedAbs;
        ImGui.TextUnformatted($"Speed={speedAbs:f3}, SpeedH={speed.XZ().Length():f3}, SpeedV={speed.Y:f3}, RSpeed={rotSpeed}, Accel={accel:f3}, Azimuth={Angle.FromDirection(new(speed.XZ()))}, Altitude={Angle.FromDirection(new(speed.Y, speed.XZ().Length()))}");
        ImGui.TextUnformatted($"MO: desired={_move.DesiredDirection}, user={_move.UserMove}, actual={_move.ActualMove}");
        //Service.Log($"Speed: {speedAbs:f3}, accel: {accel:f3}");

        var pobj = GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        if (pobj != null)
        {
            var pm = ((PlayerMove*)pobj)->Move;
            ImGui.TextUnformatted($"Turn {(nint)pobj:X}");
            ImGui.TextUnformatted($"Desired={pm.Interpolation.DesiredRotation}");
            ImGui.TextUnformatted($"Original={pm.Interpolation.OriginalRotation}");
            ImGui.TextUnformatted($"Progress={pm.Interpolation.RotationInterpolationInProgress}");
        }

        ImGui.SliderFloat("Move direction", ref _moveDir, -180, 180);
        ImGui.SameLine();
        if (ImGui.Button(_wannaMove ? "Cancel move" : "Move!"))
        {
            _wannaMove ^= true;
            //var toTarget = _navi.NaviTargetPos.Value - curPos;
            //_navi.NaviTargetRot = toTarget.Normalized();

            //var cameraFacing = _navi.CameraFacing;
            //var dot = cameraFacing.Dot(_navi.NaviTargetRot.Value);
            //if (dot < -0.707107f)
            //    _navi.NaviTargetRot = -_navi.NaviTargetRot.Value;
            //else if (dot < 0.707107f)
            //    _navi.NaviTargetRot = cameraFacing.OrthoL().Dot(_navi.NaviTargetRot.Value) > 0 ? _navi.NaviTargetRot.Value.OrthoR() : _navi.NaviTargetRot.Value.OrthoL();
            //_navi.WantJump = _jump;
        }
        if (_wannaMove)
        {
            _move.DesiredDirection = _moveDir.Degrees().ToDirection().ToVec3();
        }
        //_navi.Update(player);

        DrawGamepad();

        foreach (var n in _tree.Node("Input data"))
        {
            var idata = GetInputData();
            foreach (var n2 in _tree.Node($"Keybinds ({idata->KeybindCount} total)"))
            {
                var mapping = new VirtualKey[256];
                foreach (var vk in Service.KeyState.GetValidVirtualKeys())
                    mapping[_convertVirtualKey((int)vk)] = vk;

                string bindString(byte v) => v switch
                {
                    0 => "---",
                    < 0xA0 => mapping[v].ToString(),
                    < 0xA7 => $"mouse{v - 0xA0}",
                    _ => $"gamepad{v - 0xA7}"
                };
                string printBinding(ushort v) => $"{((v & 0x100) != 0 ? "shift+" : "")}{((v & 0x200) != 0 ? "ctrl+" : "")}{((v & 0x400) != 0 ? "alt+" : "")}{((v & 0xF800) != 0 ? "?+" : "")}{bindString((byte)v)} ({v:X4})";
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

        //foreach (var n in _tree.Node("PMC"))
        //{
        //    _tree.LeafNode($"Instance: 0x{(nint)_playerController:X}");
        //    _tree.LeafNode($"Flying: {_playerController->MoveControllerFly.IsFlying != 0}");
        //    _tree.LeafNode($"Control mode: {_playerController->ControlMode}");
        //    if (_playerController->MoveControllerFly.IsFlying == 0)
        //    {
        //        _tree.LeafNode($"Movement dir 0x10: {Utils.Vec3String(_playerController->MoveControllerWalk.MovementDir)}");
        //        _tree.LeafNode($"Base movement speed: {_playerController->MoveControllerWalk.BaseMovementSpeed:f3}");
        //        _tree.LeafNode($"Relative movement dir: {_playerController->MoveControllerWalk.MovementDirRelToCharacterFacing.Radians()}");
        //        _tree.LeafNode($"Forced: {_playerController->MoveControllerWalk.Forced}");
        //        _tree.LeafNode($"Movement dir 0xA0: {Utils.Vec3String(_playerController->MoveControllerWalk.MovementDirWorld)}");
        //        _tree.LeafNode($"Rotation dir: {_playerController->MoveControllerWalk.RotationDir}");
        //        _tree.LeafNode($"Movement state: {_playerController->MoveControllerWalk.MovementState}");
        //        _tree.LeafNode($"Movement left: {_playerController->MoveControllerWalk.MovementLeft}");
        //        _tree.LeafNode($"Movement forward: {_playerController->MoveControllerWalk.MovementFwd}");
        //    }
        //    else
        //    {
        //        _tree.LeafNode($"Angular ascent: {_playerController->MoveControllerFly.AngularAscent.Radians()}");
        //    }

        //    bool walkOverride = _rmiWalkHook.IsEnabled;
        //    if (ImGui.Checkbox("Override walk", ref walkOverride))
        //    {
        //        if (walkOverride)
        //            _rmiWalkHook.Enable();
        //        else
        //            _rmiWalkHook.Disable();
        //    }
        //    bool flyOverride = _rmiFlyHook.IsEnabled;
        //    if (ImGui.Checkbox("Override fly", ref flyOverride))
        //    {
        //        if (flyOverride)
        //            _rmiFlyHook.Enable();
        //        else
        //            _rmiFlyHook.Disable();
        //    }
        //    if (walkOverride || flyOverride)
        //    {
        //        ImGui.Checkbox("Override move direction", ref _pmcOverrideDirEnable);
        //        if (_pmcOverrideDirEnable)
        //            ImGui.DragFloat("Override move direction", ref _pmcOverrideDir, 1, -180, 180);
        //        ImGui.DragFloat("Override vertical", ref _pmcOverrideVertical, 1, -90, 90);
        //    }

        //    bool cameraOverride = _rmiCameraHook.IsEnabled;
        //    if (ImGui.Checkbox("Override camera", ref cameraOverride))
        //    {
        //        if (cameraOverride)
        //            _rmiCameraHook.Enable();
        //        else
        //            _rmiCameraHook.Disable();
        //    }
        //    if (cameraOverride)
        //    {
        //        ImGui.DragFloat("Camera desired azimuth", ref _pmcDesiredAzimuth, 1, -180, 180);
        //        ImGui.DragFloat("Camera desired altitude", ref _pmcDesiredAltitude, 1, -85, 45);
        //        ImGui.DragFloat("Camera speed H (deg/sec)", ref _pmcCameraSpeedH, 1, 0, 360);
        //        ImGui.DragFloat("Camera speed V (deg/sec)", ref _pmcCameraSpeedV, 1, 0, 360);
        //    }
        //}
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

        //var input = _amex.InputOverride;
        //Array.Fill(input.GamepadOverrides, 0);
        //if (_gamepadButtonOverride >= 0 && _gamepadButtonOverride < input.GamepadOverrides.Length)
        //{
        //    input.GamepadOverridesEnabled = true;
        //    input.GamepadOverrides[_gamepadButtonOverride] = _gamepadButtonValue;
        //}
        //else if (_gamepadAxisOverrideEnable)
        //{
        //    input.GamepadOverridesEnabled = true;
        //    input.GamepadOverrides[3] = (int)(100 * _gamepadAxisOverrideAngle.Degrees().Sin());
        //    input.GamepadOverrides[4] = (int)(100 * _gamepadAxisOverrideAngle.Degrees().Cos());
        //}
        //else if (_gamepadNavigate)
        //{
        //    var dest = new WPos(_dest);
        //    var dir = (Camera.Instance?.CameraAzimuth ?? 0).Radians() - Angle.FromDirection(dest - (_ws.Party.Player()?.Position ?? dest)) + 180.Degrees();
        //    input.GamepadOverridesEnabled = true;
        //    input.GamepadOverrides[3] = (int)(100 * dir.Sin());
        //    input.GamepadOverrides[4] = (int)(100 * dir.Cos());
        //}
        //else
        //{
        //    input.GamepadOverridesEnabled = false;
        //}
    }

    private InputData* GetInputData() => (InputData*)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUIModule()->GetUIInputData();

    //private void RMIWalkDetour(PlayerMoveControllerWalk* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk)
    //{
    //    _rmiWalkHook.Original(self, sumLeft, sumForward, sumTurnLeft, haveBackwardOrStrafe, a6, bAdditiveUnk);
    //    if (_pmcOverrideDirEnable)
    //    {
    //        var dir = _pmcOverrideDir.Degrees().ToDirection();
    //        *sumLeft = dir.X;
    //        *sumForward = dir.Z;
    //    }
    //    else
    //    {
    //        *sumLeft = 0;
    //        *sumForward = 0;
    //    }
    //    //Service.Log($"RMIWalk: l={*sumLeft:f3}, f={*sumForward:f3}, t={*sumTurnLeft:f3}, bs={*haveBackwardOrStrafe}, a6={*a6}, a7={bAdditiveUnk}");
    //}

    //private void RMIFlyDetour(PlayerMoveControllerFly* self, PlayerMoveControllerFlyInput* result)
    //{
    //    _rmiFlyHook.Original(self, result);
    //    if (_pmcOverrideDirEnable)
    //    {
    //        var dir = _pmcOverrideDir.Degrees().ToDirection();
    //        result->Left = dir.X;
    //        result->Forward = dir.Z;
    //    }
    //    else
    //    {
    //        result->Left = 0;
    //        result->Forward = 0;
    //    }
    //    result->Up = _pmcOverrideVertical != 0 ? _pmcOverrideVertical.Degrees().Rad : float.Epsilon;
    //    //Service.Log($"RMIFly: f={result->Forward:f3}, l={result->Left:f3}, up={result->Up:f3}, t={result->Turn:f3}, f10={result->u10:f3}, dmode={result->DirMode}, bs={result->HaveBackwardOrStrafe}");
    //}

    //private void RMICameraDetour(CameraX* self, int inputMode, float speedH, float speedV)
    //{
    //    _rmiCameraHook.Original(self, inputMode, speedH, speedV);
    //    if (inputMode == 0) // let user override...
    //    {
    //        var dt = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->FrameDeltaTime;
    //        var deltaH = (_pmcDesiredAzimuth.Degrees() - self->DirH.Radians()).Normalized();
    //        var deltaV = (_pmcDesiredAltitude.Degrees() - self->DirV.Radians()).Normalized();
    //        var maxH = _pmcCameraSpeedH.Degrees().Rad * dt;
    //        var maxV = _pmcCameraSpeedV.Degrees().Rad * dt;
    //        self->InputDeltaH = Math.Clamp(deltaH.Rad, -maxH, maxH);
    //        self->InputDeltaV = Math.Clamp(deltaV.Rad, -maxV, maxV);
    //    }
    //    //Service.Log($"RMICamera: dir={self->DirH.Radians()}/{self->DirV.Radians()} [{self->DirVMin.Radians()}-{self->DirVMax.Radians()}], mode={inputMode}, speed={speedH:f1}/{speedV:f1}, delta={self->InputDeltaH.Radians().Deg:f3}/{self->InputDeltaV.Radians().Deg:f3}, speed={self->InputDeltaH.Radians() / Utils.FrameDuration()}/{self->InputDeltaV.Radians() / Utils.FrameDuration()}");
    //}
}
