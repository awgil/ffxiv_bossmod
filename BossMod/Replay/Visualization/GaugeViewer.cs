using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures.TextureWraps;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using Lumina.Data.Files;

namespace BossMod.ReplayVisualization;

internal class GaugeViewer
{
    private static GaugeViewer? _instance;

    public static GaugeViewer Instance()
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
    }

    readonly TexFile GaugeSimple;
    readonly TexFile StackA;

    private GaugeViewer()
    {
        GaugeSimple = Service.LuminaGameData.GetFile<TexFile>("ui/uld/Parameter_Gauge_hr1.tex") ?? throw new Exception("Unable to load texture from game");
        StackA = Service.LuminaGameData.GetFile<TexFile>("ui/uld/JobHudSimple_StackA_hr1.tex") ?? throw new Exception("Unable to load texture from game");
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

    public void Draw(Actor player, ClientState clientState)
    {
        ImGui.SameLine();
        switch (player.Class)
        {
            case Class.DRK:
                DrawDRK(player, clientState);
                break;
        }
    }

    private void DrawDRK(Actor player, ClientState clientState)
    {
        var gauge = clientState.GetGauge<DarkKnightGauge>();

        var stance = player.FindStatus(DRK.SID.Grit) != null;

        var origin = ImGui.GetCursorPos();

        DrawBar(origin + new Vector2(28, 0), "darkside", gauge.DarksideTimer * 0.001f, 60, [80, -60, 50], true);
        DrawDiamond(origin + new Vector2(30, 8), gauge.DarkArtsState == 1);
        if (stance)
            DrawBar(origin + new Vector2(28, 40), "blood_stance", gauge.Blood, 100, [10, -50, 200], true);
        else
            DrawBar(origin + new Vector2(28, 40), "blood_normal", gauge.Blood, 100, [150, 0, 0], true);

        if (stance)
        {
            ImGui.SetCursorPos(origin + new Vector2(0, 36));
            if (Service.Texture.GetFromGame("ui/uld/JobHudDRK0_hr1.tex").TryGetWrap(out var wrap, out _))
                ImGui.Image(wrap.Handle, new Vector2(28, 28), new Vector2(0.8526f, 0.0000f), new Vector2(0.9263f, 0.0946f));
        }
    }

    readonly Dictionary<string, IDalamudTextureWrap> _tex = [];

    void DrawBar(Vector2 origin, string key, float current, float maximum, int[] color, bool showText = false)
    {
        var dl = ImGui.GetWindowDrawList();

        ImGui.SetCursorPos(origin);
        var crMin = origin + ImGui.GetWindowPos();
        // -7px on left and right edges (i think ninegrid handles this normally)
        dl.PushClipRect(crMin + new Vector2(7, 0), crMin + new Vector2(153, 20));
        ImGui.Image(Tint(GaugeSimple, color, key).Handle, new(current * 160 / maximum, 20), new Vector2(0.0000f, 0.2632f), new Vector2(1, 0.3947f));
        dl.PopClipRect();

        ImGui.SetCursorPos(origin);
        ImGui.Image(Wrap(GaugeSimple, "gauge_simple").Handle, new(160, 20), new Vector2(0.0000f, 0.0000f), new Vector2(1.0000f, 0.1316f));

        if (showText)
            ImGui.GetWindowDrawList().AddText(Service.DefaultFont, 22, crMin + new Vector2(125, 11), 0xFFFFFFFF, MathF.Floor(current).ToString());
    }

    void DrawDiamond(Vector2 origin, bool active)
    {
        var orig = ImGui.GetCursorPos();

        ImGui.SetCursorPos(origin);
        ImGui.Image(Wrap(StackA, "diamond").Handle, new(32, 32), new Vector2(0.0000f, 0.0000f), new Vector2(0.5000f, 0.5000f));

        if (active)
        {
            ImGui.SetCursorPos(origin);
            ImGui.Image(Tint(StackA, [20, -50, -150], "diamond_active").Handle, new(32, 32), new Vector2(0.5f, 0), new Vector2(1, 0.5f));
        }

        ImGui.SetCursorPos(orig);
    }
}
