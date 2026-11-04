namespace BossMod.Stormblood.Ultimate.UWU;

class P4UltimateSuppression : BossComponent
{
    bool RazorsActive;
    BitMask Eruptions;
    BitMask LightPillars;
    int NumLightPillars;
    int NumEruptions;

    enum State
    {
        Start,
        EruptionToMistral,
        MistralToFeather,
        FeatherToLandslide,
        Landslide
    }

    State CurrentState = State.Start;

    public P4UltimateSuppression(BossModule module) : base(module)
    {
        LightPillars = Raid.WithSlot().WhereActor(a => a.Role != Role.Tank).Mask();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.EruptionAOE && CurrentState == State.Start)
            CurrentState++;

        if ((AID)spell.Action.ID == AID.LandslideBoss && CurrentState == State.FeatherToLandslide)
            CurrentState++;

        if ((AID)spell.Action.ID == AID.LightPillarAOE)
        {
            NumLightPillars++;
            if (NumLightPillars >= 6)
                LightPillars.Reset();
        }

        if ((AID)spell.Action.ID == AID.EruptionAOE)
        {
            NumEruptions++;
            if (NumEruptions >= 6)
                RazorsActive = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MistralSongAdds && CurrentState == State.EruptionToMistral)
            CurrentState++;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.MistralSong)
        {
            LightPillars.Clear(Raid.FindSlot(targetID));
            if (LightPillars.NumSetBits() == 4 && Module.FindComponent<P5Eruption>() is { } eruption)
            {
                Eruptions = eruption.Targets;
                LightPillars &= ~Eruptions;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (((UWU)Module).Ultima() is { IsTargetable: true })
        {
            if (actor.Role != Role.Tank)
                hints.GoalZones.Add(AIHints.GoalProximity(new(86, 86), 12, 0.2f));
            return;
        }

        if (RazorsActive)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 15));

        switch (CurrentState)
        {
            case State.Start:
                if (assignment == PartyRolesConfig.Assignment.MT)
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(94, 108), 1));
                else if (assignment == PartyRolesConfig.Assignment.OT)
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(96, 108), 1));
                else
                {
                    var myOrder = Raid.WithoutSlot(includeDead: true).Where(a => a.Role != Role.Tank).OrderBy(o => o.InstanceID).Index().First(t => t.Item == actor).Index;
                    var myAngle = -(125 + 5 * myOrder).Degrees();
                    hints.AddForbiddenZone(ShapeDistance.PrecisePosition(Arena.Center + myAngle.ToDirection() * 18, new(0, 1), 0.5f, actor.Position, 0.1f));
                }
                break;

            case State.MistralToFeather:
                // light pillar player spread away
                if (LightPillars[slot])
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(92.5f, 109.5f), 1));
                else if (!Eruptions[slot])
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(95.5f, 107.5f), 1));
                break;

            case State.FeatherToLandslide:
                if (LightPillars[slot] && NumLightPillars < 3)
                {
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(97, 92), 1), DateTime.MaxValue);
                    hints.AddForbiddenZone(ShapeDistance.HalfPlane(Arena.Center, (-135.Degrees()).ToDirection()));
                }
                else
                    hints.GoalZones.Add(AIHints.GoalProximity(new(106.7f, 100), 10, 1));
                break;
        }
    }

    public override void Update()
    {
        if (CurrentState == State.MistralToFeather && Module.FindComponent<P1FeatherRain>() is { CastsPredicted: true })
            CurrentState++;
    }
}
