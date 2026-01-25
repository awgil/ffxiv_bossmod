namespace BossMod.Dawntrail.Savage.RM12S1TheLindwurm;

class CurtainCallSpreadHint(BossModule module) : BossComponent(module)
{
    readonly string?[] Job = new string?[8];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Job.BoundSafeAt(slot) is { } j)
            hints.Add($"Next: {j}", false);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.BurstingGrotesquerie:
                Job[Raid.FindSlot(actor.InstanceID)] = "Avoid cone";
                break;
            case SID.RottingFlesh:
                Job[Raid.FindSlot(actor.InstanceID)] = "Stand in cone";
                break;
        }
    }
}

class RavenousReachInverted(BossModule module) : Components.GenericAOEs(module, AID.RavenousReachCone)
{
    private BitMask _soak;
    public bool Risky;

    readonly List<Actor> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _casters)
            yield return new(new AOEShapeCone(35, 60.Degrees()), c.CastInfo!.LocXZ, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), Risky: Risky, Inverted: _soak[slot]);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Add(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _casters.Remove(caster);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.RottingFlesh && Raid.TryFindSlot(actor, out var slot))
            _soak.Set(slot);
    }
}

class CurtainCallStackSpread(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumCells;
    public int NumSpreads;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.RottingFlesh:
            case SID.BurstingGrotesquerie:
                Spreads.Add(new(actor, 6, status.ExpireAt));
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.RottingFlesh:
                NumCells++;
                Spreads.RemoveAll(s => s.Target == actor);
                break;
            case SID.BurstingGrotesquerie:
                NumSpreads++;
                Spreads.RemoveAll(s => s.Target == actor);
                break;
        }
    }
}

class CurtainCallChains(BossModule module) : Components.Chains(module, (uint)TetherID.Cell, chainLength: 20);
class CurtainCallChainSpread(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.UnbreakableA or SID.UnbreakableB)
            Spreads.Add(new(actor, 4, status.ExpireAt));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.UnbreakableA or SID.UnbreakableB)
            Spreads.RemoveAll(s => s.Target == actor);
    }
}
