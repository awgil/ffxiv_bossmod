using Dalamud.Game.ClientState.Keys;
using Dalamud.Hooking;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BossMod
{
    // utility for overriding keyboard input as seen in game
    // TODO: currently we don't handle cast-start while moving correctly, blocking movement on keypress is too late, cast gets cancelled anyway
    class InputOverride : IDisposable
    {
        private const int WM_KEYDOWN = 0x0100;

        public int[] GamepadOverrides = new int[7];
        public bool GamepadOverridesEnabled;

        private bool _movementBlocked = false;
        private ulong _hwnd;

        //private unsafe delegate int PeekMessageDelegate(ulong* lpMsg, void* hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
        //private Hook<PeekMessageDelegate> _peekMessageHook;

        private delegate void KbprocDelegate(ulong hWnd, uint uMsg, ulong wParam, ulong lParam, ulong uIdSubclass, ulong dwRefData);
        private Hook<KbprocDelegate> _kbprocHook;

        private delegate int GetGamepadAxisDelegate(ulong self, int axisID);
        private Hook<GetGamepadAxisDelegate> _getGamepadAxisHook;

        private delegate ref int GetRefValueDelegate(int vkCode);
        private GetRefValueDelegate _getKeyRef;

        public unsafe InputOverride()
        {
            var kbprocAddress = Service.SigScanner.ScanText("48 89 5C 24 08 55 56 57 41 56 41 57 48 8D 6C 24 B0 48 81 EC 50 01 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 40 4D 8B F9 49 8B D8 81 FA 00 01 00 00"); // note: look for callers of GetKeyboardState
            Service.Log($"[InputOverride] kbproc addess: 0x{kbprocAddress:X}");
            _kbprocHook = Hook<KbprocDelegate>.FromAddress(kbprocAddress, KbprocDetour);
            _kbprocHook.Enable();

            var getGamepadAxisAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 0F BE 0D ?? ?? ?? ?? BA 04 00 00 00 66 0F 6E F8 66 0F 6E C1 48 8B CE 0F 5B C0 0F 5B FF F3 0F 5E F8");
            Service.Log($"[InputOverride] GetGamepadAxis address: 0x{getGamepadAxisAddress:X}");
            _getGamepadAxisHook = Hook<GetGamepadAxisDelegate>.FromAddress(getGamepadAxisAddress, GetGamepadAxisDetour);
            _getGamepadAxisHook.Enable();

            _getKeyRef = Service.KeyState.GetType().GetMethod("GetRefValue", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<GetRefValueDelegate>(Service.KeyState);
        }

        public void Dispose()
        {
            _kbprocHook.Dispose();
            _getGamepadAxisHook.Dispose();
        }

        // TODO: reconsider...
        public bool IsMoving() => Service.KeyState[VirtualKey.W] || Service.KeyState[VirtualKey.S] || Service.KeyState[VirtualKey.A] || Service.KeyState[VirtualKey.D] || GamepadOverridesEnabled && (GamepadOverrides[3] != 0 || GamepadOverrides[4] != 0);
        public bool IsMoveRequested() => IsWindowActive() && (ReallyPressed(VirtualKey.W) || ReallyPressed(VirtualKey.S) || ReallyPressed(VirtualKey.A) || ReallyPressed(VirtualKey.D));

        public bool IsBlocked() => _movementBlocked;

        public void BlockMovement()
        {
            if (_movementBlocked)
                return;
            _movementBlocked = true;
            Block(VirtualKey.W);
            Block(VirtualKey.S);
            Block(VirtualKey.A);
            Block(VirtualKey.D);
            Service.Log("[InputOverride] Movement block started");
        }

        public void UnblockMovement()
        {
            if (!_movementBlocked)
                return;
            _movementBlocked = false;
            Unblock(VirtualKey.W);
            Unblock(VirtualKey.S);
            Unblock(VirtualKey.A);
            Unblock(VirtualKey.D);
            Service.Log("[InputOverride] Movement block ended");
        }

        public void SimulatePress(VirtualKey vk) => ForcePress(vk);
        public void SimulateRelease(VirtualKey vk)
        {
            if (!IsWindowActive() || !ReallyPressed(vk))
                ForceRelease(vk);
        }

        public void ForcePress(VirtualKey vk) => _getKeyRef((int)vk) = 3;
        public void ForceRelease(VirtualKey vk) => _getKeyRef((int)vk) = 0;

        private void Block(VirtualKey vk)
        {
            ForceRelease(vk);
        }

        private void Unblock(VirtualKey vk)
        {
            if (IsWindowActive() && ReallyPressed(vk))
            {
                ForcePress(vk);
            }
        }

        private bool ReallyPressed(VirtualKey vk)
        {
            return (GetKeyState((int)vk) & 0x8000) == 0x8000;
        }

        private bool IsWindowActive() => GetForegroundWindow() == _hwnd;

        private void KbprocDetour(ulong hWnd, uint uMsg, ulong wParam, ulong lParam, ulong uIdSubclass, ulong dwRefData)
        {
            if (_hwnd != hWnd)
            {
                _hwnd = hWnd;
                Service.Log($"[InputOverride] Changing active hwnd to {hWnd:X}");
            }
            if (_movementBlocked && uMsg == WM_KEYDOWN && (VirtualKey)wParam is VirtualKey.W or VirtualKey.S or VirtualKey.A or VirtualKey.D)
                return;
            _kbprocHook.Original(hWnd, uMsg, wParam, lParam, uIdSubclass, dwRefData);
        }

        private int GetGamepadAxisDetour(ulong self, int axisID)
        {
            return _movementBlocked && axisID is 3 or 4 ? 0
                : GamepadOverridesEnabled && axisID < GamepadOverrides.Length ? GamepadOverrides[axisID]
                : _getGamepadAxisHook.Original(self, axisID);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll", ExactSpelling = true)]
        private static extern ulong GetForegroundWindow();
    }
}
