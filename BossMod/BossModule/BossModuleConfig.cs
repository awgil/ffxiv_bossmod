using ImGuiNET;

namespace BossMod
{
    public class BossModuleConfig : ConfigNode
    {
        public float ArenaScale = 1;
        public bool Enable = true;
        public bool Lock = false;
        public bool RotateArena = true;
        public bool ShowCardinals = false;
        public bool ShowWaymarks = false;
        public bool ShowMechanicTimers = true;
        public bool ShowGlobalHints = true;
        public bool ShowPlayerHints = true;
        public bool ShowControlButtons = true;
        public bool TrishaMode = false;
        public bool OpaqueArenaBackground = false;
        public bool ShowWorldArrows = false;
        public bool ShowDemo = false;

        protected override void DrawContents()
        {
            if (ImGui.DragFloat("Arena scale factor", ref ArenaScale, 0.1f, 0.1f, 10, "%.1f", ImGuiSliderFlags.Logarithmic))
                NotifyModified();
            DrawProperty(ref Enable, "Enable boss modules");
            DrawProperty(ref Lock, "Lock movement and mouse interaction");
            DrawProperty(ref RotateArena, "Rotate map to match camera orientation");
            DrawProperty(ref ShowCardinals, "Show cardinal direction names");
            DrawProperty(ref ShowWaymarks, "Show waymarks on radar");
            DrawProperty(ref ShowMechanicTimers, "Show mechanics sequence and timers");
            DrawProperty(ref ShowGlobalHints, "Show raidwide hints");
            DrawProperty(ref ShowPlayerHints, "Show warnings and hints for player");
            DrawProperty(ref ShowControlButtons, "Show control buttons under radar");
            DrawProperty(ref TrishaMode, "Trisha mode: show radar without window");
            DrawProperty(ref OpaqueArenaBackground, "Add opaque background to the arena");
            DrawProperty(ref ShowWorldArrows, "Show movement hints in world");
            DrawProperty(ref ShowDemo, "Show boss module demo out of instances (useful for configuring windows)");
        }

        protected override string? NameOverride() => "Boss modules settings";
    }
}
