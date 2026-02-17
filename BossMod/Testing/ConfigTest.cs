using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;

namespace BossMod.Testing;

class ConfigTest(ConfigUI configUI, ConfigRoot cfgRoot, IPluginLog logger) : TestWindow("Config", new(400, 400), ImGuiWindowFlags.None)
{
    private string _command = "";

    public override void Draw()
    {
        ImGui.InputText("##console", ref _command, 1024);
        ImGui.SameLine();
        if (ImGui.Button("Execute"))
        {
            var output = cfgRoot.ConsoleCommand(_command);
            foreach (var msg in output)
                logger.Debug(msg);
        }

        configUI.Draw();
    }
}
