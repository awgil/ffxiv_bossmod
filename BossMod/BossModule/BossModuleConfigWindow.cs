using Dalamud.Interface.Utility.Raii;

namespace BossMod;

public class BossModuleConfigWindow(ModuleRegistry.Info info, WorldState ws) : UIWindow($"{info.ModuleType.Name} config", true, new(1200, 800))
{
    private readonly UITree _tree = new();

    public override void Draw()
    {
        if (info.ConfigType == null)
            return; // nothing to do...

        using var tabs = ImRaii.TabBar("Tabs");
        if (tabs)
        {
            using (var tab = ImRaii.TabItem("Encounter-specific config"))
                if (tab)
                    ConfigUI.DrawNode(Service.Config.Get<ConfigNode>(info.ConfigType), Service.Config, _tree, ws);
            if (ws.Party.Player() != null)
                using (var tab = ImRaii.TabItem("Party roles assignment"))
                    if (tab)
                        ConfigUI.DrawNode(Service.Config.Get<PartyRolesConfig>(), Service.Config, _tree, ws);
        }
    }
}
