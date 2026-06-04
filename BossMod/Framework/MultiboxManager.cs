#pragma warning disable CS9113 // Parameter is unread.
using BossMod.Autorotation;

namespace BossMod;

// TODO: use CS to hook chat message handlers, Dalamud chat handler is useless (missing most info including sender entity)
internal sealed class MultiboxManager(RotationModuleManager mgr, WorldState ws) : IDisposable
{
    public void Dispose()
    {
    }
}
