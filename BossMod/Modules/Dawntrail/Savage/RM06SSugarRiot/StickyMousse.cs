namespace BossMod.Dawntrail.Savage.RM06SSugarRiot;

class StickyMousse(BossModule module) : Components.GenericBaitAway(module, AID.StickyMousse, centerAtTarget: true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StickyMousseVisual)
            foreach (var tar in Raid.WithoutSlot().Where(x => x.Class.GetRole() != Role.Tank))
                CurrentBaits.Add(new(caster, tar, new AOEShapeCircle(4), Module.CastFinishAt(spell, 0.9f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            CurrentBaits.Clear();
    }
}

class StickyBurst(BossModule module) : Components.UniformStackSpread(module, 4, 0, 4)
{
    public int NumCasts;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.MousseMine)
            AddStack(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Burst)
        {
            NumCasts++;
            Stacks.Clear();
        }
    }
}
