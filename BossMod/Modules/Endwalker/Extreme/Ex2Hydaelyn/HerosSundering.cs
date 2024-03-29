namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

class HerosSundering : BossComponent
{
    private Actor? _target;
    private BitMask _otherHit;

    private static readonly AOEShapeCone _aoeShape = new(40, 45.Degrees());

    public override void Init(BossModule module)
    {
        _target = module.WorldState.Actors.Find(module.PrimaryActor.CastInfo?.TargetID ?? 0);
        if (_target == null)
            module.ReportError(this, $"Failed to determine target for heros sundering: {module.PrimaryActor.CastInfo?.TargetID:X}");
    }

    public override void Update(BossModule module)
    {
        _otherHit.Reset();
        if (_target != null)
        {
            var dir = Angle.FromDirection(_target.Position - module.PrimaryActor.Position);
            _otherHit = module.Raid.WithSlot().Exclude(_target).InShape(_aoeShape, module.PrimaryActor.Position, dir).Mask();
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_target == null)
            return;

        if (actor == _target)
        {
            if (_otherHit.Any())
                hints.Add("Turn boss away from raid!");
        }
        else
        {
            if (_otherHit[slot])
                hints.Add("GTFO from tankbuster aoe!");
        }
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_target != null)
            _aoeShape.Draw(arena, module.PrimaryActor.Position, Angle.FromDirection(_target.Position - module.PrimaryActor.Position));
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (pc == _target)
        {
            foreach (var (slot, player) in module.Raid.WithSlot().Exclude(_target))
                arena.Actor(player, _otherHit[slot] ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
        }
        else
        {
            arena.Actor(_target, ArenaColor.Danger);
        }
    }
}
