namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

// TODO: consider improving this somehow? too many ways to resolve...
class ProjectionOfTurmoil(BossModule module) : Components.CastCounter(module, AID.MightOfVollok)
{
    private readonly IReadOnlyList<Actor> _line = module.Enemies(OID.ProjectionOfTurmoil);
    private BitMask _targets;

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _targets[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Normal;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var slot in _targets.SetBits())
        {
            var actor = Raid[slot];
            if (actor != null)
                Arena.AddCircle(actor.Position, 8, ArenaColor.Safe);
        }
        foreach (var l in _line)
        {
            var off = new WDir(28.28427f - Math.Abs(l.Position.Z - Module.Center.Z), 0);
            Arena.AddLine(l.Position - off, l.Position + off, ArenaColor.Danger);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Projection)
            _targets.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Projection)
            _targets.Clear(Raid.FindSlot(actor.InstanceID));
    }
}
