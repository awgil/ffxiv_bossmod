namespace BossMod.Stormblood.Foray.Pyros;

[ConfigDisplay(Name = "Pyros", Parent = typeof(EurekaConfig))]
public class PyrosConfig : ConfigNode
{
    public NotoriousMonster FarmTarget = NotoriousMonster.None;
}

public enum NotoriousMonster : uint
{
    None,
    [NM(7759, 7776, 1388)]
    Leucosia,
    [NM(7753, 7772, 1389)]
    Flauros,
    [NM(7740, 7808, 1390)]
    Sophist,
    [NM(7736, 7812, 1391)]
    Graffiacane,
    [NM(7742, 7814, 1392)]
    Askalaphos,
    [NM(7746, 7779, 1393)]
    Batym,
    [NM(7754, 7821, 1394)]
    Aetolus,
    [NM(7726, 7824, 1395)]
    Lesath,
    [NM(7727, 7825, 1396)]
    Eldthurs,
    [NM(7760, 7830, 1397)]
    Iris,
    [NM(7748, 7832, 1398)]
    Lamebrix,
    [NM(7756, 7836, 1399)]
    Dux,
    [NM(7750, 7837, 1400)]
    LumberJack,
    [NM(7729, 7841, 1401)]
    Glaukopis,
    [NM(7763, 7845, 1402)]
    YY,
    [NM(7739, 7848, 1403)]
    Skoll,
    [NM(7731, 7846, 1404)]
    Penthesilea
}

[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 598)]
public class Pyros(WorldState ws) : EurekaZone<NotoriousMonster>(ws, "Pyros")
{
    private readonly PyrosConfig _config = Service.Config.Get<PyrosConfig>();

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
