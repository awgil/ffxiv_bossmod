namespace BossMod.Dawntrail.Ultimate.FRU;

abstract class SpiritTaker(BossModule module) : Components.UniformStackSpread(module, 0, 5)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var spread in ActiveSpreads.Where(s => s.Target != actor))
            hints.AddForbiddenZone(ShapeDistance.Capsule(spread.Target.Position, spread.Target.LastFrameMovement.Normalized(), 2, spread.Radius + 1), spread.Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SpiritTaker)
            AddSpreads(Raid.WithoutSlot(true), Module.CastFinishAt(spell, 0.3f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SpiritTakerAOE)
            Spreads.Clear();
    }
}

class DefaultSpiritTaker(BossModule module) : SpiritTaker(module); // TODO: remove...
