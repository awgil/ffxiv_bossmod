using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod.Endwalker.Ultimate.DSW1
{
    class HyperdimensionalSlash : BossModule.Component
    {
        public int NumCasts { get; private set; }
        private List<Actor> _targets = new();
        private List<(Vector3 Pos, Actor? Source)> _tears = new();
        private ulong _riskyTears;

        private static float _linkRadius = 9; // TODO: verify
        private static AOEShapeRect _aoeLaser = new(70, 4);
        private static AOEShapeCone _aoeCone = new(40, MathF.PI / 3);

        public override void Update(BossModule module)
        {
            _tears.Clear();
            foreach (var tear in module.Enemies(OID.AetherialTear))
                _tears.Add((tear.Position, null));
            foreach (var target in _targets)
                _tears.Add((TearPosition(module, target), target));

            _riskyTears = 0;
            for (int i = 0; i < _tears.Count; ++i)
            {
                for (int j = i + 1; j < _tears.Count; ++j)
                {
                    if (GeometryUtils.PointInCircle(_tears[i].Pos - _tears[j].Pos, _linkRadius))
                    {
                        BitVector.SetVector64Bit(ref _riskyTears, i);
                        BitVector.SetVector64Bit(ref _riskyTears, j);
                    }
                }
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_targets.Count == 0)
                return;

            int tearIndex = _tears.FindIndex(t => t.Source == actor);
            if (tearIndex >= 0)
            {
                // make sure actor's tear placement is good
                if (BitVector.IsVector64BitSet(_riskyTears, tearIndex))
                    hints.Add("Aim away from other tears!");
                if (GeometryUtils.PointInCircle(actor.Position - _tears[tearIndex].Pos, _linkRadius))
                    hints.Add("Stay closer to center!");
            }

            // make sure actor is not clipped by any lasers and either not hit by cone (if is target of a laser) or is hit by a cone (otherwise) regardless of target choice
            bool clippingLasers = false, clippingCones = false, awayFromCones = false;
            foreach (var p in module.Raid.WithoutSlot().Exclude(actor))
            {
                var dir = GeometryUtils.DirectionFromVec3(p.Position - module.Arena.WorldCenter);
                if (_targets.Contains(p))
                {
                    clippingLasers |= _aoeLaser.Check(actor.Position, module.Arena.WorldCenter, dir);
                }
                else
                {
                    bool clipping = _aoeCone.Check(actor.Position, module.Arena.WorldCenter, dir);
                    clippingCones |= clipping;
                    awayFromCones |= !clipping;
                }
            }

            if (clippingLasers)
                hints.Add("GTFO from laser aoe!");

            if (tearIndex >= 0 && clippingCones)
                hints.Add("GTFO from cone aoe!");
            else if (tearIndex < 0 && awayFromCones)
                hints.Add("Stack with others!");
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_targets.Count == 0)
                return;

            foreach (var p in module.Raid.WithoutSlot())
            {
                var dir = GeometryUtils.DirectionFromVec3(p.Position - arena.WorldCenter);
                if (_targets.Contains(p))
                    _aoeLaser.Draw(arena, arena.WorldCenter, dir);
                else
                    _aoeCone.Draw(arena, arena.WorldCenter, dir);
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            for (int i = 0; i < _tears.Count; ++i)
                arena.AddCircle(_tears[i].Pos, _linkRadius, BitVector.IsVector64BitSet(_riskyTears, i) ? arena.ColorDanger : arena.ColorSafe);

            if (_targets.Contains(pc))
                arena.AddLine(arena.WorldCenter, TearPosition(module, pc), arena.ColorDanger);
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (info.IsSpell(AID.HyperdimensionalSlashAOERest))
            {
                _targets.Clear();
                ++NumCasts;
            }
        }

        public override void OnEventIcon(BossModule module, uint actorID, uint iconID)
        {
            if ((IconID)iconID == IconID.HyperdimensionalSlash)
            {
                var actor = module.WorldState.Actors.Find(actorID);
                if (actor != null)
                    _targets.Add(actor);
            }
        }

        private Vector3 TearPosition(BossModule module, Actor target)
        {
            return module.Arena.ClampToBounds(module.Arena.WorldCenter + 50 * Vector3.Normalize(target.Position - module.Arena.WorldCenter));
        }
    }
}
