using ImGuiNET;

namespace BossMod;

public class ColumnPlannerTrackTarget : ColumnPlannerTrack
{
    public class OverrideElement : Element
    {
        public uint OID;
        public string Comment;

        public OverrideElement(Entry window, uint oid, string comment) : base(window)
        {
            OID = oid;
            Comment = comment;
        }
    }

    public ModuleRegistry.Info? ModuleInfo;

    public ColumnPlannerTrackTarget(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, ModuleRegistry.Info? moduleInfo)
        : base(timeline, tree, phaseBranches, "Target")
    {
        ModuleInfo = moduleInfo;
    }

    public void AddElement(StateMachineTree.Node attachNode, float delay, float windowLength, uint oid, string comment)
    {
        var elem = (OverrideElement)AddElement(attachNode, delay, windowLength);
        elem.Comment = comment;
        SetElementValue(elem, oid);
    }

    protected override Element CreateElement(Entry window)
    {
        return SetElementValue(new OverrideElement(window, 0, ""), 0);
    }

    protected override List<string> DescribeElement(Element e)
    {
        var cast = (OverrideElement)e;
        List<string> res = new();
        res.Add($"Comment: {cast.Comment}");
        res.Add($"Target: {OIDString(cast.OID)}");
        return res;
    }

    protected override void EditElement(Element e)
    {
        var cast = (OverrideElement)e;
        if (ImGui.BeginCombo("OID", OIDString(cast.OID)))
        {
            if (ImGui.Selectable(OIDString(0), cast.OID == 0))
            {
                SetElementValue(cast, 0);
                NotifyModified();
            }

            if (ModuleInfo?.ObjectIDType != null)
            {
                foreach (var opt in ModuleInfo.ObjectIDType.GetEnumValues())
                {
                    var uopt = (uint)opt;
                    if (uopt == 0)
                        continue;

                    if (ImGui.Selectable(OIDString(uopt), cast.OID == uopt))
                    {
                        SetElementValue(cast, uopt);
                        NotifyModified();
                    }
                }
            }
            ImGui.EndCombo();
        }
        if (ImGui.InputText("Comment", ref cast.Comment, 256))
        {
            NotifyModified();
        }
    }

    private string OIDString(uint oid) => oid == 0 ? "Automatic" : $"{ModuleInfo?.ObjectIDType?.GetEnumName(oid)} (0x{oid})";

    private OverrideElement SetElementValue(OverrideElement e, uint oid)
    {
        e.OID = oid;
        return e;
    }
}
