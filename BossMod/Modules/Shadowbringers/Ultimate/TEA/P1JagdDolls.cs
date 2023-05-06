using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P1JagdDolls : BossComponent
    {
        public int NumExhausts { get; private set; }
        private List<Actor> _dolls = new();
        private HashSet<ulong> _exhaustsDone = new();

        private static float _exhaustRadius = 8.8f;

        private IEnumerable<Actor> ActiveDolls => _dolls.Where(d => d.IsTargetable && !d.IsDead);
        public bool Active => ActiveDolls.Any();

        public override void Init(BossModule module)
        {
            _dolls = module.Enemies(OID.JagdDoll);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (NumExhausts < 2 && ActiveDolls.InRadius(actor.Position, _exhaustRadius).Count() > 1)
            {
                hints.Add("GTFO from exhaust intersection");
            }
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            foreach (var t in hints.PotentialTargets.Where(t => (OID)t.Actor.OID == OID.JagdDoll))
                t.ForbidDOTs = true;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var doll in ActiveDolls)
            {
                arena.Actor(doll, doll.HP.Cur < doll.HP.Max / 4 ? ArenaColor.Enemy : ArenaColor.Vulnerable);

                var tether = module.WorldState.Actors.Find(doll.Tether.Target);
                if (tether != null)
                {
                    arena.AddLine(doll.Position, tether.Position, ArenaColor.Danger);
                }

                if (NumExhausts < 2)
                {
                    arena.AddCircle(doll.Position, _exhaustRadius, ArenaColor.Safe);
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.Exhaust && NumExhausts < 2)
            {
                if (!_exhaustsDone.Contains(caster.InstanceID))
                {
                    NumExhausts = 1;
                    _exhaustsDone.Add(caster.InstanceID);
                }
                else
                {
                    NumExhausts = 2;
                }
            }
        }
    }
}
