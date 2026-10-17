namespace BossMod.Stormblood.Ultimate.UWU;

class P1Plumes(BossModule module) : BossComponent(module)
{
    private readonly IReadOnlyList<Actor> _razor = module.Enemies(OID.RazorPlume);
    private readonly IReadOnlyList<Actor> _spiny = module.Enemies(OID.SpinyPlume);
    private readonly IReadOnlyList<Actor> _satin = module.Enemies(OID.SatinPlume);

    public bool Active => _razor.Any(p => p.IsTargetable) || _spiny.Any(p => p.IsTargetable) || _satin.Any(p => p.IsTargetable);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_razor, ArenaColor.Enemy);
        Arena.Actors(_spiny, ArenaColor.Enemy);
        Arena.Actors(_satin, ArenaColor.Enemy);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.PrimaryActor.TargetID == actor.InstanceID)
            return;

        foreach (var sp in _spiny)
        {
            if (hints.FindEnemy(sp) is { } spi)
            {
                spi.DesiredPosition = new(93.3f, 105);

                var stacks = WorldState.Actors.Find(sp.TargetID)?.FindStatus(SID.ThermalLow, DateTime.MaxValue)?.Extra ?? 0;

                if (sp.Tether.ID == 0)
                {
                    if (assignment != PartyRolesConfig.Assignment.OT)
                        spi.Priority = AIHints.Enemy.PriorityForbidden;
                }
                else if (sp.Tether.Target == actor.InstanceID)
                    spi.Priority = 2;
                else
                    spi.Priority = 1;

                if (sp.PendingHPRatio < 0.1f)
                    spi.Priority = stacks == 2 ? 2 : AIHints.Enemy.PriorityForbidden;
            }
        }

        foreach (var sp in _satin)
        {
            if (hints.FindEnemy(sp) is { } sat)
            {
                sat.DesiredPosition = new(93.3f, 105);
                sat.Priority = 1;
            }
        }
    }
}

class P1Friction(BossModule module) : Components.StackWithCastTargets(module, AID.Friction, 5, 8)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.Enemies(OID.SpinyShield).FirstOrDefault() is not { } bubble)
            return;

        var toCenter = (Arena.Center - bubble.Position).Normalized();
        var target = bubble.Position + toCenter * 6;

        hints.GoalZones.Add(AIHints.GoalSingleTarget(target, 5, 0.1f));

        if (IsStackTarget(actor))
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(target, 2), Stacks[0].Activation);
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class P1SpinyShield(BossModule module) : BossComponent(module)
{
    public BitMask Forbidden = new();

    int _numFriction;
    WPos? _pending;

    WPos? Bubble => Module.Enemies(OID.SpinyShield).FirstOrDefault()?.Position ?? _pending;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Bubble is not { } e)
            return;

        if (Forbidden[pcSlot])
            return;

        if (Arena.Config.ShowOutlinesAndShadows)
            Arena.AddCircle(e, 6, 0xFF000000, 2);
        Arena.AddCircle(e, 6, ArenaColor.Safe);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Bubble is not { } e)
            return;

        if (Forbidden[pcSlot])
            Arena.ZoneCircle(e, 6, ArenaColor.AOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Bubble is not { } e)
            return;

        var safe = Forbidden[slot] != actor.Position.InCircle(e, 6);

        if (!safe)
            hints.Add(Forbidden[slot] ? "GTFO from bubble!" : "Go to bubble!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Bubble is not { } e)
            return;

        if (Forbidden[slot])
            hints.AddForbiddenZone(ShapeDistance.Circle(e, 6));
        else
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(e, 6));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Gigastorm)
        {
            _pending = caster.Position;
            Forbidden = Raid.WithSlot().WhereActor(a => a.FindStatus(SID.ThermalLow, DateTime.MaxValue) != null).Mask();
        }

        if ((AID)spell.Action.ID == AID.Friction)
            _numFriction++;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Friction)
        {
            Forbidden.Reset();

            var assignments = Service.Config.Get<PartyRolesConfig>().SlotsPerAssignment(Raid);
            if (assignments.Length == 0)
            {
                ReportError($"Don't know who should leave for friction, doing nothing");
                return;
            }

            if (_numFriction == 0)
            {
                Forbidden.Set(assignments[(int)PartyRolesConfig.Assignment.MT]);
                Forbidden.Set(assignments[(int)PartyRolesConfig.Assignment.M1]);
                Forbidden.Set(assignments[(int)PartyRolesConfig.Assignment.M2]);
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.SpinyShield)
            _pending = null;
    }
}
