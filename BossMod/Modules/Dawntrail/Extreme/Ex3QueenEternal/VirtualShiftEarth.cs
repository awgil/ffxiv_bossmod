namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class VirtualShiftEarth(BossModule module) : BossComponent(module)
{
    public BitMask Flying;

    public static readonly WPos Midpoint = new(100f, 94f);
    public static readonly WDir CenterOffset = new(8f, default);
    public static readonly WDir HalfExtent = new(4f, 8f);

    public static bool OnPlatform(WPos p)
    {
        var off = p - Midpoint;
        var offX = Math.Abs(off.X);
        off = new(offX, off.Z);
        off -= CenterOffset;
        off = off.Abs();
        return off.X <= HalfExtent.X && off.Z <= HalfExtent.Z;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var halfExtentZ = HalfExtent.Z;
        var halfExtentX = HalfExtent.X;
        var color = Colors.Border;
        Arena.AddRect(Midpoint + CenterOffset, new(default, 1f), halfExtentZ, halfExtentZ, halfExtentX, color, 2f);
        Arena.AddRect(Midpoint - CenterOffset, new(default, 1f), halfExtentZ, halfExtentZ, halfExtentX, color, 2f);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.GravitationalAnomaly)
            Flying[Raid.FindSlot(actor.InstanceID)] = true;
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.GravitationalAnomaly)
            Flying[Raid.FindSlot(actor.InstanceID)] = false;
    }
}

abstract class LawsOfEarthBurst(BossModule module) : Components.GenericTowers(module, (uint)AID.LawsOfEarthBurst)
{
    private readonly VirtualShiftEarth? _virtualShift = module.FindComponent<VirtualShiftEarth>();

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_virtualShift != null && _virtualShift.Flying[slot])
        {
            var count = Towers.Count;
            for (var i = 0; i < count; ++i)
            {
                if (!Towers[i].ForbiddenSoakers[slot])
                {
                    hints.Add("Go to ground!");
                    break;
                }
            }
        }
        base.AddHints(slot, actor, hints);
    }

    // hardcode tower positions; technically they are shown by envcontrols
    protected void AddTowers(DateTime activation, WPos center, params ReadOnlySpan<WDir> offsets)
    {
        if (offsets.Length == 0)
        {
            Towers.Add(new(center, 2f, activation: activation));
        }
        else
        {
            AddTowers(activation, center + offsets[0], offsets[1..]);
            AddTowers(activation, center - offsets[0], offsets[1..]);
        }
    }
}

sealed class LawsOfEarthBurst1 : LawsOfEarthBurst
{
    public LawsOfEarthBurst1(BossModule module) : base(module)
    {
        AddTowers(WorldState.FutureTime(5d), VirtualShiftEarth.Midpoint, VirtualShiftEarth.CenterOffset, new(default, 6f), new(2f, default));
    }
}

sealed class LawsOfEarthBurst2 : LawsOfEarthBurst
{
    public LawsOfEarthBurst2(BossModule module) : base(module)
    {
        AddTowers(WorldState.FutureTime(8.8d), VirtualShiftEarth.Midpoint, VirtualShiftEarth.CenterOffset, new(default, 5f));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GravityPillar)
        {
            var towers = CollectionsMarshal.AsSpan(Towers);
            var len = towers.Length;
            for (var i = 0; i < len; ++i)
            {
                towers[i].ForbiddenSoakers[Raid.FindSlot(spell.TargetID)] = true;
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.GravityRay)
        {
            var towers = CollectionsMarshal.AsSpan(Towers);
            var len = towers.Length;
            for (var i = 0; i < len; ++i)
            {
                towers[i].ForbiddenSoakers[Raid.FindSlot(source.InstanceID)] = true;
            }
        }
    }
}

sealed class GravityPillar(BossModule module) : Components.BaitAwayCast(module, (uint)AID.GravityPillar, 10f);

// note: the tethers appear before target is created; the target is at the same location as the boss
sealed class GravityRay(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(50f, 30f.Degrees()), (uint)TetherID.GravityRay, (uint)AID.GravityRay) // TODO: verify angle
{
    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == TID)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, source, Shape));
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == TID)
        {
            var count = CurrentBaits.Count - 1;
            for (var i = count; i >= 0; --i)
            {
                if (CurrentBaits[i].Target == source)
                {
                    CurrentBaits.RemoveAt(i);
                }
            }
        }
    }
}

