using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace BossMod;

public sealed unsafe class DebugAddon : IDisposable
{
    delegate nint AddonReceiveEventDelegate(AtkEventListener* self, AtkEventType eventType, uint eventParam, AtkEvent* eventData, ulong* inputData);
    delegate void* AgentReceiveEventDelegate(AgentInterface* self, void* eventData, AtkValue* values, int valueCount, ulong eventKind);

    private readonly Dictionary<nint, HookAddress<AddonReceiveEventDelegate>> _rcvAddonHooks = [];
    private readonly Dictionary<nint, HookAddress<AgentReceiveEventDelegate>> _rcvAgentHooks = [];
    private readonly Dictionary<string, nint> _addonRcvs = [];
    private readonly Dictionary<uint, nint> _agentRcvs = [];
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
            if (ImGui.Button($"{(hook.Enabled ? "Disable" : "Enable")} {k} ({v:X})"))
                hook.Enabled ^= true;
        }

        ImGui.TextUnformatted("Agents:");
        foreach (var (k, v) in _agentRcvs)
        {
            var hook = _rcvAgentHooks[v];
            if (ImGui.Button($"{(hook.Enabled ? "Disable" : "Enable")} {k} ({v:X})"))
                hook.Enabled ^= true;
        }

        ImGui.InputText("Addon name / agent id", ref _newHook, 256);
        if (_newHook.Length > 0 && !_addonRcvs.ContainsKey(_newHook) && (AtkUnitBase*)(Service.GameGui.GetAddonByName(_newHook).Address) is var addon && addon != null)
        {
            ImGui.SameLine();
            if (ImGui.Button("Hook addon!"))
            {
                var address = (nint)addon->VirtualTable->ReceiveEvent;
                _addonRcvs[_newHook] = address;
                if (!_rcvAddonHooks.ContainsKey(address))
                {
                    var name = _newHook;
                    HookAddress<AddonReceiveEventDelegate> hook = null!;
                    _rcvAddonHooks[address] = hook = new(address, (self, eventType, eventParam, eventData, inputData) =>
                    {
                        Service.Log($"RCV: listener={name} {(nint)self:X}, type={eventType}, param={eventParam}, input={inputData[0]:X16} {inputData[1]:X16} {inputData[2]:X16}");
                        return hook.Original(self, eventType, eventParam, eventData, inputData);
                    });
                }
            }
        }
        if (_newHook.Length > 0 && uint.TryParse(_newHook, out var agentId) && agentId > 0 && !_agentRcvs.ContainsKey(agentId) && AgentModule.Instance()->GetAgentByInternalId((AgentId)agentId) is var agent && agent != null)
        {
            ImGui.SameLine();
            if (ImGui.Button("Hook agent!"))
            {
                var address = (nint)agent->VirtualTable->ReceiveEvent;
                _agentRcvs[agentId] = address;
                if (!_rcvAgentHooks.ContainsKey(address))
                {
                    HookAddress<AgentReceiveEventDelegate> hook = null!;
                    _rcvAgentHooks[address] = hook = new(address, (self, eventData, values, valueCount, eventKind) =>
                    {
                        Service.Log($"RCV: listener={agentId} {(nint)self:X}, kind={eventKind}, values={AtkValuesString(values, valueCount)}");
                        return hook.Original(self, eventData, values, valueCount, eventKind);
                    });
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
                FFXIVClientStructs.FFXIV.Component.GUI.ValueType.Pointer => $"pointer",
                FFXIVClientStructs.FFXIV.Component.GUI.ValueType.ManagedString => $"astring",
                FFXIVClientStructs.FFXIV.Component.GUI.ValueType.ManagedVector => $"avector",
                _ => $"{values[i].Type} unknown"
            };
        }
        res += "]";
        return res;
    }
}
