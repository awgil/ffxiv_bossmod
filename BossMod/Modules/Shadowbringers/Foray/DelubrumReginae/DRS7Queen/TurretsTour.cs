using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7Queen
{
    class TurretsTour : Components.GenericAOEs
    {
        private List<(Actor turret, AOEShapeRect shape)> _turrets = new();
        private List<(Actor caster, AOEShapeRect shape)> _casters = new();
        private DateTime _activation;

        private static AOEShapeRect _turretShape = new(50, 3);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var t in _turrets)
                yield return new(t.shape, t.turret.Position, t.turret.Rotation, _activation);
            foreach (var c in _casters)
                yield return new(c.shape, c.caster.Position, c.caster.CastInfo!.Rotation, c.caster.CastInfo.FinishAt);
        }

        public override void Init(BossModule module)
        {
            var turrets = module.Enemies(OID.AutomaticTurret);
            foreach (var t in turrets)
            {
                var shape = new AOEShapeRect(50, 3);
                var target = turrets.Exclude(t).InShape(shape, t).Closest(t.Position);
                if (target != null)
                    shape.LengthFront = (target.Position - t.Position).Length();
                _turrets.Add((t, shape));
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.TurretsTourAOE1)
            {
                var shape = new AOEShapeRect(0, 3);
                shape.SetEndPoint(spell.LocXZ, caster.Position, spell.Rotation);
                _casters.Add((caster, shape));
                _activation = spell.FinishAt;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.TurretsTourAOE1)
                _casters.RemoveAll(c => c.caster == caster);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.TurretsTourAOE2 or AID.TurretsTourAOE3)
            {
                _turrets.RemoveAll(t => t.turret == caster);
                ++NumCasts;
            }
        }
    }
}
