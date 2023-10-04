using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using System;
using System.Collections.Generic;

namespace BossMod
{
    public unsafe class DebugAddon : IDisposable
    {
        delegate nint ReceiveEventDelegate(AtkEventListener* self, AtkEventType eventType, uint eventParam, AtkEvent* eventData, ulong* inputData);

        private Dictionary<nint, Hook<ReceiveEventDelegate>> _rcvHooks = new();
        private Dictionary<string, nint> _addonRcvs = new();
        private string _newHook = "";

        public DebugAddon()
        {
        }

        public void Dispose()
        {
            foreach (var h in _rcvHooks.Values)
                h.Dispose();
        }

        public void Draw()
        {
            foreach (var (k, v) in _addonRcvs)
            {
                var hook = _rcvHooks[v];
                if (ImGui.Button($"{(hook.IsEnabled ? "Disable" : "Enable")} {k} ({v:X})"))
                {
                    if (hook.IsEnabled)
                        hook.Disable();
                    else
                        hook.Enable();
                }
            }

            ImGui.InputText("Addon name", ref _newHook, 256);
            if (_newHook.Length > 0 && !_addonRcvs.ContainsKey(_newHook) && (AtkUnitBase*)Service.GameGui.GetAddonByName(_newHook) is var addon && addon != null)
            {
                ImGui.SameLine();
                if (ImGui.Button("Hook!"))
                {
                    var address = (nint)addon->AtkEventListener.vfunc[2];
                    _addonRcvs[_newHook] = address;
                    if (!_rcvHooks.ContainsKey(address))
                    {
                        var name = _newHook;
                        Hook<ReceiveEventDelegate> hook = null!;
                        _rcvHooks[address] = hook = Service.Hook.HookFromAddress<ReceiveEventDelegate>(address, (self, eventType, eventParam, eventData, inputData) =>
                        {
                            Service.Log($"RCV: listener={name} {(nint)self:X}, type={eventType}, param={eventParam}, input={inputData[0]:X16} {inputData[1]:X16} {inputData[2]:X16}");
                            return hook.Original(self, eventType, eventParam, eventData, inputData);
                        });
                        hook.Enable();
                    }
                }
            }
        }
    }
}
