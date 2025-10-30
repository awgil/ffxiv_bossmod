using Dalamud.Bindings.ImGui;

namespace BossMod;

public class BossModuleConfigWindow : UIWindow
{
    private readonly ConfigNode? _node;
    private readonly PartyRolesConfig _prc = Service.Config.Get<PartyRolesConfig>();
    private readonly WorldState _ws;
    private readonly UITree _tree = new();
    private readonly UITabs _tabs = new();

    public BossModuleConfigWindow(BossModuleRegistry.Info info, WorldState ws) : base($"{info.ModuleType.Name} config", true, new(1200, 800))
    {
        _node = info.ConfigType != null ? Service.Config.Get<ConfigNode>(info.ConfigType) : null;
        _ws = ws;
        _tabs.Add("Encounter-specific config", DrawEncounterTab);
        _tabs.Add("Party roles assignment", DrawPartyRolesAssignmentsTab);
    }

    public override void Draw() => _tabs.Draw();

    private void DrawEncounterTab()
    {
        if (_node != null)
            ConfigUI.DrawNode(_node, Service.Config, _tree, _ws);
        else
            ImGui.TextUnformatted("This module does not expose any configuration");
    }

    private void DrawPartyRolesAssignmentsTab()
    {
        if (_ws.Party.Player() != null)
            ConfigUI.DrawNode(_prc, Service.Config, _tree, _ws);
    }
}
