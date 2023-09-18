using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Criterion.C02AMR.C021Shishio
{
    class NoblePursuit : Components.GenericAOEs
    {
        private WPos _posAfterLastCharge;
        private List<AOEInstance> _charges = new();
        private List<AOEInstance> _rings = new();

        private static float _chargeHalfWidth = 6;
        private static AOEShapeRect _shapeRing = new(5, 50, 5);

        public bool Active => _charges.Count + _rings.Count > 0;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            var firstActivation = _charges.Count > 0 ? _charges[0].Activation : _rings.Count > 0 ? _rings[0].Activation : default;
            var deadline = firstActivation.AddSeconds(2.5f);
            foreach (var aoe in _charges.Concat(_rings).Where(aoe => aoe.Activation <= deadline))
                yield return aoe;
        }

        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID is OID.NRairin)
            {
                if (_charges.Count == 0)
                {
                    module.ReportError(this, "Ring appeared while no charges are in progress");
                    return;
                }

                // see whether this ring shows next charge
                if (!_charges.Last().Check(actor.Position))
                {
                    var nextDir = actor.Position - _posAfterLastCharge;
                    if (Math.Abs(nextDir.X) < 0.1)
                        nextDir.X = 0;
                    if (Math.Abs(nextDir.Z) < 0.1)
                        nextDir.Z = 0;
                    nextDir = nextDir.Normalized();
                    var ts = module.Bounds.Center + nextDir.Sign() * module.Bounds.HalfSize - _posAfterLastCharge;
                    var t = Math.Min(nextDir.X != 0 ? ts.X / nextDir.X : float.MaxValue, nextDir.Z != 0 ? ts.Z / nextDir.Z : float.MaxValue);
                    _charges.Add(new(new AOEShapeRect(t, _chargeHalfWidth), _posAfterLastCharge, Angle.FromDirection(nextDir), _charges.Last().Activation.AddSeconds(1.4f)));
                    _posAfterLastCharge += nextDir * t;
                }

                // ensure ring rotations are expected
                if (!_charges.Last().Rotation.AlmostEqual(actor.Rotation, 0.1f))
                {
                    module.ReportError(this, "Unexpected rotation for ring inside last pending charge");
                }

                _rings.Add(new(_shapeRing, actor.Position, actor.Rotation, _charges.Last().Activation.AddSeconds(0.8f)));
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.NNoblePursuitFirst or AID.SNoblePursuitFirst)
            {
                var dir = spell.LocXZ - caster.Position;
                _charges.Add(new(new AOEShapeRect(dir.Length(), _chargeHalfWidth), caster.Position, Angle.FromDirection(dir), spell.FinishAt));
                _posAfterLastCharge = spell.LocXZ;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.NNoblePursuitFirst:
                case AID.NNoblePursuitRest:
                case AID.SNoblePursuitFirst:
                case AID.SNoblePursuitRest:
                    if (_charges.Count > 0)
                        _charges.RemoveAt(0);
                    ++NumCasts;
                    break;
                case AID.NLevinburst:
                case AID.SLevinburst:
                    _rings.RemoveAll(r => r.Origin.AlmostEqual(caster.Position, 1));
                    break;
            }
        }
    }
}
