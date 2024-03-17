using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P10SPandaemonium
{
    class Silkspit : Components.UniformStackSpread
    {
        private IReadOnlyList<Actor> _pillars = ActorEnumeration.EmptyList;

        public Silkspit() : base(0, 7) { }

        public override void Init(BossModule module)
        {
            _pillars = module.Enemies(OID.Pillar);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);
            if (IsSpreadTarget(actor) && _pillars.InRadius(actor.Position, SpreadRadius).Any())
                hints.Add("GTFO from pillars!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaBackground(module, pcSlot, pc, arena);
            arena.Actors(_pillars, ArenaColor.Object, true);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.SilkspitAOE)
                Spreads.Clear();
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.Silkspit)
                AddSpread(actor, module.WorldState.CurrentTime.AddSeconds(8));
        }
    }
}
