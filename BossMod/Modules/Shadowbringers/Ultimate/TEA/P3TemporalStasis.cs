namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
sealed class P3TemporalStasis(BossModule module) : Components.GenericBaitAway(module, (uint)AID.FlarethrowerP3)
{
    public enum Mechanic { None, AvoidDamage, StayClose, StayFar }

    public bool Frozen;
    private readonly Mechanic[] _playerMechanics = new Mechanic[PartyState.MaxPartySize];

    private readonly AOEShapeCone _shapeBJ = new(100f, 45f.Degrees()); // TODO: verify angle
    private readonly AOEShapeCone _shapeCC = new(30f, 45f.Degrees()); // TODO: verify angle

    public override void Update()
    {
        CurrentBaits.Clear();
        if (BJ() is var bj && bj != null)
            CurrentBaits.AddRange(Raid.WithoutSlot(false, true, true).SortedByRange(bj.Position).Take(2).Select(t => new Bait(bj, t, _shapeBJ)));
        if (CC() is var cc && cc != null)
            CurrentBaits.AddRange(Raid.WithoutSlot(false, true, true).SortedByRange(cc.Position).Take(3).Select(t => new Bait(cc, t, _shapeCC)));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        switch (_playerMechanics[slot])
        {
            case Mechanic.StayClose:
                if (FindPartner(slot) is var partner1 && partner1 != null && (partner1.Position - actor.Position).LengthSq() > 25f)
                    hints.Add("Stay closer to partner!");
                break;
            case Mechanic.StayFar:
                if (FindPartner(slot) is var partner2 && partner2 != null && (partner2.Position - actor.Position).LengthSq() < 900f)
                    hints.Add("Stay farther from partner!");
                break;
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        movementHints.Add(actor.Position, SafeSpot(slot, actor), Colors.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        switch (_playerMechanics[pcSlot])
        {
            case Mechanic.StayClose:
                if (FindPartner(pcSlot) is var partner1 && partner1 != null)
                    Arena.AddLine(pc.Position, partner1.Position, (partner1.Position - pc.Position).LengthSq() > 25f ? default : Colors.Safe);
                break;
            case Mechanic.StayFar:
                if (FindPartner(pcSlot) is var partner2 && partner2 != null)
                    Arena.AddLine(pc.Position, partner2.Position, (partner2.Position - pc.Position).LengthSq() < 300f ? default : Colors.Safe);
                break;
        }

        Arena.Actor(BJ(), Colors.Enemy, true);
        Arena.Actor(CC(), Colors.Enemy, true);
        Arena.ZoneCircleOutline(SafeSpot(pcSlot, pc), 1f, Colors.Safe);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.AggravatedAssault:
                AssignMechanic(actor, Mechanic.AvoidDamage);
                ForbiddenPlayers.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.HouseArrest:
                AssignMechanic(actor, Mechanic.StayClose);
                break;
            case (uint)SID.RestrainingOrder:
                AssignMechanic(actor, Mechanic.StayFar);
                break;
            case (uint)SID.TemporalDisplacement:
                Frozen = true;
                break;
        }
    }

    private Actor? FindPartner(int slot)
    {
        var partnerSlot = -1;
        for (var i = 0; i < _playerMechanics.Length; ++i)
            if (i != slot && _playerMechanics[i] == _playerMechanics[slot])
                partnerSlot = i;
        return Raid[partnerSlot];
    }

    private WPos SafeSpot(int slot, Actor actor)
    {
        // using LPDU assignments:
        // - 'near' baiting N (th) / S (dd) of the eastern actor (BJ/CC doesn't matter)
        // - 'no debuff' baiting N (th) / S (dd) of the western actor (BJ/CC doesn't matter)
        // - 'far' E (th) / W (dd), whoever is closer to CC baits third aoe outside
        // - 'avoid' staying E/W, closer to BJ
        // BJ/CC are located at center +/- (6, 0)
        var bjLeft = BJ()?.PosRot.X < Arena.Center.X;
        return Arena.Center + _playerMechanics[slot] switch
        {
            Mechanic.AvoidDamage => new WDir(bjLeft ? -20f : +20f, default),
            Mechanic.StayClose => new WDir(6f, actor.Class.IsSupport() ? -2f : +2f),
            Mechanic.StayFar => new WDir(actor.Class.IsSupport() ? (bjLeft ? 15f : 20f) : (bjLeft ? -20f : -15f), default),
            _ => new WDir(-6f, actor.Class.IsSupport() ? -2f : +2f)
        };
    }

    private void AssignMechanic(Actor actor, Mechanic mechanic)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            _playerMechanics[slot] = mechanic;
    }

    private Actor? BJ() => ((TEA)Module).BruteJustice();
    private Actor? CC() => ((TEA)Module).CruiseChaser();
}
