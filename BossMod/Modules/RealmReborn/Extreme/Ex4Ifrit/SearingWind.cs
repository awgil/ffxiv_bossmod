using System;

namespace BossMod.RealmReborn.Extreme.Ex4Ifrit
{
    // note on mechanic: typically boss casts inferno howl, then target gains status, then a total of 3 searing wind casts are baited on target
    // however, sometimes (typically on phase switches) boss might cast new inferno howl while previous target still has debuff with large timer
    // in such case old target will not have any more searing winds cast on it, despite having debuff
    // TODO: verify whether searing wind on previous target can still be cast if inferno howl is in progress?
    class SearingWind : Components.StackSpread
    {
        private int _searingWindsLeft;
        private DateTime _showHintsAfter = DateTime.MaxValue;

        public SearingWind() : base(0, 14)
        {
            KeepOnPhaseChange = true;
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            // we let AI provide soft positioning hints until resolve is imminent
            if (module.WorldState.CurrentTime > _showHintsAfter)
                base.AddAIHints(module, slot, actor, assignment, hints);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.InfernoHowl)
            {
                SpreadMask.Reset();
                SpreadMask.Set(module.Raid.FindSlot(spell.TargetID));
                ActivateAt = module.WorldState.CurrentTime.AddSeconds(5.4f);
                _searingWindsLeft = 3;
                _showHintsAfter = ActivateAt.AddSeconds(-2);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            // note: there are 3 casts total, 6s apart - last one happens ~4.8s before status expires
            if ((AID)spell.Action.ID == AID.SearingWind)
            {
                if (--_searingWindsLeft == 0)
                {
                    SpreadMask.Reset();
                    _showHintsAfter = DateTime.MaxValue;
                }
                else
                {
                    ActivateAt = module.WorldState.CurrentTime.AddSeconds(6);
                }
            }
        }
    }
}
