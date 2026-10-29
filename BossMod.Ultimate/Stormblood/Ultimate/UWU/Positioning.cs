namespace BossMod.Stormblood.Ultimate.UWU;

class BossPositioning : BossComponent
{
    public Actor? Boss;
    public WPos? DesiredPosition;
    public Angle? DesiredRotation = 180.Degrees();

    public BossPositioning(BossModule module) : base(module)
    {
        Boss = module.PrimaryActor;
        DesiredPosition = module.Arena.Center;
        KeepOnPhaseChange = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (hints.FindEnemy(Boss) is { } target)
        {
            target.DesiredPosition = DesiredPosition;
            target.DesiredRotation = DesiredRotation;

            if (target.Actor.PendingHPRatio < 0.02f && target.Actor.FindStatus(SID.Woken) == null)
                target.Priority = AIHints.Enemy.PriorityForbidden;
        }
    }

    public void Clear()
    {
        DesiredPosition = null;
        DesiredRotation = null;
    }

    public void P2MoveToIntercard()
    {
        if (DesiredPosition is not { } pos || Boss == null)
        {
            ReportError("Trying to set new Ifrit goal position, but component state is wrong, doing nothing");
            return;
        }

        var destAngle = (MathF.Ceiling((pos - Arena.Center).ToAngle().Deg / 90) * 90 - 45).Degrees();
        DesiredPosition = Arena.Center + destAngle.ToDirection() * 11;
        DesiredRotation = destAngle;
    }
}

class PartyPositioning : BossComponent
{
    public PartyPositioning(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
        _lastTitan = module.Arena.Center;
    }

    int _numGeocrushes;
    int _numUltimas;
    int _numTankPurges;
    WPos _lastTitan;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Geocrush2)
            _lastTitan = caster.Position;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Geocrush1:
                _numGeocrushes++;
                break;

            case AID.Tumult:
                _lastTitan = Arena.Center;
                break;

            case AID.Ultima:
                _numUltimas++;
                break;
            case AID.TankPurge:
                _numTankPurges++;
                break;
        }
        if ((AID)spell.Action.ID == AID.Geocrush1)
            _numGeocrushes++;

        if ((AID)spell.Action.ID == AID.Tumult)
            _lastTitan = Arena.Center;

        if ((AID)spell.Action.ID == AID.TankPurge)
            _numTankPurges++;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var uwu = (UWU)Module;

        switch (Module.StateMachine.ActivePhaseIndex)
        {
            // garuda
            case 0:
                if (Module.PrimaryActor.IsTargetable)
                    return;

                hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center, Module.PrimaryActor.HitboxRadius + 3 + actor.HitboxRadius));
                break;

            // ifrit
            case 1:
                // pre dash/plume start, we haven't picked an ifrit yet (there are 4 identical ones in the object table)
                if (uwu.Ifrit() is not { } ifrit)
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 1), DateTime.MaxValue);
                else if (!ifrit.IsTargetable)
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center, ifrit.HitboxRadius + 3 + actor.HitboxRadius));
                break;

            // titan
            case 2:
                if (uwu.Titan() is not { } titan)
                    return;

                // everyone should stack together for the first proximity raidwide so healers don't get fucked
                if (_numGeocrushes == 0 && Module.FindComponent<BossPositioning>() is { DesiredRotation: { } relNorth } && !titan.IsTargetable)
                {
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center + relNorth.ToDirection() * 20, 5));
                    return;
                }

                // drawing much smaller than max melee so i can avoid writing extra code to make the party stack mid before cardinal geocrush - not enough time to react just using the castbar and we can't tell what cardinal he's going to
                if (!titan.IsTargetable)
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(_lastTitan, 4));

                // just stay near titan tbh all of these mechanics are annoying
                if (titan.IsTargetable)
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(titan.Position, titan.HitboxRadius + 3 + actor.HitboxRadius, 0.1f));
                break;

            case 3:
                if (uwu.Ultima() is not { } ultima)
                    return;

                if (_numTankPurges == 0 && _numUltimas == 1 && !ultima.IsTargetable)
                    hints.GoalZones.Add(AIHints.GoalSingleTarget(new(100, 90), ultima.HitboxRadius + 3 + actor.HitboxRadius));
                break;
        }
    }
}
