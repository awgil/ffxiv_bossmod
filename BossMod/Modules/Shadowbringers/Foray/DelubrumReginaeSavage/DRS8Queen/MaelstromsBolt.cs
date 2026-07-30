namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

// TODO: show reflect hints, show stay under dome hints
sealed class MaelstromsBolt(BossModule module) : Components.CastCounter(module, (uint)AID.MaelstromsBoltAOE)
{
    private readonly List<Actor> _ballLightnings = module.Enemies((uint)OID.BallLightning);
    private readonly List<Actor> _domes = module.Enemies((uint)OID.ProtectiveDome);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var b in _ballLightnings.Where(b => !b.IsDead))
        {
            Arena.Actor(b, Colors.Object, true);
            Arena.ZoneCircleOutline(b.Position, 8, Colors.Object);
        }
        for (var i = 0; i < _domes.Count; ++i)
            Arena.ZoneCircleOutline(_domes[i].Position, 8, Colors.Safe);
    }
}
