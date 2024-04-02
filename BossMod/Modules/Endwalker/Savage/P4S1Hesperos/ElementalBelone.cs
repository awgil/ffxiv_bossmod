namespace BossMod.Endwalker.Savage.P4S1Hesperos;

// state related to elemental belone mechanic (3 of 4 corners exploding)
class ElementalBelone : BossComponent
{
    public bool Visible = false;
    private SettingTheScene.Element _safeElement;
    private List<WPos> _imminentExplodingCorners = new();

    public override void Init(BossModule module)
    {
        var assignments = module.FindComponent<SettingTheScene>()!;
        uint forbiddenCorners = 1; // 0 corresponds to 'unknown' corner
        foreach (var actor in module.WorldState.Actors.Where(a => a.OID == (uint)OID.Helper).Tethered(TetherID.Bloodrake))
            forbiddenCorners |= 1u << (int)assignments.FromPos(module, actor.Position);
        var safeCorner = (SettingTheScene.Corner)BitOperations.TrailingZeroCount(~forbiddenCorners);
        _safeElement = assignments.FindElement(safeCorner);
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_imminentExplodingCorners.Where(p => actor.Position.InRect(p, new WDir(1, 0), 10, 10, 10)).Any())
        {
            hints.Add($"GTFO from exploding square");
        }
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        hints.Add($"Safe square: {_safeElement}");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (Visible)
        {
            var assignments = module.FindComponent<SettingTheScene>()!;
            var safeCorner = assignments.Assignment(_safeElement);
            if (safeCorner != SettingTheScene.Corner.Unknown)
            {
                var p = module.Bounds.Center + 10 * assignments.Direction(safeCorner);
                arena.ZoneRect(p, new WDir(1, 0), 10, 10, 10, ArenaColor.SafeFromAOE);
            }
        }
        foreach (var p in _imminentExplodingCorners)
        {
            arena.ZoneRect(p, new WDir(1, 0), 10, 10, 10, ArenaColor.AOE);
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.PeriaktoiDangerAcid:
            case AID.PeriaktoiDangerLava:
            case AID.PeriaktoiDangerWell:
            case AID.PeriaktoiDangerLevinstrike:
                _imminentExplodingCorners.Add(caster.Position);
                break;
        }
    }
}
