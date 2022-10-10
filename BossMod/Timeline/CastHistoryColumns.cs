using System.Collections.Generic;

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

        public CastHistoryColumns(Timeline timeline, Class @class, StateMachineTree tree, List<int> phaseBranches)
        {
            _class = @class;
            _tree = tree;

            _autoAttacks = timeline.AddColumn(new ActionUseColumn(timeline, tree, phaseBranches));
            _autoAttacks.Width = _trackWidth;

            _animLock = timeline.AddColumn(new ActionUseColumn(timeline, tree, phaseBranches));
            _animLock.Width = _trackWidth;

            foreach (var track in AbilityDefinitions.Classes[@class].Tracks)
            {
                var col = timeline.AddColumn(new ActionUseColumn(timeline, tree, phaseBranches));
                col.Width = _trackWidth;
                _columns.Add(col);
            }
        }

        public void AddEvent(ActionID aid, ActionUseColumn.Event ev)
        {
            if (aid == CommonDefinitions.IDAutoAttack || aid == CommonDefinitions.IDAutoShot)
            {
                _autoAttacks.Events.Add(ev);
            }
            else
            {
                var def = AbilityDefinitions.Classes[_class].Abilities.GetValueOrDefault(aid);
                if (def == null)
                    ev.Color = 0xff0000ff;

                _animLock.Events.Add(ev);
                _animLock.Entries.Add(new(ev.AttachNode, ev.Delay, 0, 0, def?.Definition.AnimationLock ?? 0.6f, aid.ToString()));

                if (def != null)
                {
                    var col = _columns[def.CooldownTrack];
                    col.Events.Add(ev);
                    if (def.Definition.Cooldown > 0)
                        col.Entries.Add(new(ev.AttachNode, ev.Delay, 0, def.Definition.EffectDuration, def.Definition.Cooldown, aid.ToString()));
                }
            }
        }

        public void ClearEvents()
        {
            _autoAttacks.Events.Clear();
            _animLock.Events.Clear();
            _animLock.Entries.Clear();
            foreach (var c in _columns)
            {
                c.Events.Clear();
                c.Entries.Clear();
            }
        }
    }
}
