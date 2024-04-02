namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class Turrets : Components.Knockback
{
    private Actor?[] _turrets = new Actor?[8]; // pairs in order of activation
    private DateTime _activation;
    private BitMask _forbidden;

    private static readonly float _distance = 17;
    private static readonly AOEShapeRect _shape = new(50, 2.5f);

    public Turrets() : base(ActionID.MakeSpell(AID.PealOfCondemnation), true, 1) { } // TODO: verify whether it ignores immunes

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        foreach (var t in ImminentTurretsWithTargets(module))
            if (t.source != null && t.target != null)
                yield return new(t.source.Position, _distance, _activation, _shape, Angle.FromDirection(t.target.Position - t.source.Position));
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        int inCount = 0;
        bool outOfBounds = false;
        foreach (var t in ImminentTurretsWithTargets(module))
        {
            if (t.source == null || t.target == null || !_shape.Check(actor.Position, t.source.Position, Angle.FromDirection(t.target.Position - t.source.Position)))
                continue; // not in aoe

            ++inCount;
            var dir = actor.Position != t.source.Position ? (actor.Position - t.source.Position).Normalized() : new();
            var to = actor.Position + _distance * dir;
            outOfBounds |= !Border.InsideMainPlatform(module, to);
        }

        if (inCount > 1)
            hints.Add("GTFO from one of the knockbacks!");
        else if (inCount > 0 && _forbidden[slot])
            hints.Add("GTFO from knockback!");
        else if (outOfBounds)
            hints.Add("About to be knocked off platform!");
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return ImminentTurretsWithTargets(module).Any(t => t.target == player) ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var t in ImminentTurretsWithTargets(module))
        {
            arena.Actor(t.source, ArenaColor.Enemy, true);
            if (t.source != null && t.target != null)
                _shape.Outline(arena, t.source.Position, Angle.FromDirection(t.target.Position - t.source.Position));
        }

        foreach (var t in FutureTurrets())
            arena.Actor(t, ArenaColor.Object, true);

        base.DrawArenaForeground(module, pcSlot, pc, arena);
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DarkResistanceDown)
            _forbidden.Set(module.Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        var order = (IconID)iconID switch
        {
            IconID.Order1 => 0,
            IconID.Order2 => 1,
            IconID.Order3 => 2,
            IconID.Order4 => 3,
            _ => -1
        };
        if (order < 0)
            return;

        _activation = module.WorldState.CurrentTime.AddSeconds(8.1f);
        if (_turrets[order * 2] == null)
            _turrets[order * 2] = actor;
        else if (_turrets[order * 2 + 1] == null)
            _turrets[order * 2 + 1] = actor;
        else
            module.ReportError(this, $"More than 2 turrets of order {order}");
    }

    private IEnumerable<(Actor? source, Actor? target)> ImminentTurretsWithTargets(BossModule module) => _turrets.Skip(NumCasts).Take(2).Select(t => (t, module.WorldState.Actors.Find(t?.TargetID ?? 0)));
    private IEnumerable<Actor?> FutureTurrets() => _turrets.Skip(NumCasts + 2).Take(2);
}
