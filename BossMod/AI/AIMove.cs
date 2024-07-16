using Dalamud.Game.Config;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using System.Runtime.InteropServices;

namespace BossMod.AI;

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

public sealed unsafe class AIMove : IDisposable
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "Camera already does this")]
    public static AIMove? Instance;

    public WPos? DesiredPosition;
    public float? DesiredY;
    public float Precision = 0.01f;

    private bool _legacyMode;

    private delegate bool RMIWalkIsInputEnabled(void* self);
    private readonly RMIWalkIsInputEnabled _rmiWalkIsInputEnabled1;
    private readonly RMIWalkIsInputEnabled _rmiWalkIsInputEnabled2;

    private delegate void RMIWalkDelegate(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk);
    [Signature("E8 ?? ?? ?? ?? 80 7B 3E 00 48 8D 3D")]
    private readonly Hook<RMIWalkDelegate> _rmiWalkHook = null!;

    private delegate void RMIFlyDelegate(void* self, PlayerMoveControllerFlyInput* result);
    [Signature("E8 ?? ?? ?? ?? 0F B6 0D ?? ?? ?? ?? B8")]
    private readonly Hook<RMIFlyDelegate> _rmiFlyHook = null!;

    public AIMove()
    {
        var rmiWalkIsInputEnabled1Addr = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 75 10 38 43 3C");
        var rmiWalkIsInputEnabled2Addr = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 75 03 88 47 3F");
        Service.Log($"RMIWalkIsInputEnabled1 address: 0x{rmiWalkIsInputEnabled1Addr:X}");
        Service.Log($"RMIWalkIsInputEnabled2 address: 0x{rmiWalkIsInputEnabled2Addr:X}");
        _rmiWalkIsInputEnabled1 = Marshal.GetDelegateForFunctionPointer<RMIWalkIsInputEnabled>(rmiWalkIsInputEnabled1Addr);
        _rmiWalkIsInputEnabled2 = Marshal.GetDelegateForFunctionPointer<RMIWalkIsInputEnabled>(rmiWalkIsInputEnabled2Addr);

        Service.Hook.InitializeFromAttributes(this);
        Service.Log($"RMIWalk address: 0x{_rmiWalkHook.Address:X}");
        Service.GameConfig.UiControlChanged += OnConfigChanged;
        UpdateLegacyMode();

        _rmiWalkHook.Enable();
        _rmiFlyHook.Enable();
    }

    public void Dispose()
    {
        Service.GameConfig.UiControlChanged -= OnConfigChanged;
        _rmiWalkHook.Dispose();
        _rmiFlyHook.Dispose();
    }

    private void RMIWalkDetour(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk)
    {
        _rmiWalkHook.Original(self, sumLeft, sumForward, sumTurnLeft, haveBackwardOrStrafe, a6, bAdditiveUnk);
        // TODO: we really need to introduce some extra checks that PlayerMoveController::readInput does - sometimes it skips reading input, and returning something non-zero breaks stuff...
        bool movementAllowed = bAdditiveUnk == 0 && _rmiWalkIsInputEnabled1(self) && _rmiWalkIsInputEnabled2(self); //&& !Service.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BeingMoved];
        if (movementAllowed && (*sumLeft == 0 && *sumForward == 0) && DirectionToDestination(false) is var relDir && relDir != null)
        {
            var dir = relDir.Value.h.ToDirection();
            *sumLeft = dir.X;
            *sumForward = dir.Z;
        }
    }

    private void RMIFlyDetour(void* self, PlayerMoveControllerFlyInput* result)
    {
        _rmiFlyHook.Original(self, result);
        // TODO: we really need to introduce some extra checks that PlayerMoveController::readInput does - sometimes it skips reading input, and returning something non-zero breaks stuff...
        if (result->Forward == 0 && result->Left == 0 && result->Up == 0 && DirectionToDestination(true) is var relDir && relDir != null)
        {
            var dir = relDir.Value.h.ToDirection();
            result->Forward = dir.Z;
            result->Left = dir.X;
            result->Up = relDir.Value.v.Rad;
        }
    }

    private (Angle h, Angle v)? DirectionToDestination(bool allowVertical)
    {
        if (DesiredPosition == null)
            return null;

        var player = Service.ClientState.LocalPlayer;
        if (player == null)
            return null;

        var playerXZ = new WPos(player.Position.X, player.Position.Z);

        var distV3 = DesiredPosition.Value - playerXZ;
        if (distV3.Length() <= Precision)
            return null;

        var dist = new WDir(distV3.X, distV3.Z);

        var dirH = Angle.FromDirection(dist);
        var dirV = allowVertical && DesiredY != null ? Angle.FromDirection(new(DesiredY.Value, dist.Length())) : default;

        var refDir = _legacyMode
            ? Camera.Instance!.CameraAzimuth.Radians() + 180.Degrees()
            : player.Rotation.Radians();
        return (dirH - refDir, dirV);
    }

    private void OnConfigChanged(object? sender, ConfigChangeEvent evt) => UpdateLegacyMode();
    private void UpdateLegacyMode()
    {
        _legacyMode = Service.GameConfig.UiControl.TryGetUInt("MoveMode", out var mode) && mode == 1;
        Service.Log($"Legacy mode is now {(_legacyMode ? "enabled" : "disabled")}");
    }
}
