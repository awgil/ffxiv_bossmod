namespace BossMod.Dawntrail.Ultimate.FRU;

// boss can spawn either N or S from center
class P4Preposition(BossModule module) : BossComponent(module)
{
    private readonly IReadOnlyList<Actor> _boss = module.Enemies(OID.UsurperOfFrostP4);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var b in _boss)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(b.Position, 8), DateTime.MaxValue);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var b in _boss)
            Arena.AddCircle(b.Position, 1, ArenaColor.Safe);
    }
}
