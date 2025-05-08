namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

// note: this assumes rinon strat - initial markers stack center, everyone else spreads
// TODO: reconsider visualization (player prios etc - spread part is not real...)
class CaloricTheory1Part1(BossModule module) : Components.UniformStackSpread(module, 7, 7, 1)
{
    private BitMask _initialMarkers; // these shouldn't spread

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CaloricTheory1InitialFire && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
        {
            AddStack(target, Module.CastFinishAt(spell));
            foreach (var (_, p) in Raid.WithSlot(true).ExcludedFromMask(_initialMarkers))
                AddSpread(p, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CaloricTheory1InitialFire)
        {
            Stacks.Clear();
            Spreads.Clear();
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.CaloricTheory1InitialFire)
            _initialMarkers.Set(Raid.FindSlot(actor.InstanceID));
    }
}

class CaloricTheory1Part2(BossModule module) : Components.UniformStackSpread(module, 7, 0, 2)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsStackTarget(actor))
        {
            // TODO: do all strategies require fires to stand still here?
            hints.Add("Stay still!", false);
        }
        else
        {
            // TODO: movement hints for air players (tricky for inner...)
            base.AddHints(slot, actor, hints);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Pyrefaction)
            AddStack(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.PyrePulseAOE)
            Stacks.Clear();
    }
}

class CaloricTheory1Part3(BossModule module) : Components.UniformStackSpread(module, 7, 7, 2)
{
    private BitMask _spreads;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsSpreadTarget(actor))
        {
            // TODO: do all strategies require airs to stand still here?
            hints.Add("Stay still!", false);
        }
        else
        {
            hints.Add(IsStackTarget(actor) ? "Stack with non-debuffed!" : "Stack with fire!", false);
            base.AddHints(slot, actor, hints);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
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

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.Pyrefaction:
                if (status.ExpireAt > WorldState.FutureTime(1)) // don't pick up debuffs from previous part
                    AddStack(actor, status.ExpireAt, _spreads);
                break;
            case SID.Atmosfaction:
                AddSpread(actor, status.ExpireAt);
                _spreads.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }
}

class CaloricTheory2Part1(BossModule module) : Components.UniformStackSpread(module, 7, 7, 1, 1)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CaloricTheory2InitialFire:
                if (WorldState.Actors.Find(spell.TargetID) is var fireTarget && fireTarget != null)
                    AddStack(fireTarget, Module.CastFinishAt(spell)); // fake stack
                break;
            case AID.CaloricTheory2InitialWind:
                if (WorldState.Actors.Find(spell.TargetID) is var windTarget && windTarget != null)
                    AddSpread(windTarget, Module.CastFinishAt(spell));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.CaloricTheory2InitialFire or AID.CaloricTheory2InitialWind)
        {
            Stacks.Clear();
            Spreads.Clear();
        }
    }
}

class CaloricTheory2Part2(BossModule module) : Components.UniformStackSpread(module, 7, 7, alwaysShowSpreads: true)
{
    public bool Done { get; private set; }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Entropifaction)
        {
            if (status.Extra > 1)
                AddStack(actor);
            else
                AddSpread(actor);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Entropifaction)
        {
            Stacks.RemoveAll(s => s.Target == actor);
            AddSpread(actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DynamicAtmosphere)
            Done = true;
    }
}

class EntropicExcess(BossModule module) : Components.StandardAOEs(module, AID.EntropicExcess, 7);
