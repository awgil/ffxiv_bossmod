using ImGuiNET;

namespace BossMod.Dawntrail.Foray.ForkedTower;

[ConfigDisplay(Parent = typeof(DawntrailConfig))]
class ForkedTowerConfig : ConfigNode
{
    public enum Alliance
    {
        [PropertyDisplay("None - only show generic hints")]
        None,
        A,
        B,
        C,
        [PropertyDisplay("D/1")]
        D1,
        [PropertyDisplay("E/2")]
        E2,
        [PropertyDisplay("F/3")]
        F3
    }

    [PropertyDisplay("Alliance assignment for hints")]
    public Alliance PlayerAlliance = Alliance.None;

    [PropertyDisplay("Enable config overlay while inside Forked Tower")]
    public bool DrawOverlay = true;
}

[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 1018)]
class FTAllianceSelector(WorldState ws) : ZoneModule(ws)
{
    private readonly ForkedTowerConfig _config = Service.Config.Get<ForkedTowerConfig>();

    public override bool WantDrawExtra() => _config.DrawOverlay;

    public override void DrawExtra()
    {
        var a = _config.PlayerAlliance;

        if (UICombo.Enum("Alliance assignment for hints", ref a))
        {
            _config.PlayerAlliance = a;
            _config.Modified.Fire();
        }

        if (ImGui.Button("Hide this window"))
        {
            _config.DrawOverlay = false;
            _config.Modified.Fire();
        }
    }
}

