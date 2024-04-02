namespace BossMod.Shadowbringers.Ultimate.TEA;

class P2CompressedWaterLightning : Components.GenericStackSpread
{
    public bool ResolveImminent; // we want to show hints shortly before next resolve
    private BitMask _forbiddenWater;
    private BitMask _forbiddenLighting;

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (ResolveImminent)
            base.AddHints(module, slot, actor, hints, movementHints);
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ResolveImminent)
            base.AddAIHints(module, slot, actor, assignment, hints);
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return ResolveImminent ? base.CalcPriority(module, pcSlot, pc, playerSlot, player, ref customColor) : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (ResolveImminent)
            base.DrawArenaForeground(module, pcSlot, pc, arena);
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.CompressedWater:
                Stacks.Add(new(actor, 8, 3, 6, status.ExpireAt, _forbiddenWater));
                _forbiddenWater.Reset();
                break;
            case SID.CompressedLightning:
                Stacks.Add(new(actor, 8, 2, 2, status.ExpireAt, _forbiddenLighting));
                _forbiddenLighting.Reset();
                break;
        }
    }

    // note: typical sequence is 'compressed' status loss > 'crashing' spells > 'resistance down' status gain > 'compressed' status gain
    // because of that, cast is the best point to remove previous stacks
    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CrashingWave:
                Stacks.RemoveAll(s => CheckAndRecord(s, 6, module, ref _forbiddenWater));
                ResolveImminent = false; // auto disable on resolve
                break;
            case AID.CrashingThunder:
                Stacks.RemoveAll(s => CheckAndRecord(s, 2, module, ref _forbiddenLighting));
                ResolveImminent = false; // auto disable on resolve
                break;
        }
    }

    private bool CheckAndRecord(Stack s, int maxSize, BossModule module, ref BitMask mask)
    {
        if (s.MaxSize != maxSize)
            return false;
        mask.Set(module.Raid.FindSlot(s.Target.InstanceID));
        return true;
    }
}
