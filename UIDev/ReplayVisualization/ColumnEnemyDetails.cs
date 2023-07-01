using BossMod;
using ImGuiNET;
using System.Collections.Generic;

namespace UIDev
{
    // a set of columns describing various properties of a single enemy (casts, target, statuses, etc.)
    public class ColumnEnemyDetails : Timeline.ColumnGroup
    {
        private Replay.Participant _enemy;
        private ColumnEnemyCasts _casts;
        private ColumnActorStatuses _statuses;
        private ColumnActorHP _hp;
        private ColumnSeparator _separator;

        public bool AnyVisible => _casts.Visible || _statuses.Visible || _hp.Visible;

        public ColumnEnemyDetails(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant enemy)
            : base(timeline)
        {
            //Name = ReplayUtils.ParticipantString(enemy);
            _enemy = enemy;
            _casts = Add(new ColumnEnemyCasts(timeline, tree, phaseBranches, replay, enc, enemy));
            _statuses = Add(new ColumnActorStatuses(timeline, tree, phaseBranches, replay, enc, enemy));
            _hp = Add(new ColumnActorHP(timeline, tree, phaseBranches, replay, enc, enemy));
            _separator = Add(new ColumnSeparator(timeline));
        }

        public void DrawConfig(UITree tree)
        {
            foreach (var n in tree.Node(ReplayUtils.ParticipantString(_enemy)))
            {
                DrawColumnToggle(_casts, "Casts");
                DrawColumnToggle(_hp, "HP");
                foreach (var nn in tree.Node("Statuses"))
                    _statuses.DrawConfig(tree);
            }
            _separator.Width = AnyVisible ? 1 : 0;
        }

        private void DrawColumnToggle(IToggleableColumn col, string name)
        {
            bool visible = col.Visible;
            if (ImGui.Checkbox(name, ref visible))
            {
                col.Visible = visible;
            }
        }
    }
}
