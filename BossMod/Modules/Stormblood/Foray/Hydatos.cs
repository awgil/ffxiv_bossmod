using ImGuiNET;

namespace BossMod.Stormblood.Foray.Hydatos;

[ConfigDisplay(Name = "Eureka", Parent = typeof(StormbloodConfig))]
public class EurekaConfig : ConfigNode
{
    [PropertyDisplay("Max range to look for new mobs to pull")]
    [PropertySlider(20, 100, Speed = 0.1f)]
    public float MaxPullDistance = 30f;

    [PropertyDisplay("Max number of mobs to pull at once (0 for no limit)")]
    [PropertySlider(0, 30)]
    public int MaxPullCount = 10;
}

[ConfigDisplay(Name = "Hydatos", Parent = typeof(EurekaConfig))]
public class HydatosConfig : ConfigNode
{
    public enum Farm : uint
    {
        [PropertyDisplay("<none>")]
        None,
        [PropertyDisplay("Khalamari (Xzomit)")]
        Khalamari = 0x26AB,
        [PropertyDisplay("Stegodon (Hydatos Primelephas)")]
        Stego = 0x26AF,
        [PropertyDisplay("Molech (Val Nullchu)")]
        Molech = 0x26B2,
        [PropertyDisplay("Piasa (Vivid Gastornis)")]
        Piasa = 0x26B3,
        [PropertyDisplay("Frostmane (Northern Tiger)")]
        Frostmane = 0x26B8,
        [PropertyDisplay("Daphne (Dark Void Monk)")]
        Daphne = 0x26B9,
        [PropertyDisplay("Goldemar (Hydatos Wraith)")]
        Golde = 0x26E6,
        [PropertyDisplay("Leuke (Tigerhawk)")]
        Leuke = 0x26C0,
        [PropertyDisplay("Barong (Laboratory Lion)")]
        Barong = 0x26C2,
        [PropertyDisplay("Ceto (Hydatos Delphyne)")]
        Ceto = 0x26C5,
        [PropertyDisplay("PW (Crystal Claw)")]
        PW = 0x26CA
    }

    public Farm CurrentFarmTarget = Farm.None;
}

[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 639)]
public class Hydatos : ZoneModule
{
    private readonly EurekaConfig _eurekaConfig = Service.Config.Get<EurekaConfig>();
    private readonly HydatosConfig _hydatosConfig = Service.Config.Get<HydatosConfig>();

    private static readonly Dictionary<uint, HydatosConfig.Farm> FateIDs = new()
    {
        [1412] = HydatosConfig.Farm.Khalamari,
        [1413] = HydatosConfig.Farm.Stego,
        [1414] = HydatosConfig.Farm.Molech,
        [1415] = HydatosConfig.Farm.Piasa,
        [1416] = HydatosConfig.Farm.Frostmane,
        [1417] = HydatosConfig.Farm.Daphne,
        [1418] = HydatosConfig.Farm.Golde,
        [1419] = HydatosConfig.Farm.Leuke,
        [1420] = HydatosConfig.Farm.Barong,
        [1421] = HydatosConfig.Farm.Ceto,
        [1423] = HydatosConfig.Farm.PW,
    };

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
        if (FateIDs.TryGetValue(fate.FateId, out var farm) && farm == _hydatosConfig.CurrentFarmTarget)
        {
            _hydatosConfig.CurrentFarmTarget = HydatosConfig.Farm.None;
            _hydatosConfig.Modified.Fire();
        }
    }

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        hints.ForbiddenZones.RemoveAll(z => World.Actors.Find(z.Source) is Actor src && ShouldIgnore(src, player));

        var shouldSetTarget = true; //  !player.InCombat && player.TargetID == 0;

        var farmOID = (uint)_hydatosConfig.CurrentFarmTarget;
        var farmMax = _eurekaConfig.MaxPullCount;
        var farmRange = _eurekaConfig.MaxPullDistance;

        if (farmOID > 0 && (farmMax == 0 || hints.PotentialTargets.Count(e => e.Priority >= 0) < farmMax))
            foreach (var e in hints.PotentialTargets)
                if (e.Actor.OID == farmOID && e.Priority == AIHints.Enemy.PriorityUndesirable && (e.Actor.Position - player.Position).LengthSq() <= farmRange * farmRange)
                {
                    // level 60 hydatos wraith does not spawn golde
                    if (farmOID == 0x26E6 && e.Actor.ForayInfo.Level < 61)
                        continue;

                    e.Priority = 0;

                    if (shouldSetTarget && (hints.ForcedTarget == null || (hints.ForcedTarget.Position - player.Position).LengthSq() > (e.Actor.Position - player.Position).LengthSq()))
                        hints.ForcedTarget = e.Actor;
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
        if (UICombo.Enum("Prep mob", ref _hydatosConfig.CurrentFarmTarget))
            _hydatosConfig.Modified.Fire();

        ImGui.SetNextItemWidth(200);
        if (ImGui.DragFloat("Max distance to look for new mobs", ref _eurekaConfig.MaxPullDistance, 1, 20, 80))
            _eurekaConfig.Modified.Fire();
        ImGui.SetNextItemWidth(200);
        if (ImGui.DragInt("Max mobs to pull (set to 0 for no limit)", ref _eurekaConfig.MaxPullCount, 1, 0, 30))
            _eurekaConfig.Modified.Fire();
    }
}
