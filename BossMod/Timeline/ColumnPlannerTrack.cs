using ImGuiNET;

namespace BossMod;

// column representing single planner track (could be cooldowns or anything else)
public abstract class ColumnPlannerTrack : ColumnGenericHistory
{
    public class Element
    {
        public Entry Window;
        public Entry? Effect; // null if window length is >= than effect length
        public Entry? Cooldown; // null if cooldown is zero
        public float EffectLength;
        public float CooldownLength;

        public float TotalLength => Math.Max(EffectLength, Window.Duration + CooldownLength);

        public Element(Entry window)
        {
            Window = window;
        }
    }

    private class EditState
    {
        public Element Element;
        public bool EditingEnd;

        public EditState(Element element, bool editingEnd)
        {
            Element = element;
            EditingEnd = editingEnd;
        }
    }

    public Action NotifyModified = () => { };
    public List<Element> Elements = new();
    private EditState? _edit = null;
    private Element? _popupElement = null;

    private uint _colCooldown = 0x80808080;
    private uint _colEffect = 0x8000ff00;
    private uint _colWindow = 0x8000ffff;

    public ColumnPlannerTrack(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, string name)
        : base(timeline, tree, phaseBranches, name)
    {
    }

    public override void Draw()
    {
        var popupName = $"popupProps/{GetHashCode()}";
        if (_popupElement != null)
        {
            if (ImGui.BeginPopup(popupName))
            {
                EditElement(_popupElement);
                ImGui.EndPopup();
            }
            else
            {
                _popupElement = null;
            }
        }

        var lclickPos = ImGui.GetIO().MouseClickedPos[0];
        if (ImGui.IsItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Left) && ScreenPosInTrack(lclickPos))
        {
            if (_edit == null)
            {
                // start editing
                var toEdit = Elements.Find(e => ScreenPosInElement(lclickPos, e));
                if (toEdit == null)
                {
                    // create new entry
                    var (node, delay) = Tree.AbsoluteTimeToNodeAndDelay(Timeline.ScreenCoordToTime(lclickPos.Y), PhaseBranches);
                    toEdit = AddElement(node, delay, 0);
                }
                _edit = new(toEdit, MathF.Abs(lclickPos.Y - Timeline.TimeToScreenCoord(toEdit.Window.TimeSinceGlobalStart(Tree) + toEdit.Window.Duration)) < 5);
            }

            // continue editing
            var dt = Timeline.ScreenDeltaToTimeDelta(ImGui.GetIO().MouseDelta.Y);
            if (_edit.EditingEnd)
            {
                _edit.Element.Window.Duration += dt;
            }
            else
            {
                float windowStart = _edit.Element.Window.TimeSinceGlobalStart(Tree);
                windowStart += dt;
                (_edit.Element.Window.AttachNode, _edit.Element.Window.Delay) = Tree.AbsoluteTimeToNodeAndDelay(windowStart, PhaseBranches);
            }
            UpdateElement(_edit.Element);
        }
        else
        {
            if (_edit != null)
            {
                // finish editing
                float minTime = _edit.Element.Window.AttachNode.PhaseID == 0 && _edit.Element.Window.AttachNode.Predecessor == null ? Timeline.MinTime : 0;
                _edit.Element.Window.Delay = Math.Max(MathF.Round(_edit.Element.Window.Delay, 1), minTime);
                _edit.Element.Window.Duration = Math.Max(MathF.Round(_edit.Element.Window.Duration, 1), 0.1f);
                UpdateElement(_edit.Element);
                _edit = null;
                NotifyModified();
            }
        }

        if (ImGui.IsItemActive() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && ScreenPosInTrack(lclickPos))
        {
            _popupElement = Elements.Find(e => ScreenPosInElement(lclickPos, e));
            if (_popupElement != null)
                ImGui.OpenPopup(popupName);
        }

