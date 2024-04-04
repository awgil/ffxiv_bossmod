namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2StygimolochWarrior;

class EntrapmentAttract : Components.Knockback
{
    private DateTime _activation;

    public EntrapmentAttract() : base(ActionID.MakeSpell(AID.EntrapmentAttract), true) { }

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        yield return new(new(module.Bounds.Center.X, module.Bounds.Center.Z + module.Bounds.HalfSize), 60, _activation, Kind: Kind.TowardsOrigin);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Entrapment)
            _activation = spell.NPCFinishAt.AddSeconds(0.8f);
    }
}

// the 'board' is 7x7; cell index is zzzxxx (64 bit), with (0,0) corresponding to NW cell
class Entrapment : Components.CastCounter
{
    public enum TrapType { Normal, Toad, Ice, Mini };

    public struct Pattern
    {
        public BitMask Normal;
        public BitMask Toad;
        public BitMask Ice;
        public BitMask Mini;
    }

    protected TrapType TrapToTake; // note that 'normal' means none here
    private readonly Pattern[] _allowedPatterns;
    private Pattern _curPattern;
    private BitMask _uncovered;
    private BitMask _exploded;
    private BitMask _possiblePatterns;
    private Pattern _potentiallyUnsafe;
    private bool _possiblePatternsDirty;

    public Entrapment(Pattern[] allowedPatterns) : base(ActionID.MakeSpell(AID.MassiveExplosion))
    {
        _allowedPatterns = allowedPatterns;
        _possiblePatterns = new((1u << allowedPatterns.Length) - 1);
        UpdatePotentiallyUnsafe();
    }

    public override void Update(BossModule module)
    {
        if (!_possiblePatternsDirty)
            return;
        _possiblePatternsDirty = false;

        // TODO: ideally it should not be done here, but when reacting to perception cast...
        // note that range check is a bit fuzzy - perception range is 15, i've seen traps at ~18.3 uncovered - but sometimes traps at presumably smaller range not uncovered
        // so we consider a very conservative range
        var player = module.Raid.Player();
        if (player != null)
            for (int z = 0; z < 7; ++z)
                for (int x = 0; x < 7; ++x)
                    if (player.Position.InCircle(module.Bounds.Center + CellOffset(x, z), 10))
                        _uncovered.Set(IndexFromCell(x, z));

        // remove all patterns that have difference with current state in uncovered areas
        if (_possiblePatterns.Any())
        {
            foreach (var ip in _possiblePatterns.SetBits())
            {
                ref var p = ref _allowedPatterns[ip];
                var diff = (p.Normal ^ _curPattern.Normal) | (p.Toad ^ _curPattern.Toad) | (p.Ice ^ _curPattern.Ice) | (p.Mini ^ _curPattern.Mini);
                if ((diff & _uncovered).Any())
                    _possiblePatterns.Clear(ip);
            }
            if (_possiblePatterns.None())
                module.ReportError(this, "No matching patterns left...");
        }

        UpdatePotentiallyUnsafe();
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        hints.Add($"Matching patterns: {(_possiblePatterns.Any() ? string.Join(", ", _possiblePatterns.SetBits()): "none")}");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        DrawTraps(module, _curPattern.Normal, false, true, arena);
        DrawTraps(module, _curPattern.Toad, TrapToTake == TrapType.Toad, true, arena);
        DrawTraps(module, _curPattern.Ice, TrapToTake == TrapType.Ice, true, arena);
        DrawTraps(module, _curPattern.Mini, TrapToTake == TrapType.Mini, true, arena);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        DrawTraps(module, _potentiallyUnsafe.Normal, false, false, arena);
        DrawTraps(module, _potentiallyUnsafe.Toad, TrapToTake == TrapType.Toad, false, arena);
        DrawTraps(module, _potentiallyUnsafe.Ice, TrapToTake == TrapType.Ice, false, arena);
        DrawTraps(module, _potentiallyUnsafe.Mini, TrapToTake == TrapType.Mini, false, arena);
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        switch ((OID)actor.OID)
        {
            case OID.TrapNormal:
                AddTrap(ref _curPattern.Normal, module, actor.Position, false);
                break;
            case OID.TrapToad:
                AddTrap(ref _curPattern.Toad, module, actor.Position, false);
                break;
            case OID.TrapIce:
                AddTrap(ref _curPattern.Ice, module, actor.Position, false);
                break;
            case OID.TrapMini:
                AddTrap(ref _curPattern.Mini, module, actor.Position, false);
                break;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.MassiveExplosion:
                AddTrap(ref _curPattern.Normal, module, spell.TargetXZ, true);
                break;
            case AID.Toad:
                AddTrap(ref _curPattern.Toad, module, spell.TargetXZ, true);
                break;
            case AID.TrappedUnderIce:
                AddTrap(ref _curPattern.Ice, module, spell.TargetXZ, true);
                break;
            case AID.Mini:
                AddTrap(ref _curPattern.Mini, module, spell.TargetXZ, true);
                break;
        }
    }

    private void AddTrap(ref BitMask mask, BossModule module, WPos position, bool exploded)
    {
        var index = IndexFromOffset(position - module.Bounds.Center);
        //module.ReportError(this, $"Trap @ {position} (dist={(position - module.Raid.Player()!.Position).Length()}) = {index}");
        mask.Set(index);
        _uncovered.Set(index);
        if (exploded)
        {
            _exploded.Set(index);
            ++NumCasts;
        }
        _possiblePatternsDirty = true;
    }

