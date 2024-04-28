namespace BossMod.Endwalker.Ultimate.DSW2;

class P5DeathOfTheHeavensHeavyImpact(BossModule module) : HeavyImpact(module, 10.5f);

class P5DeathOfTheHeavensGaze(BossModule module) : DragonsGaze(module, OID.BossP5);

// TODO: make more meaningful somehow
class P5DeathOfTheHeavensDooms(BossModule module) : BossComponent(module)
{
    public BitMask Dooms;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Dooms[slot])
            hints.Add("Doom", false);
    }

    // note: we could also use status, but it appears slightly later
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Deathstorm)
            foreach (var t in spell.Targets)
                Dooms.Set(Raid.FindSlot(t.ID));
    }
}

class P5DeathOfTheHeavensLightningStorm : Components.UniformStackSpread
{
    public P5DeathOfTheHeavensLightningStorm(BossModule module) : base(module, 0, 5)
    {
        AddSpreads(Raid.WithoutSlot(true));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LightningStormAOE)
            Spreads.Clear();
    }
}

class P5DeathOfTheHeavensHeavensflame(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID.HeavensflameAOE))
{
    public bool KnockbackDone { get; private set; }
    private readonly WPos[] _playerAdjustedPositions = new WPos[PartyState.MaxPartySize];
    private readonly int[] _playerIcons = new int[PartyState.MaxPartySize]; // 0 = unassigned, 1 = circle/red, 2 = triangle/green, 3 = cross/blue, 4 = square/purple
    private BitMask _brokenTethers;
    private BitMask _dooms;
    private readonly List<WPos> _cleanses = [];
    private WDir _relSouth; // TODO: this is quite hacky, works for LPDU...

    private const float _knockbackDistance = 16;
    private const float _aoeRadius = 10;
    private const float _tetherBreakDistance = 32; // TODO: verify...

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        yield return new(Module.Center, _knockbackDistance);
    }

    public override void Update()
    {
        foreach (var (slot, player) in Raid.WithSlot())
            _playerAdjustedPositions[slot] = !KnockbackDone ? AwayFromSource(player.Position, Module.Center, _knockbackDistance) : player.Position;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_playerIcons[slot] == 0)
            return;

        if (!KnockbackDone && IsImmune(slot, WorldState.CurrentTime))
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
            Arena.AddCircle(hint, 1, ArenaColor.Safe);

        int partner = FindTetheredPartner(pcSlot);
        if (partner >= 0)
            Arena.AddLine(pc.Position, Raid[partner]!.Position, ArenaColor.Safe);

        DrawKnockback(pc, _playerAdjustedPositions[pcSlot], Arena);

        foreach (var (slot, _) in Raid.WithSlot().Exclude(pc))
            Arena.AddCircle(_playerAdjustedPositions[slot], _aoeRadius, ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WingsOfSalvationAOE)
        {
            _cleanses.Add(spell.LocXZ);
            _relSouth += spell.LocXZ - Module.Center;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.FaithUnmoving)
            KnockbackDone = true;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
            _dooms.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        _brokenTethers.Set(Raid.FindSlot(source.InstanceID));
        _brokenTethers.Set(Raid.FindSlot(tether.Target));
    }

    public override void OnEventIcon(Actor actor, uint iconID)
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
        for (int i = 0; i < _playerIcons.Length; ++i)
            if (i != slot && _playerIcons[i] == _playerIcons[slot])
                return i;
        return -1;
    }

    // note: assumes LPDU strat (circles on E/W cleanses, triangles on SE/NW, crosses on N/S, squares on SW/NE)
    // TODO: handle bad cleanse placements somehow? or even deaths?
    private IEnumerable<WPos> PositionHints(int slot)
    {
        var icon = _playerIcons[slot];
        if (icon == 0)
            yield break;

        var angle = Angle.FromDirection(_relSouth) + 135.Degrees() - icon * 45.Degrees();
        var offset = _tetherBreakDistance * 0.5f * angle.ToDirection();
        switch (icon)
        {
            case 1: // circle - show two cleanses closest to E and W
                yield return ClosestCleanse(Module.Center + offset);
                yield return ClosestCleanse(Module.Center - offset);
                break;
            case 2: // triangle/square - doom to closest cleanse to SE/SW, otherwise opposite
            case 4:
                var cleanseSpot = ClosestCleanse(Module.Center + offset);
                yield return _dooms[slot] ? cleanseSpot : Module.Center - (cleanseSpot - Module.Center);
                break;
            case 3: // cross - show two spots to N and S
                yield return Module.Center + offset;
                yield return Module.Center - offset;
                break;
        }
    }

    private WPos ClosestCleanse(WPos p) => _cleanses.MinBy(c => (c - p).LengthSq());
}

class P5DeathOfTheHeavensMeteorCircle(BossModule module) : Components.Adds(module, (uint)OID.MeteorCircle);
