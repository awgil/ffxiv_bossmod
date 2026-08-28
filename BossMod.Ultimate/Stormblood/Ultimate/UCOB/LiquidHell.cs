namespace BossMod.Stormblood.Ultimate.UCOB;

class LiquidHell(BossModule module) : Components.VoidzoneAtCastTarget(module, 6, AID.LiquidHell, OID.VoidzoneLiquidHell, 1.3f, activationDelay: 1.8f)
{
    public enum BaitMode
    {
        None,
        Proximity,
        Random
    }

    BaitMode Mode;
    DateTime NextCast;

    Actor? Baiter;

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

            if (Mode == BaitMode.Random && Baiter == null)
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
                hints.AddForbiddenZone(ShapeDistance.InvertedCone(Module.PrimaryActor.Position, 50, Module.PrimaryActor.DirectionTo(Arena.Center).ToAngle(), 45.Degrees()), NextCast);

                // don't drop on neurolinks
                foreach (var nl in Module.Enemies(OID.Neurolink))
                    hints.AddForbiddenZone(ShapeDistance.Circle(nl.Position, 7), NextCast);
            }
            else
                hints.GoalZones.Add(hints.GoalSingleTarget(Module.PrimaryActor.Position, 16, 0.1f));
        }

        if (Mode == BaitMode.Random)
        {
            if (actor == Baiter && Module.FindComponent<P1Fireball>()?.Destination is { } dest && dest != default)
            {
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(dest, 16), NextCast);
                hints.AddForbiddenZone(ShapeDistance.Circle(dest, 7), NextCast);
            }

            if (Baiter != null && Baiter != actor)
                hints.GoalZones.Add(hints.GoalSingleTarget(Module.PrimaryActor, 5, 0.5f));
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

class P1LiquidHell : LiquidHell
{
    public P1LiquidHell(BossModule module) : base(module) { KeepOnPhaseChange = true; }
}
