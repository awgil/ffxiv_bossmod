namespace BossMod.Stormblood.Ultimate.UCOB;

class P3AethericProfusion(BossModule module) : Components.CastCounter(module, (uint)AID.AethericProfusion)
{
    public bool Active;
    private readonly List<Actor> _neurolinks = module.Enemies((uint)OID.Neurolink);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Active)
            hints.Add("Go to neurolink!", !_neurolinks.InRadius(actor.Position, 2f).Any());
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var neurolink in _neurolinks)
            Arena.ZoneCircleOutline(neurolink.Position, 2, Colors.Safe);
    }
}
