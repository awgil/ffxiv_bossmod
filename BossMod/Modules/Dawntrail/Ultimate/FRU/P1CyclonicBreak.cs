namespace BossMod.Dawntrail.Ultimate.FRU;

class P1CyclonicBreakSpreadStack(BossModule module) : Components.UniformStackSpread(module, 6, 6, 2, 2, true)
{
    public DateTime Activation = DateTime.MaxValue;
    private bool _fullHints; // we only need to actually stack/spread after first protean bait

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_fullHints)
            base.AddHints(slot, actor, hints);
        else if (Stacks.Count > 0)
            hints.Add("Prepare to stack", false);
        else if (Spreads.Count > 0)
            hints.Add("Prepare to spread", false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by dedicated component

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CyclonicBreakBossStack:
            case AID.CyclonicBreakImageStack:
                // TODO: this can target either supports or dd
                Activation = Module.CastFinishAt(spell, 2.7f);
                AddStacks(Module.Raid.WithoutSlot(true).Where(p => p.Class.IsSupport()), Activation);
                break;
            case AID.CyclonicBreakBossSpread:
            case AID.CyclonicBreakImageSpread:
                Activation = Module.CastFinishAt(spell, 2.7f);
                AddSpreads(Module.Raid.WithoutSlot(true), Activation);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CyclonicBreakSinsmoke:
                Stacks.Clear();
                break;
            case AID.CyclonicBreakSinsmite:
                Spreads.Clear();
                break;
            case AID.CyclonicBreakAOEFirst:
                _fullHints = true;
                break;
        }
    }
}

class P1CyclonicBreakProtean(BossModule module) : Components.BaitAwayEveryone(module, module.PrimaryActor, P1CyclonicBreakCone.Shape, AID.CyclonicBreakAOEFirst)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by dedicated component
}

class P1CyclonicBreakCone(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    private DateTime _currentBundle;

    public static readonly AOEShapeCone Shape = new(60, 11.5f.Degrees()); // TODO: verify angle

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by dedicated component

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CyclonicBreakAOEFirst:
                AOEs.Add(new(Shape, caster.Position, spell.Rotation, WorldState.FutureTime(2)));
                break;
            case AID.CyclonicBreakAOERest:
                if (WorldState.CurrentTime > _currentBundle)
                {
                    _currentBundle = WorldState.CurrentTime.AddSeconds(1);
                    ++NumCasts;
                    foreach (ref var aoe in AOEs.AsSpan())
                        aoe.Rotation -= 22.5f.Degrees();
                }
                if (!AOEs.Any(aoe => aoe.Rotation.AlmostEqual(spell.Rotation - 22.5f.Degrees(), 0.1f)))
                    ReportError($"Failed to find protean @ {spell.Rotation}");
                break;
        }
    }
}

class P1CyclonicBreakAIBait(BossModule module) : BossComponent(module)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private readonly P1CyclonicBreakSpreadStack? _spreadStack = module.FindComponent<P1CyclonicBreakSpreadStack>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var clockspot = _config.P1CyclonicBreakSpots[assignment];
        if (clockspot < 0 || _spreadStack == null || !_spreadStack.Active)
            return; // no assignment
        var assignedDirection = (180 - 45 * clockspot).Degrees();
        // TODO: think about melee vs ranged distance...
        hints.AddForbiddenZone(ShapeContains.InvertedRect(Module.PrimaryActor.Position, assignedDirection, 15, -5, 1), _spreadStack.Activation);
    }
}

class P1CyclonicBreakAIDodgeSpreadStack(BossModule module) : BossComponent(module)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private readonly P1CyclonicBreakSpreadStack? _spreadStack = module.FindComponent<P1CyclonicBreakSpreadStack>();
    private readonly P1CyclonicBreakCone? _cones = module.FindComponent<P1CyclonicBreakCone>();
    private readonly ArcList _forbiddenDirections = new(module.PrimaryActor.Position, 0);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var clockspot = _config.P1CyclonicBreakSpots[assignment];
        if (clockspot < 0 || _cones == null || _spreadStack == null || !_spreadStack.Active)
            return;

        _forbiddenDirections.Forbidden.Clear();
        foreach (var aoe in _cones.AOEs)
            _forbiddenDirections.ForbidArcByLength(aoe.Rotation, P1CyclonicBreakCone.Shape.HalfAngle);

        var isSupport = actor.Class.IsSupport();
        var dodgeCCW = _spreadStack.Stacks.Count > 0 ? _config.P1CyclonicBreakStackSupportsCCW == isSupport : isSupport ? _config.P1CyclonicBreakSpreadSupportsCCW : _config.P1CyclonicBreakSpreadDDCCW;
        var assignedDirection = (180 - 45 * clockspot).Degrees();
        var safeAngles = _forbiddenDirections.NextAllowed(assignedDirection, dodgeCCW);
        var (rangeMin, rangeMax) = _spreadStack.Stacks.Count > 0 ? (4, 10) : assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT or PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 ? (3, 6) : (7, 15);
        var safeZone = ShapeContains.DonutSector(_forbiddenDirections.Center, rangeMin, rangeMax, (safeAngles.min + safeAngles.max) * 0.5f, (safeAngles.max - safeAngles.min) * 0.5f);
        hints.AddForbiddenZone(p => !safeZone(p), _spreadStack.Activation);

        // micro adjusts if activation is imminent
        if (_spreadStack.Activation < WorldState.FutureTime(0.5f))
        {
            if (_spreadStack.Stacks.Count > 0)
            {
                var closestPartner = Module.Raid.WithoutSlot().Where(p => p.Class.IsSupport() != isSupport).Closest(actor.Position);
                if (closestPartner != null)
                    hints.AddForbiddenZone(ShapeContains.InvertedCircle(closestPartner.Position, _spreadStack.StackRadius), _spreadStack.Activation);
            }
            else
            {
                foreach (var p in Raid.WithoutSlot().Exclude(actor))
                    hints.AddForbiddenZone(ShapeContains.Circle(p.Position, _spreadStack.SpreadRadius), _spreadStack.Activation);
            }
        }
    }
}

class P1CyclonicBreakAIDodgeRest(BossModule module) : BossComponent(module)
{
    private readonly P1CyclonicBreakCone? _cones = module.FindComponent<P1CyclonicBreakCone>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_cones != null)
            foreach (var aoe in _cones.AOEs)
                hints.AddForbiddenZone(aoe.Shape.CheckFn(aoe.Origin, aoe.Rotation), aoe.Activation);
    }
}
