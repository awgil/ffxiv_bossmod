namespace BossMod.Dawntrail.Savage.RM04SWickedThunder;

class ElectrifyingWitchHuntBurst(BossModule module) : Components.StandardAOEs(module, AID.ElectrifyingWitchHuntBurst, new AOEShapeRect(40, 8));

class ElectrifyingWitchHuntSpread(BossModule module) : Components.UniformStackSpread(module, 0, 6)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ElectrifyingWitchHunt)
            AddSpreads(Raid.WithoutSlot(true), Module.CastFinishAt(spell, 0.1f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ElectrifyingWitchHuntAOE)
            Spreads.Clear();
    }
}

class ElectrifyingWitchHuntResolve(BossModule module) : Components.GenericStackSpread(module)
{
    public enum Mechanic { None, Near, Far }

    public Mechanic CurMechanic;
    public BitMask ForbidBait;
    private BitMask _baits;
    private DateTime _activation;

    public override void Update()
    {
        _baits = CurMechanic switch
        {
            Mechanic.Near => Raid.WithSlot().SortedByRange(Module.Center).Take(4).Mask(),
            Mechanic.Far => Raid.WithSlot().SortedByRange(Module.Center).TakeLast(4).Mask(),
            _ => default
        };

        Spreads.Clear();
        foreach (var (i, p) in Raid.WithSlot(true))
        {
            if (ForbidBait[i])
                Spreads.Add(new(p, 5, _activation));
            if (_baits[i])
                Spreads.Add(new(p, 6, _activation));
        }

        base.Update();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurMechanic != Mechanic.None)
        {
            var wantBait = !ForbidBait[slot];
            var baitNear = CurMechanic == Mechanic.Near;
            hints.Add(wantBait == baitNear ? "Go near!" : "Go far!", _baits[slot] == ForbidBait[slot]);
        }
        base.AddHints(slot, actor, hints);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.ForkedLightning:
                ForbidBait.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.Marker:
                CurMechanic = status.Extra switch
                {
                    0x2F6 => Mechanic.Near,
                    0x2F7 => Mechanic.Far,
                    _ => Mechanic.None
                };
                _activation = status.ExpireAt;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ElectrifyingWitchHuntBait:
                CurMechanic = Mechanic.None;
                break;
            case AID.ForkedLightning:
                ForbidBait.Reset();
                break;
        }
    }
}
