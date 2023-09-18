using System.Collections.Generic;
using System;

namespace BossMod.Endwalker.Criterion.C02AMR.C022Gorai
{
    class SealOfScurryingSparks : Components.UniformStackSpread
    {
        public int NumMechanics { get; private set; }
        private List<Actor> _spreadTargets = new();
        private List<Actor> _stackTargets = new();
        private DateTime _spreadResolve;
        private DateTime _stackResolve;

        public SealOfScurryingSparks() : base(6, 10, alwaysShowSpreads: true) { }

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
                case SID.LiveBrazier:
                    _stackTargets.Add(actor);
                    _stackResolve = status.ExpireAt;
                    UpdateStackSpread();
                    break;
                case SID.LiveCandle:
                    _spreadTargets.Add(actor);
                    _spreadResolve = status.ExpireAt;
                    UpdateStackSpread();
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.NGreaterBallOfFire:
                case AID.SGreaterBallOfFire:
                    if (_stackResolve != default)
                    {
                        ++NumMechanics;
                        _stackTargets.Clear();
                        _stackResolve = default;
                        UpdateStackSpread();
                    }
                    break;
                case AID.NGreatBallOfFire:
                case AID.SGreatBallOfFire:
                    if (_spreadResolve != default)
                    {
                        ++NumMechanics;
                        _spreadTargets.Clear();
                        _spreadResolve = default;
                        UpdateStackSpread();
                    }
                    break;
            }
        }

        private void UpdateStackSpread()
        {
            Spreads.Clear();
            Stacks.Clear();
            if (_stackResolve == default || _stackResolve > _spreadResolve)
                AddSpreads(_spreadTargets, _spreadResolve);
            if (_spreadResolve == default || _spreadResolve > _stackResolve)
                AddStacks(_stackTargets, _stackResolve);
        }
    }
}
