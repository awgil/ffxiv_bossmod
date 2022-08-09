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

        private bool _movementBlocked = false;

        //private unsafe delegate int PeekMessageDelegate(ulong* lpMsg, void* hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
        //private Hook<PeekMessageDelegate> _peekMessageHook;

        private delegate void KbprocDelegate(ulong hWnd, uint uMsg, ulong wParam, ulong lParam, ulong uIdSubclass, ulong dwRefData);
        private Hook<KbprocDelegate> _kbprocHook;

        private delegate ref int GetRefValueDelegate(int vkCode);
        private GetRefValueDelegate _getKeyRef;

        public unsafe InputOverride()
        {
            // note: it would be better to hook this instead of PeekMessage, but I didn't figure it out yet...
            var kbprocAddress = Service.SigScanner.ScanText("48 89 5C 24 08 55 56 57 41 56 41 57 48 8D 6C 24 B0 48 81 EC 50 01 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 40 4D 8B F9 49 8B D8 81 FA 00 01 00 00"); // note: look for callers of GetKeyboardState
            Service.Log($"[InputOverride] kbproc addess: 0x{kbprocAddress:X}");
            _kbprocHook = Hook<KbprocDelegate>.FromAddress(kbprocAddress, KbprocDetour);
            _kbprocHook.Enable();

            //_peekMessageHook = Hook<PeekMessageDelegate>.FromSymbol("user32.dll", "PeekMessageW", PeekMessageDetour);
            //_peekMessageHook.Enable();

            _getKeyRef = Service.KeyState.GetType().GetMethod("GetRefValue", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<GetRefValueDelegate>(Service.KeyState);
        }

        public void Dispose()
        {
            _kbprocHook.Dispose();
            //_peekMessageHook.Dispose();
        }

        // TODO: reconsider...
        public bool IsMoving() => Service.KeyState[VirtualKey.W] || Service.KeyState[VirtualKey.S] || Service.KeyState[VirtualKey.A] || Service.KeyState[VirtualKey.D];
        public bool IsMoveRequested() => ReallyPressed(VirtualKey.W) || ReallyPressed(VirtualKey.S) || ReallyPressed(VirtualKey.A) || ReallyPressed(VirtualKey.D);

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
            if (!ReallyPressed(vk))
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
            if (ReallyPressed(vk))
            {
                ForcePress(vk);
            }
        }

        private bool ReallyPressed(VirtualKey vk)
        {
            return (GetKeyState((int)vk) & 0x8000) == 0x8000;
        }

        private void KbprocDetour(ulong hWnd, uint uMsg, ulong wParam, ulong lParam, ulong uIdSubclass, ulong dwRefData)
        {
            if (_movementBlocked && uMsg == WM_KEYDOWN && (VirtualKey)wParam is VirtualKey.W or VirtualKey.S or VirtualKey.A or VirtualKey.D)
                return;
            _kbprocHook.Original(hWnd, uMsg, wParam, lParam, uIdSubclass, dwRefData);
        }

        //private unsafe int PeekMessageDetour(ulong* lpMsg, void* hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
        //{
        //    do
        //    {
        //        var res = _peekMessageHook.Original(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
        //        if (res == 0)
        //            return res;

        //        if (_movementBlocked && lpMsg[1] == WM_KEYDOWN && (VirtualKey)lpMsg[2] is VirtualKey.W or VirtualKey.S or VirtualKey.A or VirtualKey.D)
        //        {
        //            // eat message
        //            if ((wRemoveMsg & 1) == 0)
        //            {
        //                _peekMessageHook.Original(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg | 1);
        //            }
        //            continue;
        //        }

        //        return res;
        //    } while (true);
        //}

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);
    }
}
