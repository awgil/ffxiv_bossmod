namespace BossMod.Dawntrail.Ultimate.FRU;

class P1UtopianSkyBlastingZone(BossModule module) : Components.GenericAOEs(module, AID.BlastingZoneAOE)
{
    public readonly List<AOEInstance> AOEs = [];
    public BitMask DangerousSpots; // 0 = N, then CCW
    public DateTime Activation = DateTime.MaxValue;

    private static readonly AOEShapeRect _shape = new(50, 8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // micro adjust when activation is imminent; before that we have dedicated ai component
        if (WorldState.FutureTime(1) > Activation)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if ((OID)actor.OID == OID.FatebreakersImage && modelState == 4)
        {
            Activation = WorldState.FutureTime(9.1f);
            AOEs.Add(new(_shape, actor.Position, actor.Rotation, Activation));
            DangerousSpots.Set((int)MathF.Round((-Angle.FromDirection(actor.Position - Module.Center).Deg + 180) / 45) % 8);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            AOEs.Clear();
        }
    }
}

class P1UtopianSkySpreadStack(BossModule module) : Components.UniformStackSpread(module, 6, 5, 4, 4, true)
{
    public enum Mechanic { None, Spread, Stack }

    public Mechanic CurMechanic;
    public DateTime Activation = DateTime.MaxValue;

    public void Show(DateTime activation)
    {
        Activation = activation;
        ExtraAISpreadThreshold = 0;
        switch (CurMechanic)
        {
            case Mechanic.Stack:
                // TODO: this can target either tanks or healers
                AddStacks(Module.Raid.WithoutSlot(true).Where(p => p.Role == Role.Healer), activation);
                break;
            case Mechanic.Spread:
                AddSpreads(Module.Raid.WithoutSlot(true), activation);
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurMechanic != Mechanic.None)
            hints.Add($"Next: {CurMechanic}");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // micro adjust when activation is imminent; before that we have dedicated ai component
        if (WorldState.FutureTime(1.5f) > Activation)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CurMechanic = (AID)spell.Action.ID switch
        {
            AID.UtopianSkyStack => Mechanic.Stack,
            AID.UtopianSkySpread => Mechanic.Spread,
            _ => CurMechanic
        };
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SinboundFire:
                Stacks.Clear();
                break;
            case AID.SinboundThunder:
                Spreads.Clear();
                break;
        }
    }
}

// initial positions: resolve tankbuster + 'see' own image
class P1UtopianSkyAIInitial(BossModule module) : BossComponent(module)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.AddForbiddenZone(ShapeContains.Circle(Module.Center, 18)); // stay on edge

        var clockspot = _config.P1UtopianSkyInitialSpots[assignment];
        if (clockspot >= 0)
        {
            // ... and in assigned cone
            var assignedDirection = (180 - 45 * clockspot).Degrees();
            hints.AddForbiddenZone(ShapeContains.InvertedCone(Module.Center, 50, assignedDirection, 5.Degrees()), DateTime.MaxValue);
        }
    }
}

// note: here we emulate 'natural' resolve:
// - if there is no aoe on our clockspot, we stay at starting spot
// - if our clone starts aoe, we move to the center immediately
// - if opposite clone starts aoe, we move to the center as soon as our opposite partner moves
// - as soon as all aoe directions have at least one person 'near center', or if resolve is imminent, we move to our final spot
class P1UtopianSkyAIResolve(BossModule module) : BossComponent(module)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private readonly P1UtopianSkyBlastingZone? _aoes = module.FindComponent<P1UtopianSkyBlastingZone>();
    private readonly P1UtopianSkySpreadStack? _spreadStack = module.FindComponent<P1UtopianSkySpreadStack>();
    private BitMask _seenDangerSpot;

    public override void Update()
    {
        if (_aoes == null || _seenDangerSpot.NumSetBits() >= 3)
            return;

        var folded = _aoes.DangerousSpots | (_aoes.DangerousSpots >> 4);
        if (WorldState.FutureTime(6) > _aoes.Activation)
        {
            // can't wait any longer for people to think...
            _seenDangerSpot = folded & new BitMask(0xF);
            return;
        }

        foreach (var (slot, group) in _config.P1UtopianSkyInitialSpots.Resolve(Raid))
        {
            var spot = group & 3;
            if (folded[spot] && !_seenDangerSpot[spot] && Raid[slot] is var p && p != null && !p.Position.InDonutCone(Module.Center, 12, 20, (180 - 45 * group).Degrees(), 30.Degrees()))
                _seenDangerSpot.Set(spot);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var clockSpot = _config.P1UtopianSkyInitialSpots[assignment];
        if (_aoes == null)
            return;

        var numDangerSpots = _seenDangerSpot.NumSetBits();
        if (numDangerSpots < 3)
        {
            if (clockSpot >= 0 && (_aoes.DangerousSpots[clockSpot] || _seenDangerSpot[clockSpot & 3]))
            {
                // our spot is dangerous, or our partner's is and he has moved - move to center
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(Module.Center, 5), _aoes.Activation);
            }
            // else: we don't have a reason to move, stay where we are...
        }
        else if (numDangerSpots == 3/* && WorldState.FutureTime(1) <= _aoes.Activation*/)
        {
            // ok, we know where to go - move
            var spreadSpot = _config.P1UtopianSkySpreadSpots[assignment];
            if (_spreadStack?.Stacks.Count > 0)
                spreadSpot &= 4; // stack on close spot

            var direction = (4 - (~_seenDangerSpot).LowestSetBit()) % 4 * 45.Degrees() + (spreadSpot >= 4 ? 0 : 180).Degrees();
            spreadSpot &= 3;
            direction += spreadSpot switch
            {
                2 => -18.Degrees(),
                3 => 18.Degrees(),
                _ => default
            };
            var range = spreadSpot == 0 ? 13 : 19;
            hints.PathfindMapBounds = FRU.PathfindHugBorderBounds;
            hints.AddForbiddenZone(ShapeContains.PrecisePosition(Module.Center + range * direction.ToDirection(), new(0, 1), Module.Bounds.MapResolution, actor.Position, 0.1f), _aoes.Activation);
        }
    }
}
