namespace BossMod.Stormblood.Extreme.Ex3Shinryu;

class DarkMatterPush(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        var hp = Module.PrimaryActor.HPMP;
        if (hp.MaxHP == 0)
            return;
        var pct = 100f * hp.CurHP / hp.MaxHP;
        if (pct > 45)
            hints.Add($"Push to 45% for Dark Matter (now {pct:f1}%)");
    }
}
