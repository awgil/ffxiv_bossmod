namespace BossMod.Endwalker.Savage.P7SAgdistis;

// TODO: improve!
class ForbiddenFruit5(BossModule module) : ForbiddenFruitCommon(module, ActionID.MakeSpell(AID.Burst))
{
    private readonly IReadOnlyList<Actor> _towers = module.Enemies(OID.Tower);

    private const float _towerRadius = 5;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var tetherSource = TetherSources[pcSlot];
        if (tetherSource != null)
            Arena.AddLine(tetherSource.Position, pc.Position, TetherColor(tetherSource));

        foreach (var tower in _towers)
            Arena.AddCircle(tower.Position, _towerRadius, tetherSource == null ? ArenaColor.Safe : ArenaColor.Danger);
    }
}
