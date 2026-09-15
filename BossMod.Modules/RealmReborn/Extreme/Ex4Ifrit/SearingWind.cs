namespace BossMod.RealmReborn.Extreme.Ex4Ifrit;

// note on mechanic: typically boss casts inferno howl, then target gains status, then a total of 3 searing wind casts are baited on target
// however, sometimes (typically on phase switches) boss might cast new inferno howl while previous target still has debuff with large timer
// in such case old target will not have any more searing winds cast on it, despite having debuff
// TODO: verify whether searing wind on previous target can still be cast if inferno howl is in progress?
class SearingWind : Components.UniformStackSpread
{
    private int _searingWindsLeft;
    private DateTime _showHintsAfter = DateTime.MaxValue;

    public SearingWind(BossModule module) : base(module, 0, 14)
    {
        KeepOnPhaseChange = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // we let AI provide soft positioning hints until resolve is imminent
        if (WorldState.CurrentTime > _showHintsAfter)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.InfernoHowl)
        {
            Spreads.Clear();
            if (WorldState.Actors.Find(spell.TargetID) is var target && target != null)
                AddSpread(target, WorldState.FutureTime(5.4f));
            _searingWindsLeft = 3;
            _showHintsAfter = WorldState.FutureTime(3.4f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        // note: there are 3 casts total, 6s apart - last one happens ~4.8s before status expires
        if ((AID)spell.Action.ID == AID.SearingWind)
        {
            if (--_searingWindsLeft == 0)
            {
                Spreads.Clear();
                _showHintsAfter = DateTime.MaxValue;
            }
            else
            {
                foreach (ref var s in Spreads.AsSpan())
                    s.Activation = WorldState.FutureTime(6);
            }
        }
    }
}
