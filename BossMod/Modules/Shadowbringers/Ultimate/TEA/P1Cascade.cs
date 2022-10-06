using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P1Cascade : BossComponent
    {
        private List<Actor> _rages = new();
        private List<Actor> _embolus = new();

        private static float _rageRadius = 8;

        public override void Init(BossModule module)
        {
            _rages = module.Enemies(OID.LiquidRage);
            _embolus = module.Enemies(OID.Embolus);
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var rage in _rages)
                arena.ZoneCircle(rage.Position, _rageRadius, ArenaColor.AOE);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var e in _embolus)
                arena.Actor(e, ArenaColor.Object, true);
        }
    }
}
