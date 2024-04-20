namespace BossMod.Stormblood.Ultimate.UCOB;

class P3AethericProfusion(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.AethericProfusion))
{
    public bool Active;
    private readonly IReadOnlyList<Actor> _neurolinks = module.Enemies(OID.Neurolink);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Active)
            hints.Add("Go to neurolink!", !_neurolinks.InRadius(actor.Position, 2).Any());
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var neurolink in _neurolinks)
            Arena.AddCircle(neurolink.Position, 2, ArenaColor.Safe);
    }
}
