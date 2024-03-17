using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P3SPhoinix
{
    // state related to sunshadow tethers during fountain of fire mechanics
    class SunshadowTether : BossComponent
    {
        private HashSet<ulong> _chargedSunshadows = new();
        private BitMask _playersInAOE;

        private static float _chargeHalfWidth = 3;

        public int NumCharges => _chargedSunshadows.Count;

        public override void Update(BossModule module)
        {
            _playersInAOE.Reset();
            foreach (var bird in ActiveBirds(module))
            {
                ulong targetID = BirdTarget(bird);
                var target = targetID != 0 ? module.WorldState.Actors.Find(targetID) : null;
                if (target != null && target.Position != bird.Position)
                {
                    var dir = (target.Position - bird.Position).Normalized();
                    foreach ((int i, var player) in module.Raid.WithSlot().Exclude(target))
                    {
                        if (player.Position.InRect(bird.Position, dir, 50, 0, _chargeHalfWidth))
                        {
                            _playersInAOE.Set(i);
                        }
                    }
                }
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            foreach (var bird in ActiveBirds(module))
            {
                ulong birdTarget = BirdTarget(bird);
                if (birdTarget == actor.InstanceID && bird.Tether.ID != (uint)TetherID.LargeBirdFar)
                {
                    hints.Add("Too close!");
                }
            }

            if (_playersInAOE[slot])
            {
                hints.Add("GTFO from charge zone!");
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var bird in ActiveBirds(module))
            {
                ulong targetID = BirdTarget(bird);
                var target = (targetID != 0 && targetID != pc.InstanceID) ? module.WorldState.Actors.Find(targetID) : null;
                if (target != null && target.Position != bird.Position)
                {
                    var dir = (target.Position - bird.Position).Normalized();
                    arena.ZoneRect(bird.Position, dir, 50, 0, _chargeHalfWidth, ArenaColor.AOE);
                }
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (!ActiveBirds(module).Any())
                return;

            // draw all players
            foreach ((int i, var player) in module.Raid.WithSlot())
                arena.Actor(player, _playersInAOE[i] ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);

            // draw my tether
            var myBird = module.Enemies(OID.Sunshadow).FirstOrDefault(bird => BirdTarget(bird) == pc.InstanceID);
            if (myBird != null && !_chargedSunshadows.Contains(myBird.InstanceID))
            {
                arena.AddLine(myBird.Position, pc.Position, myBird.Tether.ID != (uint)TetherID.LargeBirdFar ? ArenaColor.Danger : ArenaColor.Safe);
                arena.Actor(myBird, ArenaColor.Enemy);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.Fireglide)
                _chargedSunshadows.Add(caster.InstanceID);
        }

        private ulong BirdTarget(Actor bird)
        {
            // we don't get tether messages when birds spawn, so use target as a fallback
            // TODO: investigate this... we do get actor-control 503 before spawn, maybe this is related somehow...
            return bird.Tether.Target != 0 ? bird.Tether.Target : bird.TargetID;
        }

        private IEnumerable<Actor> ActiveBirds(BossModule module)
        {
            return module.Enemies(OID.Sunshadow).Where(bird => !_chargedSunshadows.Contains(bird.InstanceID));
        }
    }
}
