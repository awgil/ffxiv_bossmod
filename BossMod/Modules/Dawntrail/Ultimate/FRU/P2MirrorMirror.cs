namespace BossMod.Dawntrail.Ultimate.FRU;

class P2MirrorMirrorReflectedScytheKickBlue : Components.GenericAOEs
{
    private WDir _blueMirror;
    private BitMask _rangedSpots;
    private AOEInstance? _aoe;

    private static readonly AOEShapeDonut _shape = new(4, 20);

    public P2MirrorMirrorReflectedScytheKickBlue(BossModule module) : base(module, AID.ReflectedScytheKickBlue)
    {
        foreach (var (slot, group) in Service.Config.Get<FRUConfig>().P2MirrorMirror1SpreadSpots.Resolve(Raid))
            _rangedSpots[slot] = group >= 4;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoe == null && Module.Enemies(OID.BossP2).FirstOrDefault() is var boss && boss != null && boss.TargetID == actor.InstanceID)
        {
            // main tank should drag the boss away
            // note: before mirror appears, we want to stay near center (to minimize movement no matter where mirror appears), so this works fine if blue mirror is zero
            // TODO: verify distance calculation - we want boss to be at least 4m away from center
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(Module.Center - 16 * _blueMirror, 1), DateTime.MaxValue);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_blueMirror != default)
        {
            Arena.Actor(Module.Center + 20 * _blueMirror, Angle.FromDirection(-_blueMirror), ArenaColor.Object);
            if (_aoe == null)
            {
                // draw preposition hint
                var distance = _rangedSpots[pcSlot] ? 19 : -11;
                Arena.AddCircle(Module.Center + distance * _blueMirror, 1, ArenaColor.Safe);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ScytheKick && _blueMirror != default)
            _aoe = new(_shape, Module.Center + 20 * _blueMirror, default, Module.CastFinishAt(spell));
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 1 and <= 8 && state == 0x00020001)
            _blueMirror = (225 - index * 45).Degrees().ToDirection();
    }
}

class P2MirrorMirrorReflectedScytheKickRed(BossModule module) : Components.StandardAOEs(module, AID.ReflectedScytheKickRed, new AOEShapeDonut(4, 20))
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Casters, ArenaColor.Object, true);
    }
}

class P2MirrorMirrorHouseOfLight(BossModule module) : Components.GenericBaitAway(module, AID.HouseOfLight)
{
    public readonly record struct Source(Actor Actor, DateTime Activation);

    public bool RedRangedLeftOfMelee;
    public readonly List<Source> FirstSources = []; // [boss, blue mirror]
    public readonly List<Source> SecondSources = []; // [melee red mirror, ranged red mirror]
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private Angle? _blueMirror;

    private List<Source> CurrentSources => NumCasts == 0 ? FirstSources : SecondSources;

    private static readonly AOEShapeCone _shape = new(60, 15.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        foreach (var s in CurrentSources)
            foreach (var p in Raid.WithoutSlot().SortedByRange(s.Actor.Position).Take(4))
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
                0 => -135.Degrees(),
                1 => 135.Degrees(),
                2 => -95.Degrees(),
                3 => 95.Degrees(),
                _ => default
            };
        }
        else
        {
            dir = Angle.FromDirection(origin.Actor.Position - Module.Center) + group switch
            {
                0 => (RedRangedLeftOfMelee ? -95 : 95).Degrees(),
                1 => (RedRangedLeftOfMelee ? 95 : -95).Degrees(),
                2 => 180.Degrees(),
                3 => (RedRangedLeftOfMelee ? -135 : 135).Degrees(),
                4 => -95.Degrees(),
                5 => 95.Degrees(),
                6 => (RedRangedLeftOfMelee ? 180 : -135).Degrees(),
                7 => (RedRangedLeftOfMelee ? 135 : 180).Degrees(),
                _ => default
            };
        }
        hints.AddForbiddenZone(ShapeContains.InvertedCone(origin.Actor.Position, 4, dir, 15.Degrees()), origin.Activation);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 1 and <= 8 && state == 0x00020001)
        {
            _blueMirror = (225 - index * 45).Degrees();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ScytheKick:
                var activation = Module.CastFinishAt(spell, 0.7f);
                FirstSources.Add(new(caster, activation));
                var mirror = _blueMirror != null ? Module.Enemies(OID.FrozenMirror).Closest(Module.Center + 20 * _blueMirror.Value.ToDirection()) : null;
                if (mirror != null)
                    FirstSources.Add(new(mirror, activation));
                break;
            case AID.ReflectedScytheKickRed:
                SecondSources.Add(new(caster, Module.CastFinishAt(spell, 0.6f)));
                if (SecondSources.Count == 2 && _blueMirror != null)
                {
                    // order two red mirrors so that first one is closer to boss and second one closer to blue mirror; if both are same distance, select CW ones (arbitrary)
                    var d1 = (Angle.FromDirection(SecondSources[0].Actor.Position - Module.Center) - _blueMirror.Value).Normalized();
                    var d2 = (Angle.FromDirection(SecondSources[1].Actor.Position - Module.Center) - _blueMirror.Value).Normalized();
                    var d1abs = d1.Abs();
                    var d2abs = d2.Abs();
                    var swap = d2abs.AlmostEqual(d1abs, 0.1f)
                        ? d2.Rad > 0 // swap if currently second one is CCW from blue mirror
                        : d2abs.Rad > d1abs.Rad; // swap if currently second one is further from the blue mirror
                    if (swap)
                        (SecondSources[1], SecondSources[0]) = (SecondSources[0], SecondSources[1]);

                    RedRangedLeftOfMelee = (SecondSources[0].Actor.Position - Module.Center).OrthoL().Dot(SecondSources[1].Actor.Position - Module.Center) > 0;
                }
                break;
        }
    }
}

class P2MirrorMirrorBanish : P2Banish
{
    private WPos _anchorMelee;
    private WPos _anchorRanged;
    private BitMask _aroundRanged;
    private BitMask _closerToCenter;
    private BitMask _leftSide;

    public P2MirrorMirrorBanish(BossModule module) : base(module)
    {
        var proteans = module.FindComponent<P2MirrorMirrorHouseOfLight>();
        if (proteans != null && proteans.FirstSources.Count == 2 && proteans.SecondSources.Count == 2)
        {
            _anchorMelee = proteans.FirstSources[0].Actor.Position;
            _anchorRanged = module.Center + 0.5f * (proteans.SecondSources[1].Actor.Position - module.Center);
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
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(prepos.Value, 1), DateTime.MaxValue);
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }

    private WPos? PrepositionLocation(int slot, PartyRolesConfig.Assignment assignment)
        => Stacks.Count > 0 && Stacks[0].Activation > WorldState.FutureTime(2.5f) ? CalculatePrepositionLocation(_aroundRanged[slot], _leftSide[slot], 90.Degrees())
        : Spreads.Count > 0 && Spreads[0].Activation > WorldState.FutureTime(2.5f) ? CalculatePrepositionLocation(_aroundRanged[slot], _leftSide[slot], (_closerToCenter[slot] ? 135 : 45).Degrees())
        : null;

    private WPos CalculatePrepositionLocation(bool aroundRanged, bool leftSide, Angle angle)
    {
        var anchor = aroundRanged ? _anchorRanged : _anchorMelee;
        var offset = Angle.FromDirection(anchor - Module.Center) + (leftSide ? angle : -angle);
        return anchor + 6 * offset.ToDirection();
    }
}
