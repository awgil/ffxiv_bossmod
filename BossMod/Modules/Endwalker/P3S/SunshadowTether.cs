using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.P3S
{
    using static BossModule;

    // state related to sunshadow tethers during fountain of fire mechanics
    class SunshadowTether : Component
    {
        private P3S _module;
        private List<Actor> _sunshadows;
        private HashSet<uint> _chargedSunshadows = new();
        private ulong _playersInAOE = 0;

        public int NumCharges => _chargedSunshadows.Count;
        private IEnumerable<Actor> _activeBirds => _sunshadows.Where(bird => !_chargedSunshadows.Contains(bird.InstanceID));

        private static float _chargeHalfWidth = 3;

        public SunshadowTether(P3S module)
        {
            _module = module;
            _sunshadows = module.Enemies(OID.Sunshadow);
        }

        public override void Update()
        {
            _playersInAOE = 0;
            foreach (var bird in _activeBirds)
            {
                uint targetID = BirdTarget(bird);
                var target = targetID != 0 ? _module.WorldState.Actors.Find(targetID) : null;
                if (target != null && target.Position != bird.Position)
                {
                    var dir = Vector3.Normalize(target.Position - bird.Position);
                    foreach ((int i, var player) in _module.Raid.WithSlot().Exclude(target))
                    {
                        if (GeometryUtils.PointInRect(player.Position - bird.Position, dir, 50, 0, _chargeHalfWidth))
                        {
                            BitVector.SetVector64Bit(ref _playersInAOE, i);
                        }
                    }
                }
            }
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            foreach (var bird in _activeBirds)
            {
                uint birdTarget = BirdTarget(bird);
                if (birdTarget == actor.InstanceID && bird.Tether.ID != (uint)TetherID.LargeBirdFar)
                {
                    hints.Add("Too close!");
                }
            }

            if (BitVector.IsVector64BitSet(_playersInAOE, slot))
            {
                hints.Add("GTFO from charge zone!");
            }
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var bird in _activeBirds)
            {
                uint targetID = BirdTarget(bird);
                var target = (targetID != 0 && targetID != pc.InstanceID) ? _module.WorldState.Actors.Find(targetID) : null;
                if (target != null && target.Position != bird.Position)
                {
                    var dir = Vector3.Normalize(target.Position - bird.Position);
                    arena.ZoneQuad(bird.Position, dir, 50, 0, _chargeHalfWidth, arena.ColorAOE);
                }
            }
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (_activeBirds.Count() == 0)
                return;

            // draw all players
            foreach ((int i, var player) in _module.Raid.WithSlot())
                arena.Actor(player, BitVector.IsVector64BitSet(_playersInAOE, i) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);

            // draw my tether
            var myBird = _sunshadows.Find(bird => BirdTarget(bird) == pc.InstanceID);
            if (myBird != null && !_chargedSunshadows.Contains(myBird.InstanceID))
            {
                arena.AddLine(myBird.Position, pc.Position, myBird.Tether.ID != (uint)TetherID.LargeBirdFar ? arena.ColorDanger : arena.ColorSafe);
                arena.Actor(myBird, arena.ColorEnemy);
            }
        }

        public override void OnEventCast(CastEvent info)
        {
            if (info.IsSpell(AID.Fireglide))
                _chargedSunshadows.Add(info.CasterID);
        }

        private uint BirdTarget(Actor bird)
        {
            // we don't get tether messages when birds spawn, so use target as a fallback
            // TODO: investigate this... we do get actor-control 503 before spawn, maybe this is related somehow...
            return bird.Tether.Target != 0 ? bird.Tether.Target : bird.TargetID;
        }
    }
}
