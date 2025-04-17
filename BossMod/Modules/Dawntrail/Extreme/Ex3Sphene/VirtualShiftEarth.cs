namespace BossMod.Dawntrail.Extreme.Ex3Sphene;

class VirtualShiftEarth(BossModule module) : BossComponent(module)
{
    public BitMask Flying;

    public static readonly WPos Midpoint = new(100, 94);
    public static readonly WDir CenterOffset = new(8, 0);
    public static readonly WDir HalfExtent = new(4, 8);

    public static bool OnPlatform(WPos p)
    {
        var off = p - Midpoint;
        off.X = Math.Abs(off.X);
        off -= CenterOffset;
        off = off.Abs();
        return off.X <= HalfExtent.X && off.Z <= HalfExtent.Z;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.AddRect(Midpoint + CenterOffset, new(0, 1), HalfExtent.Z, HalfExtent.Z, HalfExtent.X, ArenaColor.Border, 2);
        Arena.AddRect(Midpoint - CenterOffset, new(0, 1), HalfExtent.Z, HalfExtent.Z, HalfExtent.X, ArenaColor.Border, 2);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.GravitationalAnomaly)
            Flying.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.GravitationalAnomaly)
            Flying.Clear(Raid.FindSlot(actor.InstanceID));
    }
}

abstract class LawsOfEarthBurst(BossModule module) : Components.GenericTowers(module, AID.LawsOfEarthBurst)
{
    private readonly VirtualShiftEarth? _virtualShift = module.FindComponent<VirtualShiftEarth>();

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_virtualShift != null && _virtualShift.Flying[slot] && Towers.Any(t => !t.ForbiddenSoakers[slot]))
            hints.Add("Go to ground!");
        base.AddHints(slot, actor, hints);
    }

    // hardcode tower positions; technically they are shown by envcontrols
    protected void AddTowers(DateTime activation, WPos center, params ReadOnlySpan<WDir> offsets)
    {
        if (offsets.Length == 0)
        {
            Towers.Add(new(center, 2, activation: activation));
        }
        else
        {
            AddTowers(activation, center + offsets[0], offsets[1..]);
            AddTowers(activation, center - offsets[0], offsets[1..]);
        }
    }
}

class LawsOfEarthBurst1 : LawsOfEarthBurst
{
    public LawsOfEarthBurst1(BossModule module) : base(module)
    {
        AddTowers(WorldState.FutureTime(5), new(100, 94), new(8, 0), new(0, 6), new(2, 0));
    }
}

class LawsOfEarthBurst2 : LawsOfEarthBurst
{
    public LawsOfEarthBurst2(BossModule module) : base(module)
    {
        AddTowers(WorldState.FutureTime(8.8f), new(100, 94), new(8, 0), new(0, 5));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GravityPillar)
            foreach (ref var t in Towers.AsSpan())
                t.ForbiddenSoakers.Set(Raid.FindSlot(spell.TargetID));
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.GravityRay)
            foreach (ref var t in Towers.AsSpan())
                t.ForbiddenSoakers.Set(Raid.FindSlot(source.InstanceID));
    }
}

class GravityPillar(BossModule module) : Components.BaitAwayCast(module, AID.GravityPillar, new AOEShapeCircle(10), true);

// note: the tethers appear before target is created; the target is at the same location as the boss
class GravityRay(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(50, 30.Degrees()), (uint)TetherID.GravityRay, AID.GravityRay) // TODO: verify angle
{
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == TID)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, source, Shape));
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == TID)
        {
            CurrentBaits.RemoveAll(b => b.Target == source);
        }
    }
}

// TODO: figure out how failure conditions work:
// - if someone is dead, can someone else place 2 meteors?
// - what if meteors are split 3-5 between platforms?
// - how meteor overlap works?
class MeteorImpact(BossModule module) : Components.CastCounter(module, default)
{
    private BitMask _activeMeteors;
    private BitMask _meteorsAbovePlatforms;
    private int _numPlacedMeteors;

    public bool Active => _activeMeteors.Any();

    public override void Update()
    {
        _meteorsAbovePlatforms = Raid.WithSlot().IncludedInMask(_activeMeteors).WhereActor(p => VirtualShiftEarth.OnPlatform(p.Position)).Mask();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_activeMeteors[slot])
            return;

        var shouldBeAbovePlatform = _numPlacedMeteors < 8;
        if (_meteorsAbovePlatforms[slot] != shouldBeAbovePlatform)
            hints.Add(shouldBeAbovePlatform ? "Fly above platform!" : "GTFO from platform!");

        var shouldNotBeStacked = _meteorsAbovePlatforms[slot] ? Raid.WithoutSlot() : Raid.WithSlot(true).IncludedInMask(_meteorsAbovePlatforms).Actors();
        if (shouldNotBeStacked.InRadiusExcluding(actor, 4).Any())
            hints.Add("Spread!");

        // TODO: don't overlap with previous meteors?..
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _activeMeteors[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var p in Raid.WithSlot(true).IncludedInMask(_meteorsAbovePlatforms).Actors())
            Arena.AddCircle(p.Position, 4, ArenaColor.Danger);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.MeteorImpact)
        {
            _activeMeteors.Set(Raid.FindSlot(actor.InstanceID));
            NumCasts = 0;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.MeteorImpactPlatform or AID.MeteorImpactFall)
        {
            _activeMeteors.Reset(); // assume all meteors fall at the same time
            ++NumCasts;
            if ((AID)spell.Action.ID == AID.MeteorImpactPlatform)
                ++_numPlacedMeteors;
        }
    }
}

// TODO: how targeting / safe zones really work? what if <8 meteors are placed?
class WeightyBlow(BossModule module) : Components.CastCounter(module, AID.WeightyBlowAOE)
{
    private readonly VirtualShiftEarth? _virtualShift = module.FindComponent<VirtualShiftEarth>();
    private readonly List<Actor> _boulders = [];
    private bool _activeBaits;

    private const float HalfWidth = 1.5f;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_activeBaits)
            return;

        if (_virtualShift != null && _virtualShift.Flying[slot])
            hints.Add("Go to ground!");

        var origin = BaitSource(actor);
        if (!_boulders.Any(b => b.Position.InRect(origin, actor.Position - origin, HalfWidth)))
            hints.Add("Hide behind boulder!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_boulders, ArenaColor.Object, true);
        if (_activeBaits)
        {
            var origin = BaitSource(pc);
            var offset = pc.Position - origin;
            var len = offset.Length();
            Arena.AddRect(origin, offset / len, len, 0, HalfWidth, ArenaColor.Safe);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WeightyBlow)
            _activeBaits = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.MeteorImpactPlatform:
                _boulders.Add(caster);
                break;
            case AID.WeightyBlowDestroy:
                _boulders.Remove(caster);
                break;
            case AID.WeightyBlowAOE:
                ++NumCasts;
                break;
        }
    }

    private WPos BaitSource(Actor player) => new(player.Position.X < Module.Center.X ? 92 : 108, 79.5f);
}
