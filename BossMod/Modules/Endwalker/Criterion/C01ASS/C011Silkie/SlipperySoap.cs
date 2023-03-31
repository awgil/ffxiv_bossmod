using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Criterion.C01ASS.C011Silkie
{
    static class SlipperySoap
    {
        public enum Color { None, Green, Blue, Yellow }

        public static Color ColorForStatus(uint sid) => (SID)sid switch
        {
            SID.BracingSudsBoss => Color.Green,
            SID.ChillingSudsBoss => Color.Blue,
            SID.FizzlingSudsBoss => Color.Yellow,
            _ => Color.None
        };
    }

    class SlipperySoapCharge : Components.Knockback
    {
        private Actor? _chargeTarget;
        private Angle _chargeDir;
        private AOEShapeRect _chargeShape = new(0, 5);
        private SlipperySoap.Color _color;
        private DateTime _chargeResolve;

        public bool ChargeImminent => _chargeTarget != null;

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            if (_chargeTarget != null && _color == SlipperySoap.Color.Green)
                yield return new(module.PrimaryActor.Position, 15, _chargeResolve, _chargeShape, _chargeDir, Kind.DirForward);
        }

        public override void Update(BossModule module)
        {
            if (_chargeTarget != null)
            {
                var toTarget = _chargeTarget.Position - module.PrimaryActor.Position;
                _chargeShape.LengthFront = toTarget.Length() + 0.01f; // add eps to ensure charge target is considered 'inside'
                _chargeDir = Angle.FromDirection(toTarget); // keep shape's offset zero to properly support dir-forward
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);
            if (_chargeTarget != null)
            {
                if (_chargeTarget != actor && !_chargeShape.Check(actor.Position, module.PrimaryActor.Position, _chargeDir))
                    hints.Add("Stack inside charge!");

                switch (_color)
                {
                    case SlipperySoap.Color.Blue:
                        hints.Add("Move!", false);
                        break;
                    case SlipperySoap.Color.Yellow:
                        hints.Add("Prepare to spread", false);
                        break;
                }
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_chargeTarget != null)
                _chargeShape.Draw(arena, module.PrimaryActor.Position, _chargeDir, ArenaColor.SafeFromAOE);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            base.OnStatusGain(module, actor, status);
            if (actor != module.PrimaryActor)
                return;
            var color = SlipperySoap.ColorForStatus(status.ID);
            if (color != SlipperySoap.Color.None)
                _color = color;
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            switch ((AID)spell.Action.ID)
            {
                case AID.SlipperySoapTargetSelection:
                    _chargeTarget = module.WorldState.Actors.Find(spell.MainTargetID);
                    _chargeResolve = module.WorldState.CurrentTime.AddSeconds(5.5f);
                    break;
                case AID.NSlipperySoapAOEBlue:
                case AID.NSlipperySoapAOEGreen:
                case AID.NSlipperySoapAOEYellow:
                case AID.SSlipperySoapAOEBlue:
                case AID.SSlipperySoapAOEGreen:
                case AID.SSlipperySoapAOEYellow:
                    _chargeTarget = null;
                    break;
            }
        }
    }

    class SlipperySoapAOE : Components.GenericAOEs
    {
        private SlipperySoap.Color _color;

        public bool Active => _color != SlipperySoap.Color.None;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            // TODO: activation
            switch (_color)
            {
                case SlipperySoap.Color.Green:
                    yield return new(C011Silkie.ShapeGreen, module.PrimaryActor.Position, module.PrimaryActor.Rotation);
                    break;
                case SlipperySoap.Color.Blue:
                    yield return new(C011Silkie.ShapeBlue, module.PrimaryActor.Position, module.PrimaryActor.Rotation);
                    break;
                case SlipperySoap.Color.Yellow:
                    yield return new(C011Silkie.ShapeYellow, module.PrimaryActor.Position, module.PrimaryActor.Rotation + 45.Degrees());
                    yield return new(C011Silkie.ShapeYellow, module.PrimaryActor.Position, module.PrimaryActor.Rotation + 135.Degrees());
                    yield return new(C011Silkie.ShapeYellow, module.PrimaryActor.Position, module.PrimaryActor.Rotation - 135.Degrees());
                    yield return new(C011Silkie.ShapeYellow, module.PrimaryActor.Position, module.PrimaryActor.Rotation - 45.Degrees());
                    break;
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if (actor != module.PrimaryActor)
                return;
            var color = SlipperySoap.ColorForStatus(status.ID);
            if (color != SlipperySoap.Color.None)
                _color = color;
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if ((AID)spell.Action.ID is AID.NChillingDusterBoss or AID.NBracingDusterBoss or AID.NFizzlingDusterBoss or AID.SChillingDusterBoss or AID.SBracingDusterBoss or AID.SFizzlingDusterBoss)
                _color = SlipperySoap.Color.None;
        }
    }

    // note: we don't wait for forked lightning statuses to appear
    class SoapsudStatic : Components.UniformStackSpread
    {
        public SoapsudStatic() : base(0, 5) { }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if (actor == module.PrimaryActor && SlipperySoap.ColorForStatus(status.ID) == SlipperySoap.Color.Yellow)
                AddSpreads(module.Raid.WithoutSlot(true));
        }
    }
}
