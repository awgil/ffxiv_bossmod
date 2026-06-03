namespace BossMod.Stormblood.Foray.Pagos;

[ConfigDisplay(Name = "Pagos", Parent = typeof(EurekaConfig))]
public class PagosConfig : ConfigNode
{
    public NotoriousMonster FarmTarget = NotoriousMonster.None;
}

public enum NotoriousMonster : uint
{
    None,
    [NM(7535, 7465, 1369)]
    Taxim,
    [NM(7520, 7434, 1353)]
    AshDragon,
    [NM(7516, 7453, 1354)]
    Glavoid,
    [NM(7497, 7471, 1355)]
    Anapos,
    [NM(7518, 7429, 1366)]
    Hakutaku,
    [NM(7503, 7419, 1357)]
    KingIgloo,
    [NM(7522, 7431, 1356)]
    Asag,
    [NM(7512, 7423, 1352)]
    Surabhi,
    [NM(7508, 7450, 1360)]
    KingArthro,
    [NM(7515, 7442, 1358)]
    Mindertaur,
    [NM(7499, 7447, 1361)]
    HolyCow,
    [NM(7523, 7460, 1362)]
    Hadhayosh,
    [NM(7524, 7445, 1359)]
    Horus,
    [NM(7495, 7420, 1363)]
    AngraMainyu,
    [NM(7505, 7462, 1365)]
    CopycatCassie,
    [NM(7529, 7468, 1364)]
    Louhi
}

[ZoneModuleInfo(581)]
public class Pagos(WorldState ws) : EurekaZone<NotoriousMonster>(ws, "Pagos")
{
    private readonly PagosConfig _config = Service.Config.Get<PagosConfig>();

    protected override NotoriousMonster FarmTarget
    {
        get => _config.FarmTarget;
        set
        {
            _config.FarmTarget = value;
            _config.Modified.Fire();
        }
    }
}
