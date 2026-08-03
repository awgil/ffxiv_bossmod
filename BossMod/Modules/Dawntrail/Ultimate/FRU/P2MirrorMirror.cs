namespace BossMod.Dawntrail.Ultimate.FRU;

sealed class P2MirrorMirrorReflectedScytheKickBlue : Components.GenericAOEs
{
    private WDir _blueMirror;
    private BitMask _rangedSpots;
    private AOEInstance[] _aoe = [];

    private static readonly AOEShapeDonut _shape = new(4f, 20f);

    public P2MirrorMirrorReflectedScytheKickBlue(BossModule module) : base(module, (uint)AID.ReflectedScytheKickBlue)
    {
        foreach (var (slot, group) in Service.Config.Get<FRUConfig>().P2MirrorMirror1SpreadSpots.Resolve(Raid))
            _rangedSpots[slot] = group >= 4;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoe.Length == 0 && Module.Enemies((uint)OID.BossP2) is var bossP2 && bossP2.Count != 0 && bossP2[0] is var boss && boss.TargetID == actor.InstanceID)
        {
            // main tank should drag the boss away
            // note: before mirror appears, we want to stay near center (to minimize movement no matter where mirror appears), so this works fine if blue mirror is zero
            // TODO: verify distance calculation - we want boss to be at least 4m away from center
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center - 16f * _blueMirror, 1), DateTime.MaxValue);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_blueMirror != default)
        {
            Arena.Actor(Arena.Center + 20f * _blueMirror, Angle.FromDirection(-_blueMirror), Colors.Object);
            if (_aoe.Length == 0)
            {
                // draw preposition hint
                var distance = _rangedSpots[pcSlot] ? 19f : -11f;
                Arena.ZoneCircleOutline(Arena.Center + distance * _blueMirror, 1f, Colors.Safe);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ScytheKick && _blueMirror != default)
        {
            _aoe = [new(_shape, Arena.Center + 20f * _blueMirror, default, Module.CastFinishAt(spell))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x01 and <= 0x08 && state == 0x00020001u)
        {
            _blueMirror = (225f - index * 45f).Degrees().ToDirection();
        }
    }
}

sealed class P2MirrorMirrorReflectedScytheKickRed(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ReflectedScytheKickRed, new AOEShapeDonut(4f, 20f))
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = Casters.Count;
        for (var i = 0; i < count; ++i)
        {
            var caster = Casters[i];
            Arena.ActorInsideBounds(caster.Origin, caster.Rotation, Colors.Object);
        }
    }
}

sealed class P2MirrorMirrorHouseOfLight(BossModule module) : Components.GenericBaitAway(module, (uint)AID.HouseOfLight)
{
    public readonly record struct Source(Actor Actor, DateTime Activation);

    public bool RedRangedLeftOfMelee;
    public readonly List<Source> FirstSources = []; // [boss, blue mirror]
    public readonly List<Source> SecondSources = []; // [melee red mirror, ranged red mirror]
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private Angle? _blueMirror;

    private List<Source> CurrentSources => NumCasts == 0 ? FirstSources : SecondSources;

