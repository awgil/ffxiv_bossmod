namespace BossMod.Shadowbringers.Ultimate.TEA;

class P1HandOfPain(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.HandOfPain))
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        var boss = Module.Enemies(OID.BossP1).FirstOrDefault();
        var hand = Module.Enemies(OID.LiquidHand).FirstOrDefault();
        if (boss != null && hand != null)
        {
            var diff = (int)(hand.HPMP.CurHP - boss.HPMP.CurHP) * 100.0f / boss.HPMP.MaxHP;
            hints.Add($"Hand HP: {(diff > 0 ? "+" : "")}{diff:f1}%");
        }
    }
}
