namespace BossMod.Stormblood.Ultimate.UCOB;

class P3AethericProfusion : Components.CastCounter
{
    public bool Active;
    private IReadOnlyList<Actor> _neurolinks = ActorEnumeration.EmptyList;

    public P3AethericProfusion() : base(ActionID.MakeSpell(AID.AethericProfusion)) { }

    public override void Init(BossModule module)
    {
        _neurolinks = module.Enemies(OID.Neurolink);
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (Active)
            hints.Add("Go to neurolink!", !_neurolinks.InRadius(actor.Position, 2).Any());
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var neurolink in _neurolinks)
            arena.AddCircle(neurolink.Position, 2, ArenaColor.Safe);
    }
}