    private static readonly AOEShapeCone _shape = new(60f, 15f.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        foreach (var s in CurrentSources)
            foreach (var p in Raid.WithoutSlot(false, true, true).SortedByRange(s.Actor.Position).Take(4))
                CurrentBaits.Add(new(s.Actor, p, _shape, s.Activation));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var group = (NumCasts == 0 ? _config.P2MirrorMirror1SpreadSpots : _config.P2MirrorMirror2SpreadSpots)[assignment];
        var sources = CurrentSources;
        if (sources.Count < 2 || _blueMirror == null || group < 0)
            return; // inactive or no valid assignments

        var origin = sources[group < 4 ? 0 : 1];
        Angle dir;
        if (NumCasts == 0)
        {
            dir = _blueMirror.Value + (group & 3) switch
            {
                0 => -135f.Degrees(),
                1 => 135f.Degrees(),
                2 => -95f.Degrees(),
                3 => 95f.Degrees(),
                _ => default
            };
        }
        else
        {
            dir = Angle.FromDirection(origin.Actor.Position - Arena.Center) + group switch
            {
                0 => (RedRangedLeftOfMelee ? -95f : 95f).Degrees(),
                1 => (RedRangedLeftOfMelee ? 95f : -95f).Degrees(),
                2 => 180f.Degrees(),
                3 => (RedRangedLeftOfMelee ? -135f : 135f).Degrees(),
                4 => -95f.Degrees(),
                5 => 95f.Degrees(),
                6 => (RedRangedLeftOfMelee ? 180f : -135f).Degrees(),
                7 => (RedRangedLeftOfMelee ? 135f : 180f).Degrees(),
                _ => default
            };
        }
        hints.AddForbiddenZone(new SDInvertedCone(origin.Actor.Position, 4f, dir, 15f.Degrees()), origin.Activation);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x01 and <= 0x08 && state == 0x00020001u)
        {
            _blueMirror = (225f - index * 45f).Degrees();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ScytheKick:
                var activation = Module.CastFinishAt(spell, 0.7d);
                FirstSources.Add(new(caster, activation));
                var mirror = _blueMirror != null ? Module.Enemies((uint)OID.FrozenMirror).Closest(Arena.Center + 20f * _blueMirror.Value.ToDirection()) : null;
                if (mirror != null)
                    FirstSources.Add(new(mirror, activation));
                break;
            case (uint)AID.ReflectedScytheKickRed:
                SecondSources.Add(new(caster, Module.CastFinishAt(spell, 0.6d)));
                if (SecondSources.Count == 2 && _blueMirror != null)
                {
                    // order two red mirrors so that first one is closer to boss and second one closer to blue mirror; if both are same distance, select CW ones (arbitrary)
                    var d1 = (Angle.FromDirection(SecondSources[0].Actor.Position - Arena.Center) - _blueMirror.Value).Normalized();
                    var d2 = (Angle.FromDirection(SecondSources[1].Actor.Position - Arena.Center) - _blueMirror.Value).Normalized();
                    var d1abs = d1.Abs();
                    var d2abs = d2.Abs();
                    var swap = d2abs.AlmostEqual(d1abs, 0.1f)
                        ? d2.Rad > 0 // swap if currently second one is CCW from blue mirror
                        : d2abs.Rad > d1abs.Rad; // swap if currently second one is further from the blue mirror
                    if (swap)
                        (SecondSources[1], SecondSources[0]) = (SecondSources[0], SecondSources[1]);

                    RedRangedLeftOfMelee = (SecondSources[0].Actor.Position - Arena.Center).OrthoL().Dot(SecondSources[1].Actor.Position - Arena.Center) > 0;
                }
                break;
        }
    }
}

sealed class P2MirrorMirrorBanish : P2Banish
{
    private readonly WPos _anchorMelee;
    private readonly WPos _anchorRanged;
    private BitMask _aroundRanged;
    private BitMask _closerToCenter;
    private BitMask _leftSide;

    public P2MirrorMirrorBanish(BossModule module) : base(module)
    {
        var proteans = module.FindComponent<P2MirrorMirrorHouseOfLight>();
        if (proteans != null && proteans.FirstSources.Count == 2 && proteans.SecondSources.Count == 2)
        {
            _anchorMelee = proteans.FirstSources[0].Actor.Position;
            _anchorRanged = Arena.Center + 0.5f * (proteans.SecondSources[1].Actor.Position - Arena.Center);
            foreach (var (slot, group) in Service.Config.Get<FRUConfig>().P2MirrorMirror2SpreadSpots.Resolve(Raid))
            {
                _aroundRanged[slot] = group >= 4;
                _closerToCenter[slot] = (group & 2) != 0;
                _leftSide[slot] = group switch
                {
                    0 => !proteans.RedRangedLeftOfMelee,
                    1 => proteans.RedRangedLeftOfMelee,
                    2 => proteans.RedRangedLeftOfMelee,
                    3 => !proteans.RedRangedLeftOfMelee,
                    4 => false,
                    5 => true,
                    6 => false,
                    7 => true,
                    _ => false
                };
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var prepos = PrepositionLocation(slot, assignment);
        if (prepos != null)
            hints.AddForbiddenZone(new SDInvertedCircle(prepos.Value, 1f), DateTime.MaxValue);
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }

    private WPos? PrepositionLocation(int slot, PartyRolesConfig.Assignment assignment)
        => Stacks.Count > 0 && Stacks.Ref(0).Activation > WorldState.FutureTime(2.5d) ? CalculatePrepositionLocation(_aroundRanged[slot], _leftSide[slot], 90f.Degrees())
        : Spreads.Count > 0 && Spreads.Ref(0).Activation > WorldState.FutureTime(2.5d) ? CalculatePrepositionLocation(_aroundRanged[slot], _leftSide[slot], (_closerToCenter[slot] ? 135f : 45f).Degrees())
        : null;

    private WPos CalculatePrepositionLocation(bool aroundRanged, bool leftSide, Angle angle)
    {
        var anchor = aroundRanged ? _anchorRanged : _anchorMelee;
        var offset = Angle.FromDirection(anchor - Arena.Center) + (leftSide ? angle : -angle);
        return anchor + 6f * offset.ToDirection();
    }
}
