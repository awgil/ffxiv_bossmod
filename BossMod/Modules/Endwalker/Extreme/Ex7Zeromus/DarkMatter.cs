using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Extreme.Ex7Zeromus
{
    class DarkMatter : Components.GenericBaitAway
    {
        private List<int> _remainingCasts = new();

        private static AOEShapeCircle _shape = new(8);

        public int RemainingCasts => _remainingCasts.Count > 0 ? _remainingCasts.Min() : 0;

        public DarkMatter() : base(centerAtTarget: true) { }

        public override void Update(BossModule module)
        {
            for (int i = CurrentBaits.Count - 1; i >= 0; i--)
            {
                if (CurrentBaits[i].Target.IsDestroyed || CurrentBaits[i].Target.IsDead)
                {
                    CurrentBaits.RemoveAt(i);
                    _remainingCasts.RemoveAt(i);
                }
            }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.DarkMatter)
            {
                CurrentBaits.Add(new(module.PrimaryActor, actor, _shape));
                _remainingCasts.Add(3);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.DarkMatterAOE)
            {
                ++NumCasts;
                var index = CurrentBaits.FindIndex(b => b.Target.InstanceID == spell.MainTargetID);
                if (index >= 0)
                {
                    --_remainingCasts[index];
                }
            }
        }
    }

    class ForkedLightningDarkBeckons : Components.UniformStackSpread
    {
        public ForkedLightningDarkBeckons() : base(6, 5, 4, alwaysShowSpreads: true) { }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.ForkedLightning)
                AddSpread(actor, status.ExpireAt);
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            switch ((IconID)iconID)
            {
                case IconID.DarkBeckonsUmbralRays:
                    AddStack(actor, module.WorldState.CurrentTime.AddSeconds(5.1f));
                    break;
                case IconID.DarkMatter:
                    foreach (ref var s in Stacks.AsSpan())
                        s.ForbiddenPlayers.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.ForkedLightning or AID.DarkBeckons)
            {
                Spreads.Clear();
                Stacks.Clear();
            }
        }
    }
}
