using BossMod;
using ImGuiNET;

namespace UIDev;

class ConfigTest : TestWindow
{
    private string _command = "";
    private readonly ConfigUI _ui;

    public ConfigTest() : base("Config", new(400, 400), ImGuiWindowFlags.None)
    {
        _ui = new(Service.Config, new(TimeSpan.TicksPerSecond, "fake"), null, null);
    }

    protected override void Dispose(bool disposing)
    {
        _ui.Dispose();
        base.Dispose(disposing);
    }

    public override void Draw()
    {
        ImGui.InputText("##console", ref _command, 1024);
        ImGui.SameLine();
        if (ImGui.Button("Execute"))
        {
            var output = Service.Config.ConsoleCommand(_command.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            foreach (var msg in output)
            {
                Service.Log(msg);
            }
        }

        _ui.Draw();
    }
}
