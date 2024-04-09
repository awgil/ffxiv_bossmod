namespace BossMod.Stormblood.Ultimate.UWU;

// TODO :implement hints...
class P1Mesohigh : Components.CastCounter
{
    private IReadOnlyList<Actor> _sisters = ActorEnumeration.EmptyList;
    private static readonly float _radius = 3;

    public P1Mesohigh() : base(ActionID.MakeSpell(AID.Mesohigh)) { }

    public override void Init(BossModule module)
    {
        _sisters = module.Enemies(OID.GarudaSister);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var s in EnumerateTetherSources(module))
        {
            var tetherTarget = WorldState.Actors.Find(s.Tether.Target);
            if (tetherTarget != null)
            {
                arena.AddLine(s.Position, tetherTarget.Position, ArenaColor.Danger);
                arena.AddCircle(tetherTarget.Position, _radius, ArenaColor.Danger);
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _sisters.Any(s => s.Tether.Target == player.InstanceID) ? PlayerPriority.Danger : PlayerPriority.Normal;
    }

    private IEnumerable<Actor> EnumerateTetherSources(BossModule module)
    {
        foreach (var s in _sisters.Tethered(TetherID.Mesohigh))
            yield return s;
        if (Module.PrimaryActor.Tether.ID == (uint)TetherID.Mesohigh)
            yield return Module.PrimaryActor;
    }
}
