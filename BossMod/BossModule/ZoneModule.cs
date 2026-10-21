using BossMod.Autorotation;
using BossMod.Interfaces;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;

namespace BossMod;

public sealed record ZoneModuleInit(
    WorldState World,
    ConfigRoot ConfigRoot,
    IUnlockState UnlockState,
    ICamera Camera,
    ActionDefinitions ActionDefinitions,
    IUiBuilder UiBuilder,
    ITextureProvider TextureProvider,
    MediatorService Mediator
)
{
    public delegate ZoneModuleInit Factory(WorldState World);
}

public abstract class ZoneModule(ZoneModuleInit init) : IDisposable
{
    public record struct WindowDescription(string Name, ImGuiWindowFlags Flags = ImGuiWindowFlags.None);

    public readonly WorldState World = init.World;
    protected readonly ArenaColor ArenaColor = new(init.ConfigRoot.Get<ColorConfig>());
    protected ICamera Camera => init.Camera;
    protected ActionDefinitions ActionDefinitions => init.ActionDefinitions;
    protected IUiBuilder UiBuilder => init.UiBuilder;
    protected MediatorService Mediator => init.Mediator;

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

    protected void DrawPseudoTitlebar(string title, Type configType, uint iconId, string closeTooltip, Action onClose)
    {
        if (iconId != 0 && init.TextureProvider.TryGetFromGameIcon(iconId, out var tex))
        {
            UIMisc.Image(tex, new(24 * ImGuiHelpers.GlobalScale));
            ImGui.SameLine();
        }

        ImGui.AlignTextToFramePadding();
        ImGui.Text(title);

        var w = 24 * ImGuiHelpers.GlobalScale;

        ImGui.SameLine(0, 0);
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X - w - w - 10);

        if (init.UiBuilder.IconButton(FontAwesomeIcon.Cog, new(w, 0)))
            init.Mediator.Publish(new ShowConfigNode(configType));
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Open settings");
        ImGui.SameLine();
        if (init.UiBuilder.IconButton(FontAwesomeIcon.SquareXmark, new(w, 0)))
            onClose.Invoke();
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(closeTooltip);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
    }
}
