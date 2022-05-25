using BossMod;
using ImGuiNET;
using System;

namespace UIDev
{
    class ConfigTest : ITest
    {
        private string _command = "";
        private ConfigUI _ui;

        public ConfigTest()
        {
            _ui = new(Service.Config);
        }

        public void Dispose()
        {
        }

        public void Draw()
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

        public ImGuiWindowFlags WindowFlags() => ImGuiWindowFlags.None;
    }
}
