using Dalamud.Interface.Utility.Raii;

namespace BossMod;

public class BossModuleConfigWindow : UIWindow
{
    private ModuleRegistry.Info _info;
    private WorldState _ws;
    private UITree _tree = new();

    public BossModuleConfigWindow(ModuleRegistry.Info info, WorldState ws) : base($"{info.ModuleType.Name} config", true, new(1200, 800))
    {
        _info = info;
        _ws = ws;
    }

    public override void Draw()
    {
        if (_info.ConfigType == null)
            return; // nothing to do...

        using var tabs = ImRaii.TabBar("Tabs");
        if (tabs)
        {
            using (var tab = ImRaii.TabItem("Encounter-specific config"))
                if (tab)
                    ConfigUI.DrawNode(Service.Config.Get<ConfigNode>(_info.ConfigType), Service.Config, _tree, _ws);
            if (_ws.Party.Player() != null)
                using (var tab = ImRaii.TabItem("Party roles assignment"))
                    if (tab)
                        ConfigUI.DrawNode(Service.Config.Get<PartyRolesConfig>(), Service.Config, _tree, _ws);
        }
    }
}
