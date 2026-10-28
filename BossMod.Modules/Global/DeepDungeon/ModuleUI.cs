using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod.Global.DeepDungeon;

abstract partial class AutoClear : ZoneModule
{
    private int DrawMap(Actor? player, int playerSlot)
    {
        var ix = new Minimap(Palace, _config, player?.Rotation ?? default, DesiredRoom.Index, Math.Max(0, playerSlot)).Draw();

        DrawBoxes(player);

        return ix;
    }

    public override WindowDescription? GetWindowDescription() => _config.EnableMinimap && !Palace.IsBossFloor ? new("VBM Deep Dungeon minimap##VBMDD", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize) : null;

    public override void DrawWindow(RotationModuleManager rmm)
    {
        DrawTraps();

        Overlays.DrawPseudoTitlebar("VBM Deep Dungeon map", typeof(AutoDDConfig), 61824, "Disable minimap", () =>
        {
            _config.EnableMinimap = false;
            _config.Modified.Fire();
        });

        ImGui.Text($"Kills: {Kills}");

        var player = World.Party.Player();
        var playerSlot = Array.FindIndex(Palace.Party, p => p.EntityId == player?.InstanceID);
        var targetRoom = DrawMap(player, playerSlot);
        if (targetRoom >= 0)
            DesiredRoom = (targetRoom, PathfindReason.Manual);

        using (ImRaii.Disabled(true))
        {
            if (DesiredRoom.Reason == PathfindReason.Nothing)
                ImGui.Text("Click on a room to set a destination.");
            else
                ImGui.Text($"Currently: {ReasonReadable(DesiredRoom.Reason)}");
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        if (Service.IsDev && MostInterestingObject is { } obj)
            Service.Camera.DrawWorldLine(player?.PosRot.XYZ() ?? default, obj.Actor.PosRot.XYZ(), ArenaColor.Danger, 2, false);

        if (_palantirError != null)
        {
            ImGui.Text("Couldn't connect to Palantir. Trap locations will not be displayed.");
            if (ImGui.Button("Retry"))
                FilterTraps();
        }

        var maxPull = _config.MaxPull;
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Max pull size");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(200);
        if (ImGui.SliderInt("###pullsize", ref maxPull, 0, 15))
        {
            _config.MaxPull = maxPull;
            _config.Modified.Fire();
        }

        if (ImGui.Button("Reload traps"))
        {
            _trapsCurrentZone.Clear();
            FilterTraps();
        }
    }
}
