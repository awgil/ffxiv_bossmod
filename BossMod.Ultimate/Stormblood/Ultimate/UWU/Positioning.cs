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
        Boss = Module.PrimaryActor;
        KeepOnPhaseChange = true;
    }

    public Actor? Boss;
    public (WPos, float)? DesiredPosition
    {
        get;
        set
        {
            field = value;
            Priority = 0.1f;
        }
    }
    public float Priority = 0.1f;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (DesiredPosition != null)
        {
            hints.GoalZones.Add(AIHints.GoalSingleTarget(DesiredPosition.Value.Item1, DesiredPosition.Value.Item2, Priority));
            return;
        }

        if (Boss is not { } boss)
            return;

        hints.GoalZones.Add(AIHints.GoalSingleTarget(boss.Position, boss.HitboxRadius + actor.HitboxRadius + 3, 0.1f));
    }
}
