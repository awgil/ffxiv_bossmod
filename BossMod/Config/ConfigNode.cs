using ImGuiNET;
using System;

namespace BossMod
{
    // attribute that specifies how config node should be shown in the UI
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigDisplayAttribute : Attribute
    {
        public string? Name;
        public int Order;
        public Type? Parent;
    }

    // base class for configuration nodes
    public class ConfigNode
    {
        public event EventHandler? Modified;

        // notify that configuration node was modified; should be called by derived classes whenever they make any modifications
        // implementation dispatches modification event and forwards request to parent
        // root should subscribe to modification event to save updated configuration
        public void NotifyModified()
        {
            Modified?.Invoke(this, EventArgs.Empty);
        }

        // draw actual node contents; at very least leaves should override this to draw something useful
        public virtual void DrawContents(Tree tree) { }

        // draw common property types
        protected void DrawProperty(ref bool v, string label)
        {
            if (ImGui.Checkbox(label, ref v))
                NotifyModified();
        }

        protected void DrawProperty<E>(ref E v, string label) where E : struct, Enum
        {
            ImGui.SetNextItemWidth(100);
            if (ImGui.BeginCombo(label, v.ToString()))
            {
                foreach (var opt in Enum.GetValues<E>())
                {
                    if (ImGui.Selectable(opt.ToString(), opt.Equals(v)))
                    {
                        v = opt;
                        NotifyModified();
                    }
                }
                ImGui.EndCombo();
            }
        }
    }
}
