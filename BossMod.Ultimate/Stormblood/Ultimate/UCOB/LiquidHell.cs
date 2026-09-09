namespace BossMod.Stormblood.Ultimate.UCOB;

class LiquidHell(BossModule module) : Components.VoidzoneAtCastTarget(module, 6, AID.LiquidHell, OID.VoidzoneLiquidHell, 1.3f, activationDelay: 1.8f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // we only add hints for spawned fireballs since the activation is so delayed
        // this helps party not kill themselves during blackfire trio, and gives ranged lots of extra room in p1
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
    DateTime NextCast;

    public Actor? Baiter { get; private set; }

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

            if (Mode == BaitMode.Random && (Baiter == null || Baiter.IsDead))
                Baiter = Raid.WithoutSlot().Closest(spell.TargetXZ);
        }

        if (NumCasts >= 5)
        {
            NextCast = default;
            Mode = BaitMode.None;
            Baiter = null;
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

                // encourage baiter to stay on arena edge if possible
                hints.GoalZones.Add(p => p.InDonut(Arena.Center, 18, 22) ? 0.1f : 0);

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
            {
                hints.AddForbiddenZone(ShapeDistance.Circle(Module.PrimaryActor.Position, 6), NextCast);

                foreach (var p in Raid.WithoutSlot().Exclude(actor))
                    hints.AddForbiddenZone(ShapeDistance.Circle(p.Position, 0.5f), DateTime.MaxValue);
            }

            if (actor == Baiter && Module.FindComponent<P1Fireball>()?.Destination is { } dest && dest != default)
            {
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(dest, 11), NextCast.AddSeconds(1.2f * (4 - NumCasts)));
                hints.AddForbiddenZone(ShapeDistance.Circle(dest, 7), NextCast);
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => player == Baiter ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override void Update()
    {
        base.Update();

        if (Mode == BaitMode.Proximity)
            Baiter = Raid.WithoutSlot().Farthest(Module.PrimaryActor.Position);
    }
}
