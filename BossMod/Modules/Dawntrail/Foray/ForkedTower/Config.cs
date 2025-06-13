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

static class ConfigExtensions
{
    public static int Group2(this ForkedTowerConfig.Alliance a) => a switch
    {
        ForkedTowerConfig.Alliance.A or ForkedTowerConfig.Alliance.B or ForkedTowerConfig.Alliance.C => 1,
        ForkedTowerConfig.Alliance.D1 or ForkedTowerConfig.Alliance.E2 or ForkedTowerConfig.Alliance.F3 => 2,
        _ => 0
    };

    public static int Group3(this ForkedTowerConfig.Alliance a) => a switch
    {
        ForkedTowerConfig.Alliance.A or ForkedTowerConfig.Alliance.D1 => 1,
        ForkedTowerConfig.Alliance.B or ForkedTowerConfig.Alliance.E2 => 2,
        ForkedTowerConfig.Alliance.C or ForkedTowerConfig.Alliance.F3 => 3,
        _ => 0
    };
}

[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 1018)]
class FTAllianceSelector(WorldState ws) : ZoneModule(ws)
{
    private readonly ForkedTowerConfig _config = Service.Config.Get<ForkedTowerConfig>();

    public override bool WantDrawExtra() => _config.DrawOverlay && World.Party.Player()?.PosRot.Y < -200;

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

