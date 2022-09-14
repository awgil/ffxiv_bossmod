using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW1
{
    // includes knockback + charges
    class ShiningBlade : BossComponent
    {
        private Actor? _serAdelphel; // casts charges and execution
        private Actor? _knockbackSource;
        private Actor? _executionTarget;
        private List<WDir> _flares = new(); // [0] = initial boss offset from center, [2] = first charge offset, [5] = second charge offset, [7] = third charge offset, [10] = fourth charge offset == [0]
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
                var bossOffset = _serAdelphel.Position - module.Bounds.Center;
                if (Utils.AlmostEqual(bossOffset.LengthSq(), module.Bounds.HalfSize * module.Bounds.HalfSize, 1))
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

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_knockbackSource != null)
            {
                var adjPos = Components.Knockback.AwayFromSource(actor.Position, _knockbackSource, _knockbackDistance);
                if (!module.Bounds.Contains(adjPos))
                    hints.Add("About to be knocked into wall!");
                if (module.Enemies(OID.AetherialTear).InRadius(adjPos, _aoeTear.Radius).Any())
                    hints.Add("About to be knocked into tear!");
                if (_flares[0].Dot(adjPos - module.Bounds.Center) >= 0)
                    hints.Add("Aim away from boss!");
            }
            else
            {
                if (_flares.Skip(_doneFlares).Take(7).Any(o => _aoeFlare.Check(actor.Position, module.Bounds.Center + o)))
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
                    if (actor.Position.InCircle(_executionTarget.Position, _executionRadius))
                        hints.Add("GTFO from tank!");
                }
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var p in module.Enemies(OID.AetherialTear))
                _aoeTear.Draw(arena, p);
            foreach (var o in _flares.Skip(_doneFlares).Take(7))
                _aoeFlare.Draw(arena, module.Bounds.Center + o);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_knockbackSource != null)
            {
                var adjPos = Components.Knockback.AwayFromSource(pc.Position, _knockbackSource, _knockbackDistance);
                arena.Actor(adjPos, pc.Rotation, ArenaColor.Danger);
                arena.AddLine(pc.Position, adjPos, ArenaColor.Danger);
            }

            if (_doneCharges > 0 && _executionTarget != null)
            {
                arena.Actor(_executionTarget, ArenaColor.Danger);
                arena.AddCircle(_executionTarget.Position, _executionRadius, ArenaColor.Danger);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.FaithUnmoving:
                    _knockbackSource = caster;
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.FaithUnmoving:
                    _knockbackSource = null;
                    break;
                case AID.BrightFlare:
                    ++_doneFlares;
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.ShiningBlade:
                    ++_doneCharges;
                    break;
                case AID.Execution:
                    _executionTarget = null;
                    break;
            }
        }

        private void AddShortFlares(WDir startOffset, WDir endOffset)
        {
            _flares.Add((startOffset + endOffset) / 2);
            _flares.Add(endOffset);
        }

        private void AddLongFlares(WDir startOffset, WDir endOffset)
        {
            var frac = 7.5f / 22;
            _flares.Add(startOffset * frac);
            _flares.Add(endOffset * frac);
            _flares.Add(endOffset);
        }
    }
}
