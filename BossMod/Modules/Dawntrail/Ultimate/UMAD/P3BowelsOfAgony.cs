namespace BossMod.Dawntrail.Ultimate.UMAD;

class P3AeroIIIAssault(BossModule module) : Components.KnockbackFromCastTarget(module, AID.AeroIIIAssault, 15, true);

class P3ThunderIII(BossModule module) : Components.StandardAOEs(module, AID.ThunderIIICircle, 14.8f);

class P3Firewall(BossModule module) : Components.GenericInvincible(module)
{
    enum Color
    {
        None,
        Epic, // chaos
        Fated // exdeath
    }

    readonly Color[] _colors = new Color[8];

    Actor? _epicBoss;
    Actor? _fatedBoss;

    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        var otherBoss = _colors[slot] switch
        {
            Color.Fated => _epicBoss,
            Color.Epic => _fatedBoss,
            _ => null
        };

        if (otherBoss != null)
            yield return otherBoss;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.EpicHero:
                if (Raid.TryFindSlot(actor, out var slot))
                    _colors[slot] = Color.Epic;
                break;
            case SID.FatedHero:
                if (Raid.TryFindSlot(actor, out var slot2))
                    _colors[slot2] = Color.Fated;
                break;
            case SID.EpicVillain:
                _epicBoss = actor;
                break;
            case SID.FatedVillain:
                _fatedBoss = actor;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.EpicHero:
            case SID.FatedHero:
                if (Raid.TryFindSlot(actor, out var slot))
                    _colors[slot] = default;
                break;
            case SID.EpicVillain:
                _epicBoss = null;
                break;
            case SID.FatedVillain:
                _fatedBoss = null;
                break;
        }
    }
}

class P3BowelsOfAgony(BossModule module) : Components.RaidwideCast(module, AID.BowelsOfAgony);

class P3Crystals(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var f in Module.Enemies(OID.FireCrystal))
            Arena.AddCircleFilled(f.Position, 1, ArenaColor.Enemy);
        foreach (var f in Module.Enemies(OID.WaterCrystal))
            Arena.AddCircleFilled(f.Position, 1, 0xFFFF8080);
        foreach (var f in Module.Enemies(OID.WindCrystal))
            Arena.AddCircleFilled(f.Position, 1, 0xFF80FF00);
    }
}

enum Element
{
    None,
    Fire,
    Water
}

class P3EntropyFluid : Components.GenericBaitAway
{
    public static readonly AOEShape Circle = new AOEShapeCircle(5);
    public static readonly AOEShape Donut = new AOEShapeDonut(4, 10);

    public P3EntropyFluid(BossModule module) : base(module, centerAtTarget: true)
    {
        EnableHints = false;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.Entropy:
                CurrentBaits.Add(new(actor, actor, Circle, status.ExpireAt));
                break;
            case SID.DynamicFluid:
                CurrentBaits.Add(new(actor, actor, Donut, status.ExpireAt));
                break;
        }

        if (CurrentBaits.Count == 4)
        {
            CurrentBaits.SortBy(c => c.Activation);
            CurrentBaits.RemoveRange(2, 2);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.StrayFlames:
            case AID.StraySpray:
                NumCasts++;
                CurrentBaits.RemoveAll(b => b.Target.InstanceID == spell.MainTargetID);
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (EnableHints)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void Update()
    {
        CurrentBaits.RemoveAll(b => b.Target.IsDead);
    }
}

class P3InfernoTsunami : Components.GenericBaitAway
{
    Element _imminent;

    public P3InfernoTsunami(BossModule module) : base(module, centerAtTarget: true)
    {
        EnableHints = false;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.StrayFlames:
                _imminent = Element.Fire;
                break;
            case AID.StraySpray:
                _imminent = Element.Water;
                break;
            case AID.Inferno:
            case AID.Tsunami:
                NumCasts++;
                _imminent = Element.None;
                break;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        foreach (var b in CurrentBaits)
            Arena.AddLine(b.Source.Position, b.Target.Position, ArenaColor.Danger);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (EnableHints)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!EnableHints)
            return;

        var winds = Module.FindComponent<P3HeadwindTailwind>()?.Direction ?? new int[8];

        if (ActiveBaitsOn(actor).Any(b => Raid.WithSlot().Any(r => winds[r.Item1] == 0 && IsClippedBy(r.Item2, b))))
            hints.Add("Bait away from raid!");

        if (winds[slot] == 0 && ActiveBaitsNotOn(actor).Any(b => IsClippedBy(actor, b)))
            hints.Add("GTFO from crystal bait!");
    }

