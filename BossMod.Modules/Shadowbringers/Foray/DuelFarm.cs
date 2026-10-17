using BossMod.Autorotation;
using Dalamud.Bindings.ImGui;
using System.Reflection;

namespace BossMod.Shadowbringers.Foray;

[AttributeUsage(AttributeTargets.Field)]
public sealed class DuelAttribute(uint nameID, uint prepNameID, uint fateID = 0) : Attribute
{
    public uint NameID => nameID;
    public uint PrepNameID => prepNameID;
    public uint FateID => fateID;

    public string Label => $"{ModuleViewer.BNpcName(nameID)} ({ModuleViewer.BNpcName(prepNameID).Replace("4T", "4t")})";
}

[ConfigDisplay(Name = "Bozja duel farming", Parent = typeof(ShadowbringersConfig))]
public class DuelFarmConfig : ConfigNode
{
    [PropertyDisplay("Enable zone module")]
    public bool Enabled = true;

    [PropertyDisplay("Max range to look for new mobs to pull")]
    [PropertySlider(20, 120)]
    public float MaxPullDistance = 30;

    [PropertyDisplay("Max number of mobs to pull at once (0 for no limit)")]
    [PropertySlider(0, 30)]
    public int MaxPullCount = 10;

    public bool AssistMode;
}

public abstract class DuelFarm<Duel> : ZoneModule where Duel : struct, Enum
{
    protected readonly DuelFarmConfig _globalConfig = Service.Config.Get<DuelFarmConfig>();

    public readonly string Zone;

    private readonly EventSubscriptions _subscriptions;

    private static DuelAttribute? GetAttr(Enum nm) => nm.GetType().GetField(nm.ToString())?.GetCustomAttribute<DuelAttribute>();
    private static uint GetFateID(Enum nm) => GetAttr(nm)?.FateID ?? 0;
    private static uint GetPrepID(Enum nm) => GetAttr(nm)?.PrepNameID ?? 0;

    protected abstract Duel FarmTarget { get; set; }

    protected DuelFarm(WorldState ws, string zone) : base(ws)
    {
        _subscriptions = new(ws.Client.FateInfo.Subscribe(OnFateSpawn));
        Zone = zone;
    }

    private void OnFateSpawn(ClientState.OpFateInfo fate)
    {
        if (GetFateID(FarmTarget) == fate.FateId)
            FarmTarget = default;
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
        base.Dispose(disposing);
    }

    protected virtual void AddAIHints(int playerSlot, Actor player, AIHints hints) { }

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        if (!_globalConfig.Enabled)
            return;

        AddAIHints(playerSlot, player, hints);

        var farmNameID = GetPrepID(FarmTarget);
        var farmMax = _globalConfig.MaxPullCount;
        var farmRange = _globalConfig.MaxPullDistance;

        if (farmMax > 0 && hints.PotentialTargets.Count(e => e.Priority >= 0) >= farmMax)
            return;

        if (farmNameID == 0)
            return;

        // only need to check "undesirable" targets, as mobs already attacking party members will be handled by autofarm
        bool canTarget(AIHints.Enemy enemy) => enemy.Priority == AIHints.Enemy.PriorityUndesirable && (!_globalConfig.AssistMode || enemy.Actor.InCombat);

        foreach (var e in hints.PotentialTargets.Where(t => t.Actor.NameID == farmNameID))
            if (canTarget(e) && (e.Actor.Position - player.Position).LengthSq() <= farmRange * farmRange)
            {
                if (e.Actor.InCombat)
                    // someone else pulled
                    e.Priority = 0;
                else
                {
                    // we pull
                    e.ShouldBeTargeted = true;
                    e.Priority = 0;
                }
            }
    }

    public override WindowDescription? GetWindowDescription() => _globalConfig.Enabled ? new($"VBM Auto-{Zone}###Eureka module", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize) : null;
    public override void DrawWindow(RotationModuleManager rmm)
    {
        Overlays.DrawPseudoTitlebar($"VBM Duel prep: {Zone}", typeof(DuelFarmConfig), 61838, "Disable module (and hide this overlay)", () =>
        {
            _globalConfig.Enabled = false;
            _globalConfig.Modified.Fire();
        });

        var modified = false;

        var tar = FarmTarget;
        if (UICombo.Enum("###duel", ref tar, t => (uint)(object)t == 0 ? "Select a duel..." : GetAttr(t)?.Label ?? t.ToString()))
            FarmTarget = tar;

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.SetNextItemWidth(200);
        modified |= ImGui.SliderFloat("Search distance (yalms)", ref _globalConfig.MaxPullDistance, 20, 120);
        ImGui.SetNextItemWidth(200);
        modified |= ImGui.SliderInt("Max pull size (0 = unlimited)", ref _globalConfig.MaxPullCount, 0, 30);
        modified |= ImGui.Checkbox("Only attack mobs that are already in combat", ref _globalConfig.AssistMode);

        if (modified)
            _globalConfig.Modified.Fire();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var isActive = rmm.ActiveModulesFlat.Any(m => m.Module.GetType().ToString() == "BossMod.Autorotation.MiscAI.AutoTarget");

        if (ImGui.Button(isActive ? "Auto-combat: on" : "Auto-combat: off"))
        {
            if (isActive)
                rmm.Clear();
            else
            {
                rmm.Activate(rmm.Database.Presets.FindPresetByName("VBM AI")!);
                rmm.Activate(rmm.Database.Presets.FindPresetByName("VBM Default")!);
            }
        }
    }
}

[ConfigDisplay(Name = "Bozja", Parent = typeof(DuelFarmConfig))]
public class BozjaFarmConfig : ConfigNode
{
    public BozjaDuel FarmTarget = BozjaDuel.None;
}

public enum BozjaDuel : uint
{
    None,
    [Duel(9398, 9517, 1601)]
    Gabriel,
    [Duel(9409, 9539, 1608)]
    Lyon,
    [Duel(9695, 9564, 1625)]
    Menenius
}

[ZoneModuleInfo(735)]
public class Bozja(WorldState ws) : DuelFarm<BozjaDuel>(ws, "Bozja")
{
    private readonly BozjaFarmConfig _config = Service.Config.Get<BozjaFarmConfig>();

    protected override BozjaDuel FarmTarget
    {
        get => _config.FarmTarget;
        set
        {
            _config.FarmTarget = value;
            _config.Modified.Fire();
        }
    }
}

[ConfigDisplay(Name = "Zadnor", Parent = typeof(DuelFarmConfig))]
public class ZadnorFarmConfig : ConfigNode
{
    public ZadnorDuel FarmTarget = ZadnorDuel.None;
}

public enum ZadnorDuel : uint
{
    None,
    [Duel(9958, 10131, 1722)]
    Dabog,
    [Duel(9695, 10138, 1727)]
    Menenius,
    [Duel(9409, 10160, 1739)]
    Lyon
}

[ZoneModuleInfo(778)]
public class Zadnor(WorldState ws) : DuelFarm<ZadnorDuel>(ws, "Zadnor")
{
    private readonly ZadnorFarmConfig _config = Service.Config.Get<ZadnorFarmConfig>();

    protected override ZadnorDuel FarmTarget
    {
        get => _config.FarmTarget;
        set
        {
            _config.FarmTarget = value;
            _config.Modified.Fire();
        }
    }
}
