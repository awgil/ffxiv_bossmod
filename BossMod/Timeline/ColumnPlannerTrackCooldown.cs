using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BossMod
{
    public class ColumnPlannerTrackCooldown : ColumnPlannerTrack
    {
        public class ActionElement : Element
        {
            public ActionID Action;
            public PlanTarget.ISelector Target;

            public ActionElement(Entry window, PlanTarget.ISelector target) : base(window)
            {
                Target = target;
            }
        }

        public ModuleRegistry.Info? ModuleInfo;
        public PlanDefinitions.ClassData ClassDef;
        public PlanDefinitions.CooldownTrack TrackDef;
        public ActionID DefaultAction;
        public int Level;

        public ColumnPlannerTrackCooldown(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, string name, ModuleRegistry.Info? moduleInfo, PlanDefinitions.ClassData classDef, PlanDefinitions.CooldownTrack trackDef, ActionID defaultAction, int level)
            : base(timeline, tree, phaseBranches, name)
        {
            ModuleInfo = moduleInfo;
            ClassDef = classDef;
            TrackDef = trackDef;
            DefaultAction = defaultAction;
            Level = level;
        }

        public void AddElement(StateMachineTree.Node attachNode, float delay, float windowLength, ActionID aid, PlanTarget.ISelector target)
        {
            var elem = (ActionElement)AddElement(attachNode, delay, windowLength);
            elem.Target = target;
            SetElementAction(elem, aid);
        }

        protected override Element CreateElement(Entry window)
        {
            return SetElementAction(new ActionElement(window, new PlanTarget.Self()), DefaultAction);
        }

        protected override List<string> DescribeElement(Element e)
        {
            var cast = (ActionElement)e;
            List<string> res = new();
            res.Add($"Action: {cast.Action}");
            res.Add($"Target: {cast.Target.GetType().Name}: {cast.Target.Describe(ModuleInfo)}");
            return res;
        }

        protected override void EditElement(Element e)
        {
            var cast = (ActionElement)e;
            if (ImGui.BeginCombo("Action", cast.Action.ToString()))
            {
                foreach (var a in TrackDef.Actions.Where(a => a.minLevel <= Level))
                {
                    if (ImGui.Selectable(a.aid.ToString(), cast.Action == a.aid))
                    {
                        SetElementAction(cast, a.aid);
                        NotifyModified();
                    }
                }
                ImGui.EndCombo();
            }
            if (ImGui.BeginCombo("Target", cast.Target.GetType().Name))
            {
                foreach (var t in Utils.GetDerivedTypes<PlanTarget.ISelector>(Assembly.GetExecutingAssembly()))
                {
                    if (ImGui.Selectable(t.Name, t == cast.Target.GetType()))
                    {
                        cast.Target = (PlanTarget.ISelector)Activator.CreateInstance(t)!;
                        NotifyModified();
                    }
                }
                ImGui.EndCombo();
            }
            ImGui.Indent();
            if (cast.Target.Edit(ModuleInfo))
                NotifyModified();
            ImGui.Unindent();
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