    public override void Update()
    {
        var nextElement = _imminent;
        DateTime nextActivation = default;

        CurrentBaits.Clear();

        var _fireWater = Module.FindComponent<P3EntropyFluid>();
        if (_fireWater == null)
            return;

        if (_imminent == Element.None && _fireWater.CurrentBaits.Count > 0)
        {
            nextElement = _fireWater.CurrentBaits[0].Shape is AOEShapeCircle ? Element.Fire : Element.Water;
            nextActivation = _fireWater.CurrentBaits[0].Activation.AddSeconds(1);
        }

        var source = nextElement switch
        {
            Element.Fire => Module.Enemies(OID.FireCrystal).FirstOrDefault(),
            Element.Water => Module.Enemies(OID.WaterCrystal).FirstOrDefault(),
            _ => null
        };

        if (source == null)
            return;

        var targets = Raid.WithoutSlot().SortedByRange(source.Position).Take(2);
        CurrentBaits.AddRange(targets.Select(t => new Bait(source, t, nextElement == Element.Fire ? P3EntropyFluid.Donut : P3EntropyFluid.Circle, nextActivation)));
    }
}

class P3ThunderIIIBuster(BossModule module) : Components.GenericBaitAway(module, AID.ThunderIIIBuster, centerAtTarget: true)
{
    Actor? _source;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ThunderIIIBusterCast)
            _source = caster;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (NumCasts == 2)
                _source = null;
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        if (_source is { } src && Raid.WithoutSlot().Closest(_source.Position) is { } tar)
            CurrentBaits.Add(new(src, tar, new AOEShapeCircle(5), Module.CastFinishAt(_source.CastInfo)));
    }
}

class P3Shockwave(BossModule module) : Components.GenericAOEs(module, AID.Shockwave)
{
    readonly List<AOEInstance> _predicted = [];

