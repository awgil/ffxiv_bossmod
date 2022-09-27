using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    // a set of valid, priority and forbidden targets (TODO: move to ai hints)
    public class BossTargets
    {
        public List<Actor> Valid = new();

        public static bool IsValidTarget(Actor a) => a.IsTargetable && !a.IsAlly && !a.IsDead;

        public void Clear()
        {
            Valid.Clear();
        }

        public bool AddIfValid(Actor a)
        {
            if (!IsValidTarget(a))
                return false;
            Valid.Add(a);
            return true;
        }

        public bool AddIfValid(IEnumerable<Actor> actors)
        {
            var prevSize = Valid.Count;
            Valid.AddRange(actors.Where(IsValidTarget));
            return Valid.Count > prevSize;
        }

        // fill automatically based on world state
        public void Autofill(WorldState ws)
        {
            Valid.AddRange(ws.Actors.Where(a => a.Type == ActorType.Enemy && a.InCombat && IsValidTarget(a)));
        }
    }
}
