using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Criterion.C01ASS.C012Gladiator
{
    class CurseOfTheFallen : Components.UniformStackSpread
    {
        private List<Actor> _fallen = new();
        private Actor? _thunderous;
        private BitMask _lingering;
        private DateTime _spreadResolve;
        private DateTime _stackResolve;
        private bool _dirty;

        public CurseOfTheFallen() : base(5, 6, 3, 3, true) { }

        public override void Update(BossModule module)
        {
            if (_dirty)
            {
                _dirty = false;

                Spreads.Clear();
                Stacks.Clear();

                if (_fallen.Count > 0 && (_thunderous == null || _spreadResolve < _stackResolve))
                {
                    AddSpreads(_fallen, _spreadResolve);
                }
                else if (_thunderous != null && (_fallen.Count == 0 || _stackResolve < _spreadResolve))
                {
                    AddStack(_thunderous, _stackResolve, _lingering);
                }
            }
            base.Update(module);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.EchoOfTheFallen:
                    _fallen.Add(actor);
                    _spreadResolve = status.ExpireAt;
                    _dirty = true;
                    break;
                case SID.ThunderousEcho:
                    _thunderous = actor;
                    _stackResolve = status.ExpireAt;
                    _dirty = true;
                    break;
                case SID.LingeringEchoes:
                    _lingering.Set(module.Raid.FindSlot(actor.InstanceID));
                    _dirty = true;
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.NEchoOfTheFallen:
                case AID.SEchoOfTheFallen:
                    _fallen.RemoveAll(a => a.InstanceID == spell.MainTargetID);
                    _dirty = true;
                    break;
                case AID.NThunderousEcho:
                case AID.SThunderousEcho:
                    _thunderous = null;
                    _dirty = true;
                    break;
                case AID.NLingeringEcho:
                case AID.SLingeringEcho:
                    _lingering.Reset();
                    _dirty = true;
                    break;
            }
        }
    }

    class RingOfMight1Out : Components.SelfTargetedAOEs
    {
        public RingOfMight1Out(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCircle(8)) { }
    }
    class NRingOfMight1Out : RingOfMight1Out { public NRingOfMight1Out() : base(AID.NRingOfMight1Out) { } }
    class SRingOfMight1Out : RingOfMight1Out { public SRingOfMight1Out() : base(AID.SRingOfMight1Out) { } }

    class RingOfMight2Out : Components.SelfTargetedAOEs
    {
        public RingOfMight2Out(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCircle(13)) { }
    }
    class NRingOfMight2Out : RingOfMight2Out { public NRingOfMight2Out() : base(AID.NRingOfMight2Out) { } }
    class SRingOfMight2Out : RingOfMight2Out { public SRingOfMight2Out() : base(AID.SRingOfMight2Out) { } }

    class RingOfMight3Out : Components.SelfTargetedAOEs
    {
        public RingOfMight3Out(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCircle(18)) { }
    }
    class NRingOfMight3Out : RingOfMight3Out { public NRingOfMight3Out() : base(AID.NRingOfMight3Out) { } }
    class SRingOfMight3Out : RingOfMight3Out { public SRingOfMight3Out() : base(AID.SRingOfMight3Out) { } }

    class RingOfMight1In : Components.SelfTargetedAOEs
    {
        public RingOfMight1In(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeDonut(8, 30)) { }
    }
    class NRingOfMight1In : RingOfMight1In { public NRingOfMight1In() : base(AID.NRingOfMight1In) { } }
    class SRingOfMight1In : RingOfMight1In { public SRingOfMight1In() : base(AID.SRingOfMight1In) { } }

    class RingOfMight2In : Components.SelfTargetedAOEs
    {
        public RingOfMight2In(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeDonut(13, 30)) { }
    }
    class NRingOfMight2In : RingOfMight2In { public NRingOfMight2In() : base(AID.NRingOfMight2In) { } }
    class SRingOfMight2In : RingOfMight2In { public SRingOfMight2In() : base(AID.SRingOfMight2In) { } }

    class RingOfMight3In : Components.SelfTargetedAOEs
    {
        public RingOfMight3In(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeDonut(18, 30)) { }
    }
    class NRingOfMight3In : RingOfMight3In { public NRingOfMight3In() : base(AID.NRingOfMight3In) { } }
    class SRingOfMight3In : RingOfMight3In { public SRingOfMight3In() : base(AID.SRingOfMight3In) { } }
}