    private int IndexFromCell(int x, int z) => x is >= 0 and <= 6 && z is >= 0 and <= 6 ? ((z << 3) | x) : -1;
    private int IndexFromOffset(WDir offset)
    {
        var x = (int)Math.Round(offset.X / 5) + 3;
        var z = (int)Math.Round(offset.Z / 5) + 3;
        return IndexFromCell(x, z);
    }

    private WDir CellOffset(int x, int z) => 5 * new WDir(x - 3, z - 3);
    private WDir CellOffset(int index) => CellOffset(index & 7, index >> 3);

    private void DrawTraps(BossModule module, BitMask mask, bool safe, bool background, MiniArena arena)
    {
        mask &= ~_exploded; // don't draw already exploded traps
        foreach (var index in mask.SetBits())
        {
            var pos = module.Bounds.Center + CellOffset(index);
            if (background)
                arena.ZoneCircle(pos, 2.5f, safe ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
            else
                arena.AddCircle(pos, 2.5f, safe ? ArenaColor.Safe : ArenaColor.Danger);
        }
    }

    private void UpdatePotentiallyUnsafe()
    {
        _potentiallyUnsafe = default;
        var numMatches = _possiblePatterns.NumSetBits();
        if (numMatches == 0)
        {
            // no matches => all hidden are potentially unsafe
            _potentiallyUnsafe.Normal = new BitMask(0x7f7f7f7f7f7f7f) & ~_uncovered;
        }
        else if (numMatches == 1)
        {
            // single match => just show everything from the pattern that wasn't yet uncovered
            _potentiallyUnsafe = _allowedPatterns[_possiblePatterns.LowestSetBit()];
            _potentiallyUnsafe.Normal &= ~_uncovered;
            _potentiallyUnsafe.Toad &= ~_uncovered;
            _potentiallyUnsafe.Ice &= ~_uncovered;
            _potentiallyUnsafe.Mini &= ~_uncovered;
        }
        else
        {
            // multiple matches => show everything that wasn't yet uncovered as 'normal'
            foreach (var ip in _possiblePatterns.SetBits())
            {
                ref var p = ref _allowedPatterns[ip];
                _potentiallyUnsafe.Normal |= p.Normal | p.Toad | p.Ice | p.Mini;
            }
            _potentiallyUnsafe.Normal &= ~_uncovered;
        }
    }

    protected static BitMask BuildMask(params int[] bits)
    {
        var mask = new BitMask();
        foreach (var bit in bits)
            mask.Set(bit);
        return mask;
    }
}

class EntrapmentNormal : Entrapment
{
    private readonly static Pattern[] _allowedPatterns = [
        new() { Normal = BuildMask( 8,  9, 10, 11, 12, 13, 18, 20, 34, 35, 36, 37, 38, 40, 42, 45) },
        new() { Normal = BuildMask( 8,  9, 11, 16, 19, 20, 21, 22, 26, 30, 32, 36, 40, 41, 42, 45) },
        new() { Normal = BuildMask( 9, 11, 12, 13, 14, 16, 17, 27, 28, 32, 33, 38, 41, 42, 43, 44) },
        new() { Normal = BuildMask( 9, 11, 12, 13, 14, 16, 17, 27, 28, 32, 33, 38, 41, 42, 44, 46) }, // TODO: i'm not sure whether this pattern is real
        new() { Normal = BuildMask(10, 11, 13, 14, 19, 20, 21, 24, 25, 30, 33, 34, 35, 36, 44, 45) },
    ];
    public EntrapmentNormal() : base(_allowedPatterns) { }
}

class EntrapmentInescapable : Entrapment
{
    private readonly static Pattern[] _allowedPatterns = [
        new() { Normal = BuildMask(3, 4,  5,  8, 20, 25, 38, 43, 46, 49, 52), Toad = BuildMask(10, 50, 54), Ice = BuildMask(40), Mini = BuildMask(29) },
        new() { Normal = BuildMask(2, 5,  8, 11, 14, 16, 25, 29, 46, 49, 51), Toad = BuildMask( 0, 4, 44),  Ice = BuildMask(50), Mini = BuildMask(34) },
        new() { Normal = BuildMask(5, 8, 11, 16, 18, 22, 24, 29, 43, 49, 53), Toad = BuildMask( 6, 33, 38), Ice = BuildMask( 4), Mini = BuildMask(48) },
        new() { Normal = BuildMask(5, 8, 11, 25, 30, 32, 38, 43, 49, 50, 54), Toad = BuildMask(16, 21, 48), Ice = BuildMask(36), Mini = BuildMask( 1) },
    ];

    public EntrapmentInescapable() : base(_allowedPatterns) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var trap = (AID)spell.Action.ID switch
        {
            AID.SurgingFlames => TrapType.Ice,
            AID.SurgingFlood => TrapType.Toad,
            AID.WitheringCurse => TrapType.Mini,
            _ => TrapType.Normal
        };
        if (trap != TrapType.Normal)
            TrapToTake = trap;
    }
}

class LethalBlow : Components.SelfTargetedAOEs
{
    public LethalBlow() : base(ActionID.MakeSpell(AID.LethalBlow), new AOEShapeRect(44, 24)) { }
}

class LeapingSpark : Components.CastCounter
{
    public LeapingSpark() : base(ActionID.MakeSpell(AID.LeapingSparkAOE)) { }
}

class Devour : Components.SelfTargetedAOEs
{
    public Devour() : base(ActionID.MakeSpell(AID.Devour), new AOEShapeCone(6, 60.Degrees())) { }
}
