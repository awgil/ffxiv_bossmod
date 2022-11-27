using System;

namespace BossMod.Endwalker.Criterion.C01ASS.C012Gladiator
{
    class CurseOfTheFallen : Components.StackSpread
    {
        private BitMask _fallen;
        private int _thunderous = -1;
        private int _lingering = -1;
        private DateTime _spreadResolve;
        private DateTime _stackResolve;
        private bool _dirty;

        public CurseOfTheFallen() : base(5, 6, 3, 3, true) { }

        public override void Update(BossModule module)
        {
            if (_dirty)
            {
                _dirty = false;

                SpreadMask = StackMask = AvoidMask = new();
                if (_fallen.Any() && (_thunderous == -1 || _spreadResolve < _stackResolve))
                {
                    SpreadMask = _fallen;
                    ActivateAt = _spreadResolve;
                }
                else if (_thunderous != -1 && (_fallen.None() || _stackResolve < _spreadResolve))
                {
                    StackMask.Set(_thunderous);
                    AvoidMask.Set(_lingering);
                    ActivateAt = _stackResolve;
                }
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.EchoOfTheFallen:
                    _fallen.Set(module.Raid.FindSlot(actor.InstanceID));
                    _spreadResolve = status.ExpireAt;
                    _dirty = true;
                    break;
                case SID.ThunderousEcho:
                    _thunderous = module.Raid.FindSlot(actor.InstanceID);
                    _stackResolve = status.ExpireAt;
                    _dirty = true;
                    break;
                case SID.LingeringEchoes:
                    _lingering = module.Raid.FindSlot(actor.InstanceID);
                    _dirty = true;
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.EchoOfTheFallen:
                    _fallen.Clear(module.Raid.FindSlot(spell.MainTargetID));
                    _dirty = true;
                    break;
                case AID.ThunderousEcho:
                    _thunderous = -1;
                    _dirty = true;
                    break;
                case AID.LingeringEcho:
                    _lingering = -1;
                    _dirty = true;
                    break;
            }
        }
    }

    class RingOfMight1Out : Components.SelfTargetedAOEs
    {
        public RingOfMight1Out() : base(ActionID.MakeSpell(AID.RingOfMight1Out), new AOEShapeCircle(8)) { }
    }

    class RingOfMight2Out : Components.SelfTargetedAOEs
    {
        public RingOfMight2Out() : base(ActionID.MakeSpell(AID.RingOfMight2Out), new AOEShapeCircle(13)) { }
    }

    class RingOfMight3Out : Components.SelfTargetedAOEs
    {
        public RingOfMight3Out() : base(ActionID.MakeSpell(AID.RingOfMight3Out), new AOEShapeCircle(18)) { }
    }

    class RingOfMight1In : Components.SelfTargetedAOEs
    {
        public RingOfMight1In() : base(ActionID.MakeSpell(AID.RingOfMight1In), new AOEShapeDonut(8, 30)) { }
    }

    class RingOfMight2In : Components.SelfTargetedAOEs
    {
        public RingOfMight2In() : base(ActionID.MakeSpell(AID.RingOfMight2In), new AOEShapeDonut(13, 30)) { }
    }

    class RingOfMight3In : Components.SelfTargetedAOEs
    {
        public RingOfMight3In() : base(ActionID.MakeSpell(AID.RingOfMight3In), new AOEShapeDonut(18, 30)) { }
    }
}
