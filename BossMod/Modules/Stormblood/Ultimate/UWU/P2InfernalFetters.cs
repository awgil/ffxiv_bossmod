namespace BossMod.Stormblood.Ultimate.UWU;

class P2InfernalFetters(BossModule module) : BossComponent(module)
{
    public BitMask Fetters;
    private int _fettersStrength;

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return Fetters[playerSlot] ? PlayerPriority.Normal : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Fetters.NumSetBits() > 1)
        {
            var from = Raid[Fetters.LowestSetBit()];
            var to = Raid[Fetters.HighestSetBit()];
            if (from != null && to != null)
                Arena.AddLine(from.Position, to.Position, _fettersStrength > 1 ? ArenaColor.Danger : ArenaColor.Safe);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.InfernalFetters)
        {
            Fetters.Set(Raid.FindSlot(actor.InstanceID));
            _fettersStrength = status.Extra;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.InfernalFetters)
        {
            Fetters.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }
}
