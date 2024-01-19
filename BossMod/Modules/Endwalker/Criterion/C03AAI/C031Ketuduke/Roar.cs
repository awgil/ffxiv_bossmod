using System.Collections.Generic;

namespace BossMod.Endwalker.Criterion.C03AAI.C031Ketuduke
{
    class Roar : Components.GenericBaitAway
    {
        public bool Active;
        private BitMask _playerBubbles;
        private List<(Actor actor, bool bubble)> _snakes = new();
        private bool _highlightSnakes;

        private static AOEShapeCone _shape = new(60, 90.Degrees());

        public override void Update(BossModule module)
        {
            CurrentBaits.Clear();
            if (Active)
            {
                foreach (var s in _snakes)
                {
                    var target = module.Raid.WithoutSlot().Closest(s.actor.Position);
                    if (target != null)
                        CurrentBaits.Add(new(s.actor, target, _shape));
                }
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);
            foreach (var s in _snakes)
            {
                arena.Actor(s.actor, ArenaColor.Object, true);
                if (_highlightSnakes && s.bubble != _playerBubbles[pcSlot])
                    arena.AddCircle(s.actor.Position, 1, ArenaColor.Safe);
            }
        }

        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID is OID.NZaratan or OID.SZaratan)
                _snakes.Add((actor, false));
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.Bubble:
                    var index = _snakes.FindIndex(s => s.actor == actor);
                    if (index >= 0)
                        _snakes[index] = (actor, true);
                    _highlightSnakes = true;
                    break;
                case SID.BubbleWeave:
                    _playerBubbles.Set(module.Raid.FindSlot(actor.InstanceID));
                    _highlightSnakes = true;
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.NHundredLashingsNormal or AID.NHundredLashingsBubble or AID.SHundredLashingsNormal or AID.SHundredLashingsBubble)
            {
                ++NumCasts;
                _snakes.Clear();
            }
        }
    }
}
