namespace BossMod.Stormblood.Ultimate.UCOB;

class LiquidHell : Components.PersistentVoidzoneAtCastTarget
{
    public LiquidHell() : base(6, ActionID.MakeSpell(AID.LiquidHell), m => m.Enemies(OID.VoidzoneLiquidHell).Where(z => z.EventState != 7), 1.3f) { }
    public void Reset() => NumCasts = 0;
}

class P1LiquidHell : LiquidHell
{
    public P1LiquidHell() { KeepOnPhaseChange = true; }
}
