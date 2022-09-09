using System.Collections.Generic;

namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    // TODO: improve!
    class ForbiddenFruit5 : ForbiddenFruitCommon
    {
        private List<Actor> _towers = new();

        private const float _towerRadius = 5;

        public ForbiddenFruit5() : base(ActionID.MakeSpell(AID.Burst)) { }

        public override void Init(BossModule module)
        {
            _towers = module.Enemies(OID.Tower);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var tetherSource = TetherSources[pcSlot];
            if (tetherSource != null)
                arena.AddLine(tetherSource.Position, pc.Position, TetherColor(tetherSource));

            foreach (var tower in _towers)
                arena.AddCircle(tower.Position, _towerRadius, tetherSource == null ? ArenaColor.Safe : ArenaColor.Danger);
        }
    }
}
