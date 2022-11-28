using System.Collections.Generic;
using System.Linq;

namespace BossMod.Stormblood.Ultimate.UWU
{
    class P1Plumes : BossComponent
    {
        private List<Actor> _spiny = new();
        private List<Actor> _satin = new();

        public bool Active => _spiny.Any(p => p.IsTargetable) || _satin.Any(p => p.IsTargetable);

        public override void Init(BossModule module)
        {
            _spiny = module.Enemies(OID.SpinyPlume);
            _satin = module.Enemies(OID.SatinPlume);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.Actors(_spiny, ArenaColor.Enemy);
            arena.Actors(_satin, ArenaColor.Enemy);
        }
    }
}
