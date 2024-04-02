using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace BossMod;

public unsafe class DebugAddon : IDisposable
{
    delegate nint AddonReceiveEventDelegate(AtkEventListener* self, AtkEventType eventType, uint eventParam, AtkEvent* eventData, ulong* inputData);
    delegate void* AgentReceiveEventDelegate(AgentInterface* self, void* eventData, AtkValue* values, int valueCount, ulong eventKind);

    private Dictionary<nint, Hook<AddonReceiveEventDelegate>> _rcvAddonHooks = new();
    private Dictionary<nint, Hook<AgentReceiveEventDelegate>> _rcvAgentHooks = new();
    private Dictionary<string, nint> _addonRcvs = new();
    private Dictionary<uint, nint> _agentRcvs = new();
    private string _newHook = "";

    public DebugAddon()
    {
    }

    public void Dispose()
    {
        foreach (var h in _rcvAddonHooks.Values)
            h.Dispose();
        foreach (var h in _rcvAgentHooks.Values)
            h.Dispose();
    }

    public void Draw()
    {
        ImGui.TextUnformatted("Addons:");
        foreach (var (k, v) in _addonRcvs)
        {
            var hook = _rcvAddonHooks[v];
            if (ImGui.Button($"{(hook.IsEnabled ? "Disable" : "Enable")} {k} ({v:X})"))
            {
                if (hook.IsEnabled)
                    hook.Disable();
                else
                    hook.Enable();
            }
        }

        ImGui.TextUnformatted("Agents:");
        foreach (var (k, v) in _agentRcvs)
        {
            var hook = _rcvAgentHooks[v];
            if (ImGui.Button($"{(hook.IsEnabled ? "Disable" : "Enable")} {k} ({v:X})"))
            {
                if (hook.IsEnabled)
                    hook.Disable();
                else
                    hook.Enable();
            }
        }

        ImGui.InputText("Addon name / agent id", ref _newHook, 256);
        if (_newHook.Length > 0 && !_addonRcvs.ContainsKey(_newHook) && (AtkUnitBase*)Service.GameGui.GetAddonByName(_newHook) is var addon && addon != null)
        {
            ImGui.SameLine();
            if (ImGui.Button("Hook addon!"))
            {
                var address = (nint)addon->AtkEventListener.vfunc[2];
                _addonRcvs[_newHook] = address;
                if (!_rcvAddonHooks.ContainsKey(address))
                {
                    var name = _newHook;
                    Hook<AddonReceiveEventDelegate> hook = null!;
                    _rcvAddonHooks[address] = hook = Service.Hook.HookFromAddress<AddonReceiveEventDelegate>(address, (self, eventType, eventParam, eventData, inputData) =>
                    {
                        Service.Log($"RCV: listener={name} {(nint)self:X}, type={eventType}, param={eventParam}, input={inputData[0]:X16} {inputData[1]:X16} {inputData[2]:X16}");
                        return hook.Original(self, eventType, eventParam, eventData, inputData);
                    });
                    hook.Enable();
                }
            }
        }
        if (_newHook.Length > 0 && uint.TryParse(_newHook, out var agentId) && agentId > 0 && !_agentRcvs.ContainsKey(agentId) && AgentModule.Instance()->GetAgentByInternalID(agentId) is var agent && agent != null)
        {
            ImGui.SameLine();
            if (ImGui.Button("Hook agent!"))
            {
                var address = (nint)agent->VTable->ReceiveEvent;
                _agentRcvs[agentId] = address;
                if (!_rcvAgentHooks.ContainsKey(address))
                {
                    Hook<AgentReceiveEventDelegate> hook = null!;
                    _rcvAgentHooks[address] = hook = Service.Hook.HookFromAddress<AgentReceiveEventDelegate>(address, (self, eventData, values, valueCount, eventKind) =>
                    {
                        Service.Log($"RCV: listener={agentId} {(nint)self:X}, kind={eventKind}, values={AtkValuesString(values, valueCount)}");
                        return hook.Original(self, eventData, values, valueCount, eventKind);
                    });
                    hook.Enable();
                }
            }
        }
    }

    private string AtkValuesString(AtkValue* values, int count)
    {
        string res = "[";
        for (int i = 0; i < count; ++i)
        {
            if (i > 0)
                res += ", ";
            res += values[i].Type switch
            {
                FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Int => $"int {values[i].Int}",
                FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Bool => $"bool {values[i].Byte}",
                FFXIVClientStructs.FFXIV.Component.GUI.ValueType.UInt => $"uint {values[i].UInt}",
                FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Float => $"int {values[i].Float}",
                FFXIVClientStructs.FFXIV.Component.GUI.ValueType.String => $"string",
                FFXIVClientStructs.FFXIV.Component.GUI.ValueType.String8 => $"string8",
                FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Vector => $"vector",
                FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Texture => $"texture",
                FFXIVClientStructs.FFXIV.Component.GUI.ValueType.AllocatedString => $"astring",
                FFXIVClientStructs.FFXIV.Component.GUI.ValueType.AllocatedVector => $"avector",
                _ => $"{values[i].Type} unknown"
            };
        }
        res += "]";
        return res;
    }
}
