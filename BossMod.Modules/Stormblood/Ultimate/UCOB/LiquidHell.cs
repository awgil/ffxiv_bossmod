namespace BossMod.Stormblood.Ultimate.UCOB;

class LiquidHell(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, AID.LiquidHell, m => m.Enemies(OID.VoidzoneLiquidHell).Where(z => z.EventState != 7), 1.3f)
{
    public void Reset() => NumCasts = 0;
}

class P1LiquidHell : LiquidHell
{
    public P1LiquidHell(BossModule module) : base(module) { KeepOnPhaseChange = true; }
}
