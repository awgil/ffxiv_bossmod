using Dalamud.Bindings.ImGui;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using Lumina.Data.Files;

namespace BossMod.ReplayVisualization;

internal class GaugeVisualizer
{
    private static GaugeVisualizer? _instance;

    public static GaugeVisualizer Instance()
    {
        _instance ??= new();
        return _instance;
    }

    public static void Dispose()
    {
        _instance?.DisposeInstance();
        _instance = null;
    }

    void DisposeInstance()
    {
        foreach (var t in _tex.Values)
            t.Dispose();

        _tex.Clear();
        GaugeFont.Dispose();
    }

    readonly TexFile GaugeSimple;
    readonly TexFile StackA;
    readonly TexFile StackB;
    readonly TexFile HudMNK;
    readonly IFontHandle GaugeFont;

    readonly Dictionary<string, IDalamudTextureWrap> _tex = [];

    private GaugeVisualizer()
    {
        GaugeSimple = Service.LuminaGameData.GetFile<TexFile>("ui/uld/Parameter_Gauge_hr1.tex")!;
        StackA = Service.LuminaGameData.GetFile<TexFile>("ui/uld/JobHudSimple_StackA_hr1.tex")!;
        StackB = Service.LuminaGameData.GetFile<TexFile>("ui/uld/JobHudSimple_StackB_hr1.tex")!;
        HudMNK = Service.LuminaGameData.GetFile<TexFile>("ui/uld/JobHudMNK0_hr1.tex")!;

        GaugeFont = Service.FontAtlas.NewGameFontHandle(new GameFontStyle(GameFontFamilyAndSize.MiedingerMid18));
    }

    public void Draw(Actor player, ClientState clientState)
    {
        ImGui.SameLine();
        switch (player.Class)
        {
            case Class.PLD:
                DrawPLD(player, clientState);
                break;
            case Class.DRK:
                DrawDRK(player, clientState);
                break;
            case Class.MNK:
                DrawMNK(player, clientState);
                break;
        }
    }

    private IDalamudTextureWrap Wrap(TexFile tf, string key)
    {
        if (_tex.TryGetValue(key, out var wrap))
            return wrap;

        var id = ImageData.FromTexFile(tf);

        wrap = Service.Texture.CreateFromRaw(id.ImageSpecification, id.Data);
        _tex[key] = wrap;
        return wrap;
    }

    private IDalamudTextureWrap Tint(TexFile tf, int[] color, string key)
    {
        if (_tex.TryGetValue(key, out var wrap))
            return wrap;

        var id = ImageData.FromTexFile(tf);

        uint tint = 0;
        uint bias = 0;

        for (var i = 0; i < 3; i++)
        {
            var col = color[i];
            if (col > 0)
                tint |= (uint)col << (i * 8);
            else
                bias |= (uint)-col << (i * 8);
        }

        id = id.Tint(tint, bias);

        wrap = Service.Texture.CreateFromRaw(id.ImageSpecification, id.Data);
        _tex[key] = wrap;
        return wrap;
    }

    static Vector2 Scale(float x, float y) => new(x * ImGuiHelpers.GlobalScale, y * ImGuiHelpers.GlobalScale);

    void DrawBar(Vector2 origin, string key, float current, float maximum, int[] color, bool showText = false)
    {
        var dl = ImGui.GetWindowDrawList();

        ImGui.SetCursorPos(origin);
        var crMin = origin + ImGui.GetWindowPos();
        // -7px on left and right edges (i think ninegrid handles this normally)
        dl.PushClipRect(crMin + Scale(7, 0), crMin + Scale(153, 20));
        ImGui.Image(Tint(GaugeSimple, color, key).Handle, Scale(current * 160 / maximum, 20), new Vector2(0.0000f, 0.2632f), new Vector2(1, 0.3947f));
        dl.PopClipRect();

        ImGui.SetCursorPos(origin);
        ImGui.Image(Wrap(GaugeSimple, "gauge_simple").Handle, Scale(160, 20), new Vector2(0.0000f, 0.0000f), new Vector2(1.0000f, 0.1316f));

        if (showText)
        {
            var textOrig = crMin + Scale(125, 11);
            var text = MathF.Floor(current).ToString();
            if (GaugeFont.Available)
            {
                using var gaugeFont = GaugeFont.Lock();
                ImGui.GetWindowDrawList().AddText(gaugeFont.ImFont, 18 * ImGuiHelpers.GlobalScale, textOrig, 0xFFFFFFFF, text);
            }
            else
                ImGui.GetWindowDrawList().AddText(textOrig, 0xFFFFFFFF, text);
        }
    }

