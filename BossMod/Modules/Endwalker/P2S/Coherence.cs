using System.Linq;
using System.Numerics;

namespace BossMod.P2S
{
    using static BossModule;

    // state related to coherence mechanic
    // TODO: i'm not 100% sure how exactly it selects target for aoe ray, and who should that be...
    class Coherence : CommonComponents.CastCounter
    {
        private P2S _module;
        private Actor? _closest;
        private ulong _inRay = 0;

        private static float _aoeRadius = 10; // not sure about this - actual range is 60, but it has some sort of falloff
        private static float _rayHalfWidth = 3;

        public Coherence(P2S module)
            : base(ActionID.MakeSpell(AID.CoherenceRay))
        {
            _module = module;
        }

        public override void Update()
        {
            _closest = null;
            _inRay = 0;
            var boss = _module.Boss();
            if (boss == null)
                return;

            float minDistSq = 100000;
            foreach (var player in _module.Raid.WithoutSlot())
            {
                if (boss.Tether.Target == player.InstanceID)
                    continue; // assume both won't target same player for tethers and ray...

                float dist = (player.Position - boss.Position).LengthSquared();
                if (dist < minDistSq)
                {
                    minDistSq = dist;
                    _closest = player;
                }
            }

            if (_closest == null)
                return;

            var dirToClosest = Vector3.Normalize(_closest.Position - boss.Position);
            foreach ((int i, var player) in _module.Raid.WithSlot())
            {
                if (boss.Tether.Target == player.InstanceID)
                    continue; // assume both won't target same player for tethers and ray...
                if (player == _closest || GeometryUtils.PointInRect(player.Position - boss.Position, dirToClosest, 50, 0, _rayHalfWidth))
                    BitVector.SetVector64Bit(ref _inRay, i);
            }
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var boss = _module.Boss();
            if (boss?.Tether.Target == actor.InstanceID)
            {
                if (actor.Role != Role.Tank)
                {
                    hints.Add("Pass tether to tank!");
                }
                else if (_module.Raid.WithoutSlot().InRadiusExcluding(actor, _aoeRadius).Any())
                {
                    hints.Add("GTFO from raid!");
                }
            }
            else if (actor == _closest)
            {
                if (actor.Role != Role.Tank)
                {
                    hints.Add("Go behind tank!");
                }
                else if (BitOperations.PopCount(_inRay) < 7)
                {
                    hints.Add("Make sure ray hits everyone!");
                }
            }
            else
            {
                if (actor.Role == Role.Tank)
                {
                    hints.Add("Go in front of raid!");
                }
                else if (!BitVector.IsVector64BitSet(_inRay, slot))
                {
                    hints.Add("Go behind tank!");
                }
            }
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            var boss = _module.Boss();
            if (boss == null || _closest == null || boss.Position == _closest.Position)
                return;

            var dir = Vector3.Normalize(_closest.Position - boss.Position);
            arena.ZoneQuad(boss.Position, dir, 50, 0, _rayHalfWidth, arena.ColorAOE);
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            var boss = _module.Boss();
            if (boss == null || _closest == null || boss.Position == _closest.Position)
                return;

            // TODO: i'm not sure what are the exact mechanics - flare is probably distance-based, and ray is probably shared damage cast at closest target?..
            foreach ((int i, var player) in _module.Raid.WithSlot())
            {
                if (boss.Tether.Target == player.InstanceID)
                {
                    arena.AddLine(player.Position, boss.Position, arena.ColorDanger);
                    arena.Actor(player, arena.ColorDanger);
                    arena.AddCircle(player.Position, _aoeRadius, arena.ColorDanger);
                }
                else if (player == _closest)
                {
                    arena.Actor(player, arena.ColorDanger);
                }
                else
                {
                    arena.Actor(player, BitVector.IsVector64BitSet(_inRay, i) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                }
            }
        }
    }
}
