using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.Ultimate.DSW1
{
    // includes knockback + charges
    // TODO: show hint for execution
    class ShiningBlade : BossModule.Component
    {
        private Actor? _knockbackSource;
        private List<Vector3> _flares = new(); // [0] = initial boss offset from center, [2] = first charge offset, [5] = second charge offset, [7] = third charge offset, [10] = fourth charge offset == [0]
        private int _doneFlares;
        private int _doneCharges;

        public bool Done => _doneFlares >= _flares.Count;

        private static AOEShapeCircle _aoeTear = new(9); // TODO: verify
        private static AOEShapeCircle _aoeFlare = new(9);
        private static float _knockbackDistance = 16;

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
                if (_flares.Skip(_doneFlares).Take(7).Any(o => _aoeFlare.Check(actor.Position, module.Arena.WorldCenter + o, 0)))
                    hints.Add("GTFO from explosion!");
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var p in module.Enemies(OID.AetherialTear))
                _aoeTear.Draw(arena, p);
            foreach (var o in _flares.Skip(_doneFlares).Take(7))
                _aoeFlare.Draw(arena, arena.WorldCenter + o, 0);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_knockbackSource != null)
            {
                var adjPos = BossModule.AdjustPositionForKnockback(pc.Position, _knockbackSource, _knockbackDistance);
                arena.Actor(adjPos, 0, arena.ColorDanger);
                arena.AddLine(pc.Position, adjPos, arena.ColorDanger);
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (actor.CastInfo!.IsSpell(AID.FaithUnmoving))
            {
                _knockbackSource = actor;
                _flares.Add(module.PrimaryActor.Position - module.Arena.WorldCenter); // note: this assumes Ser Adelphel is primary...
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
            if (info.IsSpell(AID.ShiningBlade) && info.CasterID == module.PrimaryActor.InstanceID)
            {
                ++_doneCharges;
                if (_flares.Count == 1)
                {
                    var startOffset = module.PrimaryActor.Position - module.Arena.WorldCenter;
                    var endOffset = startOffset + GeometryUtils.DirectionToVec3(module.PrimaryActor.Rotation) * 31.113f; // 22 * sqrt(2)
                    AddShortFlares(startOffset, endOffset);
                    AddLongFlares(endOffset, -endOffset);
                    AddShortFlares(-endOffset, -startOffset);
                    AddLongFlares(-startOffset, startOffset);
                }
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