    void DrawDiamond(Vector2 origin, string key, bool active, int[] activeColor)
    {
        var orig = ImGui.GetCursorPos();

        ImGui.SetCursorPos(origin);
        ImGui.Image(Wrap(StackA, "diamond").Handle, Scale(32, 32), new Vector2(0.0000f, 0.0000f), new Vector2(0.5000f, 0.5000f));

        if (active)
        {
            ImGui.SetCursorPos(origin);
            ImGui.Image(Tint(StackA, activeColor, key).Handle, Scale(32, 32), new Vector2(0.5f, 0), new Vector2(1, 0.5f));
        }

        ImGui.SetCursorPos(orig);
    }

    void DrawDiamond(Vector2 origin, bool active) => DrawDiamond(origin, "diamond_active", active, [20, -50, -150]);

    private void DrawPLD(Actor player, ClientState clientState)
    {
        var gauge = clientState.GetGauge<PaladinGauge>();
        var stance = player.FindStatus(PLD.SID.IronWill) != null;
        var origin = ImGui.GetCursorPos();

        if (stance)
            DrawBar(origin + Scale(28, 4), "oath_stance", gauge.OathGauge, 100, [130, 100, -20], true);
        else
            DrawBar(origin + Scale(28, 4), "oath_normal", gauge.OathGauge, 100, [-50, 50, 120], true);

        if (stance && Service.Texture.GetFromGame("ui/uld/JobHudPLD_hr1.tex").TryGetWrap(out var wrap, out _))
        {
            ImGui.SetCursorPos(origin);
            ImGui.Image(wrap.Handle, Scale(32, 32), new Vector2(0.5265f, 0.8333f), new Vector2(0.6018f, 0.9143f));
        }
    }

    private void DrawDRK(Actor player, ClientState clientState)
    {
        var gauge = clientState.GetGauge<DarkKnightGauge>();
        var stance = player.FindStatus(DRK.SID.Grit) != null;
        var origin = ImGui.GetCursorPos();

        DrawBar(origin + Scale(28, 0), "darkside", gauge.DarksideTimer * 0.001f, 60, [80, -60, 50], true);
        DrawDiamond(origin + Scale(30, 8), gauge.DarkArtsState == 1);
        if (stance)
            DrawBar(origin + Scale(28, 40), "blood_stance", gauge.Blood, 100, [10, -50, 200], true);
        else
            DrawBar(origin + Scale(28, 40), "blood_normal", gauge.Blood, 100, [150, 0, 0], true);

        if (stance)
        {
            ImGui.SetCursorPos(origin + Scale(0, 36));
            if (Service.Texture.GetFromGame("ui/uld/JobHudDRK0_hr1.tex").TryGetWrap(out var wrap, out _))
                ImGui.Image(wrap.Handle, Scale(28, 28), new Vector2(0.8526f, 0.0000f), new Vector2(0.9263f, 0.0946f));
        }
    }

