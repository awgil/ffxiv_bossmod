namespace BossMod.Endwalker.Ultimate.DSW1;

class HeavensflameAOE : Components.CastCounter
{
    public HeavensflameAOE() : base(ActionID.MakeSpell(AID.HeavensflameAOE)) { }
}

class HeavensflameKnockback : Components.KnockbackFromCastTarget
{
    private WPos[] _playerAdjustedPositions = new WPos[PartyState.MaxPartySize];
    private int[] _playerIcons = new int[PartyState.MaxPartySize]; // 0 = unassigned, 1 = circle/red, 2 = triangle/green, 3 = cross/blue, 4 = square/purple
    private BitMask _brokenTethers;

    private static readonly float _aoeRadius = 10;
    private static readonly float _tetherBreakDistance = 32; // TODO: verify...

    public HeavensflameKnockback() : base(ActionID.MakeSpell(AID.FaithUnmoving), 16) { }

    public override void Update(BossModule module)
    {
        foreach (var (slot, player) in module.Raid.WithSlot())
            _playerAdjustedPositions[slot] = AwayFromSource(player.Position, Casters.FirstOrDefault(), Distance);
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_playerIcons[slot] == 0)
            return;

        if (Casters.Count > 0 && IsImmune(slot, module.WorldState.CurrentTime))
            hints.Add("Cancel knockback immunity!");

        var actorAdjPos = _playerAdjustedPositions[slot];
        if (!module.Bounds.Contains(actorAdjPos))
            hints.Add("About to be knocked into wall!");

        if (module.Raid.WithSlot().Exclude(actor).WhereSlot(s => _playerAdjustedPositions[s].InCircle(actorAdjPos, _aoeRadius)).Any())
            hints.Add("Spread!");

        int partner = FindTetheredPartner(slot);
        if (partner >= 0 && _playerAdjustedPositions[partner].InCircle(actorAdjPos, _tetherBreakDistance))
            hints.Add("Aim to break tether!");
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _playerIcons[pcSlot] == 0 ? PlayerPriority.Irrelevant :
            !_brokenTethers[pcSlot] && _playerIcons[pcSlot] == _playerIcons[playerSlot] ? PlayerPriority.Interesting
            : PlayerPriority.Normal;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_playerIcons[pcSlot] == 0)
            return;

        foreach (var hint in PositionHints(module, pcSlot))
        {
            module.Arena.AddCircle(hint, 1, ArenaColor.Safe);
            //var dir = Vector3.Normalize(pos.Value - _knockbackSource.Position);
            //var adjPos = module.Arena.ClampToBounds(_knockbackSource.Position + 50 * dir);
            //module.Arena.AddLine(module.Bounds.Center, adjPos, ArenaColor.Safe);
        }

        int partner = FindTetheredPartner(pcSlot);
        if (partner >= 0)
            arena.AddLine(pc.Position, module.Raid[partner]!.Position, ArenaColor.Safe);

        DrawKnockback(pc, _playerAdjustedPositions[pcSlot], arena);

        foreach (var (slot, _) in module.Raid.WithSlot().Exclude(pc))
            arena.AddCircle(_playerAdjustedPositions[slot], _aoeRadius, ArenaColor.Danger);
    }

    public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        _brokenTethers.Set(module.Raid.FindSlot(source.InstanceID));
        _brokenTethers.Set(module.Raid.FindSlot(tether.Target));
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
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
            var slot = module.Raid.FindSlot(actor.InstanceID);
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
        for (int i = 0; i < _playerIcons.Length; ++i)
            if (i != slot && _playerIcons[i] == _playerIcons[slot])
                return i;
        return -1;
    }

    private IEnumerable<WPos> PositionHints(BossModule module, int slot)
    {
        var icon = _playerIcons[slot];
        if (icon == 0)
            yield break;

        switch (Service.Config.Get<DSW1Config>().Heavensflame)
        {
            case DSW1Config.HeavensflameHints.Waymarks:
                {
                    if (module.WorldState.Waymarks[(Waymark)((int)Waymark.A + (icon - 1))] is var alt1 && alt1 != null)
                        yield return new(alt1.Value.XZ());
                    if (module.WorldState.Waymarks[(Waymark)((int)Waymark.N1 + (icon - 1))] is var alt2 && alt2 != null)
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
                            yield return module.Bounds.Center + offset;
                            yield return module.Bounds.Center - offset;
                            break;
                        case 2: // triangle - healer SE, dps NW
                        case 3: // cross - healer S, tank N
                            if (module.Raid[slot]?.Role == Role.Healer)
                                yield return module.Bounds.Center + offset;
                            else
                                yield return module.Bounds.Center - offset;
                            break;
                        case 4: // square - tank NE, dps SW
                            if (module.Raid[slot]?.Role == Role.Tank)
                                yield return module.Bounds.Center - offset;
                            else
                                yield return module.Bounds.Center + offset;
                            break;
                    }
                }
                break;
        }
    }
}
