namespace BossMod.Shadowbringers.Alliance.A32HanselGretel;

[SkipLocalsInit]
sealed class StrongerTogether(BossModule module) : BossComponent(module)
{
    private readonly A32HanselGretel bossmod = (A32HanselGretel)module;
    private bool strongerTogether;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.StrongerTogether)
        {
            strongerTogether = true;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.StrongerTogether)
        {
            strongerTogether = false;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (strongerTogether)
        {
            hints.Add($"Pull {bossmod.BossHansel?.Name} & {Module.PrimaryActor.Name} further apart!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (strongerTogether && bossmod.BossHansel is Actor hansel)
        {
            var primary = Module.PrimaryActor;
            var primaryID = primary.TargetID;
            var actorID = actor.InstanceID;
            var isPrimaryTarget = actorID == primaryID;
            if (isPrimaryTarget ^ actor.InstanceID == hansel.TargetID) // if actor got aggro from both bosses we can do nothing
            {
                var pos = isPrimaryTarget ? hansel.Position : primary.Position;
                hints.AddForbiddenZone(new SDCircle(pos, (pos - actor.Position).Length() + 2f), DateTime.MaxValue);
            }
        }
    }
}
