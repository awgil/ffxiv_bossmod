using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P8S2
{
    class Dominion : Components.StackSpread
    {
        public int NumDeformations { get; private set; }
        public int NumShifts { get; private set; }
        public List<Actor> Casters = new();
        private BitMask _secondOrder;

        private static float _towerRadius = 3;

        public Dominion() : base(0, 6) { }

        public override void Init(BossModule module)
        {
            SpreadTargets.AddRange(module.Raid.WithoutSlot());
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);

            if (NumDeformations >= 4)
            {
                hints.Add($"Soak order: {(_secondOrder[slot] ? "2" : "1")}", ShouldSoak(slot) && !ActiveTowers().Any(t => actor.Position.InCircle(t.Position, _towerRadius)));
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);

            if (NumDeformations >= 4 && ShouldSoak(pcSlot))
                foreach (var tower in ActiveTowers())
                    arena.AddCircle(tower.Position, _towerRadius, ArenaColor.Danger, 2);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.OrogenicShift)
            {
                Casters.Add(caster);
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.OrogenicShift)
            {
                ++NumShifts;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.OrogenicDeformation)
            {
                SpreadTargets.Clear();
                _secondOrder.Set(module.Raid.FindSlot(spell.MainTargetID));
                ++NumDeformations;
            }
        }

        private bool ShouldSoak(int slot) => NumShifts == (_secondOrder[slot] ? 4 : 0);

        private IEnumerable<Actor> ActiveTowers()
        {
            if (NumShifts == 0 && Casters.Count >= 4)
                foreach (var c in Casters.Take(4))
                    yield return c;
            else if (NumShifts == 4 && Casters.Count == 8)
                foreach (var c in Casters.Skip(4))
                    yield return c;
        }
    }
}
