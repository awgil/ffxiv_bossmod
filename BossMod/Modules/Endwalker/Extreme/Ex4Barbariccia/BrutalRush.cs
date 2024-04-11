namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

class BrutalRush(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BrutalGust), new AOEShapeRect(40, 2))
{
    private BitMask _pendingRushes;
    public bool HavePendingRushes => _pendingRushes.Any();

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.BrutalRush)
            _pendingRushes.Set(Raid.FindSlot(source.InstanceID));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.BrutalRush)
            _pendingRushes.Clear(Raid.FindSlot(source.InstanceID));
    }
}
