namespace BossMod.Endwalker.Alliance.A34OschonBig;

class BigFlintedFoehn : Components.UniformStackSpread
{
    public int NumCasts { get; private set; }

    public BigFlintedFoehn() : base(6, 0, 6) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FlintedFoehnStack && module.WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            AddStack(target);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FlintedFoehnStack)
            ++NumCasts;
    }
}