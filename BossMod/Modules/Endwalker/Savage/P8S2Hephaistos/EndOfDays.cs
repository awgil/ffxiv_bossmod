namespace BossMod.Endwalker.Savage.P8S2;

class EndOfDays(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.EndOfDays))
{
    public List<(Actor caster, DateTime finish)> Casters = [];

    private static readonly AOEShapeRect _shape = new(60, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in Casters.Take(3))
        {
            if (c.caster.CastInfo == null)
                yield return new(_shape, c.caster.Position, c.caster.Rotation, c.finish);
            else
                yield return new(_shape, c.caster.Position, c.caster.CastInfo.Rotation, c.caster.CastInfo.NPCFinishAt);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.RemoveAll(c => c.caster == caster);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.IllusoryHephaistosLanes && id == 0x11D3)
            Casters.Add((actor, WorldState.FutureTime(8))); // ~2s before cast start
    }
}
