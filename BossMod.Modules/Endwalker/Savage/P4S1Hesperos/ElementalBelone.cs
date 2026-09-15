namespace BossMod.Endwalker.Savage.P4S1Hesperos;

// state related to elemental belone mechanic (3 of 4 corners exploding)
class ElementalBelone : BossComponent
{
    public bool Visible;
    private readonly SettingTheScene.Element _safeElement;
    private readonly List<WPos> _imminentExplodingCorners = [];

    public ElementalBelone(BossModule module) : base(module)
    {
        var assignments = module.FindComponent<SettingTheScene>()!;
        uint forbiddenCorners = 1; // 0 corresponds to 'unknown' corner
        foreach (var actor in WorldState.Actors.Where(a => a.OID == (uint)OID.Helper).Tethered(TetherID.Bloodrake))
            forbiddenCorners |= 1u << (int)assignments.FromPos(actor.Position);
        var safeCorner = (SettingTheScene.Corner)BitOperations.TrailingZeroCount(~forbiddenCorners);
        _safeElement = assignments.FindElement(safeCorner);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_imminentExplodingCorners.Any(p => actor.Position.InRect(p, new WDir(1, 0), 10, 10, 10)))
        {
            hints.Add($"GTFO from exploding square");
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"Safe square: {_safeElement}");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Visible)
        {
            var assignments = Module.FindComponent<SettingTheScene>()!;
            var safeCorner = assignments.Assignment(_safeElement);
            if (safeCorner != SettingTheScene.Corner.Unknown)
            {
                var p = Module.Center + 10 * assignments.Direction(safeCorner);
                Arena.ZoneRect(p, new WDir(1, 0), 10, 10, 10, ArenaColor.SafeFromAOE);
            }
        }
        foreach (var p in _imminentExplodingCorners)
        {
            Arena.ZoneRect(p, new WDir(1, 0), 10, 10, 10, ArenaColor.AOE);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
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
