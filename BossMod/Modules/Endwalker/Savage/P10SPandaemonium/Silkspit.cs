namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class Silkspit(BossModule module) : Components.UniformStackSpread(module, 0, 7)
{
    private readonly IReadOnlyList<Actor> _pillars = module.Enemies(OID.Pillar);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (IsSpreadTarget(actor) && _pillars.InRadius(actor.Position, SpreadRadius).Any())
            hints.Add("GTFO from pillars!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        Arena.Actors(_pillars, ArenaColor.Object, true);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SilkspitAOE)
            Spreads.Clear();
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Silkspit)
            AddSpread(actor, WorldState.FutureTime(8));
    }
}
