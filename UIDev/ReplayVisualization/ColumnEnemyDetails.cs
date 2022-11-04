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
        private ColumnSeparator _separator;

        public bool AnyVisible => _casts.Width > 0;

        public ColumnEnemyDetails(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant enemy)
            : base(timeline)
        {
            //Name = ReplayUtils.ParticipantString(enemy);
            _enemy = enemy;
            _casts = Add(new ColumnEnemyCasts(timeline, tree, phaseBranches, replay, enc, enemy));
            _separator = Add(new ColumnSeparator(timeline));
        }

        public void DrawConfig()
        {
            ImGui.PushID(_enemy.InstanceID.ToString());
            DrawColumnToggle(_casts, "Casts");
            ImGui.TextUnformatted(ReplayUtils.ParticipantString(_enemy));
            ImGui.PopID();
        }

        private void DrawColumnToggle(Timeline.Column col, string name)
        {
            bool visible = col.Width > 0;
            if (ImGui.Checkbox(name, ref visible))
            {
                col.Width = visible ? ColumnGenericHistory.DefaultWidth : 0;
                _separator.Width = AnyVisible ? 1 : 0;
            }
            ImGui.SameLine();
        }
    }
}
