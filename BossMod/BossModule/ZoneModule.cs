using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;

namespace BossMod;

public sealed record ZoneModuleInit(WorldState World, ConfigRoot ConfigRoot, IUnlockState UnlockState)
{
    public delegate ZoneModuleInit Factory(WorldState World);
}

public abstract class ZoneModule(ZoneModuleInit init) : IDisposable
{
    public record struct WindowDescription(string Name, ImGuiWindowFlags Flags = ImGuiWindowFlags.None);

    public readonly WorldState World = init.World;
    public readonly ArenaColor ArenaColor = new(init.ConfigRoot.Get<ColorConfig>());

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public virtual void Update() { }
    public virtual bool WantDrawHints() => false; // return true if it wants to draw hints (higher priority than inactive boss modules, but lower priority than active)
    public virtual List<string> CalculateGlobalHints() => [];
    public virtual void CalculateAIHints(int playerSlot, Actor player, AIHints hints) { } // note: this is called after framework automatically fills auto-detected hints

    public virtual WindowDescription? GetWindowDescription() => null;
    public virtual void DrawWindow(RotationModuleManager rmm) { }
    public virtual void OnWindowClose() { }

    public void DrawGlobalHints()
    {
        using var color = ImRaii.PushColor(ImGuiCol.Text, 0xffffff00);
        foreach (var hint in CalculateGlobalHints())
        {
            ImGui.TextUnformatted(hint);
            //ImGui.SameLine();
        }
        //ImGui.NewLine();
    }
}
