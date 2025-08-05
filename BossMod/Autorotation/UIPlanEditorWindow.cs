using Dalamud.Bindings.ImGui;

namespace BossMod.Autorotation;

public class UIPlanEditorWindow : UIWindow
{
    private readonly PlanDatabase _db;
    private Plan _original;
    private readonly Timeline _timeline = new();
    private readonly ColumnStateMachineBranch _colStates;
    private readonly CooldownPlannerColumns _planner;
    private int _selectedPhase;

    public UIPlanEditorWindow(PlanDatabase db, Plan plan, StateMachine sm) : base($"Cooldown planner: {plan.Guid}", true, new(1200, 900))
    {
        _db = db;
        _original = plan;

        var tree = new StateMachineTree(sm);
        var phaseBranches = Enumerable.Repeat(0, tree.Phases.Count).ToList();
        _colStates = _timeline.Columns.Add(new ColumnStateMachineBranch(_timeline, tree, phaseBranches));
        _planner = _timeline.Columns.Add(new CooldownPlannerColumns(plan.MakeClone(), _timeline, tree, phaseBranches, true, [], default));

        _timeline.MinTime = -30;
        _timeline.MaxTime = tree.TotalMaxTime;
    }

    public override void PreOpenCheck() => RespectCloseHotkey = !_planner.Modified;

    public override void Draw()
    {
        if (UIMisc.Button("Save", !_planner.Modified, "No changes"))
            Save();
        ImGui.SameLine();
        if (UIMisc.Button("Delete", !ImGui.GetIO().KeyShift, "Hold shift to delete"))
            Delete();
        ImGui.SameLine();
        _planner.DrawCommonControls();

        _selectedPhase = _planner.DrawPhaseControls(_selectedPhase);

        _timeline.MaxTime = _colStates.Tree.TotalMaxTime;
        _timeline.Draw();
    }

    private void Save()
    {
        var newPlan = _planner.Plan.MakeClone();
        _db.ModifyPlan(_original, newPlan);
        _original = newPlan;
        _planner.Modified = false;
    }

    private void Delete()
    {
        _db.ModifyPlan(_original, null);
        IsOpen = false;
    }
}
