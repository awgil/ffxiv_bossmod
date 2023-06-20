using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P12S1Athena
{
    // TODO: for first instance, common strategy has tanks baiting everything and invulning - accomodate that
    // TODO: for second instance, set forbidden baiters
    class WhiteFlame : Components.GenericBaitAway
    {
        private bool _enabled;
        private List<Actor> _sources = new();

        private static AOEShapeRect _shape = new(100, 2);

        public void Enable() => _enabled = true;

        public override void Update(BossModule module)
        {
            CurrentBaits.Clear();
            if (_enabled)
                foreach (var s in _sources.Take(2))
                    foreach (var t in module.Raid.WithoutSlot().SortedByRange(s.Position).Take(2))
                        CurrentBaits.Add(new(s, t, _shape));
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.Actors(_sources, ArenaColor.Object, true);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.WhiteFlameAOE)
            {
                foreach (var t in spell.Targets)
                    ForbiddenPlayers.Set(module.Raid.FindSlot(t.ID));
                _sources.RemoveAll(p => p.Position.AlmostEqual(caster.Position, 1));
                ++NumCasts;
            }
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            // note: technically we could determine location ~1.1s earlier by looking at TeleportAdd cast location
            // however, BaitAway component requires source actor, and teleporting actors don't move to target immediately
            // TODO: consider using 3F2E actor PATE 24E3 instead (it gets destroyed before resolve, guaranteeing that source doesn't move)
            if ((OID)actor.OID == OID.Anthropos && id == 0x1E46)
                _sources.Add(actor);
        }
    }
}
