using BossMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UIDev
{
    // replay visualization using player's actions timeline + embedded personal cooldown planner
    class PlayerActions
    {
        private class BossColumn
        {
            public ActionUseColumn Column;
            public List<(Replay.Participant?, ActionID)> Filter = new(); // caster + action

            public BossColumn(PlayerActions owner)
            {
                Column = owner._timeline.AddColumnBefore(new ActionUseColumn(owner._timeline, owner._stateTree, owner._phaseBranches), owner._colStates);
                Column.Width = 10;
            }
        }

        private Replay _replay;
        private Replay.Encounter _encounter;
        private Replay.Participant? _pc;
        private StateMachineTree _stateTree;
        private List<int> _phaseBranches;
        private List<(Replay.Participant?, ActionID)> _enemyActions = new();
        private Timeline _timeline = new();
        private StateMachineBranchColumn _colStates;
        private List<BossColumn> _colBoss = new();
        private CooldownPlannerColumns _planner;
        private CastHistoryColumns? _casts;
        private WindowManager.Window? _config;
        private UITree _configTree = new();

        public PlayerActions(Replay replay, Replay.Encounter enc, Class pcClass, Replay.Participant? pc = null)
        {
            _replay = replay;
            _encounter = enc;
            _pc = pc;

            // TODO: we should be able to reuse state data from encounter instead of re-running whole simulation...
            ReplayPlayer player = new(replay);
            player.AdvanceTo(enc.Time.Start, () => { });
            var bmm = new BossModuleManager(player.WorldState);
            var m = bmm.LoadedModules.FirstOrDefault(m => m.PrimaryActor.InstanceID == enc.InstanceID);
            if (m == null)
                throw new Exception($"Encounter module not available");

            var curPhase = m.StateMachine.ActivePhase;
            var curState = m.StateMachine.ActiveState;
            DateTime curPhaseEnter = player.WorldState.CurrentTime;
            DateTime curStateEnter = player.WorldState.CurrentTime;
            List<StateMachine.State?> lastStates = new();
            while (player.TickForward() && player.WorldState.CurrentTime <= enc.Time.End)
            {
                m.Update();

                if (curPhase != m.StateMachine.ActivePhase)
                {
                    if (curPhase != null)
                    {
                        curPhase.ExpectedDuration = (float)(player.WorldState.CurrentTime - curPhaseEnter).TotalSeconds;
                        lastStates.Add(curState);
                    }
                    curPhase = m.StateMachine.ActivePhase;
                    curState = null;
                    curPhaseEnter = player.WorldState.CurrentTime;
                }

                if (curState != m.StateMachine.ActiveState)
                {
                    if (curState != null)
                        curState.Duration = (float)(player.WorldState.CurrentTime - curStateEnter).TotalSeconds;
                    curState = m.StateMachine.ActiveState;
                    curStateEnter = player.WorldState.CurrentTime;
                }

                if (curState == null)
                    break;
            }

            _stateTree = new StateMachineTree(m.StateMachine);
            _phaseBranches = Enumerable.Repeat(0, _stateTree.Phases.Count).ToList();
            for (int i = 0; i < lastStates.Count; ++i)
                if (lastStates[i] != null)
                    _phaseBranches[i] = _stateTree.Nodes[lastStates[i]!.ID].BranchID - _stateTree.Phases[i].StartingNode.BranchID;

            _stateTree.ApplyTimings(null);
            _timeline.MaxTime = _stateTree.TotalMaxTime;

            _colStates = _timeline.AddColumn(new StateMachineBranchColumn(_timeline, _stateTree, _phaseBranches));

            var defaultBossCol = new BossColumn(this);
            _colBoss.Add(defaultBossCol);

            // TODO: use cooldown plan selector...
            _planner = new(new(pcClass, ""), () => _timeline.MaxTime = _stateTree.TotalMaxTime, _timeline, _stateTree, _phaseBranches);
            if (pc != null)
                _casts = new(_timeline, pcClass, _stateTree, _phaseBranches);

            foreach (var a in _replay.EncounterActions(_encounter))
            {
                if (!(a.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo))
                {
                    var entry = (a.Source, a.ID);
                    if (!_enemyActions.Contains(entry))
                        _enemyActions.Add(entry);
                    if (!defaultBossCol.Filter.Contains(entry))
                        defaultBossCol.Filter.Add(entry);
                    AddEvent(a, false);
                }
                else if (a.Source == _pc)
                {
                    AddEvent(a, true);
                }
            }
        }

        public void Draw()
        {
            if (ImGui.Button(_config == null ? "Show config" : "Hide config"))
            {
                if (_config == null)
                {
                    _config = WindowManager.CreateWindow($"Player actions timeline config: {_planner.PlanClass} {_pc?.Name} {_replay.Path} @ {_encounter.Time.Start:O}", DrawConfig, () => _config = null, () => true);
                    _config.SizeHint = new(600, 600);
                    _config.MinSize = new(100, 100);
                }
                else
                {
                    WindowManager.CloseWindow(_config);
                }
            }
            ImGui.SameLine();
            _planner.DrawControls();
            _timeline.Draw();
        }

        public void Close()
        {
            if (_config != null)
                WindowManager.CloseWindow(_config);
        }

        private void AddEvent(Replay.Action a, bool isPlayer)
        {
            var (node, delay) = _colBoss[0].Column.AbsoluteTimeToNodeAndDelay((float)(a.Timestamp - _encounter.Time.Start).TotalSeconds);
            var ev = new ActionUseColumn.Event(node, delay, $"{a.ID} {ReplayUtils.ParticipantString(a.Source)} -> {ReplayUtils.ParticipantString(a.MainTarget)}", 0);

            bool damage = false;
            foreach (var t in a.Targets)
            {
                ev.TooltipExtra.Add($"- {ReplayUtils.ParticipantString(t.Target)}");
                foreach (var e in t.Effects)
                {
                    ev.TooltipExtra.Add($"-- {ReplayUtils.ActionEffectString(e)}");
                    damage |= t.Target?.Type == ActorType.Player && e.Type is ActionEffectType.Damage or ActionEffectType.BlockedDamage or ActionEffectType.ParriedDamage;
                }
            }

            bool highlight = isPlayer ? (a.ID != new ActionID(ActionType.Spell, 7)) : damage;
            ev.Color = highlight ? 0xffffffff : 0x80808080;

            if (isPlayer)
            {
                _planner.AddEvent(a.ID, ev);
                _casts?.AddEvent(a.ID, ev);
            }
            else
            {
                foreach (var c in _colBoss.Where(c => c.Filter.Contains((a.Source, a.ID))))
                    c.Column.Events.Add(ev);
            }
        }

        private void DrawConfig()
        {
            foreach (var n in _configTree.Node("Boss cast columns"))
            {
                if (ImGui.Button("Add new!"))
                    _colBoss.Add(new(this));

                int i = 0;
                foreach (var e in _enemyActions)
                {
                    int j = 0;
                    foreach (var c in _colBoss)
                    {
                        bool set = c.Filter.Contains(e);
                        if (ImGui.Checkbox($"###{i}/{j}", ref set))
                        {
                            if (set)
                                c.Filter.Add(e);
                            else
                                c.Filter.Remove(e);
                            foreach (var cc in _colBoss)
                                cc.Column.Events.Clear();
                            foreach (var a in _replay.EncounterActions(_encounter))
                                if (!(a.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo))
                                    AddEvent(a, false);
                        }
                        ImGui.SameLine();
                        ++j;
                    }
                    ImGui.TextUnformatted($"{ReplayUtils.ParticipantString(e.Item1)}: {e.Item2}");
                    ++i;
                }
            }
        }
    }
}
