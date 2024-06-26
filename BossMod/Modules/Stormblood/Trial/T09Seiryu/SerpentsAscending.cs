namespace BossMod.Stormblood.Trial.T09Seiryu;

class SerpentAscending(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Towers)
            Towers.Add(new(actor.Position, 3, 1, 1, activation: Module.WorldState.FutureTime(7.8f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SerpentsFang or AID.SerpentsJaws)
            Towers.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var forbidden = new List<Func<WPos, float>>();
        foreach (var t in Towers.Where(x => !Raid.WithoutSlot().InRadius(x.Position, 3).Any() || actor.Position.InCircle(x.Position, 3)))
            forbidden.Add(ShapeDistance.InvertedCircle(t.Position, 3));
        if (forbidden.Count > 0)
            hints.AddForbiddenZone(p => forbidden.Select(f => f(p)).Max(), Towers[0].Activation);
    }
}
