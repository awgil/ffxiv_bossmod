namespace BossMod.Stormblood.Ultimate.UWU;

class P2InfernalFetters : BossComponent
{
    public BitMask Fetters;
    private int _fettersStrength;

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return Fetters[playerSlot] ? PlayerPriority.Normal : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (Fetters.NumSetBits() > 1)
        {
            var from = module.Raid[Fetters.LowestSetBit()];
            var to = module.Raid[Fetters.HighestSetBit()];
            if (from != null && to != null)
                arena.AddLine(from.Position, to.Position, _fettersStrength > 1 ? ArenaColor.Danger : ArenaColor.Safe);
        }
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.InfernalFetters)
        {
            Fetters.Set(module.Raid.FindSlot(actor.InstanceID));
            _fettersStrength = status.Extra;
        }
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.InfernalFetters)
        {
            Fetters.Clear(module.Raid.FindSlot(actor.InstanceID));
        }
    }
}
