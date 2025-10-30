using Dalamud.Bindings.ImGui;
using Dalamud.Game.Config;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Client.UI;
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

[StructLayout(LayoutKind.Explicit, Size = 0x140)]
public unsafe struct MoveControllerSubMemberForMine
{
    [FieldOffset(0x94)] public byte Spinning;
}

public sealed unsafe class MovementOverride : IDisposable
{
    public Vector3? DesiredDirection;
    public Angle MisdirectionThreshold;
    public Angle? DesiredSpinDirection;

    public WDir UserMove { get; private set; } // unfiltered movement direction, as read from input
    public WDir ActualMove { get; private set; } // actual movement direction, as of last input read

    private readonly IDalamudPluginInterface _dalamud;
    private readonly ActionTweaksConfig _tweaksConfig = Service.Config.Get<ActionTweaksConfig>();
    private bool? _forcedControlState;
    private bool _legacyMode;
    private bool[]? _navmeshPathIsRunning;

    public bool IsMoving() => ActualMove != default;
    public bool IsMoveRequested() => UserMove != default;

    public bool IsForceUnblocked() => _tweaksConfig.MoveEscapeHatch switch
    {
        ActionTweaksConfig.ModifierKey.Ctrl => ImGui.GetIO().KeyCtrl,
        ActionTweaksConfig.ModifierKey.Alt => ImGui.GetIO().KeyAlt,
        ActionTweaksConfig.ModifierKey.Shift => ImGui.GetIO().KeyShift,
        ActionTweaksConfig.ModifierKey.M12 => UIInputData.Instance()->UIFilteredCursorInputs.MouseButtonHeldFlags.HasFlag(MouseButtonFlags.LBUTTON | MouseButtonFlags.RBUTTON),
        _ => false,
    };

    public bool MovementBlocked
    {
        get => field && !IsForceUnblocked();
        set;
    }

    public static readonly float* ForcedMovementDirection = (float*)Service.SigScanner.GetStaticAddressFromSig("F3 0F 11 0D ?? ?? ?? ?? 48 85 DB");

    private delegate bool RMIWalkIsInputEnabled(void* self);
    private readonly RMIWalkIsInputEnabled _rmiWalkIsInputEnabled1;
    private readonly RMIWalkIsInputEnabled _rmiWalkIsInputEnabled2;

    private delegate void RMIWalkDelegate(MoveControllerSubMemberForMine* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk);
    private readonly HookAddress<RMIWalkDelegate> _rmiWalkHook;

    private delegate void RMIFlyDelegate(void* self, PlayerMoveControllerFlyInput* result);
    private readonly HookAddress<RMIFlyDelegate> _rmiFlyHook;

    // input source flags: 1 = kb/mouse, 2 = gamepad
    private delegate byte MoveControlIsInputActiveDelegate(void* self, byte inputSourceFlags);
    private readonly HookAddress<MoveControlIsInputActiveDelegate> _mcIsInputActiveHook;

    public MovementOverride(IDalamudPluginInterface dalamud)
    {
        _dalamud = dalamud;

        var rmiWalkIsInputEnabled1Addr = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 75 10 38 43 3C");
        var rmiWalkIsInputEnabled2Addr = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 75 03 88 47 3F");
        Service.Log($"RMIWalkIsInputEnabled1 address: 0x{rmiWalkIsInputEnabled1Addr:X}");
        Service.Log($"RMIWalkIsInputEnabled2 address: 0x{rmiWalkIsInputEnabled2Addr:X}");
        _rmiWalkIsInputEnabled1 = Marshal.GetDelegateForFunctionPointer<RMIWalkIsInputEnabled>(rmiWalkIsInputEnabled1Addr);
        _rmiWalkIsInputEnabled2 = Marshal.GetDelegateForFunctionPointer<RMIWalkIsInputEnabled>(rmiWalkIsInputEnabled2Addr);

        _rmiWalkHook = new("E8 ?? ?? ?? ?? 80 7B 3E 00 48 8D 3D", RMIWalkDetour);
        _rmiFlyHook = new("E8 ?? ?? ?? ?? 0F B6 0D ?? ?? ?? ?? B8", RMIFlyDetour);
        _mcIsInputActiveHook = new("E8 ?? ?? ?? ?? 84 C0 74 09 84 DB 74 1A", MCIsInputActiveDetour);

        Service.GameConfig.UiControlChanged += OnConfigChanged;
        UpdateLegacyMode();
    }

    public void Dispose()
    {
        _dalamud.RelinquishData("vnav.PathIsRunning");
        Service.GameConfig.UiControlChanged -= OnConfigChanged;
        MovementBlocked = false;
        _mcIsInputActiveHook.Dispose();
        _rmiWalkHook.Dispose();
        _rmiFlyHook.Dispose();
    }

    public bool FollowPathActive()
    {
        if (_navmeshPathIsRunning == null && _dalamud.TryGetData<bool[]>("vnav.PathIsRunning", out var data))
            _navmeshPathIsRunning = data;

        return _navmeshPathIsRunning != null && _navmeshPathIsRunning[0];
    }