// TODO: figure out how failure conditions work:
// - if someone is dead, can someone else place 2 meteors?
// - what if meteors are split 3-5 between platforms?
// - how meteor overlap works?
sealed class MeteorImpact(BossModule module) : Components.CastCounter(module, default)
{
    private BitMask _activeMeteors;
    private BitMask _meteorsAbovePlatforms;
    private int _numPlacedMeteors;

    public bool Active => _activeMeteors != default;

    public override void Update()
    {
        var party = Raid.WithSlot(false, true, true);
        var len = party.Length;
        _meteorsAbovePlatforms = default;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var p = ref party[i];
            if (_activeMeteors[p.Item1] && VirtualShiftEarth.OnPlatform(p.Item2.Position))
            {
                _meteorsAbovePlatforms.Set(p.Item1);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_activeMeteors[slot])
            return;

        var shouldBeAbovePlatform = _numPlacedMeteors < 8;
        if (_meteorsAbovePlatforms[slot] != shouldBeAbovePlatform)
            hints.Add(shouldBeAbovePlatform ? "Fly above platform!" : "GTFO from platform!");

        var showHint = false;
        var origin = actor.Position;
        if (_meteorsAbovePlatforms[slot])
        {
            var partyWithoutSlot = Raid.WithoutSlot(false, true, true);
            var lenPWithoutSlot = partyWithoutSlot.Length;
            for (var i = 0; i < lenPWithoutSlot; ++i)
            {
                var p = partyWithoutSlot[i];
                if (p == actor)
                    continue;
                if (p.Position.InCircle(origin, 4f))
                {
                    showHint = true;
                    break;
                }
            }
        }
        else
        {
            var partyWithSlot = Raid.WithSlot(true, true, true);
            var len = partyWithSlot.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref partyWithSlot[i];
                if (_meteorsAbovePlatforms[p.Item1] && p.Item2.Position.InCircle(origin, 4f))
                {
                    showHint = true;
                    break;
                }
            }
        }
        if (showHint)
            hints.Add("Spread!");

        // TODO: don't overlap with previous meteors?..
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _activeMeteors[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var partyWithSlot = Raid.WithSlot(true, true, true);
        var len = partyWithSlot.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var p = ref partyWithSlot[i];
            if (_meteorsAbovePlatforms[p.Item1])
            {
                Arena.ZoneCircleOutline(p.Item2.Position, 4f);
            }
        }
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
        if (spell.Action.ID is (uint)AID.MeteorImpactPlatform or (uint)AID.MeteorImpactFall)
        {
            _activeMeteors = default; // assume all meteors fall at the same time
            ++NumCasts;
            if (spell.Action.ID == (uint)AID.MeteorImpactPlatform)
                ++_numPlacedMeteors;
        }
    }
}

// TODO: how targeting / safe zones really work? what if <8 meteors are placed?
sealed class WeightyBlow(BossModule module) : Components.CastCounter(module, (uint)AID.WeightyBlowAOE)
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
        var count = _boulders.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_boulders[i].Position.InRect(origin, actor.Position - origin, HalfWidth))
            {
                return;
            }
        }
        hints.Add("Hide behind boulder!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_boulders, Colors.Object, true);
        if (_activeBaits)
        {
            var origin = BaitSource(pc);
            var offset = pc.Position - origin;
            var len = offset.Length();
            Arena.AddRect(origin, offset / len, len, 0, HalfWidth, Colors.Safe);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WeightyBlow)
            _activeBaits = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.MeteorImpactPlatform:
                _boulders.Add(caster);
                break;
            case (uint)AID.WeightyBlowDestroy:
                _boulders.Remove(caster);
                break;
            case (uint)AID.WeightyBlowAOE:
                ++NumCasts;
                break;
        }
    }

    private WPos BaitSource(Actor player) => new(player.PosRot.X < Ex3QueenEternal.ArenaCenter.X ? 92f : 108f, 79.5f);
}
