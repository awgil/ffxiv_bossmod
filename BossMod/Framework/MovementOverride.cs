using Dalamud.Game.Config;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Client.UI;
using ImGuiNET;
using System.Runtime.InteropServices;

namespace BossMod;

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

public sealed unsafe class MovementOverride : IDisposable
{
    public Vector3? DesiredDirection;

    private float UserMoveLeft;
    private float UserMoveUp;
    private float ActualMoveLeft;
    private float ActualMoveUp;

    private bool _movementBlocked;
    private bool _legacyMode;

    public bool IsMoving() => ActualMoveLeft != 0 || ActualMoveUp != 0;
    public bool IsMoveRequested() => UserMoveLeft != 0 || UserMoveUp != 0;

    public bool IsForceUnblocked() => Service.Config.Get<ActionTweaksConfig>().MoveEscapeHatch switch
    {
        ActionTweaksConfig.ModifierKey.Ctrl => ImGui.GetIO().KeyCtrl,
        ActionTweaksConfig.ModifierKey.Alt => ImGui.GetIO().KeyAlt,
        ActionTweaksConfig.ModifierKey.Shift => ImGui.GetIO().KeyShift,
        ActionTweaksConfig.ModifierKey.M12 => UIInputData.Instance()->UIFilteredCursorInputs.MouseButtonHeldFlags.HasFlag(MouseButtonFlags.LBUTTON | MouseButtonFlags.RBUTTON),
        _ => false,
    };

    public bool MovementBlocked
    {
        get => _movementBlocked && !IsForceUnblocked();
        set => _movementBlocked = value;
    }

    private delegate bool RMIWalkIsInputEnabled(void* self);
    private readonly RMIWalkIsInputEnabled _rmiWalkIsInputEnabled1;
    private readonly RMIWalkIsInputEnabled _rmiWalkIsInputEnabled2;

    private delegate void RMIWalkDelegate(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk);
    private readonly HookAddress<RMIWalkDelegate> _rmiWalkHook;

    private delegate void RMIFlyDelegate(void* self, PlayerMoveControllerFlyInput* result);
    private readonly HookAddress<RMIFlyDelegate> _rmiFlyHook = null!;

    public MovementOverride()
    {
        var rmiWalkIsInputEnabled1Addr = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 75 10 38 43 3C");
        var rmiWalkIsInputEnabled2Addr = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 75 03 88 47 3F");
        Service.Log($"RMIWalkIsInputEnabled1 address: 0x{rmiWalkIsInputEnabled1Addr:X}");
        Service.Log($"RMIWalkIsInputEnabled2 address: 0x{rmiWalkIsInputEnabled2Addr:X}");
        _rmiWalkIsInputEnabled1 = Marshal.GetDelegateForFunctionPointer<RMIWalkIsInputEnabled>(rmiWalkIsInputEnabled1Addr);
        _rmiWalkIsInputEnabled2 = Marshal.GetDelegateForFunctionPointer<RMIWalkIsInputEnabled>(rmiWalkIsInputEnabled2Addr);

        _rmiWalkHook = new("E8 ?? ?? ?? ?? 80 7B 3E 00 48 8D 3D", RMIWalkDetour);
        _rmiFlyHook = new("E8 ?? ?? ?? ?? 0F B6 0D ?? ?? ?? ?? B8", RMIFlyDetour);

        Service.GameConfig.UiControlChanged += OnConfigChanged;
        UpdateLegacyMode();
    }

    public void Dispose()
    {
        Service.GameConfig.UiControlChanged -= OnConfigChanged;
        _movementBlocked = false;
        UserMoveLeft = 0;
        UserMoveUp = 0;
        ActualMoveLeft = 0;
        ActualMoveUp = 0;
        _rmiWalkHook.Dispose();
        _rmiFlyHook.Dispose();
    }

    private void RMIWalkDetour(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk)
    {
        _rmiWalkHook.Original(self, sumLeft, sumForward, sumTurnLeft, haveBackwardOrStrafe, a6, bAdditiveUnk);
        UserMoveLeft = *sumLeft;
        UserMoveUp = *sumForward;

        // TODO: this allows AI mode to move even if movement is "blocked", is this the right behavior? AI mode should try to avoid moving while casting anyway...
        if (MovementBlocked)
        {
            *sumLeft = 0;
            *sumForward = 0;
        }

        // TODO: we really need to introduce some extra checks that PlayerMoveController::readInput does - sometimes it skips reading input, and returning something non-zero breaks stuff...
        var movementAllowed = bAdditiveUnk == 0 && _rmiWalkIsInputEnabled1(self) && _rmiWalkIsInputEnabled2(self); //&& !_movementBlocked
        if (movementAllowed && *sumLeft == 0 && *sumForward == 0 && DirectionToDestination(false) is var relDir && relDir != null)
        {
            var dir = relDir.Value.h.ToDirection();
            *sumLeft = dir.X;
            *sumForward = dir.Z;
        }
        ActualMoveLeft = *sumLeft;
        ActualMoveUp = *sumForward;
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
        if (DesiredDirection == null || DesiredDirection.Value == default)
            return null;

        var player = GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        if (player == null)
            return null;

        var dxz = new WDir(DesiredDirection.Value.X, DesiredDirection.Value.Z);
        var dirH = Angle.FromDirection(dxz);
        var dirV = allowVertical ? Angle.FromDirection(new(DesiredDirection.Value.Y, dxz.Length())) : default;

        var refDir = _legacyMode
            ? Camera.Instance!.CameraAzimuth.Radians() + 180.Degrees()
            : player->Rotation.Radians();
        return (dirH - refDir, dirV);
    }

    private void OnConfigChanged(object? sender, ConfigChangeEvent evt) => UpdateLegacyMode();
    private void UpdateLegacyMode()
    {
        _legacyMode = Service.GameConfig.UiControl.TryGetUInt("MoveMode", out var mode) && mode == 1;
        Service.Log($"Legacy mode is now {(_legacyMode ? "enabled" : "disabled")}");
    }
}
