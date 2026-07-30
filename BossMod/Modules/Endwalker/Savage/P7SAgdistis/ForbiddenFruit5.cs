namespace BossMod.Endwalker.Savage.P7SAgdistis;

// TODO: improve!
class ForbiddenFruit5(BossModule module) : ForbiddenFruitCommon(module, (uint)AID.Burst)
{
    private readonly List<Actor> _towers = module.Enemies((uint)OID.Tower);

    private const float _towerRadius = 5;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var tetherSource = TetherSources[pcSlot];
        if (tetherSource != null)
            Arena.AddLine(tetherSource.Position, pc.Position, TetherColor(tetherSource));

        for (var i = 0; i < _towers.Count; ++i)
            Arena.ZoneCircleOutline(_towers[i].Position, _towerRadius, tetherSource == null ? Colors.Safe : 0);
    }
}
