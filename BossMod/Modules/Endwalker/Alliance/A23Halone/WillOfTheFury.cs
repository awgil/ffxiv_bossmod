namespace BossMod.Endwalker.Alliance.A23Halone;

sealed class WillOfTheFury(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeDonut(24f, 30f), new AOEShapeDonut(18f, 24f), new AOEShapeDonut(12f, 18f), new AOEShapeDonut(6f, 12f), new AOEShapeCircle(6f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WillOfTheFuryAOE1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.WillOfTheFuryAOE1 => 0,
                (uint)AID.WillOfTheFuryAOE2 => 1,
                (uint)AID.WillOfTheFuryAOE3 => 2,
                (uint)AID.WillOfTheFuryAOE4 => 3,
                (uint)AID.WillOfTheFuryAOE5 => 4,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}
