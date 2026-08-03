namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2StygimolochWarrior;

sealed class EntrapmentAttract(BossModule module) : Components.GenericKnockback(module, (uint)AID.EntrapmentAttract)
{
    private DateTime _activation;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        return new Knockback[1] { new(new(Arena.Center.X, Arena.Center.Z + Arena.Bounds.Radius), 60f, _activation, kind: Kind.TowardsOrigin, ignoreImmunes: true) };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Entrapment)
            _activation = Module.CastFinishAt(spell, 0.8f);
    }
}

// the 'board' is 7x7; cell index is zzzxxx (64 bit), with (0,0) corresponding to NW cell
class Entrapment : Components.CastCounter
{
    public enum TrapType { Normal, Toad, Ice, Mini }

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

    public Entrapment(BossModule module, Pattern[] allowedPatterns) : base(module, (uint)AID.MassiveExplosion)
    {
        _allowedPatterns = allowedPatterns;
        _possiblePatterns = new((1u << allowedPatterns.Length) - 1);
        UpdatePotentiallyUnsafe();
    }

    public override void Update()
    {
        if (!_possiblePatternsDirty)
            return;
        _possiblePatternsDirty = false;

        // TODO: ideally it should not be done here, but when reacting to perception cast...
        // note that range check is a bit fuzzy - perception range is 15, i've seen traps at ~18.3 uncovered - but sometimes traps at presumably smaller range not uncovered
        // so we consider a very conservative range
        var player = Raid.Player();
        if (player != null)
            for (var z = 0; z < 7; ++z)
                for (var x = 0; x < 7; ++x)
                    if (player.Position.InCircle(Arena.Center + CellOffset(x, z), 10f))
                        _uncovered[IndexFromCell(x, z)] = true;

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
                ReportError("No matching patterns left...");
        }

        UpdatePotentiallyUnsafe();
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"Matching patterns: {(_possiblePatterns.Any() ? string.Join(", ", _possiblePatterns.SetBits()) : "none")}");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        DrawTraps(_curPattern.Normal, false, true);
        DrawTraps(_curPattern.Toad, TrapToTake == TrapType.Toad, true);
        DrawTraps(_curPattern.Ice, TrapToTake == TrapType.Ice, true);
        DrawTraps(_curPattern.Mini, TrapToTake == TrapType.Mini, true);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        DrawTraps(_potentiallyUnsafe.Normal, false, false);
        DrawTraps(_potentiallyUnsafe.Toad, TrapToTake == TrapType.Toad, false);
        DrawTraps(_potentiallyUnsafe.Ice, TrapToTake == TrapType.Ice, false);
        DrawTraps(_potentiallyUnsafe.Mini, TrapToTake == TrapType.Mini, false);
    }

    public override void OnActorCreated(Actor actor)
    {
        switch (actor.OID)
        {
            case (uint)OID.TrapNormal:
                AddTrap(ref _curPattern.Normal, actor.Position, false);
                break;
            case (uint)OID.TrapToad:
                AddTrap(ref _curPattern.Toad, actor.Position, false);
                break;
            case (uint)OID.TrapIce:
                AddTrap(ref _curPattern.Ice, actor.Position, false);
                break;
            case (uint)OID.TrapMini:
                AddTrap(ref _curPattern.Mini, actor.Position, false);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.MassiveExplosion:
                AddTrap(ref _curPattern.Normal, spell.TargetXZ, true);
                break;
            case (uint)AID.Toad:
                AddTrap(ref _curPattern.Toad, spell.TargetXZ, true);
                break;
            case (uint)AID.TrappedUnderIce:
                AddTrap(ref _curPattern.Ice, spell.TargetXZ, true);
                break;
            case (uint)AID.Mini:
                AddTrap(ref _curPattern.Mini, spell.TargetXZ, true);
                break;
        }
    }

    private void AddTrap(ref BitMask mask, WPos position, bool exploded)
    {
        var index = IndexFromOffset(position - Arena.Center);
        //ReportError($"Trap @ {position} (dist={(position - Raid.Player()!.Position).Length()}) = {index}");
        mask[index] = true;
        _uncovered[index] = true;
        if (exploded)
        {
            _exploded[index] = true;
            ++NumCasts;
        }
        _possiblePatternsDirty = true;
    }

    private static int IndexFromCell(int x, int z) => x is >= 0 and <= 6 && z is >= 0 and <= 6 ? ((z << 3) | x) : -1;
    private static int IndexFromOffset(WDir offset)
    {
        var x = (int)Math.Round(offset.X / 5) + 3;
        var z = (int)Math.Round(offset.Z / 5) + 3;
        return IndexFromCell(x, z);
    }

    private static WDir CellOffset(int x, int z) => 5 * new WDir(x - 3, z - 3);
    private static WDir CellOffset(int index) => CellOffset(index & 7, index >> 3);

    private void DrawTraps(BitMask mask, bool safe, bool background)
    {
        mask &= ~_exploded; // don't draw already exploded traps
        foreach (var index in mask.SetBits())
        {
            var pos = Arena.Center + CellOffset(index);
            if (background)
                Arena.ZoneCircle(pos, 2.5f, safe ? Colors.SafeFromAOE : Colors.AOE);
            else
                Arena.ZoneCircleOutline(pos, 2.5f, safe ? Colors.Safe : 0);
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
            mask[bit] = true;
        return mask;
    }
}

