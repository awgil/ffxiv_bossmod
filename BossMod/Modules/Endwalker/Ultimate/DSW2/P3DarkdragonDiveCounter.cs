namespace BossMod.Endwalker.Ultimate.DSW2;

class P3DarkdragonDiveCounter : Components.GenericTowers
{
    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var numSoakers = (AID)spell.Action.ID switch
        {
            AID.DarkdragonDive1 => 1,
            AID.DarkdragonDive2 => 2,
            AID.DarkdragonDive3 => 3,
            AID.DarkdragonDive4 => 4,
            _ => 0
        };
        if (numSoakers == 0)
            return;

        Towers.Add(new(caster.Position, 5, numSoakers, numSoakers));
        if (Towers.Count == 4)
            InitAssignments(module);
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DarkdragonDive1 or AID.DarkdragonDive2 or AID.DarkdragonDive3 or AID.DarkdragonDive4)
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
    }

    // 0 = NW, then CW order
    private int ClassifyTower(BossModule module, WPos tower)
    {
        var offset = tower - module.Bounds.Center;
        return offset.Z > 0 ? (offset.X > 0 ? 2 : 3) : (offset.X > 0 ? 1 : 0);
    }

    private void InitAssignments(BossModule module)
    {
        int[] towerIndices = { -1, -1, -1, -1 };
        for (int i = 0; i < Towers.Count; ++i)
            towerIndices[ClassifyTower(module, Towers[i].Position)] = i;

        var config = Service.Config.Get<DSW2Config>();
        var assign = config.P3DarkdragonDiveCounterGroups.Resolve(module.Raid);
        foreach (var (slot, group) in assign)
        {
            var pos = group & 3;
            if (group < 4 && Towers[towerIndices[pos]].MinSoakers == 1)
            {
                // flex
                pos = FlexOrder(pos, config.P3DarkdragonDiveCounterPreferCCWFlex).First(p => Towers[towerIndices[p]].MinSoakers > 2);
            }

            ref var tower = ref Towers.AsSpan()[towerIndices[pos]];
            if (tower.ForbiddenSoakers.None())
                tower.ForbiddenSoakers = new(0xff);
            tower.ForbiddenSoakers.Clear(slot);
        }
    }

    private IEnumerable<int> FlexOrder(int starting, bool preferCCW)
    {
        if (preferCCW)
        {
            yield return (starting + 3) & 3;
            yield return (starting + 1) & 3;
        }
        else
        {
            yield return (starting + 1) & 3;
            yield return (starting + 3) & 3;
        }
        yield return (starting + 2) & 3;
    }
}
