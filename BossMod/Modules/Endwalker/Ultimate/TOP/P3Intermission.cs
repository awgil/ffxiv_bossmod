namespace BossMod.Endwalker.Ultimate.TOP;

sealed class P3SniperCannon(BossModule module) : Components.UniformStackSpread(module, 6f, 6f)
{
    enum PlayerRole { None, Stack, Spread }

    struct PlayerState
    {
        public PlayerRole Role;
        public int Order;
    }

    private readonly TOPConfig _config = Service.Config.Get<TOPConfig>();
    private readonly PlayerState[] _playerStates = new PlayerState[PartyState.MaxPartySize];
    private bool _haveSafeSpots;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var s in EnumerateSafeSpots(pcSlot))
            Arena.ZoneCircleOutline(s, 1f, Colors.Safe);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.SniperCannonFodder:
                AddSpread(actor, status.ExpireAt);
                Assign(Raid.FindSlot(actor.InstanceID), PlayerRole.Spread);
                break;
            case (uint)SID.HighPoweredSniperCannonFodder:
                AddStack(actor, status.ExpireAt);
                Assign(Raid.FindSlot(actor.InstanceID), PlayerRole.Stack);
                break;
        }
    }

    // note: if player dies, stack/spread immediately hits random target, so we use status loss to end stack/spread
    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.SniperCannonFodder:
                Spreads.RemoveAll(s => s.Target == actor);
                break;
            case (uint)SID.HighPoweredSniperCannonFodder:
                Stacks.RemoveAll(s => s.Target == actor);
                break;
        }
    }

    private void Assign(int slot, PlayerRole role)
    {
        if (slot < 0)
            return;
        _playerStates[slot].Role = role;

        if (Spreads.Count < 4 || Stacks.Count < 2)
            return; // too early to build assignments

        _haveSafeSpots = true;

        var slotsInPriorityOrder = Utils.MakeArray(PartyState.MaxPartySize, -1);
        foreach (var a in _config.P3IntermissionAssignments.Resolve(Raid))
            slotsInPriorityOrder[a.group] = a.slot;

        int[] assignedRoles = [0, 0, 0];
        foreach (var s in slotsInPriorityOrder.Where(s => s >= 0))
            _playerStates[s].Order = ++assignedRoles[(int)_playerStates[s].Role];
    }

    private List<WPos> EnumerateSafeSpots(int slot)
    {
        if (!_haveSafeSpots)
            return [];
        var safespots = new List<WPos>();
        var ps = _playerStates[slot];
        if (ps.Role == PlayerRole.Spread)
        {
            if (ps.Order is 0 or 1)
                safespots.Add(SafeSpotAt(-90f.Degrees()));
            if (ps.Order is 0 or 2)
                safespots.Add(SafeSpotAt(-45f.Degrees()));
            if (ps.Order is 0 or 3)
                safespots.Add(SafeSpotAt(45f.Degrees()));
            if (ps.Order is 0 or 4)
                safespots.Add(SafeSpotAt(90f.Degrees()));
        }
        else
        {
            if (ps.Order is 0 or 1)
                safespots.Add(SafeSpotAt(-135f.Degrees()));
            if (ps.Order is 0 or 2)
                safespots.Add(SafeSpotAt(135f.Degrees()));
        }
        return safespots;
    }

    private WPos SafeSpotAt(Angle dirIfStacksNorth) => Arena.Center + 19f * (_config.P3IntermissionStacksNorth ? dirIfStacksNorth : 180f.Degrees() - dirIfStacksNorth).ToDirection();
}

sealed class P3WaveRepeater(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(6f), new AOEShapeDonut(6f, 12f), new AOEShapeDonut(12f, 18f), new AOEShapeDonut(18f, 24f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WaveRepeater1)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = spell.Action.ID switch
        {
            (uint)AID.WaveRepeater1 => 0,
            (uint)AID.WaveRepeater2 => 1,
            (uint)AID.WaveRepeater3 => 2,
            (uint)AID.WaveRepeater4 => 3,
            _ => -1
        };
        if (!AdvanceSequence(order, caster.Position, WorldState.FutureTime(2.1d)))
            ReportError($"Unexpected ring {order}");
    }
}

class P3IntermissionVoidzone(BossModule module) : Components.Voidzone(module, 6f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.P3IntermissionVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

sealed class P3ColossalBlow(BossModule module) : Components.GenericAOEs(module)
{
    public List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCircle _shape = new(11f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var max = count > 3 ? 3 : count;
        return CollectionsMarshal.AsSpan(AOEs)[..max];
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID is (uint)OID.LeftArmUnit or (uint)OID.RightArmUnit && id is 0x1E43 or 0x1E44)
            AOEs.Add(new(_shape, actor.Position.Quantized(), default, WorldState.FutureTime(13.5d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ColossalBlow)
        {
            ++NumCasts;
            AOEs.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1f));
        }
    }
}