    private void DrawMNK(Actor player, ClientState clientState)
    {
        var gauge = clientState.GetGauge<MonkGauge>();
        var origin = ImGui.GetCursorPos();

        var wrap = Wrap(HudMNK, "mnk_hud");

        // beast chakra
        ImGui.Image(wrap.Handle, Scale(114, 48), new Vector2(0.0000f, 0.4602f), new Vector2(0.2767f, 0.6726f));

        void drawChakra(int num)
        {
            switch (gauge.BeastChakra[num])
            {
                case BeastChakraType.OpoOpo:
                    ImGui.SetCursorPos(origin + Scale(8 + 30 * num, 8));
                    ImGui.Image(wrap.Handle, Scale(32, 32), new Vector2(0.2767f, 0.4602f), new Vector2(0.3544f, 0.6018f));
                    break;
                case BeastChakraType.Raptor:
                    ImGui.SetCursorPos(origin + Scale(8 + 30 * num, 8));
                    ImGui.Image(wrap.Handle, Scale(32, 32), new Vector2(0.4320f, 0.4602f), new Vector2(0.5097f, 0.6018f));
                    break;
                case BeastChakraType.Coeurl:
                    ImGui.SetCursorPos(origin + Scale(8 + 30 * num, 8));
                    ImGui.Image(wrap.Handle, Scale(32, 32), new Vector2(0.3544f, 0.4602f), new Vector2(0.4320f, 0.6018f));
                    break;
            }
        }

        drawChakra(0);
        drawChakra(1);
        drawChakra(2);

        // nadis
        ImGui.SetCursorPos(origin + Scale(100, 11));
        ImGui.Image(wrap.Handle, Scale(46, 30), new Vector2(0.2767f, 0.6018f), new Vector2(0.3883f, 0.7345f));

        if (gauge.Nadi.HasFlag(NadiFlags.Lunar))
        {
            ImGui.SetCursorPos(origin + Scale(98, 9));
            ImGui.Image(Tint(HudMNK, [60, -20, 255], "mnk_purple").Handle, Scale(30, 30), new Vector2(0.4612f, 0.6018f), new Vector2(0.3883f, 0.7345f));
        }

        if (gauge.Nadi.HasFlag(NadiFlags.Solar))
        {
            ImGui.SetCursorPos(origin + Scale(118, 9));
            ImGui.Image(Tint(HudMNK, [100, 50, -40], "mnk_yellow").Handle, Scale(30, 30), new Vector2(0.3883f, 0.6018f), new Vector2(0.4612f, 0.7345f));
        }

        // balls
        ImGui.SetCursorPos(origin + Scale(0, 42));
        ImGui.Image(wrap.Handle, Scale(32, 32), new Vector2(0.4126f, 0.7434f), new Vector2(0.4903f, 0.8850f));
        DrawDiamond(origin + Scale(26, 42), "stacks_opo", gauge.OpoOpoStacks == 1, [80, -80, 0]);

        ImGui.SetCursorPos(origin + Scale(50, 42));
        ImGui.Image(wrap.Handle, Scale(32, 32), new Vector2(0.4903f, 0.7434f), new Vector2(0.5680f, 0.8850f));
        DrawDiamond(origin + Scale(76, 42), "stacks_raptor", gauge.RaptorStacks == 1, [-50, -100, 255]);

        ImGui.SetCursorPos(origin + Scale(100, 42));
        ImGui.Image(wrap.Handle, Scale(32, 32), new Vector2(0.5680f, 0.7434f), new Vector2(0.6456f, 0.8850f));
        DrawDiamond(origin + Scale(126, 42), "stacks_coeurl", gauge.CoeurlStacks > 0, [-255, 60, 50]);
        DrawDiamond(origin + Scale(145, 42), "stacks_coeurl", gauge.CoeurlStacks == 2, [-255, 60, 50]);

        var chevron = Wrap(StackB, "stack_b");
        var chevronLit = Tint(StackB, [95, 0, -115], "stack_b_active");
        var chevronOvercap = Tint(StackB, [0, -168, -255], "stack_b_overcap");

        for (var i = 0; i < 5; i++)
        {
            ImGui.SetCursorPos(origin + Scale(i * 18, 75));
            ImGui.Image(chevron.Handle, new Vector2(32, 32), new Vector2(0.0000f, 0.0000f), new Vector2(0.5000f, 0.5000f));
            if (gauge.Chakra > i)
            {
                ImGui.SetCursorPos(origin + Scale(i * 18, 75));
                ImGui.Image(chevronLit.Handle, new Vector2(32, 32), new Vector2(0.5000f, 0.0000f), new Vector2(1.0000f, 0.5000f));
            }
            if (gauge.Chakra > i + 5)
            {
                ImGui.SetCursorPos(origin + Scale(i * 18, 75));
                ImGui.Image(chevronOvercap.Handle, new Vector2(32, 32), new Vector2(0.5000f, 0.0000f), new Vector2(1.0000f, 0.5000f));
            }
        }
    }
}
