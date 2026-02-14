using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;

namespace BossMod;

public abstract class ZoneModule(ZoneModuleArgs args) : IDisposable
{
    public WorldState World => args.World;
    public readonly ConfigRoot ConfigGlobal = args.Config;
    public readonly StandardColors Colors = new(args.Config.Get<ColorConfig>());
    protected ITextureProvider Tex => args.TextureProvider;

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
    public virtual bool WantDrawExtra() => false; // return true if it wants to draw something in a separate window
    public virtual void DrawExtra() { }
    public virtual string WindowName() => "";

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
