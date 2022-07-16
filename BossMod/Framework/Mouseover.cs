using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using System;

namespace BossMod
{
    class Mouseover
    {
        public static Mouseover? Instance;
        public GameObject? Object { get; private set; }

        private delegate void SetUIMouseoverDelegate(ulong self, ulong ptr);
        private Hook<SetUIMouseoverDelegate> _setUIMouseoverHook;

        public Mouseover()
        {
            // that's a funny signature, lifted from MOAction - interesting function just sets a field at +0x290 to arg, but I dunno what is the 'this' pointer...
            var address = Service.SigScanner.ScanText("48 89 91 ?? ?? ?? ?? C3 CC CC CC CC CC CC CC CC 48 89 5C 24 ?? 55 56 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 8D B1 ?? ?? ?? ?? 44 89 44 24 ?? 48 8B EA 48 8B D9 48 8B CE 48 8D 15 ?? ?? ?? ?? 41 B9 ?? ?? ?? ??");
            _setUIMouseoverHook = Hook<SetUIMouseoverDelegate>.FromAddress(address, SetUIMouseoverDetour);
            _setUIMouseoverHook.Enable();
        }

        private void SetUIMouseoverDetour(ulong self, ulong ptr)
        {
            Object = Service.ObjectTable.CreateObjectReference((IntPtr)ptr);
            _setUIMouseoverHook.Original(self, ptr);
        }
    }
}
