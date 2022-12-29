using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2Dahu
{
    class SpitFlame : Components.StackSpread
    {
        private List<Actor> _adds = new();

        public SpitFlame() : base(0, 4, alwaysShowSpreads: true, raidwideOnResolve: false) { }

        public override void Init(BossModule module)
        {
            _adds = module.Enemies(OID.Marchosias);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);
            if (SpreadTargets.IndexOf(actor) is var order && order >= 0)
            {
                hints.Add($"Order: {order}", false);
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
            if ((IconID)iconID is IconID.SpitFlame1 or IconID.SpitFlame2 or IconID.SpitFlame3 or IconID.SpitFlame4)
                SpreadTargets.Add(actor);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.SpitFlameAOE)
                SpreadTargets.RemoveAll(a => a.InstanceID == spell.MainTargetID);
        }
    }
}
