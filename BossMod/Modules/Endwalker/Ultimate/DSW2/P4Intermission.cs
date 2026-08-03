namespace BossMod.Endwalker.Ultimate.DSW2;

sealed class P4IntermissionBrightwing(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeCone _shape = new(18f, 15f.Degrees()); // TODO: verify angle
    private readonly DSW2 bossmod = (DSW2)module;

    public override void Update()
    {
        CurrentBaits.Clear();
        if (NumCasts < 8)
        {
            var charibert = bossmod.SerCharibert();
            if (charibert != null)
            {
                foreach (var target in Raid.WithoutSlot(false, true, true).SortedByRange(charibert.Position).Take(2))
                    CurrentBaits.Add(new(charibert, target, _shape));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Brightwing)
        {
            ++NumCasts;
            foreach (var t in spell.Targets)
                ForbiddenPlayers.Set(Raid.FindSlot(t.ID));
        }
    }
}

sealed class P4IntermissionSkyblindBait(BossModule module) : BossComponent(module)
{
    private BitMask _baiters;

    private const float _radius = 3;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_baiters[slot] && Raid.WithSlot(false, true, true).ExcludedFromMask(_baiters).InRadius(actor.Position, _radius).Any())
            hints.Add("GTFO from raid!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _baiters[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Normal;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (_, player) in Raid.WithSlot(false, true, true).IncludedInMask(_baiters))
            Arena.ZoneCircleOutline(player.Position, _radius, Colors.Danger);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Skyblind)
            _baiters.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Skyblind)
            _baiters.Clear(Raid.FindSlot(actor.InstanceID));
    }
}

sealed class P4IntermissionSkyblind(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Skyblind, 3f);

sealed class P4Haurchefant(BossModule module) : BossComponent(module)
{
    public bool Appear;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D3 && actor.OID == (uint)OID.Haurchefant)
            Appear = true;
    }
}