    private void RMIWalkDetour(MoveControllerSubMemberForMine* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk)
    {
        _forcedControlState = null;
        _rmiWalkHook.Original(self, sumLeft, sumForward, sumTurnLeft, haveBackwardOrStrafe, a6, bAdditiveUnk);

        // handling the Spinning status, during which we can only steer toward a desired safe spot
        // all vanilla movement input is ignored in this state so we skip the rest of the override logic
        if (self->Spinning == 1 && bAdditiveUnk == 1)
        {
            if (DesiredSpinDirection is { } dir && *sumLeft == 0 && *sumForward == 0)
            {
                var turnDir = (dir - ForwardMovementDirection()).ToDirection();
                *sumLeft = turnDir.X;
                *sumForward = turnDir.Z;
                _forcedControlState = true;
            }

            return;
        }

        // TODO: we really need to introduce some extra checks that PlayerMoveController::readInput does - sometimes it skips reading input, and returning something non-zero breaks stuff...
        var movementAllowed = bAdditiveUnk == 0 && _rmiWalkIsInputEnabled1(self) && _rmiWalkIsInputEnabled2(self) && !FollowPathActive();
        var misdirectionMode = PlayerHasMisdirection();
        if (!movementAllowed && misdirectionMode)
        {
            // in misdirection mode, when we are already moving, the 'original' call will not actually sample input and just return immediately
            // we actually want to know the direction, in case user changes input mid movement - so force sample raw input
            float realTurn = 0;
            byte realStrafe = 0, realUnk = 0;
            _rmiWalkHook.Original(self, sumLeft, sumForward, &realTurn, &realStrafe, &realUnk, 1);
        }

        // at this point, UserMove contains true user input
        UserMove = new(*sumLeft, *sumForward);

        // apply movement block logic
        // note: currently movement block is ignored in misdirection mode
        // the assumption is that, with misdirection active, it's not safe to block movement just because player is casting or doing something else (as arrow will rotate away)
        ActualMove = !MovementBlocked || misdirectionMode ? UserMove : default;

        // movement override logic
        // note: currently we follow desired direction, only if user does not have any input _or_ if manual movement is blocked
        // this allows AI mode to move even if movement is blocked (TODO: is this the right behavior? AI mode should try to avoid moving while casting anyway...)
        var allowAuto = movementAllowed ? !MovementBlocked : misdirectionMode;
        if (allowAuto && ActualMove == default && DirectionToDestination(false) is var relDir && relDir != null)
        {
            ActualMove = relDir.Value.h.ToDirection();
        }

        // misdirection override logic
        if (misdirectionMode)
        {
            var thresholdDeg = UserMove != default ? _tweaksConfig.MisdirectionThreshold : MisdirectionThreshold.Deg;
            if (thresholdDeg < 180)
            {
                // note: if we are already moving, it doesn't matter what we do here, only whether 'is input active' function returns true or false
                _forcedControlState = ActualMove != default && (Angle.FromDirection(ActualMove) + ForwardMovementDirection() - ForcedMovementDirection->Radians()).Normalized().Abs().Deg <= thresholdDeg;
            }
        }

        // finally, update output
        var output = !misdirectionMode ? ActualMove // standard mode - just return desired movement
            : !movementAllowed ? default // misdirection and already moving - always return 0, as game does
            : _forcedControlState == null ? ActualMove // misdirection mode, but we're not trying to help user
            : _forcedControlState.Value ? ActualMove // misdirection mode, not moving yet, but want to start - can return anything really
            : default; // misdirection mode, not moving yet and don't want to
        *sumLeft = output.X;
        *sumForward = output.Z;
    }

    private void RMIFlyDetour(void* self, PlayerMoveControllerFlyInput* result)
    {
        _forcedControlState = null;
        _rmiFlyHook.Original(self, result);

        // do nothing while followpath is running
        if (FollowPathActive())
            return;

        // TODO: we really need to introduce some extra checks that PlayerMoveController::readInput does - sometimes it skips reading input, and returning something non-zero breaks stuff...
        if (result->Forward == 0 && result->Left == 0 && result->Up == 0 && DirectionToDestination(true) is var relDir && relDir != null)
        {
            var dir = relDir.Value.h.ToDirection();
            result->Forward = dir.Z;
            result->Left = dir.X;
            result->Up = relDir.Value.v.Rad;
        }
    }

    private byte MCIsInputActiveDetour(void* self, byte inputSourceFlags)
    {
        return _forcedControlState != null ? (byte)(_forcedControlState.Value ? 1 : 0) : _mcIsInputActiveHook.Original(self, inputSourceFlags);
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
        return (dirH - ForwardMovementDirection(), dirV);
    }

    private Angle ForwardMovementDirection() => _legacyMode ? Camera.Instance!.CameraAzimuth.Radians() + 180.Degrees() : GameObjectManager.Instance()->Objects.IndexSorted[0].Value->Rotation.Radians();

    private bool PlayerHasMisdirection()
    {
        var player = (Character*)GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        var sm = player != null && player->IsCharacter() ? player->GetStatusManager() : null;
        if (sm == null)
            return false;
        for (int i = 0; i < sm->NumValidStatuses; ++i)
            if (sm->Status[i].StatusId is 1422 or 2936 or 3694 or 3909)
                return true;
        return false;
    }

    private void OnConfigChanged(object? sender, ConfigChangeEvent evt) => UpdateLegacyMode();
    private void UpdateLegacyMode()
    {
        _legacyMode = Service.GameConfig.UiControl.TryGetUInt("MoveMode", out var mode) && mode == 1;
        Service.Log($"Legacy mode is now {(_legacyMode ? "enabled" : "disabled")}");
    }
}
