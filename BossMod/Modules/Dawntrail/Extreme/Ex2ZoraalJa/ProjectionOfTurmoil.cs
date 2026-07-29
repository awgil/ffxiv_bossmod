namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

// TODO: consider improving this somehow? too many ways to resolve...
sealed class ProjectionOfTurmoil(BossModule module) : Components.CastCounter(module, (uint)AID.MightOfVollok)
{
    private readonly List<Actor> _line = module.Enemies((uint)OID.ProjectionOfTurmoil);
    private BitMask _targets;

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _targets[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Normal;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var slot in _targets.SetBits())
        {
            var actor = Raid[slot];
            if (actor != null)
                Arena.ZoneCircleOutline(actor.Position, 8f, Colors.Safe);
        }
        var count = _line.Count;
        for (var i = 0; i < count; ++i)
        {
            var l = _line[i];
            var off = new WDir(28.28427f - Math.Abs(l.PosRot.Z - Arena.Center.Z), default);
            Arena.AddLine(l.Position - off, l.Position + off);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Projection)
            _targets.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Projection)
            _targets.Clear(Raid.FindSlot(actor.InstanceID));
    }
}
