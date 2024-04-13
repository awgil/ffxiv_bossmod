namespace BossMod.Endwalker.Quest.Endwalker;

class Candlewick(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10), new AOEShapeDonut(10, 30)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CandlewickPointBlank)
            AddSequence(caster.Position, spell.NPCFinishAt.AddSeconds(2));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Sequences.Count > 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.CandlewickPointBlank => 0,
                AID.CandlewickDonut => 1,
                _ => -1
            };
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(2));
        }
    }
}
