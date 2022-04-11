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
            StateMachine.State? prevState = m?.StateMachine.ActiveState;
            DateTime prevStateEnter = player.WorldState.CurrentTime;
            if (m != null)
            {
                while (player.TickForward() && player.WorldState.CurrentTime <= enc.Time.End)
                {
                    m.Update();
                    if (m.StateMachine.ActiveState == null)
                        break;

                    if (prevState != m.StateMachine.ActiveState)
                    {
                        if (prevState != null)
                            prevState.Duration = (float)(player.WorldState.CurrentTime - prevStateEnter).TotalSeconds;
                        prevState = m.StateMachine.ActiveState;
                        prevStateEnter = player.WorldState.CurrentTime;
                    }
                }
            }

            var stateTree = new StateMachineTree(m?.InitialState);
            int curBranch = prevState != null ? (stateTree.Nodes.GetValueOrDefault(prevState.ID)?.BranchID ?? 0) : 0;

            _timeline.MaxTime = stateTree.MaxTime;

            _colBoss = _timeline.AddColumn(new ActionUseColumn(_timeline, stateTree));
            _colBoss.Width = 10;
            _colBoss.SelectedBranch = curBranch;

            _colStates = _timeline.AddColumn(new StateMachineBranchColumn(_timeline, stateTree));
            _colStates.Branch = curBranch;

            _planner = new(new(pcClass, ""), () => { }, _timeline, stateTree, curBranch);
            if (pc != null)
                _casts = new(_timeline, pcClass, stateTree, curBranch);

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
            if (_colStates.DrawControls())
                SyncBranch();
            ImGui.SameLine();
            _planner.DrawControls();

            _timeline.Draw();
        }

        private void AddEvent(Replay.Encounter enc, Replay.Action a, bool isPlayer)
        {
            var ev = new ActionUseColumn.Event();
            ev.Timestamp = (float)(a.Timestamp - enc.Time.Start).TotalSeconds;
            ev.AttachNode = _colStates.Tree.TimeToBranchNode(_colStates.Branch, ev.Timestamp);
            ev.Name = $"{a.ID} {ReplayUtils.ParticipantString(a.Source)} -> {ReplayUtils.ParticipantString(a.MainTarget)}";

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

        private void SyncBranch()
        {
            _colBoss.SelectedBranch = _colStates.Branch;
            _planner.SelectBranch(_colStates.Branch);
            _casts?.SelectBranch(_colStates.Branch);
        }
    }
}
