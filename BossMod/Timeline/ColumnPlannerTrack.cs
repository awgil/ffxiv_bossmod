using BossMod.Autorotation;
using ImGuiNET;
using System.Runtime.CompilerServices;

namespace BossMod;

// column representing single planner track (could be cooldowns or anything else)
public abstract class ColumnPlannerTrack(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, string name) : ColumnGenericHistory(timeline, tree, phaseBranches, name)
{
    public sealed class Element(Entry window, float windowLength, bool disabled, StrategyValue value)
    {
        public Entry Window = window; // entry duration is window length clamped to phase duration
        public Entry? Effect; // null if window length is >= than effect length
        public Entry? Cooldown; // null if cooldown is zero
        public float WindowLength = windowLength;
        public float EffectLength;
        public float CooldownLength;
        public bool Disabled = disabled;
        public StrategyValue Value = value;

        public float TotalVisualLength => Math.Max(EffectLength, Window.Duration + CooldownLength);
    }

    private sealed class EditState(Element element, bool editingEnd)
    {
        public Element Element = element;
        public bool EditingEnd = editingEnd;
    }

    public Action NotifyModified = () => { };
    public List<Element> Elements = [];
    private EditState? _edit;
    private Element? _popupElement;

    public override void Draw()
    {
        var popupName = $"popupProps/{GetHashCode()}";
        if (_popupElement != null)
        {
            if (ImGui.BeginPopup(popupName))
            {
                if (EditElement(_popupElement))
                {
                    UpdateElement(_popupElement);
                    NotifyModified();
                }
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
                    toEdit = AddElement(node, delay, 0, false, GetDefaultValue());
                }

                _edit = new(toEdit, MathF.Abs(lclickPos.Y - Timeline.TimeToScreenCoord(toEdit.Window.TimeSinceGlobalStart(Tree) + toEdit.Window.Duration)) < 5);
                if (_edit.EditingEnd)
                    toEdit.WindowLength = toEdit.Window.Duration; // if we're starting edit of the window-end, ensure it's matching visual value (clamped to phase duration)
            }

            // continue editing
            var dt = Timeline.ScreenDeltaToTimeDelta(ImGui.GetIO().MouseDelta.Y);
            if (_edit.EditingEnd)
            {
                _edit.Element.WindowLength += dt;
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
                if (_edit.EditingEnd)
                    _edit.Element.WindowLength = Math.Max(MathF.Round(_edit.Element.WindowLength, 1), 0.1f);
                else
                    _edit.Element.Window.Delay = Math.Max(MathF.Round(_edit.Element.Window.Delay, 1), minTime);
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
            var clickedElemIndex = Elements.FindIndex(e => e != _edit?.Element && ScreenPosInElement(rclickPos, e));
            if (clickedElemIndex >= 0)
            {
                if (ImGui.IsKeyDown(ImGuiKey.ModShift))
                {
                    RemoveElement(clickedElemIndex);
                }
                else
                {
                    Elements[clickedElemIndex].Disabled ^= true;
                    UpdateElement(Elements[clickedElemIndex]);
                    NotifyModified();
                }
            }
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

    public Element AddElement(StateMachineTree.Node attachNode, float delay, float windowLength, bool disabled, StrategyValue value)
    {
        var w = new Entry(Entry.Type.Range, attachNode, delay, windowLength, "", Timeline.Colors.PlannerWindow[0]);
        Entries.Add(w);
        var e = new Element(w, windowLength, disabled, value);
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

    public void UpdateAllElements()
    {
        foreach (var e in Elements)
            UpdateElement(e);
    }

    protected abstract StrategyValue GetDefaultValue();
    protected abstract void RefreshElement(Element e);
    protected abstract bool EditElement(Element e);
    protected abstract List<string> DescribeElement(Element e);

    protected bool EditElementWindow(Element e)
    {
        bool modified = false;

        var startGlobal = e.Window.TimeSinceGlobalStart(Tree);
        if (ImGui.InputFloat("Press at (relative to pull)", ref startGlobal))
        {
            (e.Window.AttachNode, e.Window.Delay) = Tree.AbsoluteTimeToNodeAndDelay(startGlobal, PhaseBranches);
            modified = true;
        }

        var startPhase = e.Window.TimeSincePhaseStart();
        if (ImGui.InputFloat("Press at (relative to phase)", ref startPhase))
        {
            (e.Window.AttachNode, e.Window.Delay) = Tree.PhaseTimeToNodeAndDelay(startPhase, e.Window.AttachNode.PhaseID, PhaseBranches);
            modified = true;
        }

        modified |= ImGui.InputFloat("Press at (relative to state)", ref e.Window.Delay);
        modified |= ImGui.InputFloat("Window length", ref e.WindowLength);
        return modified;
    }

    protected bool ScreenPosInElement(Vector2 pos, Element e)
    {
        if (!IsEntryVisible(e.Window))
            return false;
        var tStart = e.Window.TimeSinceGlobalStart(Tree);
        var yMin = Timeline.TimeToScreenCoord(tStart);
        var yMax = Timeline.TimeToScreenCoord(tStart + e.TotalVisualLength);
        return pos.Y >= (yMin - 4) && pos.Y <= (yMax + 4);
    }

    private void UpdateElement(Element element)
    {
        if (element.Disabled)
        {
            element.Window.Color = Timeline.Colors.PlannerCooldown;
            element.EffectLength = 0;
            element.CooldownLength = 0;
        }
        else
        {
            RefreshElement(element);
        }

        var maxWindow = Math.Max(0, Tree.Phases[element.Window.AttachNode.PhaseID].Duration - element.Window.TimeSincePhaseStart());
        element.Window.Duration = Math.Min(element.WindowLength, maxWindow);

        var effectDuration = element.EffectLength - element.Window.Duration;
        if (effectDuration > 0)
        {
            if (element.Effect == null)
            {
                element.Effect = new Entry(Entry.Type.Range, element.Window.AttachNode, 0, 0, "", Timeline.Colors.PlannerEffect);
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
                element.Cooldown = new Entry(Entry.Type.Range, element.Window.AttachNode, 0, 0, "", Timeline.Colors.PlannerCooldown);
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

        var tooltip = DescribeElement(element);
        tooltip.Add($"Press at: {tStart:f1}s ({element.Window.TimeSincePhaseStart():f1}s since phase start, {element.Window.Delay:f1}s after state start)");
        if (element.Window.AttachNode.Predecessor != null)
            tooltip.Add($"Attached: {element.Window.Delay:f1}s after {element.Window.AttachNode.Predecessor.State.ID:X} '{element.Window.AttachNode.Predecessor.State.Name}' ({element.Window.AttachNode.Predecessor.State.Comment})");
        else
            tooltip.Add($"Attached: {element.Window.Delay:f1}s after pull");
        tooltip.Add($"Next state: {element.Window.AttachNode.State.Duration - element.Window.Delay:f1}s before {element.Window.AttachNode.State.ID:X} '{element.Window.AttachNode.State.Name}' ({element.Window.AttachNode.State.Comment})");
        tooltip.Add($"Window: {element.WindowLength:f1}s");
        if (element.Disabled)
            tooltip.Add("*** DISABLED *** (right-click to reenable, shift-right-click to delete)");

        Timeline.AddTooltip(tooltip);
        Timeline.HighlightTime(tStart);
        Timeline.HighlightTime(tStart + element.Window.Duration);
        Timeline.HighlightTime(tStart + element.EffectLength);
        Timeline.HighlightTime(tStart + element.Window.Duration + element.CooldownLength);
    }
}
