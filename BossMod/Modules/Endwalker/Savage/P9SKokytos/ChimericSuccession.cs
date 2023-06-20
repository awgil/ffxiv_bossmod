using System;

namespace BossMod.Endwalker.Savage.P9SKokytos
{
    class ChimericSuccession : Components.UniformStackSpread
    {
        public int NumCasts { get; private set; }
        private Actor?[] _baitOrder = { null, null, null, null };
        private BitMask _forbiddenStack;
        private DateTime _jumpActivation;

        public bool JumpActive => _jumpActivation != default;

        public ChimericSuccession() : base(6, 20, 4, alwaysShowSpreads: true) { }

        public override void Update(BossModule module)
        {
            Stacks.Clear();
            var target = JumpActive ? module.Raid.WithSlot().ExcludedFromMask(_forbiddenStack).Actors().Farthest(module.PrimaryActor.Position) : null;
            if (target != null)
                AddStack(target, _jumpActivation, _forbiddenStack);
            base.Update(module);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.FrontFirestrikes or AID.RearFirestrikes)
                _jumpActivation = spell.FinishAt.AddSeconds(0.4f);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.Icemeld1:
                case AID.Icemeld2:
                case AID.Icemeld3:
                case AID.Icemeld4:
                    ++NumCasts;
                    InitBaits(module);
                    break;
                case AID.PyremeldFront:
                case AID.PyremeldRear:
                    _jumpActivation = default;
                    break;
            }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            int order = (IconID)iconID switch
            {
                IconID.Icon1 => 0,
                IconID.Icon2 => 1,
                IconID.Icon3 => 2,
                IconID.Icon4 => 3,
                _ => -1
            };
            if (order < 0)
                return;
            _baitOrder[order] = actor;
            _forbiddenStack.Set(module.Raid.FindSlot(actor.InstanceID));
            if (order == 0)
                InitBaits(module);
        }

        private void InitBaits(BossModule module)
        {
            Spreads.Clear();
            var target = NumCasts < _baitOrder.Length ? _baitOrder[NumCasts] : null;
            if (target != null)
                AddSpread(target, module.WorldState.CurrentTime.AddSeconds(NumCasts == 0 ? 10.1f : 3));
        }
    }

    // TODO: think of a way to show baits before cast start to help aiming outside...
    class SwingingKickFront : Components.SelfTargetedAOEs
    {
        public SwingingKickFront() : base(ActionID.MakeSpell(AID.SwingingKickFront), new AOEShapeCone(40, 90.Degrees())) { }
    }
    class SwingingKickRear : Components.SelfTargetedAOEs
    {
        public SwingingKickRear() : base(ActionID.MakeSpell(AID.SwingingKickRear), new AOEShapeCone(40, 90.Degrees())) { }
    }
}
