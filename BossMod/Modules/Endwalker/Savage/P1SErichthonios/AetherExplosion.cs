namespace BossMod.Endwalker.Savage.P1SErichthonios;

// state related to aether explosion mechanics, done as part of aetherflails, aetherchain and shackles of time abilities
class AetherExplosion : BossComponent
{
    private enum Cell { None, Red, Blue }

    private Actor? _memberWithSOT = null; // if not null, then every update exploding cells are recalculated based on this raid member's position
    private Cell _explodingCells = Cell.None;

    private static readonly uint _colorSOTActor = 0xff8080ff;

    public bool SOTActive => _memberWithSOT != null;

    public override void Update(BossModule module)
    {
        if (_memberWithSOT != null)
            _explodingCells = CellFromOffset(_memberWithSOT.Position - module.Bounds.Center);
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (actor != _memberWithSOT && _explodingCells != Cell.None && _explodingCells == CellFromOffset(actor.Position - module.Bounds.Center))
        {
            hints.Add("Hit by aether explosion!");
        }
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_explodingCells == Cell.None || pc == _memberWithSOT)
            return; // nothing to draw

        if (module.Bounds is not ArenaBoundsCircle)
        {
            module.ReportError(this, "Trying to draw aether AOE when cells mode is not active...");
            return;
        }

        var start = _explodingCells == Cell.Blue ? 0.Degrees() : 45.Degrees();
        for (int i = 0; i < 4; ++i)
        {
            arena.ZoneCone(module.Bounds.Center, 0, P1S.InnerCircleRadius, start + 22.5f.Degrees(), 22.5f.Degrees(), ArenaColor.AOE);
            arena.ZoneCone(module.Bounds.Center, P1S.InnerCircleRadius, module.Bounds.HalfSize, start + 67.5f.Degrees(), 22.5f.Degrees(), ArenaColor.AOE);
            start += 90.Degrees();
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_memberWithSOT != pc)
            arena.Actor(_memberWithSOT, _colorSOTActor);
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.AetherExplosion:
                // we rely on parameter of an invisible status on boss to detect red/blue
                _explodingCells = status.Extra switch
                {
                    0x14C => Cell.Red,
                    0x14D => Cell.Blue,
                    _ => Cell.None
                };
                if (_explodingCells == Cell.None)
                    module.ReportError(this, $"Unexpected aether explosion param {status.Extra:X2}");
                if (_memberWithSOT != null)
                {
                    module.ReportError(this, "Unexpected forced explosion while SOT is active");
                    _memberWithSOT = null;
                }
                break;

            case SID.ShacklesOfTime:
                if (_memberWithSOT != null)
                    module.ReportError(this, "Unexpected ShacklesOfTime: another is already active!");
                _memberWithSOT = actor;
                _explodingCells = Cell.None;
                break;
        }
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.ShacklesOfTime:
                if (_memberWithSOT == actor)
                    _memberWithSOT = null;
                _explodingCells = Cell.None;
                break;
        }
    }

    private static Cell CellFromOffset(WDir offsetFromCenter)
    {
        var phi = Angle.FromDirection(offsetFromCenter) + 180.Degrees();
        int coneIndex = (int)(4 * phi.Rad / MathF.PI); // phi / (pi/4); range [0, 8]
        bool oddCone = (coneIndex & 1) != 0;
        bool outerCone =  offsetFromCenter.LengthSq() > P1S.InnerCircleRadius * P1S.InnerCircleRadius;
        return (oddCone == outerCone) ? Cell.Blue : Cell.Red; // outer odd = inner even = blue
    }
}
