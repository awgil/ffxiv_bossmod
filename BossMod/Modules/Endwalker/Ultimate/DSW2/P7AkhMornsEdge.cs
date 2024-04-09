namespace BossMod.Endwalker.Ultimate.DSW2;

// TODO: assignments?
class P7AkhMornsEdge(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AkhMornsEdgeAOEFirstNormal1 or AID.AkhMornsEdgeAOEFirstNormal2 or AID.AkhMornsEdgeAOEFirstTanks)
            Towers.Add(new(caster.Position, 4, 1, 6));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AkhMornsEdgeAOEFirstTanks or AID.AkhMornsEdgeAOERestTanks)
            ++NumCasts;
    }
}
