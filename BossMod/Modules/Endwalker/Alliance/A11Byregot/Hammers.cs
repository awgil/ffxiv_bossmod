namespace BossMod.Endwalker.Alliance.A11Byregot;

class HammersCells : Components.GenericAOEs
{
    public bool Active { get; private set; }
    public bool MovementPending { get; private set; }
    private int[] _lineOffset = new int[5];
    private int[] _lineMovement = new int[5];

    private static readonly AOEShapeRect _shape = new(5, 5, 5);

    public HammersCells() : base(ActionID.MakeSpell(AID.DestroySideTiles), "GTFO from dangerous cell!") { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (!Active)
            yield break;

        for (int z = -2; z <= 2; ++z)
        {
            for (int x = -2; x <= 2; ++x)
            {
                if (CellDangerous(x, z, true))
                    yield return new(_shape, CellCenter(module, x, z), color: ArenaColor.AOE);
                else if (CellDangerous(x, z, false))
                    yield return new(_shape, CellCenter(module, x, z), color: ArenaColor.SafeFromAOE);
            }
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (!Active)
            return;

        arena.AddLine(module.Bounds.Center + new WDir(-15, -25), module.Bounds.Center + new WDir(-15, +25), ArenaColor.Border);
        arena.AddLine(module.Bounds.Center + new WDir( -5, -25), module.Bounds.Center + new WDir( -5, +25), ArenaColor.Border);
        arena.AddLine(module.Bounds.Center + new WDir( +5, -25), module.Bounds.Center + new WDir( +5, +25), ArenaColor.Border);
        arena.AddLine(module.Bounds.Center + new WDir(+15, -25), module.Bounds.Center + new WDir(+15, +25), ArenaColor.Border);
        arena.AddLine(module.Bounds.Center + new WDir(-25, -15), module.Bounds.Center + new WDir(+25, -15), ArenaColor.Border);
        arena.AddLine(module.Bounds.Center + new WDir(-25,  -5), module.Bounds.Center + new WDir(+25,  -5), ArenaColor.Border);
        arena.AddLine(module.Bounds.Center + new WDir(-25,  +5), module.Bounds.Center + new WDir(+25,  +5), ArenaColor.Border);
        arena.AddLine(module.Bounds.Center + new WDir(-25, +15), module.Bounds.Center + new WDir(+25, +15), ArenaColor.Border);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Active = true;
    }

    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        if (index is >= 7 and <= 11)
        {
            int i = index - 7;
            (_lineOffset[i], _lineMovement[i]) = state switch
            {
                0x00020001 => ( 0, +1),
                0x08000400 => (-1, +1),
                0x00800040 => ( 0, -1),
                0x80004000 => (+1, -1),
                _ => (_lineOffset[i], 0),
            };
            MovementPending = true;
            if (_lineMovement[i] == 0)
                module.ReportError(this, $"Unexpected env-control {i}={state:X}, offset={_lineOffset[i]}");
        }
        else if (index == 26)
        {
            MovementPending = false;
            for (int i = 0; i < 5; ++i)
            {
                _lineOffset[i] += _lineMovement[i];
                _lineMovement[i] = 0;
            }
        }
        else if (index == 79 && state == 0x00080004)
        {
            Active = false;
            Array.Fill(_lineOffset, 0);
            Array.Fill(_lineMovement, 0);
        }
    }

    private WPos CellCenter(BossModule module, int x, int z) => module.Bounds.Center + 10 * new WDir(x, z);

    private bool CellDangerous(int x, int z, bool future)
    {
        int off = _lineOffset[z + 2];
        if (future)
            off += _lineMovement[z + 2];
        return Math.Abs(x - off) > 1;
    }
}

class HammersLevinforge : Components.SelfTargetedAOEs
{
    public HammersLevinforge() : base(ActionID.MakeSpell(AID.Levinforge), new AOEShapeRect(50, 5)) { }
}

class HammersSpire : Components.SelfTargetedAOEs
{
    public HammersSpire() : base(ActionID.MakeSpell(AID.ByregotSpire), new AOEShapeRect(50, 15)) { }
}
