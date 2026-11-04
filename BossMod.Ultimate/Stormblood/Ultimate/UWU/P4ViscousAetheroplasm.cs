namespace BossMod.Stormblood.Ultimate.UWU;

class P4ViscousAetheroplasmApply(BossModule module) : Components.StrictUniformStackSpread(module, 0, 2)
{
    bool Applied;
    public DateTime NextExpected;
    public int NumCasts { get; private set; }

    public int Iteration;

    public override (WPos, float)? GetSpreadDestination(int order, Actor target)
    {
        // on ultima spawn, take it on A marker
        if (Iteration == 1)
            return (new(100, 93.3f), 0);

        // post-predation, assume we're using PF convention to dodge landslides north near 2 marker
        if (Iteration == 2)
            return (new(104, 88), 0);

        return default;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ViscousAetheroplasmApply)
            NumCasts++;
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.ViscousAetheroplasm)
        {
            Applied = true;
            Spreads.Clear();
        }
    }

    public override void Update()
    {
        if (Applied)
            return;

        if (((UWU)Module).Ultima() is { } ultima && WorldState.Actors.Find(ultima.TargetID) is { } target)
        {
            Spreads.Clear();
            AddSpread(target, NextExpected);
        }
    }
}

class P4ViscousAetheroplasmResolve(BossModule module) : Components.StrictUniformStackSpread(module, 4, 4, 7, alwaysShowSpreads: true)
{
    bool LaserSelected;
    BitMask Aetheroplasms;

    // aetheroplasm 1: stack on south edge of ultima, why not
    public override (WPos, float)? GetStackDestination(int order, Actor target) => (new(100, 98), 1);

    // aetheroplasm 2: invulning tank parks east
    public override (WPos, float)? GetSpreadDestination(int order, Actor target) => (new(105, 95), 0);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HomingLasers)
        {
            LaserSelected = true;

            var player = WorldState.Actors.Find(spell.TargetID);
            if (player != null)
                SetLaserTarget(Raid.FindSlot(player.InstanceID), player);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ViscousAetheroplasmResolve:
                Aetheroplasms.Reset();
                Stacks.Clear();
                Spreads.Clear();
                break;
            case AID.HomingLasers:
                foreach (ref var s in Stacks.AsSpan())
                    s.ForbiddenPlayers.Clear(Raid.FindSlot(spell.MainTargetID));
                break;
        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.ViscousAetheroplasm)
            Aetheroplasms.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID is OID.Garuda or OID.GarudaSister && id == 0x1E3A)
            EnableHints = true;
    }

    public override void Update()
    {
        if (LaserSelected || ((UWU)Module).Ultima() is not { } boss)
            return;

        if (Raid.WithSlot().WhereActor(a => a.Role == Role.Tank && boss.TargetID != a.InstanceID).FirstOrNull() is not (var laserSlot, var laserTarget))
            return;

        SetLaserTarget(laserSlot, laserTarget);
    }

    void SetLaserTarget(int slot, Actor player)
    {
        Stacks.Clear();
        Spreads.Clear();

        foreach (var (t, target) in Raid.WithSlot().IncludedInMask(Aetheroplasms))
        {
            if (t == slot)
                AddSpread(player, player.FindStatus(SID.ViscousAetheroplasm)?.ExpireAt ?? default);
            else
                AddStack(target, target.FindStatus(SID.ViscousAetheroplasm)?.ExpireAt ?? default, BitMask.Build(slot));
        }
    }
}

class P5ViscousAetheroplasmTriple : Components.UniformStackSpread
{
    public int NumCasts { get; private set; }
    private readonly List<(Actor target, DateTime resolve)> _aetheroplasms = [];

    public P5ViscousAetheroplasmTriple(BossModule module) : base(module, 4, 0, 8)
    {
        EnableHints = false;
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.ViscousAetheroplasm)
        {
            _aetheroplasms.Add((actor, status.ExpireAt));
            _aetheroplasms.SortBy(a => a.resolve);
            UpdateStackTargets();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ViscousAetheroplasmResolve)
        {
            ++NumCasts;
            _aetheroplasms.RemoveAll(a => a.target.InstanceID == spell.MainTargetID);
            UpdateStackTargets();
        }
    }

    private void UpdateStackTargets()
    {
        Stacks.Clear();
        if (_aetheroplasms.Count > 0)
            AddStack(_aetheroplasms[0].target, _aetheroplasms[0].resolve);
    }
}
