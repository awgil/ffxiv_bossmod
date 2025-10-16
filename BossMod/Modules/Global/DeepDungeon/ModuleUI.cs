using Dalamud.Bindings.ImGui;

namespace BossMod.Global.DeepDungeon;

abstract partial class AutoClear : ZoneModule
{
    bool _triggerboxView;

    private int DrawMap(Actor? player, int playerSlot)
    {
        if (Service.IsDev)
            ImGui.Checkbox("Geometry view", ref _triggerboxView);

        if (_triggerboxView)
        {
            DrawBoxes(player);
            return -1;
        }

        return new Minimap(Palace, player?.Rotation ?? default, DesiredRoom, Math.Max(0, playerSlot)).Draw();
    }

    public override void DrawExtra()
    {
        var player = World.Party.Player();
        var playerSlot = Array.FindIndex(Palace.Party, p => p.EntityId == player?.InstanceID);
        var targetRoom = DrawMap(player, playerSlot);
        if (targetRoom >= 0)
            DesiredRoom = targetRoom;

        ImGui.Text($"Kills: {Kills}");

        var maxPull = _config.MaxPull;

        ImGui.SetNextItemWidth(200);
        if (ImGui.DragInt("Max mobs to pull", ref maxPull, 0.05f, 0, 15))
        {
            _config.MaxPull = maxPull;
            _config.Modified.Fire();
        }

        if (!Service.IsDev)
            return;

        if (ImGui.Button("Reload obstacles"))
        {
            _obstacles.Dispose();
            _obstacles = new(World);
        }

        if (player == null)
            return;

        var (entry, data) = _obstacles.Find(player.PosRot.XYZ());
        if (entry == null)
        {
            ImGui.SameLine();
            UIMisc.HelpMarker(() => "Obstacle map missing for floor!", Dalamud.Interface.FontAwesomeIcon.ExclamationTriangle);
        }

        if (data != null && data.PixelSize != 0.5f)
        {
            ImGui.SameLine();
            UIMisc.HelpMarker(() => $"Wrong resolution for map; should be 0.5, got {data.PixelSize}", Dalamud.Interface.FontAwesomeIcon.ExclamationTriangle);
        }

        if (ImGui.Button("Set closest trap location as ignored"))
        {
            var pos = _trapsCurrentZone.Except(ProblematicTrapLocations).MinBy(t => (t - player.Position).LengthSq()).Rounded(0.1f);
            ProblematicTrapLocations.Add(pos);
            IgnoreTraps.Add(pos);
        }
    }
}
