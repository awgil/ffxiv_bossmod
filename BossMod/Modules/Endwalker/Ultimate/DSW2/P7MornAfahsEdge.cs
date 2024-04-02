namespace BossMod.Endwalker.Ultimate.DSW2;

class P7MornAfahsEdge : Components.GenericTowers
{
    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MornAfahsEdgeFirst1 or AID.MornAfahsEdgeFirst2 or AID.MornAfahsEdgeFirst3)
            Towers.Add(new(caster.Position, 4));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.MornAfahsEdgeFirst1 or AID.MornAfahsEdgeFirst2 or AID.MornAfahsEdgeFirst3 or AID.MornAfahsEdgeRest)
            ++NumCasts;
    }
}
