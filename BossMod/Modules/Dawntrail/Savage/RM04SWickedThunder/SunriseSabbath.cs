namespace BossMod.Dawntrail.Savage.RM04SWickedThunder;

class SunriseSabbath(BossModule module) : BossComponent(module)
{
    public BitMask Positron;
    public int[] BaitOrder = new int[PartyState.MaxPartySize];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (BaitOrder[slot] != 0)
            hints.Add($"Bait order: {BaitOrder[slot]}", false);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.Positron or SID.Negatron && Raid.TryFindSlot(actor.InstanceID, out var slot))
        {
            Positron[slot] = (SID)status.ID == SID.Positron;
            BaitOrder[slot] = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds < 30 ? 1 : 2;
        }
    }
}

class SunriseSabbathSoaringSoulpress(BossModule module) : Components.GenericTowers(module)
{
    private readonly SunriseSabbath? _sabbath = module.FindComponent<SunriseSabbath>();
    private readonly List<Actor> _birds = [];

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.WickedReplica && id == 0x11D5)
        {
            _birds.Add(actor);
            AddTower(actor, 1);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SoaringSoulpress1:
            case AID.SoaringSoulpress2:
                ++NumCasts;
                Towers.Clear();
                break;
            case AID.WickedSpecialCenterAOE:
            case AID.WickedSpecialSidesAOE:
                foreach (var b in _birds)
                    AddTower(b, 2);
                break;
        }
    }

    private void AddTower(Actor bird, int forbiddenOrder)
    {
        var intercard = ((int)Math.Round(bird.Rotation.Deg / 45) & 1) != 0;
        var pos = bird.Position + bird.Rotation.ToDirection() * (intercard ? 21.21320344f : 30);
        var forbidden = _sabbath != null ? Raid.WithSlot(true).WhereSlot(i => _sabbath.BaitOrder[i] == forbiddenOrder).Mask() : default;
        Towers.Add(new(pos, 3, 2, 2, forbiddenSoakers: forbidden));
    }
}

class SunriseSabbathElectronStream(BossModule module) : Components.GenericBaitAway(module)
{
    public readonly List<(Actor cannon, bool positron)> Cannons = [];
    private readonly SunriseSabbath? _sabbath = module.FindComponent<SunriseSabbath>();

    private static readonly AOEShapeRect _shape = new(40, 6);

    public override void Update()
    {
        CurrentBaits.Clear();
        foreach (var c in Cannons)
        {
            var t = Raid.WithoutSlot().Closest(c.cannon.Position);
            if (t != null)
                CurrentBaits.Add(new(c.cannon, t, _shape));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (var c in Cannons)
            Arena.ActorInsideBounds(c.cannon.Position, c.cannon.Rotation, ArenaColor.Object); // these cannons are right on bounds, it's quite ugly if half of them are drawn differently...
        if (!ForbiddenPlayers[pcSlot] && _sabbath != null)
            foreach (var c in Cannons)
                if (c.positron == _sabbath.Positron[pcSlot])
                    Arena.AddCircle(c.cannon.Position, 1, ArenaColor.Safe);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((OID)actor.OID == OID.WickedReplica && (SID)status.ID == SID.Marker)
        {
            Cannons.Add((actor, status.Extra == 0x2F5));
            var baitOrder = NumCasts == 0 ? 1 : 2;
            ForbiddenPlayers = _sabbath != null ? Raid.WithSlot(true).WhereSlot(i => _sabbath.BaitOrder[i] != baitOrder).Mask() : default;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SunriseSabbathPositronStream1 or AID.SunriseSabbathNegatronStream1 or AID.SunriseSabbathPositronStream2 or AID.SunriseSabbathNegatronStream2)
        {
            ++NumCasts;
            Cannons.Clear();
        }
    }
}
