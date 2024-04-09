namespace BossMod.Endwalker.Savage.P7SAgdistis;

// TODO: improve!
class ForbiddenFruit5 : ForbiddenFruitCommon
{
    private IReadOnlyList<Actor> _towers;

    private const float _towerRadius = 5;

    public ForbiddenFruit5(BossModule module) : base(module, ActionID.MakeSpell(AID.Burst))
    {
        _towers = module.Enemies(OID.Tower);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var tetherSource = TetherSources[pcSlot];
        if (tetherSource != null)
            Arena.AddLine(tetherSource.Position, pc.Position, TetherColor(tetherSource));

        foreach (var tower in _towers)
            Arena.AddCircle(tower.Position, _towerRadius, tetherSource == null ? ArenaColor.Safe : ArenaColor.Danger);
    }
}
