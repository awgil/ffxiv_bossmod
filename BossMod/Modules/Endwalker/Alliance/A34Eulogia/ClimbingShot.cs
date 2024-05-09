namespace BossMod.Endwalker.Alliance.A34Eulogia;

class ClimbingShot(BossModule module) : Components.Knockback(module)
{
    private readonly AsAboveSoBelow? _exaflare = module.FindComponent<AsAboveSoBelow>();
    private Source? _knockback;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_knockback);
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos) || (_exaflare?.ActiveAOEs(slot, actor).Any(z => z.Check(pos)) ?? false);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ClimbingShotNald or AID.ClimbingShotThal)
            _knockback = new(Module.PrimaryActor.Position, 20, spell.NPCFinishAt.AddSeconds(0.2f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ClimbingShotAOE1 or AID.ClimbingShotAOE2 or AID.ClimbingShotAOE3 or AID.ClimbingShotAOE4)
        {
            ++NumCasts;
            _knockback = null;
        }
    }
}
