namespace BossMod.Endwalker.Ultimate.DSW1;

class PureOfHeartBrightwing : Components.GenericBaitAway
{
    private static readonly AOEShapeCone _shape = new(18, 15.Degrees()); // TODO: verify angle

    public override void Update(BossModule module)
    {
        CurrentBaits.Clear();
        if (NumCasts < 8)
            foreach (var source in module.Enemies(OID.SerCharibert))
                foreach (var target in module.Raid.WithoutSlot().SortedByRange(source.Position).Take(2))
                    CurrentBaits.Add(new(source, target, _shape));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Brightwing)
        {
            ++NumCasts;
            foreach (var t in spell.Targets)
                ForbiddenPlayers.Set(module.Raid.FindSlot(t.ID));
        }
    }
}

class PureOfHeartSkyblindBait : BossComponent
{
    private BitMask _baiters;

    private static readonly float _radius = 3;

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_baiters[slot] && module.Raid.WithSlot().ExcludedFromMask(_baiters).InRadius(actor.Position, _radius).Any())
            hints.Add("GTFO from raid!");
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _baiters[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Normal;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var (_, player) in module.Raid.WithSlot().IncludedInMask(_baiters))
            arena.AddCircle(player.Position, _radius, ArenaColor.Danger);
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Skyblind)
            _baiters.Set(module.Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Skyblind)
            _baiters.Clear(module.Raid.FindSlot(actor.InstanceID));
    }
}

class PureOfHeartSkyblind : Components.LocationTargetedAOEs
{
    public PureOfHeartSkyblind() : base(ActionID.MakeSpell(AID.Skyblind), 3) { }
}
