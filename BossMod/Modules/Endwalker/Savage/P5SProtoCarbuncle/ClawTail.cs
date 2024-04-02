namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

class ClawTail : Components.GenericAOEs
{
    public int Progress { get; private set; } // 7 claws + 1 tail total
    private bool _tailFirst;

    private static readonly AOEShapeCone _shape = new(45, 90.Degrees());

    public ClawTail() : base(new()) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        var rotation = module.PrimaryActor.CastInfo?.Rotation ?? module.PrimaryActor.Rotation;
        if (_tailFirst ? Progress == 0 : Progress >= 7)
            rotation += 180.Degrees();
        yield return new(_shape, module.PrimaryActor.Position, rotation, module.PrimaryActor.CastInfo?.NPCFinishAt ?? module.WorldState.CurrentTime);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TailToClaw)
            _tailFirst = true;
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RagingClawFirst or AID.RagingClawFirstRest or AID.RagingTailSecond or AID.RagingTailFirst or AID.RagingClawSecond or AID.RagingClawSecondRest)
            ++Progress;
    }
}
