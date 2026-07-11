using Dalamud.Bindings.ImGui;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Textures;
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

    readonly IFontHandle GaugeFont;

    readonly Dictionary<string, IDalamudTextureWrap> _tex = [];

    private GaugeVisualizer()
    {
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

    private IDalamudTextureWrap Generate(string key, Func<IDalamudTextureWrap> generate)
    {
        return _tex.TryGetValue(key, out var wrap) ? wrap : (_tex[key] = generate());
    }

    private IDalamudTextureWrap Wrap(string key, string path) => Generate(key, () =>
    {
        if (Service.LuminaGameData.GetFile<TexFile>(path) is { } tex)
        {
            var im = ImageData.FromTexFile(tex);
            return Service.Texture.CreateFromRaw(im.ImageSpecification, im.Data);
        }
        else
            return Service.Texture.CreateEmpty(RawImageSpecification.Rgba32(1, 1), true, true);
    });

    private IDalamudTextureWrap Tint(string key, string path, int[] color) => Generate(key, () =>
    {
        if (Service.LuminaGameData.GetFile<TexFile>(path) is { } tex)
        {
            var id = ImageData.FromTexFile(tex);

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

            return Service.Texture.CreateFromRaw(id.ImageSpecification, id.Data);
        }
        else
            return Service.Texture.CreateEmpty(RawImageSpecification.Rgba32(1, 1), true, true);
    });

    static Vector2 Scale(float x, float y) => new(x * ImGuiHelpers.GlobalScale, y * ImGuiHelpers.GlobalScale);

    void DrawBar(Vector2 origin, string key, float current, float maximum, int[] color, bool showText = false)
    {
        var dl = ImGui.GetWindowDrawList();

        ImGui.SetCursorPos(origin);
        var crAbs = ImGui.GetCursorScreenPos();
        // -7px on left and right edges (i think ninegrid handles this normally)
        dl.PushClipRect(crAbs + Scale(7, 0), crAbs + Scale(153, 20), true);
        ImGui.Image(Tint(key, "ui/uld/Parameter_Gauge_hr1.tex", color).Handle, Scale(current * 160 / maximum, 20), new Vector2(0, 0.2632f), new Vector2(1, 0.3947f));
        dl.PopClipRect();

        ImGui.SetCursorPos(origin);
        ImGui.Image(Wrap("gauge_simple", "ui/uld/Parameter_Gauge_hr1.tex").Handle, Scale(160, 20), new Vector2(0, 0), new Vector2(1, 0.1316f));

        if (showText)
        {
            var textOrig = crAbs + Scale(125, 11);
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
        ImGui.SetCursorPos(origin);
        ImGui.Image(Wrap("diamond", "ui/uld/JobHudSimple_StackA_hr1.tex").Handle, Scale(32, 32), new Vector2(0, 0), new Vector2(0.5f, 0.5f));

        if (active)
        {
            ImGui.SetCursorPos(origin);
            ImGui.Image(Tint(key, "ui/uld/JobHudSimple_StackA_hr1.tex", activeColor).Handle, Scale(32, 32), new Vector2(0.5f, 0), new Vector2(1, 0.5f));
        }
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
                ImGui.Image(wrap.Handle, Scale(28, 28), new Vector2(0.8526f, 0.f), new Vector2(0.9263f, 0.0946f));
        }
    }

    private void DrawMNK(Actor player, ClientState clientState)
    {
        var gauge = clientState.GetGauge<MonkGauge>();
        var origin = ImGui.GetCursorPos();

        if (!Service.Texture.GetFromGame("ui/uld/JobHudMNK0_hr1.tex").TryGetWrap(out var wrap, out _))
            return;

        // beast chakra
        ImGui.Image(wrap.Handle, Scale(114, 48), new Vector2(0, 0.4602f), new Vector2(0.2767f, 0.6726f));

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
                    ImGui.Image(wrap.Handle, Scale(32, 32), new Vector2(0.432f, 0.4602f), new Vector2(0.5097f, 0.6018f));
                    break;
                case BeastChakraType.Coeurl:
                    ImGui.SetCursorPos(origin + Scale(8 + 30 * num, 8));
                    ImGui.Image(wrap.Handle, Scale(32, 32), new Vector2(0.3544f, 0.4602f), new Vector2(0.432f, 0.6018f));
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
            ImGui.Image(Tint("mnk_purple", "ui/uld/JobHudMNK0_hr1.tex", [60, -20, 255]).Handle, Scale(30, 30), new Vector2(0.4612f, 0.6018f), new Vector2(0.3883f, 0.7345f));
        }

        if (gauge.Nadi.HasFlag(NadiFlags.Solar))
        {
            ImGui.SetCursorPos(origin + Scale(118, 9));
            ImGui.Image(Tint("mnk_yellow", "ui/uld/JobHudMNK0_hr1.tex", [100, 50, -40]).Handle, Scale(30, 30), new Vector2(0.3883f, 0.6018f), new Vector2(0.4612f, 0.7345f));
        }

        // balls
        ImGui.SetCursorPos(origin + Scale(0, 42));
        ImGui.Image(wrap.Handle, Scale(32, 32), new Vector2(0.4126f, 0.7434f), new Vector2(0.4903f, 0.885f));
        DrawDiamond(origin + Scale(26, 42), "stacks_opo", gauge.OpoOpoStacks == 1, [80, -80, 0]);

        ImGui.SetCursorPos(origin + Scale(50, 42));
        ImGui.Image(wrap.Handle, Scale(32, 32), new Vector2(0.4903f, 0.7434f), new Vector2(0.568f, 0.885f));
        DrawDiamond(origin + Scale(76, 42), "stacks_raptor", gauge.RaptorStacks == 1, [-50, -100, 255]);

        ImGui.SetCursorPos(origin + Scale(100, 42));
        ImGui.Image(wrap.Handle, Scale(32, 32), new Vector2(0.568f, 0.7434f), new Vector2(0.6456f, 0.885f));
        DrawDiamond(origin + Scale(126, 42), "stacks_coeurl", gauge.CoeurlStacks > 0, [-255, 60, 50]);
        DrawDiamond(origin + Scale(145, 42), "stacks_coeurl", gauge.CoeurlStacks == 2, [-255, 60, 50]);

        var chevron = Wrap("stack_b", "ui/uld/JobHudSimple_StackB_hr1.tex");
        var chevronLit = Tint("stack_b_active", "ui/uld/JobHudSimple_StackB_hr1.tex", [95, 0, -115]);
        var chevronOvercap = Tint("stack_b_overcap", "ui/uld/JobHudSimple_StackB_hr1.tex", [0, -168, -255]);

        for (var i = 0; i < 5; i++)
        {
            ImGui.SetCursorPos(origin + Scale(i * 18, 75));
            ImGui.Image(chevron.Handle, new Vector2(32, 32), new Vector2(0, 0), new Vector2(0.5f, 0.5f));
            if (gauge.Chakra > i)
            {
                ImGui.SetCursorPos(origin + Scale(i * 18, 75));
                ImGui.Image(chevronLit.Handle, new Vector2(32, 32), new Vector2(0.5f, 0), new Vector2(1, 0.5f));
            }
            if (gauge.Chakra > i + 5)
            {
                ImGui.SetCursorPos(origin + Scale(i * 18, 75));
                ImGui.Image(chevronOvercap.Handle, new Vector2(32, 32), new Vector2(0.5f, 0), new Vector2(1, 0.5f));
            }
        }
    }
}
