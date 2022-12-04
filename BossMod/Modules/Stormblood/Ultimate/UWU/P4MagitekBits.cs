using System.Collections.Generic;
using System.Linq;

namespace BossMod.Stormblood.Ultimate.UWU
{
    class P4MagitekBits : BossComponent
    {
        private List<Actor> _bits = new();

        public bool Active => _bits.Any(b => b.IsTargetable);

        public override void Init(BossModule module)
        {
            _bits = module.Enemies(OID.MagitekBit);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.Actors(_bits, ArenaColor.Enemy);
        }
    }
}
