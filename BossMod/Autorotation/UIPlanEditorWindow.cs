//using ImGuiNET;

//namespace BossMod.Autorotation;

//public class UIPlanEditorWindow : UIWindow
//{
//    private readonly PlanDatabase _db;
//    private readonly Timeline _timeline = new();
//    private readonly ColumnStateMachineBranch _colStates;
//    private readonly CooldownPlannerColumns _planner;
//    private int _selectedPhase;
//    private bool _modified;

//    public UIPlanEditorWindow(PlanDatabase db, string guid, Plan plan, StateMachine sm, ModuleRegistry.Info? moduleInfo) : base($"Cooldown planner##{guid}", true, new(600, 600))
//    {
//        _db = db;

//        var tree = new StateMachineTree(sm);
//        var phaseBranches = Enumerable.Repeat(0, tree.Phases.Count).ToList();
//        _colStates = _timeline.Columns.Add(new ColumnStateMachineBranch(_timeline, tree, phaseBranches));
//        _planner = _timeline.Columns.Add(new CooldownPlannerColumns(plan, OnPlanModified, _timeline, tree, phaseBranches, moduleInfo, true));

//        _timeline.MinTime = -30;
//        _timeline.MaxTime = tree.TotalMaxTime;
//    }

//    public override void PreOpenCheck() => RespectCloseHotkey = !_modified;

//    public override void Draw()
//    {
//        if (UIMisc.Button("Save", !_modified, "No changes"))
//            Save();
//        ImGui.SameLine();
//        _planner.DrawCommonControls();

//        _selectedPhase = _planner.DrawPhaseControls(_selectedPhase);

//        _timeline.Draw();
//    }

//    private void Save()
//    {
//        _planner.UpdateEditedPlan();
//        _onModified();
//        _modified = false;
//    }

//    private void OnPlanModified()
//    {
//        _timeline.MaxTime = _colStates.Tree.TotalMaxTime;
//        _modified = true;
//    }
//}
