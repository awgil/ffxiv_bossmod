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
        private Timeline _timeline = new();
        private StateMachineBranchColumn _colStates;
        private ActionUseColumn _colBoss;
        private CooldownPlannerColumns _planner;
        private CastHistoryColumns? _casts;

        public PlayerActions(Replay replay, Replay.Encounter enc, Class pcClass, Replay.Participant? pc = null)
        {
            ReplayPlayer player = new(replay);
            player.AdvanceTo(enc.Time.Start, () => { });
            var bmm = new BossModuleManager(player.WorldState, new());
            var m = bmm.ActiveModules.FirstOrDefault(m => m.PrimaryActor.InstanceID == enc.InstanceID);
            if (m?.StateMachine == null)
                throw new Exception($"Encounter state machine not available");

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

            var stateTree = new StateMachineTree(m.StateMachine);
            var phaseBranches = Enumerable.Repeat(0, stateTree.Phases.Count).ToList();
            for (int i = 0; i < lastStates.Count; ++i)
                if (lastStates[i] != null)
                    phaseBranches[i] = stateTree.Nodes[lastStates[i]!.ID].BranchID - stateTree.Phases[i].StartingNode.BranchID;

            stateTree.ApplyTimings(null);
            _timeline.MaxTime = stateTree.TotalMaxTime;

            _colBoss = _timeline.AddColumn(new ActionUseColumn(_timeline, stateTree, phaseBranches));
            _colBoss.Width = 10;

            _colStates = _timeline.AddColumn(new StateMachineBranchColumn(_timeline, stateTree, phaseBranches));

            _planner = new(new(pcClass, ""), () => _timeline.MaxTime = stateTree.TotalMaxTime, _timeline, stateTree, phaseBranches);
            if (pc != null)
                _casts = new(_timeline, pcClass, stateTree, phaseBranches);

            foreach (var a in replay.EncounterActions(enc))
            {
                if (!(a.Source?.Type is ActorType.Player or ActorType.Pet or ActorType.Chocobo))
                    AddEvent(enc, a, false);
                else if (a.Source == pc)
                    AddEvent(enc, a, true);
            }
        }

        public void Draw()
        {
            _planner.DrawControls(true);
            _timeline.Draw();
        }

        private void AddEvent(Replay.Encounter enc, Replay.Action a, bool isPlayer)
        {
            var (node, delay) = _colBoss.AbsoluteTimeToNodeAndDelay((float)(a.Timestamp - enc.Time.Start).TotalSeconds);
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
                _colBoss.Events.Add(ev);
            }
        }
    }
}
