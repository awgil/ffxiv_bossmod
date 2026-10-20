using Autofac;
using Dalamud.Bindings.ImGui;

namespace BossMod.Autorotation;

public sealed class UIPlanEditorWindow : DetachedWindow, IDisposable
{
    private readonly PlanDatabase _db;
    private Plan _original;
    private readonly Timeline _timeline;
    private readonly ColumnStateMachineBranch _colStates;
    private readonly CooldownPlannerColumns _planner;
    private int _selectedPhase;

    private readonly ILifetimeScope _scope;

    public delegate UIPlanEditorWindow Factory(ColorConfig colors, Plan plan, StateMachine sm);

    public UIPlanEditorWindow(PlanDatabase db, ColorConfig colors, Plan plan, StateMachine sm, ILifetimeScope parentScope) : base($"Cooldown planner: {plan.Guid}", new(1200, 900))
    {
        _db = db;
        _original = plan;
        _timeline = new(colors);

        var tree = new StateMachineTree(sm);
        var phaseBranches = Enumerable.Repeat(0, tree.Phases.Count).ToList();

        _scope = parentScope.BeginLifetimeScope(ctx =>
        {
            ctx.Register(_ => _timeline).SingleInstance();
            ctx.Register(_ => tree).SingleInstance();
            ctx.Register(_ => phaseBranches).SingleInstance();
        });

        _colStates = _timeline.Columns.Add(new ColumnStateMachineBranch(_timeline, tree, phaseBranches));
        _planner = _timeline.Columns.Add(_scope.Resolve<CooldownPlannerColumns.Factory>().Invoke(plan.MakeClone(), true, [], default));

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

    void IDisposable.Dispose()
    {
        _scope.Dispose();
    }
}
