using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;

namespace BossMod;

public class BossModuleConfigWindow : UIWindow
{
    private readonly ConfigNode? _node;
    private readonly PartyRolesConfig _prc;
    private readonly ConfigRoot cfgRoot;
    private readonly WorldState _ws;
    private readonly UITree _tree = new();
    private readonly UITabs _tabs = new();
    private readonly ITextureProvider _tex;

    public BossModuleConfigWindow(BossModuleRegistry.Info info, ConfigRoot cfgRoot, WorldState ws, ITextureProvider tex) : base($"{info.ModuleType.Name} config", true, new(1200, 800))
    {
        _prc = cfgRoot.Get<PartyRolesConfig>();
        _node = info.ConfigType != null ? cfgRoot.Get<ConfigNode>(info.ConfigType) : null;
        this.cfgRoot = cfgRoot;
        _ws = ws;
        _tex = tex;
        _tabs.Add("Encounter-specific config", DrawEncounterTab);
        _tabs.Add("Party roles assignment", DrawPartyRolesAssignmentsTab);
    }

    public override void Draw() => _tabs.Draw();

    private void DrawEncounterTab()
    {
        if (_node != null)
            ConfigUI.DrawNode(_node, cfgRoot, _tree, _ws, _tex);
        else
            ImGui.TextUnformatted("This module does not expose any configuration");
    }

    private void DrawPartyRolesAssignmentsTab()
    {
        if (_ws.Party.Player() != null)
            ConfigUI.DrawNode(_prc, cfgRoot, _tree, _ws, _tex);
    }
}
