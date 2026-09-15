namespace BossMod.Stormblood.Ultimate.UWU;

// TODO :implement hints...
class P1Mesohigh(BossModule module) : Components.CastCounter(module, AID.Mesohigh)
{
    private readonly IReadOnlyList<Actor> _sisters = module.Enemies(OID.GarudaSister);
    private const float _radius = 3;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var s in EnumerateTetherSources())
        {
            var tetherTarget = WorldState.Actors.Find(s.Tether.Target);
            if (tetherTarget != null)
            {
                Arena.AddLine(s.Position, tetherTarget.Position, ArenaColor.Danger);
                Arena.AddCircle(tetherTarget.Position, _radius, ArenaColor.Danger);
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _sisters.Any(s => s.Tether.Target == player.InstanceID) ? PlayerPriority.Danger : PlayerPriority.Normal;
    }

    private IEnumerable<Actor> EnumerateTetherSources()
    {
        foreach (var s in _sisters.Tethered(TetherID.Mesohigh))
            yield return s;
        if (Module.PrimaryActor.Tether.ID == (uint)TetherID.Mesohigh)
            yield return Module.PrimaryActor;
    }
}