sealed class EntrapmentNormal(BossModule module) : Entrapment(module, _allowedPatterns)
{
    private static readonly Pattern[] _allowedPatterns = [
        new() { Normal = BuildMask( 8,  9, 10, 11, 12, 13, 18, 20, 34, 35, 36, 37, 38, 40, 42, 45) },
        new() { Normal = BuildMask( 8,  9, 11, 16, 19, 20, 21, 22, 26, 30, 32, 36, 40, 41, 42, 45) },
        new() { Normal = BuildMask( 9, 11, 12, 13, 14, 16, 17, 27, 28, 32, 33, 38, 41, 42, 43, 44) },
        new() { Normal = BuildMask( 9, 11, 12, 13, 14, 16, 17, 27, 28, 32, 33, 38, 41, 42, 44, 46) }, // TODO: i'm not sure whether this pattern is real
        new() { Normal = BuildMask(10, 11, 13, 14, 19, 20, 21, 24, 25, 30, 33, 34, 35, 36, 44, 45) },
    ];
}

sealed class EntrapmentInescapable(BossModule module) : Entrapment(module, _allowedPatterns)
{
    private static readonly Pattern[] _allowedPatterns = [
        new() { Normal = BuildMask(3, 4,  5,  8, 20, 25, 38, 43, 46, 49, 52), Toad = BuildMask(10, 50, 54), Ice = BuildMask(40), Mini = BuildMask(29) },
        new() { Normal = BuildMask(2, 5,  8, 11, 14, 16, 25, 29, 46, 49, 51), Toad = BuildMask( 0, 4, 44),  Ice = BuildMask(50), Mini = BuildMask(34) },
        new() { Normal = BuildMask(5, 8, 11, 16, 18, 22, 24, 29, 43, 49, 53), Toad = BuildMask( 6, 33, 38), Ice = BuildMask( 4), Mini = BuildMask(48) },
        new() { Normal = BuildMask(5, 8, 11, 25, 30, 32, 38, 43, 49, 50, 54), Toad = BuildMask(16, 21, 48), Ice = BuildMask(36), Mini = BuildMask( 1) },
    ];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var trap = spell.Action.ID switch
        {
            (uint)AID.SurgingFlames => TrapType.Ice,
            (uint)AID.SurgingFlood => TrapType.Toad,
            (uint)AID.WitheringCurse => TrapType.Mini,
            _ => TrapType.Normal
        };
        if (trap != TrapType.Normal)
            TrapToTake = trap;
    }
}

sealed class LethalBlow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LethalBlow, new AOEShapeRect(44f, 24f));
sealed class LeapingSpark(BossModule module) : Components.CastCounter(module, (uint)AID.LeapingSparkAOE);
sealed class Devour(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Devour, new AOEShapeCone(6f, 60f.Degrees()));
