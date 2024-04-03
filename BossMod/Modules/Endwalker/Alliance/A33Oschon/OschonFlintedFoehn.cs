namespace BossMod.Endwalker.Alliance.A33Oschon;

class FlintedFoehn : Components.UniformStackSpread
{
    public FlintedFoehn() : base(6, 0) { }
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