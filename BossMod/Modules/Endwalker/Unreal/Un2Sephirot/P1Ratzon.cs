namespace BossMod.Endwalker.Unreal.Un2Sephirot;

// TODO: generalize
class P1Ratzon : BossComponent
{
    private BitMask _greenTargets;
    private BitMask _purpleTargets;

    private static readonly float _greenRadius = 5;
    private static readonly float _purpleRadius = 10;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if ((_greenTargets | _purpleTargets).None())
            return;

        bool clippedByGreen = Raid.WithSlot().Exclude(slot).IncludedInMask(_greenTargets).InRadius(actor.Position, _greenRadius).Any();
        bool clippedByPurple = Raid.WithSlot().Exclude(slot).IncludedInMask(_purpleTargets).InRadius(actor.Position, _purpleRadius).Any();
        hints.Add($"Spread! (debuff: {(_greenTargets[slot] ? "green" : _purpleTargets[slot] ? "purple" : "none")})", clippedByGreen || clippedByPurple);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return (_greenTargets | _purpleTargets)[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (slot, actor) in Raid.WithSlot().IncludedInMask(_greenTargets))
            arena.AddCircle(actor.Position, _greenRadius, 0xff00ff00, slot == pcSlot ? 2 : 1);
        foreach (var (slot, actor) in Raid.WithSlot().IncludedInMask(_purpleTargets))
            arena.AddCircle(actor.Position, _purpleRadius, 0xffff00ff, slot == pcSlot ? 2 : 1);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.RatzonAOEGreen:
                _greenTargets.Clear(Raid.FindSlot(spell.MainTargetID));
                break;
            case AID.RatzonAOEPurple:
                _purpleTargets.Clear(Raid.FindSlot(spell.MainTargetID));
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        switch ((IconID)iconID)
        {
            case IconID.RatzonGreen:
                _greenTargets.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case IconID.RatzonPurple:
                _purpleTargets.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }
}
