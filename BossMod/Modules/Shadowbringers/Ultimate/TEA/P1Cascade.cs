using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P1Cascade : BossComponent
    {
        private IEnumerable<Actor> _rages = Enumerable.Empty<Actor>();

        private static float _rageRadius = 8;

        public override void Init(BossModule module)
        {
            _rages = module.Enemies(OID.LiquidRage);
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var rage in _rages)
                arena.ZoneCircle(rage.Position, _rageRadius, ArenaColor.AOE);
        }
    }
}
