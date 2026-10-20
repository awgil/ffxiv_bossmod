using Autofac;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod.Autorotation;

public sealed class UIPlanDatabaseEditor(ILifetimeScope parentScope, MediatorService mediator)
{
    // utility: draw combo that allows selecting a plan from a list; used in variety of contexts
    public static int DrawPlanCombo(PlanDatabase.PlanList list, int selected, string label)
    {
        ImGui.SetNextItemWidth(150);
        using var combo = ImRaii.Combo(label, selected >= 0 && selected < list.Plans.Count ? list.Plans[selected].Name : "<none>");
        if (combo)
        {
            if (ImGui.Selectable("<none>", selected < 0))
            {
                selected = -1;
            }
            for (var i = 0; i < list.Plans.Count; ++i)
            {
                if (ImGui.Selectable(list.Plans[i].Name, selected == i))
                {
                    selected = i;
                }
            }
        }
        return selected;
    }

    public void StartPlanEditor(ColorConfig colors, PlanDatabase db, Plan plan, StateMachine sm)
    {
        mediator.CreateWindow(new UIPlanEditorWindow(db, colors, plan, sm, parentScope));
    }

    public void StartPlanEditor(ColorConfig colors, PlanDatabase db, Plan plan)
    {
        var m = db.BossModuleRegistry.CreateModuleForConfigPlanning(plan.Encounter);
        if (m != null)
            StartPlanEditor(colors, db, plan, m.StateMachine);
    }
}
