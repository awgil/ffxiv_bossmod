namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

sealed class SunriseSabbath(BossModule module) : BossComponent(module)
{
    public BitMask Positron;
    public int[] BaitOrder = new int[PartyState.MaxPartySize];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (BaitOrder[slot] != 0)
            hints.Add($"Bait order: {BaitOrder[slot]}", false);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.Positron or (uint)SID.Negatron && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            Positron[slot] = status.ID == (uint)SID.Positron;
            BaitOrder[slot] = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds < 30d ? 1 : 2;
        }
    }
}

sealed class SunriseSabbathSoaringSoulpress(BossModule module) : Components.GenericTowers(module)
{
    private readonly SunriseSabbath? _sabbath = module.FindComponent<SunriseSabbath>();
    private readonly List<Actor> _birds = [];

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D5 && actor.OID == (uint)OID.WickedReplica)
        {
            _birds.Add(actor);
            AddTower(actor, 1);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.SoaringSoulpress1:
            case (uint)AID.SoaringSoulpress2:
                ++NumCasts;
                Towers.Clear();
                break;
            case (uint)AID.WickedSpecialCenterAOE:
            case (uint)AID.WickedSpecialSidesAOE:
                foreach (var b in _birds)
                    AddTower(b, 2);
                break;
        }
    }

    private void AddTower(Actor bird, int forbiddenOrder)
    {
        var intercard = ((int)Math.Round(bird.Rotation.Deg / 45f) & 1) != 0;
        var pos = bird.Position + bird.Rotation.ToDirection() * (intercard ? 21.21320344f : 30f);
        var forbidden = _sabbath != null ? Raid.WithSlot(true, true, true).WhereSlot(i => _sabbath.BaitOrder[i] == forbiddenOrder).Mask() : default;
        Towers.Add(new(pos, 3.6f, 2, 2, forbiddenSoakers: forbidden));
    }
}

sealed class SunriseSabbathElectronStream(BossModule module) : Components.GenericBaitAway(module)
{
    public readonly List<(Actor cannon, bool positron)> Cannons = [];
    private readonly SunriseSabbath? _sabbath = module.FindComponent<SunriseSabbath>();

    private static readonly AOEShapeRect _shape = new(40f, 6f);

    public override void Update()
    {
        CurrentBaits.Clear();
        foreach (var c in Cannons)
        {
            var t = Raid.WithoutSlot(false, true, true).Closest(c.cannon.Position);
            if (t != null)
                CurrentBaits.Add(new(c.cannon, t, _shape));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (var c in Cannons)
            Arena.ActorInsideBounds(c.cannon.Position, c.cannon.Rotation, Colors.Object); // these cannons are right on bounds, it's quite ugly if half of them are drawn differently...
        if (!ForbiddenPlayers[pcSlot] && _sabbath != null)
            foreach (var c in Cannons)
                if (c.positron == _sabbath.Positron[pcSlot])
                    Arena.ZoneCircleOutline(c.cannon.Position, 1, Colors.Safe);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor.OID == (uint)OID.WickedReplica && status.ID == (uint)SID.Marker)
        {
            Cannons.Add((actor, status.Extra == 0x2F5));
            var baitOrder = NumCasts == 0 ? 1 : 2;
            ForbiddenPlayers = _sabbath != null ? Raid.WithSlot(true, true, true).WhereSlot(i => _sabbath.BaitOrder[i] != baitOrder).Mask() : default;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SunriseSabbathPositronStream1 or (uint)AID.SunriseSabbathNegatronStream1 or (uint)AID.SunriseSabbathPositronStream2 or (uint)AID.SunriseSabbathNegatronStream2)
        {
            ++NumCasts;
            Cannons.Clear();
        }
    }
}
