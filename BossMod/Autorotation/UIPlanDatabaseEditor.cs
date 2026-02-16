using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod.Autorotation;

public sealed class UIPlanDatabaseEditor
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
            for (int i = 0; i < list.Plans.Count; ++i)
            {
                if (ImGui.Selectable(list.Plans[i].Name, selected == i))
                {
                    selected = i;
                }
            }
        }
        return selected;
    }

    public static void StartPlanEditor(MediatorService mediator, BossModuleRegistry bmr, RotationModuleRegistry registry, Serializer ser, ActionEffectParser aep, ColorConfig colors, PlanDatabase db, Plan plan, StateMachine sm)
    {
        _ = new UIPlanEditorWindow(mediator, bmr, registry, ser, aep, colors, db, plan, sm);
    }

    public static void StartPlanEditor(MediatorService mediator, BossModuleRegistry bmr, RotationModuleRegistry autorot, Serializer ser, ActionEffectParser aep, ColorConfig colors, PlanDatabase db, Plan plan)
    {
        var m = bmr.CreateModuleForConfigPlanning(plan.Encounter);
        if (m != null)
            StartPlanEditor(mediator, bmr, autorot, ser, aep, colors, db, plan, m.StateMachine);
    }
}
