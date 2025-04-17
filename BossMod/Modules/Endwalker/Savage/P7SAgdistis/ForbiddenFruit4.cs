namespace BossMod.Endwalker.Savage.P7SAgdistis;

class ForbiddenFruit4(BossModule module) : ForbiddenFruitCommon(module, AID.BullishSwipeAOE)
{
    private int _bullPlatform;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (NumAssignedTethers > 0 && !MinotaursBaited && TetherSources[pcSlot] == null)
        {
            Arena.AddCircle(Module.Center - 2 * PlatformDirection(_bullPlatform).ToDirection(), 2, ArenaColor.Safe);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var slot = TryAssignTether(source, tether);
        if (slot < 0)
            return;
        switch ((TetherID)tether.ID)
        {
            case TetherID.Bull:
                SafePlatforms[slot].Set(_bullPlatform);
                break;
            case TetherID.MinotaurFar:
            case TetherID.MinotaurClose:
                var safePlatforms = ValidPlatformsMask;
                safePlatforms.Clear(_bullPlatform);
                safePlatforms.Clear(PlatformIDFromOffset(source.Position - Module.Center));
                SafePlatforms[slot] = safePlatforms;
                break;
        }
    }

    protected override DateTime? PredictUntetheredCastStart(Actor fruit)
    {
        if ((OID)fruit.OID == OID.ForbiddenFruitBull)
            _bullPlatform = PlatformIDFromOffset(fruit.Position - Module.Center);
        return null;
    }
}