        var rclickPos = ImGui.GetIO().MouseClickedPos[1];
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && ScreenPosInTrack(rclickPos))
        {
            var toDelete = Elements.FindIndex(e => e != _edit?.Element && ScreenPosInElement(rclickPos, e));
            if (toDelete >= 0)
                RemoveElement(toDelete);
        }

        DrawEntries();
        if (_edit != null)
        {
            // edit tooltip
            HoverElement(_edit.Element);
        }
        else
        {
            // normal tooltip
            var mousePos = ImGui.GetMousePos();
            if (ScreenPosInTrack(mousePos))
            {
                foreach (var e in Elements.Where(e => ScreenPosInElement(mousePos, e)))
                {
                    HoverElement(e);
                }
            }
            DrawHover();
        }
    }

    public Element AddElement(StateMachineTree.Node attachNode, float delay, float windowLength)
    {
        var w = new Entry(Entry.Type.Range, attachNode, delay, windowLength, "", _colWindow);
        Entries.Add(w);
        var e = CreateElement(w);
        Elements.Add(e);
        UpdateElement(e);
        return e;
    }

    public void RemoveElement(int index)
    {
        var e = Elements[index];
        Entries.Remove(e.Window);
        if (e.Effect != null)
            Entries.Remove(e.Effect);
        if (e.Cooldown != null)
            Entries.Remove(e.Cooldown);
        if (_edit?.Element == e)
            _edit = null;
        if (_popupElement == e)
            _popupElement = null;
        Elements.RemoveAt(index);
        NotifyModified();
    }

    protected abstract Element CreateElement(Entry window);
    protected abstract void EditElement(Element e);
    protected abstract List<string> DescribeElement(Element e);

    protected bool ScreenPosInElement(Vector2 pos, Element e)
    {
        if (!IsEntryVisible(e.Window))
            return false;
        var tStart = e.Window.TimeSinceGlobalStart(Tree);
        var yMin = Timeline.TimeToScreenCoord(tStart);
        var yMax = Timeline.TimeToScreenCoord(tStart + e.TotalLength);
        return pos.Y >= (yMin - 4) && pos.Y <= (yMax + 4);
    }

    private void UpdateElement(Element element)
    {
        var effectDuration = element.EffectLength - element.Window.Duration;
        if (effectDuration > 0)
        {
            if (element.Effect == null)
            {
                element.Effect = new Entry(Entry.Type.Range, element.Window.AttachNode, 0, 0, "", _colEffect);
                Entries.Add(element.Effect);
            }
            element.Effect.AttachNode = element.Window.AttachNode;
            element.Effect.Delay = element.Window.Delay + element.Window.Duration;
            element.Effect.Duration = effectDuration;
        }
        else if (element.Effect != null)
        {
            Entries.Remove(element.Effect);
            element.Effect = null;
        }

        var cooldownDuration = element.CooldownLength - Math.Max(effectDuration, 0);
        if (cooldownDuration > 0)
        {
            if (element.Cooldown == null)
            {
                element.Cooldown = new Entry(Entry.Type.Range, element.Window.AttachNode, 0, 0, "", _colCooldown);
                Entries.Add(element.Cooldown);
            }
            element.Cooldown.AttachNode = element.Window.AttachNode;
            element.Cooldown.Delay = element.Window.Delay + Math.Max(element.Window.Duration, element.EffectLength);
            element.Cooldown.Duration = cooldownDuration;
        }
        else if (element.Cooldown != null)
        {
            Entries.Remove(element.Cooldown);
            element.Cooldown = null;
        }
    }

    private void HoverElement(Element element)
    {
        var tStart = element.Window.TimeSinceGlobalStart(Tree);

        List<string> tooltip = DescribeElement(element);
        tooltip.Add($"Press at: {tStart:f1}s ({element.Window.TimeSincePhaseStart():f1}s since phase start, {element.Window.Delay:f1}s after state start)");
        if (element.Window.AttachNode.Predecessor != null)
            tooltip.Add($"Attached: {element.Window.Delay:f1}s after {element.Window.AttachNode.Predecessor.State.ID:X} '{element.Window.AttachNode.Predecessor.State.Name}' ({element.Window.AttachNode.Predecessor.State.Comment})");
        else
            tooltip.Add($"Attached: {element.Window.Delay:f1}s after pull");
        tooltip.Add($"Next state: {element.Window.AttachNode.State.Duration - element.Window.Delay:f1}s before {element.Window.AttachNode.State.ID:X} '{element.Window.AttachNode.State.Name}' ({element.Window.AttachNode.State.Comment})");
        tooltip.Add($"Window: {element.Window.Duration:f1}s");

        Timeline.AddTooltip(tooltip);
        Timeline.HighlightTime(tStart);
        Timeline.HighlightTime(tStart + element.Window.Duration);
        Timeline.HighlightTime(tStart + element.EffectLength);
        Timeline.HighlightTime(tStart + element.Window.Duration + element.CooldownLength);
    }
}
