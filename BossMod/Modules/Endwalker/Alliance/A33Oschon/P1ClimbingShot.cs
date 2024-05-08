namespace BossMod.Endwalker.Alliance.A33Oschon;

class P1ClimbingShot(BossModule module) : Components.Knockback(module)
{
    private readonly P1Downhill? _downhill = module.FindComponent<P1Downhill>();
    private Source? _knockback;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_knockback);
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos) || (_downhill?.ActiveAOEs(slot, actor).Any(z => z.Check(pos)) ?? false);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ClimbingShot1 or AID.ClimbingShot2 or AID.ClimbingShot3 or AID.ClimbingShot4)
            _knockback = new(Module.PrimaryActor.Position, 20, spell.NPCFinishAt);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ClimbingShot1 or AID.ClimbingShot2 or AID.ClimbingShot3 or AID.ClimbingShot4)
            _knockback = null;
    }
}
