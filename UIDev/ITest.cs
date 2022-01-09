using ImGuiNET;
using System;

namespace UIDev
{
    interface ITest : IDisposable
    {
        public void Draw();
        public ImGuiWindowFlags WindowFlags() { return ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse; }
    }
}
