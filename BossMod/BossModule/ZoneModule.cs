using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace BossMod;

public abstract class ZoneModule(WorldState ws) : IDisposable
{
    public readonly WorldState World = ws;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public virtual void Update() { }
    public virtual bool WantToBeDrawn() => false; // return true if it wants to be drawn (higher priority than inactive boss modules, but lower priority than active)
    public virtual List<string> CalculateGlobalHints() => [];
    public virtual void CalculateAIHints(int playerSlot, Actor player, AIHints hints) { } // note: this is called after framework automatically fills auto-detected hints
    public virtual void DrawExtra() { }

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
