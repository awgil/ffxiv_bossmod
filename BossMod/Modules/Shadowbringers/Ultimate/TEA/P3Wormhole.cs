namespace BossMod.Shadowbringers.Ultimate.TEA;

class P3WormholeLimitCut(BossModule module) : LimitCut(module, 2.7f);
class P3WormholeSacrament(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SacramentWormhole), new AOEShapeCross(100, 8));

class P3WormholeRepentance(BossModule module) : BossComponent(module)
{
    public int NumSoaks { get; private set; }
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

        var dirToAlex = (alex.Position - Module.Center).Normalized();
        var dirToSide = SelectSide(order, dirToAlex);
        bool shouldSoak = ShouldSoakWormhole(order);

        if (_chakramsDone)
        {
            // show hints for soaking or avoiding wormhole
            bool soakingWormhole = _wormholes.Any(w => actor.Position.InCircle(w, _radiuses[NumSoaks]));
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
            var dirToAlex = (alex.Position - Module.Center).Normalized();
            var dirToSide = SelectSide(order, dirToAlex);
            movementHints.Add(actor.Position, Module.Center + SafeSpotOffset(order, dirToAlex, dirToSide), ArenaColor.Safe);
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

        var dirToAlex = (alex.Position - Module.Center).Normalized();
        var dirToSide = SelectSide(pcOrder, dirToAlex);
        var shouldSoak = ShouldSoakWormhole(pcOrder);

        foreach (var w in _wormholes)
            Arena.AddCircle(w, _radiuses[NumSoaks], shouldSoak && dirToSide.Dot(w - Module.Center) > 0 ? ArenaColor.Safe : ArenaColor.Danger);

        if (!shouldSoak || !_chakramsDone)
            Arena.AddCircle(Module.Center + SafeSpotOffset(pcOrder, dirToAlex, dirToSide), 1, ArenaColor.Safe);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Wormhole1)
            _wormholes.Add(actor.Position);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.EyeOfTheChakram:
                _chakramsDone = true;
                break;
            case AID.Repentance1:
                NumSoaks = 1;
                break;
            case AID.Repentance2:
                NumSoaks = 2;
                break;
            case AID.Repentance3:
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
            bool towardsAlex = order is 3 or 4 or 7 or 8;
            // distance = 19, divided by sqrt(2)
            return 13.435f * (towardsAlex ? dirToSide + dirToAlex : dirToSide - dirToAlex);
        }
        else if (_chakramsDone)
        {
            // after chakrams are done, assume inactive chill at sides, this avoids sacrament
            return 19 * dirToSide;
        }
        else
        {
            // before chakrams, move slightly away from alex, to guarantee not being clipped by jump
            // ~20 degrees => 6.5 away from alex => 17.8 to the side
            return 17.8f * dirToSide - 6.5f * dirToAlex;
        }
    }
}

class P3WormholeIncineratingHeat(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.IncineratingHeat), 5, 8);

class P3WormholeEnumeration(BossModule module) : Components.UniformStackSpread(module, 5, 0, 3, 3, raidwideOnResolve: false) // TODO: verify enumeration radius
{
    private BitMask _targets; // we start showing stacks only after incinerating heat is resolved
    private DateTime _activation;

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Enumeration)
        {
            _targets.Set(Raid.FindSlot(actor.InstanceID));
            _activation = WorldState.FutureTime(5.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Enumeration:
                Stacks.Clear();
                break;
            case AID.IncineratingHeat:
                AddStacks(Raid.WithSlot(true).IncludedInMask(_targets).Actors(), _activation);
                break;
        }
    }
}
