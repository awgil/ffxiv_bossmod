using ImGuiNET;

namespace BossMod.Stormblood.Foray.Hydatos;

[ConfigDisplay(Name = "Eureka", Parent = typeof(StormbloodConfig))]
public class EurekaConfig : ConfigNode
{
    [PropertyDisplay("Max range to look for new mobs to pull")]
    [PropertySlider(20, 100, Speed = 0.1f)]
    public float MaxPullDistance = 30f;

    [PropertyDisplay("Max number of mobs to pull at once (0 for no limit)")]
    [PropertySlider(0, 30, Speed = 0.1f)]
    public int MaxPullCount = 10;

    public bool AssistMode;
}

[ConfigDisplay(Name = "Hydatos", Parent = typeof(EurekaConfig))]
public class HydatosConfig : ConfigNode
{
    public NotoriousMonster CurrentFarmTarget = NotoriousMonster.None;
}

public enum NotoriousMonster : uint
{
    None,
    [PropertyDisplay("Khalamari (Xzomit)")]
    Khalamari,
    [PropertyDisplay("Stegodon (Hydatos Primelephas)")]
    Stego,
    [PropertyDisplay("Molech (Val Nullchu)")]
    Molech,
    [PropertyDisplay("Piasa (Vivid Gastornis)")]
    Piasa,
    [PropertyDisplay("Frostmane (Northern Tiger)")]
    Frostmane,
    [PropertyDisplay("Daphne (Dark Void Monk)")]
    Daphne,
    [PropertyDisplay("Goldemar (Hydatos Wraith)")]
    Golde,
    [PropertyDisplay("Leuke (Tigerhawk)")]
    Leuke,
    [PropertyDisplay("Barong (Laboratory Lion)")]
    Barong,
    [PropertyDisplay("Ceto (Hydatos Delphyne)")]
    Ceto,
    [PropertyDisplay("PW (Crystal Claw)")]
    PW
}

static class NMExtensions
{
    public static uint GetMobID(this NotoriousMonster opt) => opt switch
    {
        NotoriousMonster.Khalamari => 0x26AB,
        NotoriousMonster.Stego => 0x26AF,
        NotoriousMonster.Molech => 0x26B2,
        NotoriousMonster.Piasa => 0x26B3,
        NotoriousMonster.Frostmane => 0x26B8,
        NotoriousMonster.Daphne => 0x26B9,
        NotoriousMonster.Golde => 0x26E6,
        NotoriousMonster.Leuke => 0x26C0,
        NotoriousMonster.Barong => 0x26C2,
        NotoriousMonster.Ceto => 0x26C5,
        NotoriousMonster.PW => 0x26CA,
        _ => 0
    };

    public static uint GetFateID(this NotoriousMonster opt) => opt switch
    {
        NotoriousMonster.Khalamari => 1412,
        NotoriousMonster.Stego => 1413,
        NotoriousMonster.Molech => 1414,
        NotoriousMonster.Piasa => 1415,
        NotoriousMonster.Frostmane => 1416,
        NotoriousMonster.Daphne => 1417,
        NotoriousMonster.Golde => 1418,
        NotoriousMonster.Leuke => 1419,
        NotoriousMonster.Barong => 1420,
        NotoriousMonster.Ceto => 1421,
        NotoriousMonster.PW => 1423,
        _ => 0
    };
}

[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 639)]
public class Hydatos : ZoneModule
{
    private readonly EurekaConfig _eurekaConfig = Service.Config.Get<EurekaConfig>();
    private readonly HydatosConfig _hydatosConfig = Service.Config.Get<HydatosConfig>();

    private readonly EventSubscriptions _subscriptions;

    public Hydatos(WorldState ws) : base(ws)
    {
        _subscriptions = new(
            ws.Client.FateInfo.Subscribe(OnFateSpawned)
        );
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
        base.Dispose(disposing);
    }

    private void OnFateSpawned(ClientState.OpFateInfo fate)
    {
        if (_hydatosConfig.CurrentFarmTarget.GetFateID() == fate.FateId)
        {
            _hydatosConfig.CurrentFarmTarget = NotoriousMonster.None;
            _hydatosConfig.Modified.Fire();
        }
    }

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        hints.ForbiddenZones.RemoveAll(z => World.Actors.Find(z.Source) is Actor src && ShouldIgnore(src, player));

        var farmOID = _hydatosConfig.CurrentFarmTarget.GetMobID();
        var farmMax = _eurekaConfig.MaxPullCount;
        var farmRange = _eurekaConfig.MaxPullDistance;

        if (farmMax > 0 && hints.PotentialTargets.Count(e => e.Priority >= 0) >= farmMax)
            return;

        if (farmOID == 0)
            return;

        // only need to check "undesirable" targets, as mobs already attacking party members will be handled by autofarm
        bool canTarget(AIHints.Enemy enemy) => enemy.Priority == AIHints.Enemy.PriorityUndesirable && (!_eurekaConfig.AssistMode || enemy.Actor.InCombat);

        foreach (var e in hints.PotentialTargets.Where(t => t.Actor.OID == farmOID))
        {
            if (canTarget(e) && (e.Actor.Position - player.Position).LengthSq() <= farmRange * farmRange)
            {
                // only level 61+ wraiths spawn golde
                if (farmOID == 0x26E6 && e.Actor.ForayInfo.Level < 61)
                    continue;

                e.Priority = 0;

                var wePull = !e.Actor.InCombat;

                if (wePull && (hints.ForcedTarget == null || (hints.ForcedTarget.Position - player.Position).LengthSq() > (e.Actor.Position - player.Position).LengthSq()))
                    hints.ForcedTarget = e.Actor;
            }
        }
    }

    private bool ShouldIgnore(Actor caster, Actor player)
    {
        return caster.CastInfo != null
            && caster.CastInfo.Action.ID switch
            {
                15415 or 15416 => true,
                15449 or 15295 => caster.CastInfo.TargetID == player.InstanceID,
                _ => false,
            };
    }

    public override bool WantDrawExtra() => true;

    public override string WindowName() => "Hydatos###Eureka module";

    public override void DrawExtra()
    {
        if (UICombo.Enum("Prep", ref _hydatosConfig.CurrentFarmTarget))
            _hydatosConfig.Modified.Fire();

        ImGui.SetNextItemWidth(200);
        if (ImGui.DragFloat("Max distance to look for new mobs", ref _eurekaConfig.MaxPullDistance, 1, 20, 80))
            _eurekaConfig.Modified.Fire();
        ImGui.SetNextItemWidth(200);
        if (ImGui.DragInt("Max mobs to pull (set to 0 for no limit)", ref _eurekaConfig.MaxPullCount, 1, 0, 30))
            _eurekaConfig.Modified.Fire();

        if (ImGui.Checkbox("Assist mode (only attack mobs that are already in combat)", ref _eurekaConfig.AssistMode))
            _eurekaConfig.Modified.Fire();
    }
}
