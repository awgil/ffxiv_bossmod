namespace BossMod.Endwalker.Ultimate.DSW2;

class P7MornAfahsEdge(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MornAfahsEdgeFirst1 or AID.MornAfahsEdgeFirst2 or AID.MornAfahsEdgeFirst3)
            Towers.Add(new(caster.Position, 4));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.MornAfahsEdgeFirst1 or AID.MornAfahsEdgeFirst2 or AID.MornAfahsEdgeFirst3 or AID.MornAfahsEdgeRest)
            ++NumCasts;
    }
}
