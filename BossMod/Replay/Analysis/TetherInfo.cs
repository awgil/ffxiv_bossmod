using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;
using System.Text;

namespace BossMod.ReplayAnalysis;

class TetherInfo : CommonEnumInfo
{
    public readonly record struct Instance(Replay Replay, Replay.Encounter Enc, Replay.Tether Tether)
    {
        public string TimestampString() => $"{Replay.Path} @ {Enc.Time.Start:O}+{(Tether.Time.Start - Enc.Time.Start).TotalSeconds:f4}";

        public override string ToString() => $"{TimestampString()}: {ReplayUtils.ParticipantPosRotString(Tether.Source, Tether.Time.Start)} -> {ReplayUtils.ParticipantPosRotString(Tether.Target, Tether.Time.Start)}, active for {Tether.Time}s";
    }

    class BreakAnalysis
    {
        private readonly UIPlot _plot = new();
        private readonly List<(Instance inst, Vector2 startEnd)> _points = [];

        public BreakAnalysis(List<Instance> infos)
        {
            _plot.DataMin = new(float.MaxValue, float.MaxValue);
            _plot.DataMax = new(float.MinValue, float.MinValue);
            _plot.TickAdvance = new(5, 5);
            foreach (var inst in infos)
            {
                var s = (inst.Tether.Target.PosRotAt(inst.Tether.Time.Start).XZ() - inst.Tether.Source.PosRotAt(inst.Tether.Time.Start).XZ()).Length();
                var e = (inst.Tether.Target.PosRotAt(inst.Tether.Time.End).XZ() - inst.Tether.Source.PosRotAt(inst.Tether.Time.End).XZ()).Length();
                _plot.DataMin.X = Math.Min(_plot.DataMin.X, s);
                _plot.DataMin.Y = Math.Min(_plot.DataMin.Y, e);
                _plot.DataMax.X = Math.Max(_plot.DataMax.X, s);
                _plot.DataMax.Y = Math.Max(_plot.DataMax.Y, e);
                _points.Add((inst, new(s, e)));
            }
            _plot.DataMin.X -= 1;
            _plot.DataMin.Y -= 1;
            _plot.DataMax.X += 1;
            _plot.DataMax.Y += 1;
        }

        public void Draw()
        {
            _plot.Begin();
            foreach (var i in _points)
                _plot.Point(i.startEnd, 0xff808080, i.inst.TimestampString);
            _plot.End();
        }
    }

    private class TetherData
    {
        public readonly List<Instance> Instances = [];
        public readonly HashSet<uint> SourceOIDs = [];
        public readonly HashSet<uint> TargetOIDs = [];
        public BreakAnalysis? BreakAnalysis;
    }

    private readonly Type? _tidType;
    private readonly Dictionary<uint, TetherData> _data = [];

    public TetherInfo(List<Replay> replays, uint oid)
    {
        var moduleInfo = BossModuleRegistry.FindByOID(oid);
        _oidType = moduleInfo?.ObjectIDType;
        _tidType = moduleInfo?.TetherIDType;
        foreach (var replay in replays)
        {
            foreach (var enc in replay.Encounters.Where(enc => enc.OID == oid))
            {
                foreach (var tether in replay.EncounterTethers(enc))
                {
                    var data = _data.GetOrAdd(tether.ID);
                    data.Instances.Add(new(replay, enc, tether));
                    data.SourceOIDs.Add(tether.Source.Type != ActorType.DutySupport ? tether.Source.OID : 0);
                    data.TargetOIDs.Add(tether.Target.Type != ActorType.DutySupport ? tether.Target.OID : 0);
                }
            }
        }
    }

    public void Draw(UITree tree)
    {
        UITree.NodeProperties map(KeyValuePair<uint, TetherData> kv)
        {
            var name = _tidType?.GetEnumName(kv.Key);
            return new($"{kv.Key} ({name})", false, name == null ? 0xff00ffff : 0xffffffff);
        }
        foreach (var (tid, data) in tree.Nodes(_data, map))
        {
            tree.LeafNode($"Source IDs: {OIDListString(data.SourceOIDs)}");
            tree.LeafNode($"Target IDs: {OIDListString(data.TargetOIDs)}");
            tree.LeafNode($"VFX: {Service.LuminaRow<Channeling>(tid)?.File}");
            foreach (var n in tree.Node("Instances", data.Instances.Count == 0))
            {
                tree.LeafNodes(data.Instances, inst => inst.ToString());
            }
            foreach (var an in tree.Node("Break distance analysis"))
            {
                data.BreakAnalysis ??= new(data.Instances);
                data.BreakAnalysis.Draw();
            }
        }
    }

    public void DrawContextMenu()
    {
        if (ImGui.MenuItem("Generate enum for boss module"))
        {
            var sb = new StringBuilder("public enum TetherID : uint\n{\n");
            foreach (var (tid, data) in _data)
                sb.Append($"    {EnumMemberString(tid, data)}\n");
            sb.Append("}\n");
            ImGui.SetClipboardText(sb.ToString());
        }

        if (ImGui.MenuItem("Generate missing enum values for boss module"))
        {
            var sb = new StringBuilder();
            foreach (var (tid, data) in _data.Where(kv => _tidType?.GetEnumName(kv.Key) == null))
                sb.AppendLine(EnumMemberString(tid, data));
            ImGui.SetClipboardText(sb.ToString());
        }
    }

    private string EnumMemberString(uint tid, TetherData data)
    {
        string generateTetherName() => Service.LuminaRow<Channeling>(tid)?.File.ToString() ?? tid.ToString();

        var name = _tidType?.GetEnumName(tid) ?? $"_Gen_Tether_{generateTetherName()}";
        return $"{name} = {tid}, // {OIDListString(data.SourceOIDs)}->{OIDListString(data.TargetOIDs)}";
    }
}
