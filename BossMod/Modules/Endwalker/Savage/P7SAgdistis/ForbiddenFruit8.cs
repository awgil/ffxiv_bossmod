namespace BossMod.Endwalker.Savage.P7SAgdistis;

class ForbiddenFruit8 : ForbiddenFruitCommon
{
    private BitMask _noBirdsPlatforms = ValidPlatformsMask;

    public ForbiddenFruit8() : base(ActionID.MakeSpell(AID.StymphalianStrike)) { }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        var slot = TryAssignTether(module, source, tether);
        if (slot < 0)
            return;
        var safe = ValidPlatformsMask & ~_noBirdsPlatforms;
        safe.Clear(PlatformIDFromOffset(source.Position - module.Bounds.Center));
        SafePlatforms[slot] = safe;
    }

    protected override DateTime? PredictUntetheredCastStart(BossModule module, Actor fruit)
    {
        if ((OID)fruit.OID != OID.ForbiddenFruitBird)
            return null;

        _noBirdsPlatforms.Clear(PlatformIDFromOffset(fruit.Position - module.Bounds.Center));
        Array.Fill(SafePlatforms, _noBirdsPlatforms);
        return module.WorldState.CurrentTime.AddSeconds(12.5);
    }
}
