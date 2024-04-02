namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

// note: this assumes rinon strat - initial markers stack center, everyone else spreads
// TODO: reconsider visualization (player prios etc - spread part is not real...)
class CaloricTheory1Part1 : Components.UniformStackSpread
{
    private BitMask _initialMarkers; // these shouldn't spread

    public CaloricTheory1Part1() : base(7, 7, 1) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CaloricTheory1InitialFire && module.WorldState.Actors.Find(spell.TargetID) is var target && target != null)
        {
            AddStack(target, spell.NPCFinishAt);
            foreach (var (_, p) in module.Raid.WithSlot(true).ExcludedFromMask(_initialMarkers))
                AddSpread(p, spell.NPCFinishAt);
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CaloricTheory1InitialFire)
        {
            Stacks.Clear();
            Spreads.Clear();
        }
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.CaloricTheory1InitialFire)
            _initialMarkers.Set(module.Raid.FindSlot(actor.InstanceID));
    }
}

class CaloricTheory1Part2 : Components.UniformStackSpread
{
    public CaloricTheory1Part2() : base(7, 0, 2) { }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (IsStackTarget(actor))
        {
            // TODO: do all strategies require fires to stand still here?
            hints.Add("Stay still!", false);
        }
        else
        {
            // TODO: movement hints for air players (tricky for inner...)
            base.AddHints(module, slot, actor, hints, movementHints);
        }
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Pyrefaction)
            AddStack(actor, status.ExpireAt);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.PyrePulseAOE)
            Stacks.Clear();
    }
}

class CaloricTheory1Part3 : Components.UniformStackSpread
{
    private BitMask _spreads;

    public CaloricTheory1Part3() : base(7, 7, 2) { }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (IsSpreadTarget(actor))
        {
            // TODO: do all strategies require airs to stand still here?
            hints.Add("Stay still!", false);
        }
        else
        {
            hints.Add(IsStackTarget(actor) ? "Stack with non-debuffed!" : "Stack with fire!", false);
            base.AddHints(module, slot, actor, hints, movementHints);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.PyrePulseAOE:
                Stacks.Clear();
                break;
            case AID.DynamicAtmosphere:
                Spreads.Clear();
                break;
        }
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.Pyrefaction:
                if (status.ExpireAt > module.WorldState.CurrentTime.AddSeconds(1)) // don't pick up debuffs from previous part
                    AddStack(actor, status.ExpireAt, _spreads);
                break;
            case SID.Atmosfaction:
                AddSpread(actor, status.ExpireAt);
                _spreads.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
        }
    }
}

class CaloricTheory2Part1 : Components.UniformStackSpread
{
    public CaloricTheory2Part1() : base(7, 7, 1, 1) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CaloricTheory2InitialFire:
                if (module.WorldState.Actors.Find(spell.TargetID) is var fireTarget && fireTarget != null)
                    AddStack(fireTarget, spell.NPCFinishAt); // fake stack
                break;
            case AID.CaloricTheory2InitialWind:
                if (module.WorldState.Actors.Find(spell.TargetID) is var windTarget && windTarget != null)
                    AddSpread(windTarget, spell.NPCFinishAt);
                break;
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.CaloricTheory2InitialFire or AID.CaloricTheory2InitialWind)
        {
            Stacks.Clear();
            Spreads.Clear();
        }
    }
}

class CaloricTheory2Part2 : Components.UniformStackSpread
{
    public bool Done { get; private set; }

    public CaloricTheory2Part2() : base(7, 7, alwaysShowSpreads: true) { }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Entropifaction)
        {
            if (status.Extra > 1)
                AddStack(actor);
            else
                AddSpread(actor);
        }
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Entropifaction)
        {
            Stacks.RemoveAll(s => s.Target == actor);
            AddSpread(actor);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DynamicAtmosphere)
            Done = true;
    }
}

class EntropicExcess : Components.LocationTargetedAOEs
{
    public EntropicExcess() : base(ActionID.MakeSpell(AID.EntropicExcess), 7) { }
}
