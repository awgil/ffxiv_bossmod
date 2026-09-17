namespace BossMod.Stormblood.Ultimate.UCOB;

class LiquidHell(BossModule module) : Components.VoidzoneAtCastTarget(module, 6, AID.LiquidHell, OID.VoidzoneLiquidHell, 1.3f, activationDelay: 1.8f)
{
    protected DateTime NextCast;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // it should be possible to only draw hints for active puddles since the delay is so long, but in practice, baiter gets burns about 5% of the time
        foreach (var p in _predictedByEvent)
            if (p.time < WorldState.FutureTime(1))
                hints.AddForbiddenZone(Shape, p.pos, default, p.time);
        foreach (var (z, spawn) in _sources)
            hints.AddForbiddenZone(Shape, z.Position, activation: spawn.AddSeconds(ActivationDelay));

    }
}

class P1LiquidHell : LiquidHell
{
    public P1LiquidHell(BossModule module) : base(module) { KeepOnPhaseChange = true; }

    public enum BaitMode
    {
        None,
        Proximity,
        Random
    }

    BaitMode Mode;

    public BitMask Baiters;

    public void Reset(float delay, BaitMode mode)
    {
        Mode = mode;
        NextCast = WorldState.FutureTime(delay);
        NumCasts = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (spell.Action == WatchedAction)
        {
            NextCast = WorldState.FutureTime(1.2f);

            if (Mode == BaitMode.Random && !Baiters.Any())
                Baiters |= Raid.WithSlot().InRadius(spell.TargetXZ, 1).Mask();
        }

        if (NumCasts >= 5)
        {
            NextCast = default;
            Mode = BaitMode.None;
            Baiters.Reset();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Mode == BaitMode.Proximity)
        {
            var assignments = Service.Config.Get<PartyRolesConfig>().SlotsPerAssignment(Raid);

            if (assignments.Length == 0)
                return;

            bool isBaiter;

            var slotR1 = assignments[(int)PartyRolesConfig.Assignment.R1];
            if (Module.FindComponent<Hatch>()?.IsTarget(slotR1) == true)
                isBaiter = assignment == PartyRolesConfig.Assignment.H1;
            else
                isBaiter = assignment == PartyRolesConfig.Assignment.R1;

            if (isBaiter)
            {
                hints.AddForbiddenZone(ShapeDistance.Circle(Module.PrimaryActor.Position, 18), NextCast);

                // encourage baiter to stay on the opposite half of the arena, because it tends to walk itself into a corner otherwise
                hints.AddForbiddenZone(ShapeDistance.InvertedCone(Module.PrimaryActor.Position, 50, Module.PrimaryActor.DirectionTo(Arena.Center).ToAngle(), 45.Degrees()), DateTime.MaxValue);

                var center = Arena.Center;
                // encourage baiter to stay on arena edge if possible
                hints.GoalZones.Add(p => p.InDonut(center, 18, 22) ? 0.1f : 0);

                // don't drop on neurolinks
                foreach (var nl in Module.Enemies(OID.Neurolink))
                    hints.AddForbiddenZone(ShapeDistance.Circle(nl.Position, 7), NextCast);
            }
            else
                hints.GoalZones.Add(AIHints.GoalSingleTarget(Module.PrimaryActor.Position, 16, 0.1f));
        }

        if (Mode == BaitMode.Random)
        {
            if (NumCasts == 0 && Module.PrimaryActor.TargetID != actor.InstanceID && Module.FindComponent<Hatch>()?.IsTarget(slot) == false && assignment is not (PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.MT))
                hints.AddForbiddenZone(ShapeDistance.Circle(Module.PrimaryActor.Position, 6), NextCast);

            if (Baiters[slot] && Module.FindComponent<P1Fireball>()?.Destination is { } dest && dest != default)
            {
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(dest, 11), NextCast.AddSeconds(1.2f * (4 - NumCasts)));
                hints.AddForbiddenZone(ShapeDistance.Circle(dest, 7), NextCast);
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => Baiters[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override void Update()
    {
        base.Update();

        if (Mode == BaitMode.Proximity)
            Baiters = BitMask.Build(Raid.WithSlot().Farthest(Module.PrimaryActor.Position).Item1);
    }
}

class P3LiquidHell : LiquidHell
{
    public P3LiquidHell(BossModule module) : base(module)
    {
        NextCast = module.WorldState.FutureTime(8.2f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (assignment == PartyRolesConfig.Assignment.R1 && Module.Enemies(OID.Twintania).FirstOrDefault() is { } twin)
        {
            if (_predictedByEvent.Count + _sources.Count == 0)
                hints.GoalZones.Add(AIHints.GoalProximity(new(4, 20), 10, 1));

            hints.AddForbiddenZone(ShapeDistance.InvertedCone(twin.Position, 50, twin.DirectionTo(Arena.Center).ToAngle(), 45.Degrees()), DateTime.MaxValue);

            if (NextCast != default)
            {
                hints.AddForbiddenZone(ShapeDistance.Circle(twin.Position, 18), NextCast);

                foreach (var nl in Module.Enemies(OID.Neurolink))
                    hints.AddForbiddenZone(ShapeDistance.Circle(nl.Position, 7), NextCast);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (spell.Action == WatchedAction)
            NextCast = WorldState.FutureTime(1.2f);

        if (NumCasts >= 5)
            NextCast = default;
    }
}
