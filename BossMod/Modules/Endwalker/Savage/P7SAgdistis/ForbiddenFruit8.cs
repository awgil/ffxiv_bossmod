namespace BossMod.Endwalker.Savage.P7SAgdistis;

class ForbiddenFruit8(BossModule module) : ForbiddenFruitCommon(module, ActionID.MakeSpell(AID.StymphalianStrike))
{
    private BitMask _noBirdsPlatforms = ValidPlatformsMask;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var slot = TryAssignTether(source, tether);
        if (slot < 0)
            return;
        var safe = ValidPlatformsMask & ~_noBirdsPlatforms;
        safe.Clear(PlatformIDFromOffset(source.Position - Module.Center));
        SafePlatforms[slot] = safe;
    }

    protected override DateTime? PredictUntetheredCastStart(Actor fruit)
    {
        if ((OID)fruit.OID != OID.ForbiddenFruitBird)
            return null;

        _noBirdsPlatforms.Clear(PlatformIDFromOffset(fruit.Position - Module.Center));
        Array.Fill(SafePlatforms, _noBirdsPlatforms);
        return WorldState.FutureTime(12.5f);
    }
}
