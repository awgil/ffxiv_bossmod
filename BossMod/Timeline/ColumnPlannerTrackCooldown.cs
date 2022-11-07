using ImGuiNET;
using System.Collections.Generic;

namespace BossMod
{
    public class ColumnPlannerTrackCooldown : ColumnPlannerTrack
    {
        public class ActionElement : Element
        {
            public ActionID Action;

            public ActionElement(Entry window) : base(window) { }
        }

        public PlanDefinitions.ClassData ClassDef;
        public PlanDefinitions.CooldownTrack TrackDef;
        public ActionID DefaultAction;

        public ColumnPlannerTrackCooldown(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, string name, PlanDefinitions.ClassData classDef, PlanDefinitions.CooldownTrack trackDef, ActionID defaultAction)
            : base(timeline, tree, phaseBranches, name)
        {
            ClassDef = classDef;
            TrackDef = trackDef;
            DefaultAction = defaultAction;
        }

        public void AddElement(StateMachineTree.Node attachNode, float delay, float windowLength, ActionID aid)
        {
            var elem = (ActionElement)AddElement(attachNode, delay, windowLength);
            SetElementAction(elem, aid);
        }

        protected override Element CreateElement(Entry window)
        {
            return SetElementAction(new ActionElement(window), DefaultAction);
        }

        protected override List<string> DescribeElement(Element e)
        {
            var cast = (ActionElement)e;
            List<string> res = new();
            res.Add($"Action: {cast.Action}");
            return res;
        }

        protected override void EditElement(Element e)
        {
            var cast = (ActionElement)e;
            ImGui.TextUnformatted($"Action: {cast.Action}"); // TODO: should be a selector...
        }

        private ActionElement SetElementAction(ActionElement e, ActionID action)
        {
            e.Action = action;
            var actionDef = ClassDef.Abilities[action];
            e.EffectLength = actionDef.EffectDuration;
            e.CooldownLength = actionDef.Cooldown;
            return e;
        }
    }
}
