namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

class HerosSundering : BossComponent
{
    private readonly Actor? _target;
    private BitMask _otherHit;

    private static readonly AOEShapeCone _aoeShape = new(40, 45.Degrees());

    public HerosSundering(BossModule module) : base(module)
    {
        _target = WorldState.Actors.Find(Module.PrimaryActor.CastInfo?.TargetID ?? 0);
        if (_target == null)
            ReportError($"Failed to determine target for heros sundering: {Module.PrimaryActor.CastInfo?.TargetID:X}");
    }

    public override void Update()
    {
        _otherHit.Reset();
        if (_target != null)
        {
            var dir = Angle.FromDirection(_target.Position - Module.PrimaryActor.Position);
            _otherHit = Raid.WithSlot().Exclude(_target).InShape(_aoeShape, Module.PrimaryActor.Position, dir).Mask();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
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

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_target != null)
            _aoeShape.Draw(Arena, Module.PrimaryActor.Position, Angle.FromDirection(_target.Position - Module.PrimaryActor.Position));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (pc == _target)
        {
            foreach (var (slot, player) in Raid.WithSlot().Exclude(_target))
                Arena.Actor(player, _otherHit[slot] ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
        }
        else
        {
            Arena.Actor(_target, ArenaColor.Danger);
        }
    }
}
