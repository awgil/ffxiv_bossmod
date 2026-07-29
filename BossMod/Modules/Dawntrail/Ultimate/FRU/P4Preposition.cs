namespace BossMod.Dawntrail.Ultimate.FRU;

// boss can spawn either N or S from center
sealed class P4Preposition(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _boss = module.Enemies((uint)OID.UsurperOfFrostP4);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _boss.Count;
        for (var i = 0; i < count; ++i)
            hints.AddForbiddenZone(new SDInvertedCircle(_boss[i].Position, 8f), DateTime.MaxValue);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = _boss.Count;
        for (var i = 0; i < count; ++i)
            Arena.ZoneCircleOutline(_boss[i].Position, 1, Colors.Safe);
    }
}

// utility to draw hitbox around crystal, so that it's easier not to clip
sealed class P4FragmentOfFate(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _fragment = module.Enemies((uint)OID.FragmentOfFate);

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (playerSlot >= PartyState.MaxPartySize)
        {
            customColor = Colors.Object;
            return PlayerPriority.Danger;
        }
        return PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = _fragment.Count;
        for (var i = 0; i < count; ++i)
        {
            var f = _fragment[i];
            Arena.ZoneCircleOutline(f.Position, f.HitboxRadius, Colors.Object);
        }
    }
}
