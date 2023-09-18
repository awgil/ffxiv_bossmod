using System.Collections.Generic;
using System;

namespace BossMod.Endwalker.Criterion.C02AMR.C021Shishio
{
    class UnnaturalWail : Components.UniformStackSpread
    {
        public int NumMechanics { get; private set; }
        private List<Actor> _spreadTargets = new();
        private List<Actor> _stackTargets = new();
        private DateTime _spreadResolve;
        private DateTime _stackResolve;

        public UnnaturalWail() : base(6, 6, 2, 2, alwaysShowSpreads: true) { }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (_spreadResolve == default || _stackResolve == default)
                return;
            var orderHint = _spreadResolve > _stackResolve ? $"Stack -> Spread" : $"Spread -> Stack";
            hints.Add($"Debuff order: {orderHint}");
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.ScatteredWailing:
                    _spreadTargets.Add(actor);
                    _spreadResolve = status.ExpireAt;
                    UpdateMechanic();
                    break;
                case SID.IntensifiedWailing:
                    _stackTargets.Add(actor);
                    _stackResolve = status.ExpireAt;
                    UpdateMechanic();
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.NUnnaturalAilment:
                case AID.SUnnaturalAilment:
                    if (_spreadResolve != default)
                    {
                        _spreadResolve = default;
                        ++NumMechanics;
                        UpdateMechanic();
                    }
                    break;
                case AID.NUnnaturalForce:
                case AID.SUnnaturalForce:
                    if (_stackResolve != default)
                    {
                        _stackResolve = default;
                        ++NumMechanics;
                        UpdateMechanic();
                    }
                    break;
            }
        }

        private void UpdateMechanic()
        {
            Stacks.Clear();
            Spreads.Clear();
            if (_stackResolve != default && (_spreadResolve == default || _spreadResolve > _stackResolve))
                AddStacks(_stackTargets, _stackResolve);
            if (_spreadResolve != default && (_stackResolve == default || _stackResolve > _spreadResolve))
                AddSpreads(_spreadTargets, _spreadResolve);
        }
    }
}
