namespace BossMod.Endwalker.Ultimate.TOP;

class P3SniperCannon(BossModule module) : Components.UniformStackSpread(module, 6, 6, alwaysShowSpreads: true)
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
            Arena.AddCircle(s, 1, ArenaColor.Safe);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.SniperCannonFodder:
                AddSpread(actor, status.ExpireAt);
                Assign(Raid.FindSlot(actor.InstanceID), PlayerRole.Spread);
                break;
            case SID.HighPoweredSniperCannonFodder:
                AddStack(actor, status.ExpireAt);
                Assign(Raid.FindSlot(actor.InstanceID), PlayerRole.Stack);
                break;
        }
    }

    // note: if player dies, stack/spread immediately hits random target, so we use status loss to end stack/spread
    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.SniperCannonFodder:
                Spreads.RemoveAll(s => s.Target == actor);
                break;
            case SID.HighPoweredSniperCannonFodder:
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

        int[] slotsInPriorityOrder = Utils.MakeArray(PartyState.MaxPartySize, -1);
        foreach (var a in _config.P3IntermissionAssignments.Resolve(Raid))
            slotsInPriorityOrder[a.group] = a.slot;

        int[] assignedRoles = [0, 0, 0];
        foreach (var s in slotsInPriorityOrder.Where(s => s >= 0))
            _playerStates[s].Order = ++assignedRoles[(int)_playerStates[s].Role];
    }

    private IEnumerable<WPos> EnumerateSafeSpots(int slot)
    {
        if (!_haveSafeSpots)
            yield break;

        var ps = _playerStates[slot];
        if (ps.Role == PlayerRole.Spread)
        {
            if (ps.Order is 0 or 1)
                yield return SafeSpotAt(-90.Degrees());
            if (ps.Order is 0 or 2)
                yield return SafeSpotAt(-45.Degrees());
            if (ps.Order is 0 or 3)
                yield return SafeSpotAt(45.Degrees());
            if (ps.Order is 0 or 4)
                yield return SafeSpotAt(90.Degrees());
        }
        else
        {
            if (ps.Order is 0 or 1)
                yield return SafeSpotAt(-135.Degrees());
            if (ps.Order is 0 or 2)
                yield return SafeSpotAt(135.Degrees());
        }
    }

    private WPos SafeSpotAt(Angle dirIfStacksNorth) => Module.Center + 19 * (_config.P3IntermissionStacksNorth ? dirIfStacksNorth : 180.Degrees() - dirIfStacksNorth).ToDirection();
}

class P3WaveRepeater(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(6), new AOEShapeDonut(6, 12), new AOEShapeDonut(12, 18), new AOEShapeDonut(18, 24)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WaveRepeater1)
            AddSequence(caster.Position, spell.NPCFinishAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.WaveRepeater1 => 0,
            AID.WaveRepeater2 => 1,
            AID.WaveRepeater3 => 2,
            AID.WaveRepeater4 => 3,
            _ => -1
        };
        if (!AdvanceSequence(order, caster.Position, WorldState.FutureTime(2.1f)))
            ReportError($"Unexpected ring {order}");
    }
}

class P3IntermissionVoidzone(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.P3IntermissionVoidzone).Where(z => z.EventState != 7));

class P3ColossalBlow(BossModule module) : Components.GenericAOEs(module)
{
    public List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCircle _shape = new(11);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Take(3);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID is OID.LeftArmUnit or OID.RightArmUnit && id is 0x1E43 or 0x1E44)
            AOEs.Add(new(_shape, actor.Position, default, WorldState.FutureTime(13.5f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ColossalBlow)
        {
            ++NumCasts;
            AOEs.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
        }
    }
}
