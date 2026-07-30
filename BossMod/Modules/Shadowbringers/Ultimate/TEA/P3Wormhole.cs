namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
sealed class P3WormholeLimitCut(BossModule module) : LimitCut(module, 2.7d);
[SkipLocalsInit]
sealed class P3WormholeSacrament(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SacramentWormhole, new AOEShapeCross(100f, 8f));

[SkipLocalsInit]
sealed class P3WormholeRepentance(BossModule module) : BossComponent(module)
{
    public int NumSoaks;
    private bool _chakramsDone;
    private readonly LimitCut? _limitCut = module.FindComponent<LimitCut>();
    private readonly List<WPos> _wormholes = [];

    private static readonly float[] _radiuses = [8, 6, 3];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var alex = ((TEA)Module).AlexPrime();
        if (alex == null || NumSoaks >= 3)
            return;

        var order = _limitCut?.PlayerOrder[slot] ?? 0;
        if (order == 0)
            return;

        var dirToAlex = (alex.Position - Arena.Center).Normalized();
        var dirToSide = SelectSide(order, dirToAlex);
        var shouldSoak = ShouldSoakWormhole(order);

        if (_chakramsDone)
        {
            // show hints for soaking or avoiding wormhole
            var soakingWormhole = _wormholes.Any(w => actor.Position.InCircle(w, _radiuses[NumSoaks]));
            if (soakingWormhole != shouldSoak)
                hints.Add(shouldSoak ? "Soak the wormhole!" : "GTFO from wormhole!");
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        var alex = ((TEA)Module).AlexPrime();
        if (alex == null || NumSoaks >= 3)
            return;

        var order = _limitCut?.PlayerOrder[slot] ?? 0;
        if (order == 0)
            return;

        if (!ShouldSoakWormhole(order) || !_chakramsDone)
        {
            var dirToAlex = (alex.Position - Arena.Center).Normalized();
            var dirToSide = SelectSide(order, dirToAlex);
            movementHints.Add(actor.Position, Arena.Center + SafeSpotOffset(order, dirToAlex, dirToSide), Colors.Safe);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var alex = ((TEA)Module).AlexPrime();
        if (alex == null || NumSoaks >= 3)
            return;

        var pcOrder = _limitCut?.PlayerOrder[pcSlot] ?? 0;
        if (pcOrder == 0)
            return;

        var dirToAlex = (alex.Position - Arena.Center).Normalized();
        var dirToSide = SelectSide(pcOrder, dirToAlex);
        var shouldSoak = ShouldSoakWormhole(pcOrder);

        foreach (var w in _wormholes)
            Arena.ZoneCircleOutline(w, _radiuses[NumSoaks], shouldSoak && dirToSide.Dot(w - Arena.Center) > 0f ? Colors.Safe : default);

        if (!shouldSoak || !_chakramsDone)
            Arena.ZoneCircleOutline(Arena.Center + SafeSpotOffset(pcOrder, dirToAlex, dirToSide), 1f, Colors.Safe);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Wormhole1)
            _wormholes.Add(actor.Position);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.EyeOfTheChakram:
                _chakramsDone = true;
                break;
            case (uint)AID.Repentance1:
                NumSoaks = 1;
                break;
            case (uint)AID.Repentance2:
                NumSoaks = 2;
                break;
            case (uint)AID.Repentance3:
                NumSoaks = 3;
                break;
        }
    }

    private bool ShouldSoakWormhole(int order) => order switch
    {
        5 or 6 => NumSoaks == 0,
        7 or 8 => NumSoaks == 1,
        1 or 2 => NumSoaks == 2,
        _ => false
    };

    // assume LPDU strats: looking away from alex, odd go left, even go right
    private WDir SelectSide(int order, WDir dirToAlex) => (order & 1) != 0 ? dirToAlex.OrthoR() : dirToAlex.OrthoL();

    private WDir SafeSpotOffset(int order, WDir dirToAlex, WDir dirToSide)
    {
        var ordersDone = _limitCut?.NumCasts ?? 0;
        if (order > ordersDone && order <= ordersDone + 4) // next jump at player, so go to assigned spot
        {
            // assume LPDU assignments: 1/2/5/6 go opposite alex, rest go towards
            var towardsAlex = order is 3 or 4 or 7 or 8;
            // distance = 19, divided by sqrt(2)
            return 13.435f * (towardsAlex ? dirToSide + dirToAlex : dirToSide - dirToAlex);
        }
        else if (_chakramsDone)
        {
            // after chakrams are done, assume inactive chill at sides, this avoids sacrament
            return 19f * dirToSide;
        }
        else
        {
            // before chakrams, move slightly away from alex, to guarantee not being clipped by jump
            // ~20 degrees => 6.5 away from alex => 17.8 to the side
            return 17.8f * dirToSide - 6.5f * dirToAlex;
        }
    }
}

[SkipLocalsInit]
sealed class P3WormholeIncineratingHeat(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.IncineratingHeat, 5f, 8, 8);

[SkipLocalsInit]
sealed class P3WormholeEnumeration(BossModule module) : Components.UniformStackSpread(module, 5f, default, 3, 3, raidwideOnResolve: false) // TODO: verify enumeration radius
{
    private BitMask _targets; // we start showing stacks only after incinerating heat is resolved
    private DateTime _activation;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Enumeration)
        {
            _targets.Set(Raid.FindSlot(actor.InstanceID));
            _activation = WorldState.FutureTime(5.1d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Enumeration:
                Stacks.Clear();
                break;
            case (uint)AID.IncineratingHeat:
                AddStacks(Raid.WithSlot(true, true, true).IncludedInMask(_targets).Actors(), _activation);
                break;
        }
    }
}
