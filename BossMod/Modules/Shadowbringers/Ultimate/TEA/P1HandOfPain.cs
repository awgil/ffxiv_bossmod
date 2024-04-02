namespace BossMod.Shadowbringers.Ultimate.TEA;

class P1HandOfPain : Components.CastCounter
{
    public P1HandOfPain() : base(ActionID.MakeSpell(AID.HandOfPain)) { }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        var boss = module.Enemies(OID.BossP1).FirstOrDefault();
        var hand = module.Enemies(OID.LiquidHand).FirstOrDefault();
        if (boss != null && hand != null)
        {
            var diff = (int)(hand.HP.Cur - boss.HP.Cur) * 100.0f / boss.HP.Max;
            hints.Add($"Hand HP: {(diff > 0 ? "+" : "")}{diff:f1}%");
        }
    }
}
