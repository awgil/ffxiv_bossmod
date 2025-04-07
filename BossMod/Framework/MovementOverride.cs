﻿using Dalamud.Game.Config;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
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
    public Angle MisdirectionThreshold;

    public WDir UserMove { get; private set; } // unfiltered movement direction, as read from input
    public WDir ActualMove { get; private set; } // actual movement direction, as of last input read
    public bool UserControlActive { get; private set; } // indicates if user manual control is currently active

    private readonly IDalamudPluginInterface _dalamud;
    private readonly ActionTweaksConfig _tweaksConfig = Service.Config.Get<ActionTweaksConfig>();
    private bool? _forcedControlState;
    private bool _legacyMode;
    private bool[]? _navmeshPathIsRunning;
    private DateTime _lastUserControlTime = DateTime.MinValue; // track when user last had control

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

    private delegate void RMIWalkDelegate(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk);
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

    private bool FollowpathActive()
    {
        if (_navmeshPathIsRunning == null && _dalamud.TryGetData<bool[]>("vnav.PathIsRunning", out var data))
            _navmeshPathIsRunning = data;

        return _navmeshPathIsRunning != null && _navmeshPathIsRunning[0];
    }

    private void RMIWalkDetour(void* self, float* sumLeft, float* sumForward, float* sumTurnLeft, byte* haveBackwardOrStrafe, byte* a6, byte bAdditiveUnk)
    {
        _forcedControlState = null;
        _rmiWalkHook.Original(self, sumLeft, sumForward, sumTurnLeft, haveBackwardOrStrafe, a6, bAdditiveUnk);

        // TODO: we really need to introduce some extra checks that PlayerMoveController::readInput does - sometimes it skips reading input, and returning something non-zero breaks stuff...
        var movementAllowed = bAdditiveUnk == 0 && _rmiWalkIsInputEnabled1(self) && _rmiWalkIsInputEnabled2(self) && !FollowpathActive();
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

        // Get AIConfig to check for the movement override key
        var aiConfig = Service.Config.Get<AI.AIConfig>();

        // Check if the override key is being held down
        bool overrideKeyPressed = false;
        switch (aiConfig.MovementOverrideKey)
        {
            case AI.AIConfig.ManualOverrideKey.Shift:
                overrideKeyPressed = ImGui.GetIO().KeyShift;
                break;
            case AI.AIConfig.ManualOverrideKey.Ctrl:
                overrideKeyPressed = ImGui.GetIO().KeyCtrl;
                break;
            case AI.AIConfig.ManualOverrideKey.Alt:
                overrideKeyPressed = ImGui.GetIO().KeyAlt;
                break;
        }

        // Check if user is attempting to move
        bool hasUserInput = UserMove != default;

        // Keep user in control for a short period (0.5 seconds) after they stop input
        // This prevents AI from immediately taking control when you briefly let go of keys
        bool extendedUserControl = overrideKeyPressed || (DateTime.Now - _lastUserControlTime).TotalSeconds < 0.5;

        // When override key is pressed and user is providing input,
        // keep their movement intact and don't apply AI movement
        if (overrideKeyPressed || (extendedUserControl && hasUserInput))
        {
            if (hasUserInput)
            {
                _lastUserControlTime = DateTime.Now;
            }

            // Only log when control changes from AI to user to avoid spamming
            if (!UserControlActive)
            {
                UserControlActive = true;
                Service.Log("Hybrid mode: Manual control active (key held)");
            }

            // User is in control - we already set ActualMove to UserMove above
        }
        else
        {
            // Only log when control changes from user to AI to avoid spamming
            if (UserControlActive)
            {
                UserControlActive = false;
                Service.Log("Hybrid mode: AI control resumed");
            }

            // movement override logic - AI takes control when override key is not pressed
            var allowAuto = movementAllowed ? !MovementBlocked : misdirectionMode;
            if (allowAuto && ActualMove == default && DirectionToDestination(false) is var relDir && relDir != null)
            {
                ActualMove = relDir.Value.h.ToDirection();
            }
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
        if (FollowpathActive())
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
