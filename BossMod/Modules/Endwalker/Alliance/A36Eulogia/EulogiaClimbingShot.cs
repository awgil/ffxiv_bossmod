namespace BossMod.Endwalker.Alliance.A36Eulogia;

class ClimbingShot : Components.Knockback
{
    private Source? _knockback;

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_knockback);

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ClimbingShotVisual or AID.ClimbingShotVisual2)
            _knockback = new(module.PrimaryActor.Position, 20, spell.NPCFinishAt.AddSeconds(0.2f));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ClimbingShot1 or AID.ClimbingShot2 or AID.ClimbingShot3 or AID.ClimbingShot4)
            _knockback = null;
    }

    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => module.FindComponent<AsAboveSoBelow>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false || !module.Bounds.Contains(pos);
}
