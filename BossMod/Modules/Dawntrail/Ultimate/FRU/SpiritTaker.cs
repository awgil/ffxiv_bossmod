namespace BossMod.Dawntrail.Ultimate.FRU;

abstract class SpiritTaker(BossModule module) : Components.GenericStackSpread(module)
{
    public const float Radius = 5;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var spread in ActiveSpreads.Where(s => s.Target != actor))
            hints.AddForbiddenZone(ShapeContains.Capsule(spread.Target.Position, spread.Target.LastFrameMovement.Normalized(), 2, spread.Radius + 1), spread.Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SpiritTaker)
        {
            var activation = Module.CastFinishAt(spell, 0.3f);
            foreach (var (i, p) in Raid.WithSlot(true))
            {
                // TODO: i think this is right - we can't clip the entire hitbox of the fragment?..
                Spreads.Add(new(p, Radius + (i < PartyState.MaxPartySize ? 0 : p.HitboxRadius), activation));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SpiritTakerAOE)
            Spreads.Clear();
    }
}
