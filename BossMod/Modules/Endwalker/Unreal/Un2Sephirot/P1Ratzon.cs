namespace BossMod.Endwalker.Unreal.Un2Sephirot;

// TODO: generalize
class P1Ratzon : BossComponent
{
    private BitMask _greenTargets;
    private BitMask _purpleTargets;

    private static readonly float _greenRadius = 5;
    private static readonly float _purpleRadius = 10;

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if ((_greenTargets | _purpleTargets).None())
            return;

        bool clippedByGreen = module.Raid.WithSlot().Exclude(slot).IncludedInMask(_greenTargets).InRadius(actor.Position, _greenRadius).Any();
        bool clippedByPurple = module.Raid.WithSlot().Exclude(slot).IncludedInMask(_purpleTargets).InRadius(actor.Position, _purpleRadius).Any();
        hints.Add($"Spread! (debuff: {(_greenTargets[slot] ? "green" : _purpleTargets[slot] ? "purple" : "none")})", clippedByGreen || clippedByPurple);
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return (_greenTargets | _purpleTargets)[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var (slot, actor) in module.Raid.WithSlot().IncludedInMask(_greenTargets))
            arena.AddCircle(actor.Position, _greenRadius, 0xff00ff00, slot == pcSlot ? 2 : 1);
        foreach (var (slot, actor) in module.Raid.WithSlot().IncludedInMask(_purpleTargets))
            arena.AddCircle(actor.Position, _purpleRadius, 0xffff00ff, slot == pcSlot ? 2 : 1);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.RatzonAOEGreen:
                _greenTargets.Clear(module.Raid.FindSlot(spell.MainTargetID));
                break;
            case AID.RatzonAOEPurple:
                _purpleTargets.Clear(module.Raid.FindSlot(spell.MainTargetID));
                break;
        }
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        switch ((IconID)iconID)
        {
            case IconID.RatzonGreen:
                _greenTargets.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
            case IconID.RatzonPurple:
                _purpleTargets.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
        }
    }
}