    public static readonly AOEShapeCone Shape = new(40, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(2);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.LongitudinalImplosion:
                _predicted.Add(new(Shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                _predicted.Add(new(Shape, spell.LocXZ, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell)));
                _predicted.Add(new(Shape, spell.LocXZ, spell.Rotation + 90.Degrees(), Module.CastFinishAt(spell)));
                _predicted.Add(new(Shape, spell.LocXZ, spell.Rotation - 90.Degrees(), Module.CastFinishAt(spell)));
                break;
            case AID.LatitudinalImplosion:
                _predicted.Add(new(Shape, spell.LocXZ, spell.Rotation + 90.Degrees(), Module.CastFinishAt(spell)));
                _predicted.Add(new(Shape, spell.LocXZ, spell.Rotation - 90.Degrees(), Module.CastFinishAt(spell)));
                _predicted.Add(new(Shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                _predicted.Add(new(Shape, spell.LocXZ, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

// Vacuum Wave "does nothing", the knockback from the boss is an ActorControl (because regular knockbacks only have one fixed distance)
// thus Vacuum Wave on cleansed players deals a 20y knockback, 10y on debuffed players, and $someBigNumber on debuffed players who fail the gaze mechanic
class P3HeadwindTailwind(BossModule module) : Components.Knockback(module, ignoreImmunes: true)
{
    record struct VacuumWave(WPos Origin, DateTime Activation);
    VacuumWave? _wave;

    // 1 = headwind, 2 = tailwind
    public readonly int[] Direction = new int[8];

    // hack for AI mode
    readonly bool[] _knockbackPending = new bool[8];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.VacuumWave)
            _wave = new(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.VacuumWave)
        {
            _wave = null;
            foreach (var tar in spell.Targets)
                if (Raid.TryFindSlot(tar.ID, out var slot))
                    _knockbackPending[slot] = true;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var dir = (SID)status.ID switch
        {
            SID.Headwind => 1,
            SID.Tailwind => 2,
            _ => 0
        };
        if (dir > 0 && Raid.TryFindSlot(actor, out var slot))
            Direction[slot] = dir;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.Headwind or SID.Tailwind && Raid.TryFindSlot(actor, out var slot))
        {
            _knockbackPending[slot] = false;
            Direction[slot] = 0;
        }
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (Module.FindComponent<P3InfernoTsunami>() is { } tsunami && tsunami.EnableHints)
            foreach (var src in tsunami.ActiveBaitsNotOn(actor))
                if (src.Shape.Check(actor.Position, src.Target))
                    yield return new(src.Target.Position, KnockDistance(slot, actor, src.Target.Position), src.Activation);

        if (_wave is { } vw)
            yield return new(vw.Origin, KnockDistance(slot, actor, vw.Origin), vw.Activation);
    }

    float KnockDistance(int pcSlot, Actor pc, WPos source)
    {
        var dir = Direction[pcSlot];
        if (dir == 0)
            return 20;

        var toSource = (source - pc.Position).Normalized();
        var safeFacing = dir == 1 ? -toSource : toSource;

        var rel = safeFacing.Normalized().Dot(pc.Rotation.ToDirection());

        if (rel > 0.7071067f)
            return 10;
        if (rel < -0.7071068f)
            return 50;
        return 20;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (Direction[pcSlot] == 0)
            return;

        if (Sources(pcSlot, pc).Any())
        {
            var safeFacing = (Direction[pcSlot] == 1 ? pc.Rotation + 180.Degrees() : pc.Rotation).Normalized();

            Arena.PathArcTo(pc.Position, 1, (safeFacing + 45.Degrees()).Rad, (safeFacing - 45.Degrees()).Rad);
            Arena.PathStroke(false, ArenaColor.Safe);
            Arena.PathArcTo(pc.Position, 1, (safeFacing + 225.Degrees()).Rad, (safeFacing + 135.Degrees()).Rad);
            Arena.PathStroke(false, ArenaColor.Enemy);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_knockbackPending[slot])
        {
            // TODO: uhhhh this basically just replicates what NormalMovement does during pending knockbacks...is there any sane way to handle this? please don't make me generalize pending knockbacks...
            hints.ForcedMovement = new();
            return;
        }

        if (Direction[slot] == 0)
            return;

        foreach (var src in Sources(slot, actor))
        {
            var dangerFacing = (src.Origin - actor.Position).ToAngle();
            if (Direction[slot] == 2)
                dangerFacing += 180.Degrees();
            hints.ForbiddenDirections.Add((dangerFacing.Normalized(), 135.Degrees(), src.Activation));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Direction[slot] == 0)
        {
            base.AddHints(slot, actor, hints);
            return;
        }

        foreach (var src in Sources(slot, actor).Take(1))
        {
            var dir = Direction[slot] == 1 ? "away from" : "toward";
            var hint = _wave == null ? $"Face {dir} bait!" : $"Face {dir} boss!";
            hints.Add(hint, KnockDistance(slot, actor, src.Origin) > 10);
        }
    }
}

class P3Cyclone(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumCasts { get; private set; }

    DateTime _pendingActivation;
    int _numPending;

    bool HasWind(ulong instanceId) => WorldState.Actors.Find(instanceId)?.Statuses.Any(s => (SID)s.ID is SID.Headwind or SID.Tailwind) == true;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.VacuumWave && Module.FindComponent<P3HeadwindTailwind>() is { } wind && wind.Direction.Count(c => c > 0) == 8)
            EnableHints = false;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Inferno or AID.Tsunami or AID.VacuumWave)
        {
            var numHit = spell.Targets.Count(t => t.ID != spell.MainTargetID && HasWind(t.ID));
            _pendingActivation = WorldState.FutureTime(2);
            _numPending += numHit;
        }

        if ((AID)spell.Action.ID is AID.Cyclone)
        {
            NumCasts++;
            _numPending--;
        }
    }

    public override void Update()
    {
        Stacks.Clear();

        if (Module.Enemies(OID.WindCrystal).FirstOrDefault() is not { } crystal)
            return;

        if (Module.FindComponent<P3HeadwindTailwind>() is not { } wind)
            return;

        var hitPlayers = new BitMask();
        var activation = _pendingActivation == default ? DateTime.MaxValue : _pendingActivation;

        var fireWater = Module.FindComponent<P3InfernoTsunami>();

        if (fireWater != null && fireWater.EnableHints)
        {
            foreach (var bait in fireWater.ActiveBaits)
            {
                var hitHere = Raid.WithSlot().Exclude(bait.Target).WhereSlot(s => wind.Direction[s] > 0).WhereActor(a => bait.Shape.Check(a.Position, bait.Target)).Mask();
                if (hitHere.Any() && bait.Activation < activation)
                    activation = bait.Activation;

                hitPlayers |= hitHere;
            }
        }

        if (Module.Enemies(OID.ExdeathP3).FirstOrDefault() is { CastInfo: { } ci } && ci.IsSpell(AID.VacuumWave))
        {
            hitPlayers |= Raid.WithSlot().WhereSlot(s => wind.Direction[s] > 0).Mask();
            activation = Module.CastFinishAt(ci);
        }

        var totalPredicted = hitPlayers.NumSetBits() + _numPending;

        foreach (var player in Raid.WithoutSlot().SortedByRange(crystal.Position).Take(totalPredicted))
            Stacks.Add(new(player, 6, 2, 2, activation));
    }
}
