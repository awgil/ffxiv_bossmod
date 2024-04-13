namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

class ClawTail(BossModule module) : Components.GenericAOEs(module)
{
    public int Progress { get; private set; } // 7 claws + 1 tail total
    private bool _tailFirst;

    private static readonly AOEShapeCone _shape = new(45, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var rotation = Module.PrimaryActor.CastInfo?.Rotation ?? Module.PrimaryActor.Rotation;
        if (_tailFirst ? Progress == 0 : Progress >= 7)
            rotation += 180.Degrees();
        yield return new(_shape, Module.PrimaryActor.Position, rotation, Module.PrimaryActor.CastInfo?.NPCFinishAt ?? WorldState.CurrentTime);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TailToClaw)
            _tailFirst = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RagingClawFirst or AID.RagingClawFirstRest or AID.RagingTailSecond or AID.RagingTailFirst or AID.RagingClawSecond or AID.RagingClawSecondRest)
            ++Progress;
    }
}
