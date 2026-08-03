namespace BossMod.Endwalker.Ultimate.DSW1;

sealed class HeavensflameAOE(BossModule module) : Components.CastCounter(module, (uint)AID.HeavensflameAOE);

sealed class HeavensflameKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.FaithUnmoving, 16f)
{
    private readonly WPos[] _playerAdjustedPositions = new WPos[PartyState.MaxPartySize];
    private readonly int[] _playerIcons = new int[PartyState.MaxPartySize]; // 0 = unassigned, 1 = circle/red, 2 = triangle/green, 3 = cross/blue, 4 = square/purple
    private BitMask _brokenTethers;

    private const float _aoeRadius = 10f;
    private const float _tetherBreakDistance = 32f; // TODO: verify...

    public override void Update()
    {
        if (Casters.Count == 0)
        {
            return;
        }
        ref readonly var c = ref Casters.Ref(0);
        var origin = c.Origin;
        foreach (var (slot, player) in Raid.WithSlot(false, true, true))
            _playerAdjustedPositions[slot] = AwayFromSource(player.Position, origin, Distance);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_playerIcons[slot] == 0)
            return;

        if (Casters.Count > 0 && IsImmune(slot, WorldState.CurrentTime))
            hints.Add("Cancel knockback immunity!");

        var actorAdjPos = _playerAdjustedPositions[slot];
        if (!Arena.InBounds(actorAdjPos))
            hints.Add("About to be knocked into wall!");

        if (Raid.WithSlot(false, true, true).Exclude(actor).WhereSlot(s => _playerAdjustedPositions[s].InCircle(actorAdjPos, _aoeRadius)).Any())
            hints.Add("Spread!");

        var partner = FindTetheredPartner(slot);
        if (partner >= 0 && _playerAdjustedPositions[partner].InCircle(actorAdjPos, _tetherBreakDistance))
            hints.Add("Aim to break tether!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _playerIcons[pcSlot] == 0 ? PlayerPriority.Irrelevant :
            !_brokenTethers[pcSlot] && _playerIcons[pcSlot] == _playerIcons[playerSlot] ? PlayerPriority.Interesting
            : PlayerPriority.Normal;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_playerIcons[pcSlot] == 0)
            return;

        foreach (var hint in PositionHints(pcSlot))
        {
            Arena.ZoneCircleOutline(hint, 1f, Colors.Safe);
            //var dir = Vector3.Normalize(pos.Value - _knockbackSource.Position);
            //var adjPos = Arena.ClampToBounds(_knockbackSource.Position + 50 * dir);
            //Arena.AddLine(Arena.Center, adjPos, Colors.Safe);
        }

        var partner = FindTetheredPartner(pcSlot);
        if (partner >= 0)
            Arena.AddLine(pc.Position, Raid[partner]!.Position, Colors.Safe);

        DrawKnockback(pc, _playerAdjustedPositions[pcSlot], Arena);

        foreach (var (slot, _) in Raid.WithSlot(false, true, true).Exclude(pc))
            Arena.ZoneCircleOutline(_playerAdjustedPositions[slot], _aoeRadius);
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        _brokenTethers.Set(Raid.FindSlot(source.InstanceID));
        _brokenTethers.Set(Raid.FindSlot(tether.Target));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var icon = iconID switch
        {
            (uint)IconID.HeavensflameCircle => 1,
            (uint)IconID.HeavensflameTriangle => 2,
            (uint)IconID.HeavensflameCross => 3,
            (uint)IconID.HeavensflameSquare => 4,
            _ => 0
        };
        if (icon != 0)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _playerIcons[slot] = icon;
        }
    }

    private int FindTetheredPartner(int slot)
    {
        if (_brokenTethers[slot])
            return -1;
        if (_playerIcons[slot] == 0)
            return -1;
        for (var i = 0; i < _playerIcons.Length; ++i)
            if (i != slot && _playerIcons[i] == _playerIcons[slot])
                return i;
        return -1;
    }

    private List<WPos> PositionHints(int slot)
    {
        var icon = _playerIcons[slot];
        if (icon == 0)
            return [];
        var hints = new List<WPos>(2);
        switch (Service.Config.Get<DSW1Config>().Heavensflame)
        {
            case DSW1Config.HeavensflameHints.Waymarks:
                {
                    if (WorldState.Waymarks.GetFieldMark((int)Waymark.A + (icon - 1)) is var alt1 && alt1 != null)
                        hints.Add(new(alt1.Value.XZ()));
                    if (WorldState.Waymarks.GetFieldMark((int)Waymark.N1 + (icon - 1)) is var alt2 && alt2 != null)
                        hints.Add(new(alt2.Value.XZ()));
                }
                break;
            case DSW1Config.HeavensflameHints.LPDU:
                {
                    var angle = 135f.Degrees() - icon * 45f.Degrees();
                    var offset = _tetherBreakDistance * 0.5f * angle.ToDirection();
                    var center = Arena.Center;
                    switch (icon)
                    {
                        case 1: // circle - both on DPS, show both E and W and let players adjust
                            hints.Add(center + offset);
                            hints.Add(center - offset);
                            break;
                        case 2: // triangle - healer SE, dps NW
                        case 3: // cross - healer S, tank N
                            if (Raid[slot]?.Role == Role.Healer)
                                hints.Add(center + offset);
                            else
                                hints.Add(center - offset);
                            break;
                        case 4: // square - tank NE, dps SW
                            if (Raid[slot]?.Role == Role.Tank)
                                hints.Add(center - offset);
                            else
                                hints.Add(center + offset);
                            break;
                    }
                }
                break;
        }
        return hints;
    }
}
