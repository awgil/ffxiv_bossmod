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

        private unsafe delegate int PeekMessageDelegate(ulong* lpMsg, void* hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
        private Hook<PeekMessageDelegate> _peekMessageHook;

        //private delegate void WndprocDelegate(ulong hWnd, uint uMsg, ulong wParam, ulong lParam, ulong uIdSubclass, ulong dwRefData);
        //private Hook<WndprocDelegate> _wndprocHook;

        private delegate ref int GetRefValueDelegate(int vkCode);
        private GetRefValueDelegate _getKeyRef;

        public unsafe InputOverride()
        {
            // note: it would be better to hook this instead of PeekMessage, but I didn't figure it out yet...
            //var wndprocAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 4C 8B CB 4C 8B C7 8B D6 48 8B CD 48 8B 5C 24 40 48 8B 6C 24 48 48 8B 74 24 50 48 83 C4 30 5F E9 0A B2 01 01"); // note: look for callers of GetKeyboardState
            //Service.Log($"Addr: {wndprocAddress}");
            //_wndprocHook = new(wndprocAddress, new WndprocDelegate(WndprocDetour));

            _peekMessageHook = Hook<PeekMessageDelegate>.FromSymbol("user32.dll", "PeekMessageW", PeekMessageDetour);
            _peekMessageHook.Enable();

            _getKeyRef = Service.KeyState.GetType().GetMethod("GetRefValue", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<GetRefValueDelegate>(Service.KeyState);
        }

        public void Dispose()
        {
            //_wndprocHook.Dispose();
            _peekMessageHook.Dispose();
        }

        // TODO: reconsider...
        public bool IsMoving()
        {
            return Service.KeyState[VirtualKey.W] || Service.KeyState[VirtualKey.S] || Service.KeyState[VirtualKey.A] || Service.KeyState[VirtualKey.D];
        }

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
            Service.Log("Movement block started");
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
            Service.Log("Movement block ended");
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

        //private void WndprocDetour(ulong hWnd, uint uMsg, ulong wParam, ulong lParam, ulong uIdSubclass, ulong dwRefData)
        //{
        //    if (_movementBlocked && uMsg == WM_KEYDOWN && (VirtualKey)wParam is VirtualKey.W or VirtualKey.S or VirtualKey.A or VirtualKey.D)
        //        return;
        //    _wndprocHook.Original(hWnd, uMsg, wParam, lParam, uIdSubclass, dwRefData);
        //}

        private unsafe int PeekMessageDetour(ulong* lpMsg, void* hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg)
        {
            do
            {
                var res = _peekMessageHook.Original(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
                if (res == 0)
                    return res;

                if (_movementBlocked && lpMsg[1] == WM_KEYDOWN && (VirtualKey)lpMsg[2] is VirtualKey.W or VirtualKey.S or VirtualKey.A or VirtualKey.D)
                {
                    // eat message
                    if ((wRemoveMsg & 1) == 0)
                    {
                        _peekMessageHook.Original(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg | 1);
                    }
                    continue;
                }

                return res;
            } while (true);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);
    }
}
