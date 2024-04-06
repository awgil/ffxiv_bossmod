namespace BossMod.Endwalker.Alliance.A33Oschon;

class ClimbingShot : Components.Knockback
{
    private Source? _knockback;

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_knockback);

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ClimbingShot or AID.ClimbingShot2 or AID.ClimbingShot3 or AID.ClimbingShot4)
            _knockback = new(module.PrimaryActor.Position, 20, spell.NPCFinishAt);
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ClimbingShot or AID.ClimbingShot2 or AID.ClimbingShot3 or AID.ClimbingShot4)
            _knockback = null;
    }

    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => module.FindComponent<DownhillP1>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false || !module.Bounds.Contains(pos);
}
