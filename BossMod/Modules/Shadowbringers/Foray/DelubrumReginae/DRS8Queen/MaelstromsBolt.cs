namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

// TODO: show reflect hints, show stay under dome hints
class MaelstromsBolt(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.MaelstromsBoltAOE))
{
    private readonly IReadOnlyList<Actor> _ballLightnings = module.Enemies(OID.BallLightning);
    private readonly IReadOnlyList<Actor> _domes = module.Enemies(OID.ProtectiveDome);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var b in _ballLightnings.Where(b => !b.IsDead))
        {
            Arena.Actor(b, ArenaColor.Object, true);
            Arena.AddCircle(b.Position, 8, ArenaColor.Object);
        }
        foreach (var d in _domes)
        {
            Arena.AddCircle(d.Position, 8, ArenaColor.Safe);
        }
    }
}
