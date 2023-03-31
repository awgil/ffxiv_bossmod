using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2Dahu
{
    class SpitFlame : Components.UniformStackSpread
    {
        private Actor?[] _targets = { null, null, null, null };
        private List<Actor> _adds = new();

        public SpitFlame() : base(0, 4, alwaysShowSpreads: true, raidwideOnResolve: false) { }

        public override void Init(BossModule module)
        {
            _adds = module.Enemies(OID.Marchosias);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);
            if (Array.IndexOf(_targets, actor) is var order && order >= 0)
            {
                hints.Add($"Order: {order + 1}", false);
                if (!_adds.Any(add => add.Position.InCircle(actor.Position, SpreadRadius)))
                    hints.Add("Hit at least one add!");
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.Actors(_adds, ArenaColor.Object, true);
            base.DrawArenaForeground(module, pcSlot, pc, arena);
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            var order = (IconID)iconID switch
            {
                IconID.SpitFlame1 => 1,
                IconID.SpitFlame2 => 2,
                IconID.SpitFlame3 => 3,
                IconID.SpitFlame4 => 4,
                _ => 0
            };
            if (order > 0)
            {
                AddSpread(actor);
                _targets[order - 1] = actor;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.SpitFlameAOE)
                Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
        }
    }
}
