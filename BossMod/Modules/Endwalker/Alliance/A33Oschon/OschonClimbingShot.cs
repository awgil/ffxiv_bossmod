namespace BossMod.Endwalker.Alliance.A33Oschon;

class ClimbingShot(BossModule module) : Components.Knockback(module)
{
    private Source? _knockback;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_knockback);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ClimbingShot or AID.ClimbingShot2 or AID.ClimbingShot3 or AID.ClimbingShot4)
            _knockback = new(Module.PrimaryActor.Position, 20, spell.NPCFinishAt);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ClimbingShot or AID.ClimbingShot2 or AID.ClimbingShot3 or AID.ClimbingShot4)
            _knockback = null;
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<DownhillP1>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.Bounds.Contains(pos);
}
