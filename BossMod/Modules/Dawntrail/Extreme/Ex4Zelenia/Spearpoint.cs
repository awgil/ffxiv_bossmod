namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class SpearpointBait(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly Dictionary<ulong, Actor> tethers = [];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.AddsTether && WorldState.Actors.Find(tether.Target) is { } player)
            tethers[source.InstanceID] = player;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.ZeleniasShade)
        {
            switch (id)
            {
                // RH slash
                case 0x0C90:
                    CurrentBaits.Add(new(actor, tethers[actor.InstanceID], new AOEShapeRect(33, 37, 1, -90.Degrees())));
                    break;

                // LH slash
                case 0x0C91:
                    CurrentBaits.Add(new(actor, tethers[actor.InstanceID], new AOEShapeRect(33, 37, 1, 90.Degrees())));
                    break;
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        foreach (var (src, tar) in tethers)
            Arena.AddLine(WorldState.Actors.Find(src)!.Position, tar.Position, ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_SpearpointPush or AID._Weaponskill_SpearpointPush1)
        {
            CurrentBaits.RemoveAll(t => t.Source == caster);
            tethers.Remove(caster.InstanceID);
        }
    }
}
class SpearpointAOE(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_SpearpointPush, AID._Weaponskill_SpearpointPush1], new AOEShapeRect(33, 37));
