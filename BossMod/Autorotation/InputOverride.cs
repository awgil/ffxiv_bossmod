using Dalamud.Game.ClientState.Keys;
using Dalamud.Hooking;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BossMod
{
    // utility for overriding keyboard input as seen in game
    class InputOverride
    {
        private bool _movementBlocked;

        private delegate ref int GetRefValueDelegate(int vkCode);
        private GetRefValueDelegate _getKeyRef;

        public InputOverride()
        {
            _getKeyRef = Service.KeyState.GetType().GetMethod("GetRefValue", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<GetRefValueDelegate>(Service.KeyState);
        }

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

        private void Block(VirtualKey vk)
        {
            if (Pressed(vk))
            {
                //InjectEvent(vk, false);
                _getKeyRef((int)vk) = 0;
            }
        }

        private void Unblock(VirtualKey vk)
        {
            if (Pressed(vk))
            {
                //InjectEvent(vk, true);
                _getKeyRef((int)vk) = 1;
            }
        }

        private bool Pressed(VirtualKey vk)
        {
            return (GetKeyState((int)vk) & 0x8000) == 0x8000;
        }

        private void InjectEvent(VirtualKey vk, bool press)
        {
            keybd_event((byte)vk, 0, press ? 0 : 2, 0);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
    }
}
