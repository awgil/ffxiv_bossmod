using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    // a set of columns representing actor's action history
    public class CastHistoryColumns
    {
        private Class _class;
        private StateMachineTree _tree;
        private ActionUseColumn _autoAttacks;
        private ActionUseColumn _animLock;
        private List<ActionUseColumn> _columns = new();

        private float _trackWidth = 10;

        public CastHistoryColumns(Timeline timeline, Class @class, StateMachineTree tree, int initialBranch)
        {
            _class = @class;
            _tree = tree;

            _autoAttacks = timeline.AddColumn(new ActionUseColumn(timeline, tree));
            _autoAttacks.Width = _trackWidth;
            _autoAttacks.SelectedBranch = initialBranch;

            _animLock = timeline.AddColumn(new ActionUseColumn(timeline, tree));
            _animLock.Width = _trackWidth;
            _animLock.SelectedBranch = initialBranch;

            foreach (var track in AbilityDefinitions.Classes[@class].Tracks)
            {
                var col = timeline.AddColumn(new ActionUseColumn(timeline, tree));
                col.Width = _trackWidth;
                col.SelectedBranch = initialBranch;
                _columns.Add(col);
            }
        }

        public void AddEvent(ActionID aid, ActionUseColumn.Event ev)
        {
            if (aid == CommonRotation.IDAutoAttack)
            {
                _autoAttacks.Events.Add(ev);
            }
            else
            {
                var def = AbilityDefinitions.Classes[_class].Abilities.GetValueOrDefault(aid);
                if (def == null)
                    ev.Color = 0xff0000ff;

                _animLock.Events.Add(ev);
                _animLock.Entries.Add(new() { AttachNode = ev.AttachNode, WindowStart = ev.Timestamp, Cooldown = def?.AnimLock ?? 0.6f, Name = aid.ToString() });

                if (def != null)
                {
                    var col = _columns[def.CooldownTrack];
                    col.Events.Add(ev);
                    if (def.Cooldown > 0)
                        col.Entries.Add(new() { AttachNode = ev.AttachNode, WindowStart = ev.Timestamp, Duration = def.EffectDuration, Cooldown = def.Cooldown, Name = aid.ToString() });
                }
            }
        }

        public void SelectBranch(int branch)
        {
            _animLock.SelectedBranch = branch;
            foreach (var c in _columns)
                c.SelectedBranch = branch;
        }
    }
}
