namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

// TODO: show reflect hints, show stay under dome hints
class MaelstromsBolt : Components.CastCounter
{
    private IReadOnlyList<Actor> _ballLightnings = ActorEnumeration.EmptyList;
    private IReadOnlyList<Actor> _domes = ActorEnumeration.EmptyList;

    public MaelstromsBolt() : base(ActionID.MakeSpell(AID.MaelstromsBoltAOE)) { }

    public override void Init(BossModule module)
    {
        _ballLightnings = module.Enemies(OID.BallLightning);
        _domes = module.Enemies(OID.ProtectiveDome);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var b in _ballLightnings.Where(b => !b.IsDead))
        {
            arena.Actor(b, ArenaColor.Object, true);
            arena.AddCircle(b.Position, 8, ArenaColor.Object);
        }
        foreach (var d in _domes)
        {
            arena.AddCircle(d.Position, 8, ArenaColor.Safe);
        }
    }
}
