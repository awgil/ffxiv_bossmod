using System;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P3DarkdragonDiveCounter : BossComponent
    {
        private (Actor?, int)[] _towers = new (Actor?, int)[4]; // NW-NE-SE-SW
        private int _numActiveTowers;
        private int[] _assignments = new int[PartyState.MaxPartySize];

        private static float _towerRadius = 5;

        public bool Active => _numActiveTowers > 0;

        public P3DarkdragonDiveCounter()
        {
            Array.Fill(_assignments, -1);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            for (int i = 0; i < _towers.Length; ++i)
            {
                var tower = _towers[i].Item1;
                if (tower == null)
                    continue;

                if (_assignments[pcSlot] == i)
                    arena.AddCircle(tower.Position, _towerRadius, ArenaColor.Safe, 2);
                else
                    arena.AddCircle(tower.Position, _towerRadius, ArenaColor.Danger, 1);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var towerCount = (AID)spell.Action.ID switch
            {
                AID.DarkdragonDive1 => 1,
                AID.DarkdragonDive2 => 2,
                AID.DarkdragonDive3 => 3,
                AID.DarkdragonDive4 => 4,
                _ => 0
            };
            if (towerCount == 0)
                return;

            _towers[ClassifyTower(module, caster)] = (caster, towerCount);
            if (++_numActiveTowers == 4)
                InitAssignments(module);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var index = Array.FindIndex(_towers, ai => ai.Item1 == caster);
            if (index >= 0)
                --_numActiveTowers;
        }

        private int ClassifyTower(BossModule module, Actor tower)
        {
            var offset = tower.Position - module.Bounds.Center;
            return offset.Z > 0 ? (offset.X > 0 ? 2 : 3) : (offset.X > 0 ? 1 : 0);
        }

        private void InitAssignments(BossModule module)
        {
            var config = Service.Config.Get<DSW2Config>();
            var assign = config.P3DarkdragonDiveCounterGroups.Resolve(module.Raid);
            foreach (var (slot, group) in assign)
            {
                var pos = group & 3;
                if (group < 4 && _towers[pos].Item2 == 1)
                {
                    // flex
                    var dest = config.P3DarkdragonDiveCounterPreferCCWFlex ? PrevGroup(pos) : NextGroup(pos);
                    if (_towers[dest].Item2 <= 2)
                    {
                        dest = config.P3DarkdragonDiveCounterPreferCCWFlex ? NextGroup(pos) : PrevGroup(pos);
                        if (_towers[dest].Item2 <= 2)
                            dest = DiagGroup(pos);
                    }
                    pos = dest;
                }
                _assignments[slot] = pos;
            }
        }

        private int NextGroup(int group) => group < 3 ? group + 1 : 0;
        private int PrevGroup(int group) => group > 0 ? group - 1 : 3;
        private int DiagGroup(int group) => group > 1 ? group - 2 : group + 2;
    }
}
