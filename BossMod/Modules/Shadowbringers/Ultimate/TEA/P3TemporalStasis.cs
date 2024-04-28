namespace BossMod.Shadowbringers.Ultimate.TEA;

class P3TemporalStasis(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.FlarethrowerP3))
{
    public enum Mechanic { None, AvoidDamage, StayClose, StayFar }

    public bool Frozen { get; private set; }
    private readonly Mechanic[] _playerMechanics = new Mechanic[PartyState.MaxPartySize];

    private static readonly AOEShapeCone _shapeBJ = new(100, 45.Degrees()); // TODO: verify angle
    private static readonly AOEShapeCone _shapeCC = new(30, 45.Degrees()); // TODO: verify angle

    public override void Update()
    {
        CurrentBaits.Clear();
        if (BJ() is var bj && bj != null)
            CurrentBaits.AddRange(Raid.WithoutSlot().SortedByRange(bj.Position).Take(2).Select(t => new Bait(bj, t, _shapeBJ)));
        if (CC() is var cc && cc != null)
            CurrentBaits.AddRange(Raid.WithoutSlot().SortedByRange(cc.Position).Take(3).Select(t => new Bait(cc, t, _shapeCC)));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        switch (_playerMechanics[slot])
        {
            case Mechanic.StayClose:
                if (FindPartner(slot) is var partner1 && partner1 != null && (partner1.Position - actor.Position).LengthSq() > 5 * 5)
                    hints.Add("Stay closer to partner!");
                break;
            case Mechanic.StayFar:
                if (FindPartner(slot) is var partner2 && partner2 != null && (partner2.Position - actor.Position).LengthSq() < 30 * 30)
                    hints.Add("Stay farther from partner!");
                break;
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        movementHints.Add(actor.Position, SafeSpot(slot, actor), ArenaColor.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        switch (_playerMechanics[pcSlot])
        {
            case Mechanic.StayClose:
                if (FindPartner(pcSlot) is var partner1 && partner1 != null)
                    Arena.AddLine(pc.Position, partner1.Position, (partner1.Position - pc.Position).LengthSq() > 5 * 5 ? ArenaColor.Danger : ArenaColor.Safe);
                break;
            case Mechanic.StayFar:
                if (FindPartner(pcSlot) is var partner2 && partner2 != null)
                    Arena.AddLine(pc.Position, partner2.Position, (partner2.Position - pc.Position).LengthSq() < 30 * 30 ? ArenaColor.Danger : ArenaColor.Safe);
                break;
        }

        Arena.Actor(BJ(), ArenaColor.Enemy, true);
        Arena.Actor(CC(), ArenaColor.Enemy, true);
        Arena.AddCircle(SafeSpot(pcSlot, pc), 1, ArenaColor.Safe);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.AggravatedAssault:
                AssignMechanic(actor, Mechanic.AvoidDamage);
                ForbiddenPlayers.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.HouseArrest:
                AssignMechanic(actor, Mechanic.StayClose);
                break;
            case SID.RestrainingOrder:
                AssignMechanic(actor, Mechanic.StayFar);
                break;
            case SID.TemporalDisplacement:
                Frozen = true;
                break;
        }
    }

    private Actor? FindPartner(int slot)
    {
        var partnerSlot = -1;
        for (int i = 0; i < _playerMechanics.Length; ++i)
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
        var bjLeft = BJ()?.Position.X < Module.Center.X;
        return Module.Center + _playerMechanics[slot] switch
        {
            Mechanic.AvoidDamage => new WDir(bjLeft ? -20 : +20, 0),
            Mechanic.StayClose => new WDir(6, actor.Class.IsSupport() ? -2 : +2),
            Mechanic.StayFar => new WDir(actor.Class.IsSupport() ? (bjLeft ? 15 : 20) : (bjLeft ? -20 : -15), 0),
            _ => new WDir(-6, actor.Class.IsSupport() ? -2 : +2)
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
