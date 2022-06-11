using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.Ultimate.DSW1
{
    // includes knockback + charges
    class ShiningBlade : BossModule.Component
    {
        private Actor? _serAdelphel; // casts charges and execution
        private Actor? _knockbackSource;
        private Actor? _executionTarget;
        private List<Vector3> _flares = new(); // [0] = initial boss offset from center, [2] = first charge offset, [5] = second charge offset, [7] = third charge offset, [10] = fourth charge offset == [0]
        private int _doneFlares;
        private int _doneCharges;

        public bool Done => _doneFlares >= _flares.Count;

        private static AOEShapeCircle _aoeTear = new(9); // TODO: verify
        private static AOEShapeCircle _aoeFlare = new(9);
        private static float _knockbackDistance = 16;
        private static float _executionRadius = 5;

        public override void Init(BossModule module)
        {
            _serAdelphel = module.Enemies(OID.SerAdelphel).FirstOrDefault();
            _executionTarget = module.WorldState.Actors.Find(_serAdelphel?.TargetID ?? 0);
        }

        public override void Update(BossModule module)
        {
            if (_serAdelphel == null)
                return;
            if (_flares.Count == 0)
            {
                // add first flare as soon as boss teleports to border
                var bossOffset = _serAdelphel.Position - module.Arena.WorldCenter;
                if (Utils.AlmostEqual(bossOffset.LengthSquared(), module.Arena.WorldHalfSize * module.Arena.WorldHalfSize, 1))
                {
                    _flares.Add(bossOffset);
                }
            }
            if (_flares.Count == 1 && Utils.AlmostEqual(_serAdelphel.Rotation.Abs().Rad % (MathF.PI / 2), MathF.PI / 4, 0.1f))
            {
                // add remaining flares as soon as boss rotates
                var startOffset = _flares[0];
                var endOffset = startOffset + _serAdelphel.Rotation.ToDirection() * 31.113f; // 22 * sqrt(2)
                AddShortFlares(startOffset, endOffset);
                AddLongFlares(endOffset, -endOffset);
                AddShortFlares(-endOffset, -startOffset);
                AddLongFlares(-startOffset, startOffset);
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
        {
            if (_knockbackSource != null)
            {
                var adjPos = BossModule.AdjustPositionForKnockback(actor.Position, _knockbackSource, _knockbackDistance);
                if (!module.Arena.InBounds(adjPos))
                    hints.Add("About to be knocked into wall!");
                if (module.Enemies(OID.AetherialTear).InRadius(adjPos, _aoeTear.Radius).Any())
                    hints.Add("About to be knocked into tear!");
                if (Vector3.Dot(adjPos - module.Arena.WorldCenter, _flares[0]) >= 0)
                    hints.Add("Aim away from boss!");
            }
            else
            {
                if (_flares.Skip(_doneFlares).Take(7).Any(o => _aoeFlare.Check(actor.Position, module.Arena.WorldCenter + o, new())))
                    hints.Add("GTFO from explosion!");
            }

            if (_doneCharges > 0 && _executionTarget != null)
            {
                if (_executionTarget == actor)
                {
                    if (module.Raid.WithoutSlot().InRadiusExcluding(_executionTarget, _executionRadius).Any())
                        hints.Add("GTFO from raid!");
                }
                else
                {
                    if (GeometryUtils.PointInCircle(actor.Position - _executionTarget.Position, _executionRadius))
                        hints.Add("GTFO from tank!");
                }
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var p in module.Enemies(OID.AetherialTear))
                _aoeTear.Draw(arena, p);
            foreach (var o in _flares.Skip(_doneFlares).Take(7))
                _aoeFlare.Draw(arena, arena.WorldCenter + o, new());
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_knockbackSource != null)
            {
                var adjPos = BossModule.AdjustPositionForKnockback(pc.Position, _knockbackSource, _knockbackDistance);
                arena.Actor(adjPos, new(), arena.ColorDanger);
                arena.AddLine(pc.Position, adjPos, arena.ColorDanger);
            }

            if (_doneCharges > 0 && _executionTarget != null)
            {
                arena.Actor(_executionTarget, arena.ColorDanger);
                arena.AddCircle(_executionTarget.Position, _executionRadius, arena.ColorDanger);
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.FaithUnmoving))
            {
                _knockbackSource = actor;
            }
        }

        public override void OnCastFinished(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.FaithUnmoving))
            {
                _knockbackSource = null;
            }
            else if (actor.CastInfo.IsSpell(AID.BrightFlare))
            {
                ++_doneFlares;
            }
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (!info.IsSpell())
                return;
            switch ((AID)info.Action.ID)
            {
                case AID.ShiningBlade:
                    ++_doneCharges;
                    break;
                case AID.Execution:
                    _executionTarget = null;
                    break;
            }
        }

        private void AddShortFlares(Vector3 startOffset, Vector3 endOffset)
        {
            _flares.Add((startOffset + endOffset) / 2);
            _flares.Add(endOffset);
        }

        private void AddLongFlares(Vector3 startOffset, Vector3 endOffset)
        {
            var frac = 7.5f / 22;
            _flares.Add(startOffset * frac);
            _flares.Add(endOffset * frac);
            _flares.Add(endOffset);
        }
    }
}
