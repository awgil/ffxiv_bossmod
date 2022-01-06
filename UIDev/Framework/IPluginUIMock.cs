using ImGuiScene;
using System;

namespace UIDev
{
    interface IPluginUIMock : IDisposable
    {
        void Initialize(SimpleImGuiScene scene);
    }
}
