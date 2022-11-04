using BossMod;
using System.Collections.Generic;

namespace UIDev
{
    public class ColumnPlayerDetails : Timeline.ColumnGroup
    {
        private ColumnPlayerActions _actions;
        private ColumnSeparator _separator;

        public bool AnyVisible => _actions.Width > 0;

        public ColumnPlayerDetails(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant player, Class playerClass)
            : base(timeline)
        {
            Name = player.Name;
            _actions = Add(new ColumnPlayerActions(timeline, tree, phaseBranches, replay, enc, player, playerClass));
            _separator = Add(new ColumnSeparator(timeline));
        }

        public void DrawConfig(UITree tree)
        {
            foreach (var n in tree.Node("Actions"))
                _actions.DrawConfig(tree);
            _separator.Width = AnyVisible ? 1 : 0;
        }
    }
}
