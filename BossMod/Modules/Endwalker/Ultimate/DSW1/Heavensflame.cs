namespace BossMod.Endwalker.Ultimate.DSW1;

class HeavensflameAOE(BossModule module) : Components.CastCounter(module, AID.HeavensflameAOE);

class HeavensflameKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.FaithUnmoving, 16)
{
    private readonly WPos[] _playerAdjustedPositions = new WPos[PartyState.MaxPartySize];
    private readonly int[] _playerIcons = new int[PartyState.MaxPartySize]; // 0 = unassigned, 1 = circle/red, 2 = triangle/green, 3 = cross/blue, 4 = square/purple
    private BitMask _brokenTethers;

    private const float _aoeRadius = 10;
    private const float _tetherBreakDistance = 32; // TODO: verify...

    public override void Update()
    {
        foreach (var (slot, player) in Raid.WithSlot())
            _playerAdjustedPositions[slot] = AwayFromSource(player.Position, Casters.FirstOrDefault(), Distance);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_playerIcons[slot] == 0)
            return;

        if (Casters.Count > 0 && IsImmune(slot, WorldState.CurrentTime))
            hints.Add("Cancel knockback immunity!");

        var actorAdjPos = _playerAdjustedPositions[slot];
        if (!Module.InBounds(actorAdjPos))
            hints.Add("About to be knocked into wall!");

        if (Raid.WithSlot().Exclude(actor).WhereSlot(s => _playerAdjustedPositions[s].InCircle(actorAdjPos, _aoeRadius)).Any())
            hints.Add("Spread!");

        int partner = FindTetheredPartner(slot);
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
            Arena.AddCircle(hint, 1, ArenaColor.Safe);
            //var dir = Vector3.Normalize(pos.Value - _knockbackSource.Position);
            //var adjPos = Arena.ClampToBounds(_knockbackSource.Position + 50 * dir);
            //Arena.AddLine(Module.Center, adjPos, ArenaColor.Safe);
        }

        int partner = FindTetheredPartner(pcSlot);
        if (partner >= 0)
            Arena.AddLine(pc.Position, Raid[partner]!.Position, ArenaColor.Safe);

        DrawKnockback(pc, _playerAdjustedPositions[pcSlot], Arena);

        foreach (var (slot, _) in Raid.WithSlot().Exclude(pc))
            Arena.AddCircle(_playerAdjustedPositions[slot], _aoeRadius, ArenaColor.Danger);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        _brokenTethers.Set(Raid.FindSlot(source.InstanceID));
        _brokenTethers.Set(Raid.FindSlot(tether.Target));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        int icon = (IconID)iconID switch
        {
            IconID.HeavensflameCircle => 1,
            IconID.HeavensflameTriangle => 2,
            IconID.HeavensflameCross => 3,
            IconID.HeavensflameSquare => 4,
            _ => 0
        };
        if (icon != 0)
        {
            if (Raid.TryFindSlot(actor, out var slot))
                _playerIcons[slot] = icon;
        }
    }

    private int FindTetheredPartner(int slot)
    {
        if (_brokenTethers[slot])
            return -1;
        if (_playerIcons[slot] == 0)
            return -1;
        for (int i = 0; i < _playerIcons.Length; ++i)
            if (i != slot && _playerIcons[i] == _playerIcons[slot])
                return i;
        return -1;
    }

    private IEnumerable<WPos> PositionHints(int slot)
    {
        var icon = _playerIcons[slot];
        if (icon == 0)
            yield break;

        switch (Service.Config.Get<DSW1Config>().Heavensflame)
        {
            case DSW1Config.HeavensflameHints.Waymarks:
                {
                    if (WorldState.Waymarks.GetFieldMark((int)Waymark.A + (icon - 1)) is var alt1 && alt1 != null)
                        yield return new(alt1.Value.XZ());
                    if (WorldState.Waymarks.GetFieldMark((int)Waymark.N1 + (icon - 1)) is var alt2 && alt2 != null)
                        yield return new(alt2.Value.XZ());
                }
                break;
            case DSW1Config.HeavensflameHints.LPDU:
                {
                    var angle = 135.Degrees() - icon * 45.Degrees();
                    var offset = _tetherBreakDistance * 0.5f * angle.ToDirection();
                    switch (icon)
                    {
                        case 1: // circle - both on DPS, show both E and W and let players adjust
                            yield return Module.Center + offset;
                            yield return Module.Center - offset;
                            break;
                        case 2: // triangle - healer SE, dps NW
                        case 3: // cross - healer S, tank N
                            if (Raid[slot]?.Role == Role.Healer)
                                yield return Module.Center + offset;
                            else
                                yield return Module.Center - offset;
                            break;
                        case 4: // square - tank NE, dps SW
                            if (Raid[slot]?.Role == Role.Tank)
                                yield return Module.Center - offset;
                            else
                                yield return Module.Center + offset;
                            break;
                    }
                }
                break;
        }
    }
}
