namespace BossMod.Dawntrail.Alliance.A22OmegaTheOne;

class GuidedMissile(BossModule module) : Components.StandardAOEs(module, AID.GuidedMissile, 6);
class GuidedMissileBait(BossModule module) : Components.CastCounter(module, AID.GuidedMissile)
{
    private BitMask _targetsMask;

    record struct Bait(Actor Target, WDir Offset, DateTime Activation)
    {
        public readonly bool Clips(Actor a) => a.Position.InCircle(Target.Position + Offset, 6);
    }

    private readonly List<Bait> _baits = [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.BaitEast:
                AddBait(actor, new(5, 0));
                break;
            case IconID.BaitWest:
                AddBait(actor, new(-5, 0));
                break;
            case IconID.BaitSouth:
                AddBait(actor, new(0, 5));
                break;
            case IconID.BaitNorth:
                AddBait(actor, new(0, -5));
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _baits.Clear();
            _targetsMask.Reset();
        }
    }

    private void AddBait(Actor actor, WDir dir)
    {
        _targetsMask.Set(Raid.FindSlot(actor.InstanceID));
        _baits.Add(new(actor, dir, WorldState.FutureTime(9.8f)));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_baits.FirstOrNull(b => b.Target == actor) is { } thisBait)
        {
            if (Raid.WithoutSlot().Exclude(actor).Any(thisBait.Clips))
                hints.Add("Bait away from party!");
        }

        if (_baits.Any(b => b.Target != actor && b.Clips(actor)))
            hints.Add("GTFO from baited AOE!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_baits.FirstOrNull(b => b.Target == actor) is { } thisBait)
        {
            List<Func<WPos, bool>> selfShape = [];
            foreach (var r in Raid.WithoutSlot().Exclude(actor))
                selfShape.Add(ShapeContains.Circle(r.Position - thisBait.Offset, 6));

            if (selfShape.Count > 0)
                hints.AddForbiddenZone(ShapeContains.Union(selfShape), thisBait.Activation);
        }

        List<Func<WPos, bool>> othersShape = [];
        foreach (var b in _baits.Where(b => b.Target != actor))
            othersShape.Add(ShapeContains.Circle(b.Target.Position + b.Offset, 6));

        if (othersShape.Count > 0)
            hints.AddForbiddenZone(ShapeContains.Union(othersShape), _baits[0].Activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var b in _baits)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddCircle(b.Target.Position + b.Offset, 6, 0xFF000000, 2);
            Arena.AddCircle(b.Target.Position + b.Offset, 6, ArenaColor.Danger);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => _targetsMask[playerSlot] ? PlayerPriority.Danger : base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
}
