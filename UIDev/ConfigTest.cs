using BossMod;
using ImGuiNET;

namespace UIDev
{
    class ConfigTest : ITest
    {
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
            _ui.Draw();
        }

        public ImGuiWindowFlags WindowFlags() => ImGuiWindowFlags.None;
    }
}
