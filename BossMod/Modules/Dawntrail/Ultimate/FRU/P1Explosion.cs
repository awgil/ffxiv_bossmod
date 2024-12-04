namespace BossMod.Dawntrail.Ultimate.FRU;

// TODO: non-fixed conga?
class P1Explosion(BossModule module) : Components.GenericTowers(module)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var numSoakers = (AID)spell.Action.ID switch
        {
            AID.Explosion11 or AID.Explosion12 => 1,
            AID.Explosion21 or AID.Explosion22 => 2,
            AID.Explosion31 or AID.Explosion32 => 3,
            AID.Explosion41 or AID.Explosion42 => 4,
            _ => 0
        };
        if (numSoakers != 0)
        {
            Towers.Add(new(caster.Position, 4, numSoakers, numSoakers, default, Module.CastFinishAt(spell)));
            if (Towers.Count == 3)
                InitAssignments();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Explosion11 or AID.Explosion12 or AID.Explosion21 or AID.Explosion22 or AID.Explosion31 or AID.Explosion32 or AID.Explosion41 or AID.Explosion42)
        {
            ++NumCasts;
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
        }
    }

    private void InitAssignments()
    {
        Towers.SortBy(t => t.Position.Z);
        if (Towers.Count != 3 || Towers.Sum(t => t.MinSoakers) != 6)
        {
            ReportError($"Unexpected tower state");
            return;
        }

        Span<int> slotByGroup = [-1, -1, -1, -1, -1, -1, -1, -1];
        foreach (var (slot, group) in _config.P1ExplosionsAssignment.Resolve(Raid))
            slotByGroup[group] = slot;
        if (slotByGroup.Contains(-1))
            return;
        var nextFlex = 5;
        for (int i = 0; i < 3; ++i)
        {
            ref var tower = ref Towers.Ref(i);
            tower.ForbiddenSoakers.Raw = 0xFF;
            tower.ForbiddenSoakers.Clear(slotByGroup[i + 2]); // fixed assignment
            for (int j = 1; j < tower.MinSoakers; ++j)
                tower.ForbiddenSoakers.Clear(slotByGroup[nextFlex++]);
        }
    }
}
