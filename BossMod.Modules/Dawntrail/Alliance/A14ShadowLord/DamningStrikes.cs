namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

class DamningStrikes(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DamningStrikesImpact1 or AID.DamningStrikesImpact2 or AID.DamningStrikesImpact3)
        {
            Towers.Add(new(caster.Position, 3, 8, 8, default, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DamningStrikesImpact1 or AID.DamningStrikesImpact2 or AID.DamningStrikesImpact3)
        {
            ++NumCasts;
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
            var forbidden = Raid.WithSlot().WhereActor(a => spell.Targets.Any(t => t.ID == a.InstanceID)).Mask();
            foreach (ref var t in Towers.AsSpan())
                t.ForbiddenSoakers |= forbidden;
        }
    }
}
