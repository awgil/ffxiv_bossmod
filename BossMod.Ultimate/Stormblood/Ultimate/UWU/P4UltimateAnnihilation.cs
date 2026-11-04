namespace BossMod.Stormblood.Ultimate.UWU;

class P4UltimateAnnihilation(BossModule module) : BossComponent(module)
{
    private readonly IReadOnlyList<Actor> _orbs = module.Enemies(OID.Aetheroplasm);

    int _explosions;
    WPos _lastOrb;
    WPos partyDodgeBasis;
    bool stackDone;
    int windTarget = -1;

    private const float _radius = 6;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var orb in _orbs.Where(o => !o.IsDead))
        {
            Arena.Actor(orb, ArenaColor.Object, true);
            Arena.AddCircle(orb.Position, _radius, ArenaColor.Object);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if ((AID)spell.Action.ID == AID.WeightOfTheLandAOE && stackDone)
            partyDodgeBasis = spell.LocXZ;

        if ((AID)spell.Action.ID == AID.InfernoHowl && windTarget < 0)
            windTarget = Raid.FindSlot(spell.TargetID);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Aetheroplasm)
        {
            _explosions++;
            _lastOrb = caster.Position;
        }

        if ((AID)spell.Action.ID == AID.FlamingCrush)
            stackDone = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_explosions >= 4)
            return;

        var soaking = assignment == PartyRolesConfig.Assignment.MT || _explosions < 2 && assignment == PartyRolesConfig.Assignment.OT;

        var orb = _orbs.FirstOrDefault(o => !o.IsDead)?.Position ?? _lastOrb;

        switch (assignment)
        {
            // R1 can move early since they won't get in anyone's way
            case PartyRolesConfig.Assignment.R1:
                if (stackDone && actor.FindStatus(SID.ThermalLow) != null)
                {
                    if (WorldState.Actors.Find(Module.PrimaryActor.Tether.Target) is { } tetherTarget && tetherTarget != actor)
                    {
                        var toGaruda = tetherTarget.DirectionTo(Module.PrimaryActor);
                        hints.AddForbiddenZone(ShapeDistance.PrecisePosition(tetherTarget.Position + toGaruda * 3, new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f));
                    }
                    else
                        // take tether around C (rect so we can dodge feather rain)
                        hints.AddForbiddenZone(ShapeDistance.InvertedRect(new WPos(100, 105), new WPos(100, 110), 1), DateTime.MaxValue);
                }
                break;

            // dps have no particular job, dodge north
            case PartyRolesConfig.Assignment.M1:
            case PartyRolesConfig.Assignment.M2:
            case PartyRolesConfig.Assignment.R2:
                if (partyDodgeBasis != default)
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(96, 84), 1), DateTime.MaxValue);
                break;

            // tanks walk into where first orb will spawn
            case PartyRolesConfig.Assignment.MT:
            case PartyRolesConfig.Assignment.OT:
                if (partyDodgeBasis != default && _lastOrb == default)
                    hints.AddForbiddenZone(ShapeDistance.InvertedRect(partyDodgeBasis, new WDir(1, 0), 100, 0, 1));
                break;

            // healers spread W
            case PartyRolesConfig.Assignment.H1:
            case PartyRolesConfig.Assignment.H2:
                if (partyDodgeBasis != default)
                {
                    if (windTarget < 0)
                        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(84, 100), 1), DateTime.MaxValue);
                    else if (windTarget != slot)
                        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(96, 84), 1), DateTime.MaxValue);
                }
                break;
        }

        if (orb != default)
        {
            if (soaking)
                hints.AddForbiddenZone(ShapeDistance.PrecisePosition(orb, new(0, 1), 0.5f, actor.Position, 0.1f), DateTime.MaxValue);
            else
                hints.AddForbiddenZone(ShapeDistance.Circle(orb, 6), WorldState.FutureTime(1));
        }
    }
}

class P4Bury(BossModule module) : Components.GenericAOEs(module, AID.Bury)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RadiantPlumeUltima)
        {
            var activation = Module.CastFinishAt(spell, 1);
            _predicted.Add(new(new AOEShapeCircle(4.3f), new(103, 105), default, activation));
            _predicted.Add(new(new AOEShapeCircle(4.3f), new(97, 105), default, activation));
            _predicted.Add(new(new AOEShapeCircle(4.3f), new(94, 100), default, activation));
            _predicted.Add(new(new AOEShapeCircle(4.3f), new(97, 95), default, activation));
            _predicted.Add(new(new AOEShapeCircle(4.3f), new(103, 95), default, activation));
            _predicted.Add(new(new AOEShapeCircle(4.3f), new(106, 100), default, activation));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_predicted.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 5), _predicted[0].Activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _predicted.RemoveAll(p => p.Origin.AlmostEqual(caster.Position, 1));
            foreach (ref var p in _predicted.AsSpan())
                p.Activation = WorldState.FutureTime(2.1f);

        }
    }
}
