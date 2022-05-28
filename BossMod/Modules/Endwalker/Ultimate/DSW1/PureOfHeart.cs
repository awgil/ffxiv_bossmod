using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW1
{
    // counts cone casts; do we also care about shockwaves?..
    class PureOfHeart : CommonComponents.CastCounter
    {
        private List<Actor> _skyblindCasters = new();
        private Actor? _boss;
        private ulong _skyblindPlayers;
        private ulong _coneTargets;

        private static AOEShapeCone _brightwingAOE = new(18, MathF.PI / 12); // TODO: verify angle
        private static float _skyblindRadius = 3;

        public PureOfHeart() : base(ActionID.MakeSpell(AID.Brightwing)) { }

        public override void Init(BossModule module)
        {
            _boss = module.Enemies(OID.SerCharibert).FirstOrDefault();
        }

        public override void Update(BossModule module)
        {
            _coneTargets = _boss != null && NumCasts < 8 ? module.Raid.WithSlot().SortedByRange(_boss.Position).Take(2).Mask() : 0;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_boss == null)
                return;

            if (BitVector.IsVector64BitSet(_coneTargets, slot))
            {
                var dir = GeometryUtils.DirectionFromVec3(actor.Position - _boss.Position);
                if (module.Raid.WithoutSlot().Exclude(actor).Any(p => _brightwingAOE.Check(p.Position, _boss.Position, dir)))
                    hints.Add("Aim cone away from others!");
            }
            else
            {
                if (module.Raid.WithSlot().IncludedInMask(_coneTargets).Any(sa => _brightwingAOE.Check(actor.Position, _boss.Position, GeometryUtils.DirectionFromVec3(sa.Item2.Position - _boss.Position))))
                    hints.Add("GTFO from cone!");
            }

            if (BitVector.IsVector64BitSet(_skyblindPlayers, slot) && module.Raid.WithSlot().ExcludedFromMask(_skyblindPlayers).InRadius(actor.Position, _skyblindRadius).Any())
                hints.Add("GTFO from raid!");

            if (_skyblindCasters.Any(c => GeometryUtils.PointInCircle(actor.Position - c.CastInfo!.Location, _skyblindRadius)))
                hints.Add("GTFO from puddle!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_boss != null)
            {
                foreach (var (_, p) in module.Raid.WithSlot().IncludedInMask(_coneTargets))
                {
                    _brightwingAOE.Draw(arena, _boss.Position, GeometryUtils.DirectionFromVec3(p.Position - _boss.Position));
                }
            }

            foreach (var c in _skyblindCasters)
            {
                arena.ZoneCircle(c.CastInfo!.Location, _skyblindRadius, arena.ColorAOE);
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var (slot, player) in module.Raid.WithSlot())
            {
                if (BitVector.IsVector64BitSet(_skyblindPlayers, slot))
                {
                    arena.Actor(player, arena.ColorDanger);
                    arena.AddCircle(player.Position, _skyblindRadius, arena.ColorDanger);
                }
                else
                {
                    arena.Actor(player, BitVector.IsVector64BitSet(_coneTargets, slot) ? arena.ColorDanger : arena.ColorPlayerGeneric);
                }
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, int index)
        {
            if ((SID)actor.Statuses[index].ID == SID.Skyblind)
            {
                int slot = module.Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    BitVector.SetVector64Bit(ref _skyblindPlayers, slot);
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, int index)
        {
            if ((SID)actor.Statuses[index].ID == SID.Skyblind)
            {
                int slot = module.Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    BitVector.ClearVector64Bit(ref _skyblindPlayers, slot);
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.Skyblind))
                _skyblindCasters.Add(actor);
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.Skyblind))
                _skyblindCasters.Remove(actor);
        }
    }
}
