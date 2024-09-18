using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System.Runtime.InteropServices;

namespace BossMod.QuestBattle;

public unsafe partial class QuestBattleWindow : UIWindow
{
    private readonly QuestBattleDirector _director;
    private readonly QuestBattleConfig _config = Service.Config.Get<QuestBattleConfig>();
    private const string _windowID = "vbm Quest###Quest module";
    private WorldState World => _director.World;

    private readonly List<WPos> Waymarks = [];

    private delegate void AbandonDuty(bool a1);
    private readonly AbandonDuty abandonDutyHook = Marshal.GetDelegateForFunctionPointer<AbandonDuty>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 41 B2 01 EB 39"));

    public QuestBattleWindow(QuestBattleDirector director) : base(_windowID, false, new(300, 200), ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse)
    {
        _director = director;
        _director.World.CurrentZoneChanged.Subscribe(OnZoneChange);
    }

    private void OnZoneChange(WorldState.OpZoneChange zc)
    {
        Waymarks.Clear();
    }

    public override void PreOpenCheck()
    {
        IsOpen = _director.Enabled && _config.ShowUI;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _director.Dispose();
        base.Dispose(disposing);
    }

    public override void Draw()
    {
        if (_director.CurrentModule is QuestBattle qb)
        {
            ImGui.Text($"Module: {qb.GetType().Name}");
            DrawObjectives(qb);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
        }

        ImGui.TextUnformatted($"Zone: {World.CurrentZone} / CFC: {World.CurrentCFCID}");
#if DEBUG
        ImGui.SameLine();
        GenerateModule();
#endif
        if (World.Party.Player() is Actor player)
        {
            ImGui.TextUnformatted($"Position: {Utils.Vec3String(player.PosRot.XYZ())}");
#if DEBUG
            ImGui.SameLine();
            if (ImGui.Button("Copy vec"))
            {
                var x = player.PosRot.X;
                var y = player.PosRot.Y;
                var z = player.PosRot.Z;
                ImGui.SetClipboardText($"new Vector3({x:F2}f, {y:F2}f, {z:F2}f)");
            }
            ImGui.SameLine();
            if (ImGui.Button("Copy moveto"))
            {
                var x = player.PosRot.X;
                var y = player.PosRot.Y;
                var z = player.PosRot.Z;
                ImGui.SetClipboardText($"/vnav moveto {x:F2} {y:F2} {z:F2}");
            }
#endif
            if (World.Actors.Find(player.TargetID) is Actor tar)
            {
                ImGui.TextUnformatted($"Target: {tar.Name} ({tar.Type}; {tar.OID:X}) (hb={tar.HitboxRadius})");
                ImGui.TextUnformatted($"Distance: {player.DistanceToHitbox(tar)}");
                ImGui.TextUnformatted($"Angle: {player.AngleTo(tar)}");
            }

#if DEBUG
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button("Record position"))
                Waymarks.Add(player.Position);

            if (ImGui.Button("Copy all"))
                ImGui.SetClipboardText(string.Join(", ", Waymarks.Select(w => $"new({w.X:F2}f, {w.Z:F2}f)")));

            foreach (var w in Waymarks)
                ImGui.TextUnformatted($"{w}");
#endif
        }
    }

    private void GenerateModule()
    {
        if (World.CurrentCFCID == 0)
            return;

        if (ImGui.Button("Generate module stub"))
        {
            var cfc = Service.LuminaRow<ContentFinderCondition>(World.CurrentCFCID);
            if (cfc == null)
                return;

            string name;
            if (cfc.ContentLinkType == 5)
            {
                var qb = Service.LuminaRow<Lumina.Excel.GeneratedSheets.QuestBattle>(cfc.Content)!;
                var quest = Service.LuminaRow<Quest>((uint)qb.Quest)!;
                name = quest.Name;
            }
            else
            {
                name = cfc.Name;
            }

            var expansion = cfc.ClassJobLevelSync switch
            {
                > 0 and <= 50 => "ARealmReborn",
                > 50 and <= 60 => "Heavensward",
                > 60 and <= 70 => "Stormblood",
                > 70 and <= 80 => "Shadowbringers",
                > 80 and <= 90 => "Endwalker",
                > 90 and <= 100 => "Dawntrail",
                _ => "Unknown"
            };

            var questname = Utils.StringToIdentifier(name);

            var module = $"namespace BossMod.QuestBattle.{expansion};\n" +
                        $"\n" +
                        $"[Quest(BossModuleInfo.Maturity.Contributed, {World.CurrentCFCID})]\n" +
                        $"internal class {questname}(WorldState ws) : QuestBattle(ws)\n" +
                        "{\n" +
                        "   public override List<QuestObjective> DefineObjectives(WorldState ws) => [\n" +
                        "       new QuestObjective(ws)\n" +
                        "   ];\n" +
                        "}\n";

            ImGui.SetClipboardText(module);
        }
    }

    private void DrawObjectives(QuestBattle sqb)
    {
        if (_director.Paused)
        {
            if (ImGui.Button("Resume"))
                _director.Paused = false;
        }
        else if (ImGui.Button("Pause"))
            _director.Paused = true;

        ImGui.SameLine();

        if (UIMisc.Button("Leave duty", !ImGui.GetIO().KeyShift, "Hold shift to leave"))
            abandonDutyHook.Invoke(false);

        ImGui.SameLine();
        UIMisc.HelpMarker("Attempt to leave duty by directly sending the \"abandon duty\" packet, which may be able to bypass the out-of-combat restriction. Only works in some duties.");

        if (ImGui.Button("Skip current step"))
            sqb.Advance();
        ImGui.SameLine();
        if (ImGui.Button("Restart from step 1"))
            sqb.Reset();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        for (var i = 0; i < sqb.Objectives.Count; i++)
        {
            var n = sqb.Objectives[i];
            var highlight = n == _director.CurrentObjective;
            using var c = ImRaii.PushColor(ImGuiCol.Text, highlight ? ImGuiColors.DalamudWhite : ImGuiColors.DalamudGrey);
            ImGui.TextUnformatted($"#{i + 1} {n.DisplayName}");
#if DEBUG
            if (highlight)
                foreach (var vec in _director.CurrentWaypoints)
                {
                    if (vec.SpecifiedInPath)
                    {
                        using (var f = ImRaii.PushFont(UiBuilder.IconFont))
                            ImGui.Text(FontAwesomeIcon.Star.ToIconString());
                        ImGui.SameLine();
                    }
                    ImGui.TextUnformatted(Utils.Vec3String(vec.Position));
                }
#endif
        }
    }
}
