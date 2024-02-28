using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    // generic component used for drawing adds
    public class Adds : BossComponent
    {
        private uint _actorOID;
        private IReadOnlyList<Actor> _actors = ActorEnumeration.EmptyList;

        public IReadOnlyList<Actor> Actors => _actors;
        public IEnumerable<Actor> ActiveActors => Actors.Where(a => a.IsTargetable && !a.IsDead);

        public Adds(uint actorOID)
        {
            _actorOID = actorOID;
        }

        public override void Init(BossModule module)
        {
            _actors = module.Enemies(_actorOID);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.Actors(_actors, ArenaColor.Enemy);
        }
    }
}
