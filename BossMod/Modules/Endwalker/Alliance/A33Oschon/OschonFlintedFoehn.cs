namespace BossMod.Endwalker.Alliance.A33Oschon;

class FlintedFoehnP1 : Components.UniformStackSpread
{
    public FlintedFoehnP1() : base(6, 0) { }
    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.FlintedFoehnStack)
            AddStack(actor, module.WorldState.CurrentTime.AddSeconds(10.45f));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FlintedFoehnStack)
            Stacks.Clear();
    }
}
class FlintedFoehnP2 : Components.UniformStackSpread
{
    public int NumCasts { get; private set; }

    public FlintedFoehnP2() : base(6, 0, 6) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FlintedFoehnStackP2 && module.WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            AddStack(target);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FlintedFoehnStackP2)
            ++NumCasts;
    }
}