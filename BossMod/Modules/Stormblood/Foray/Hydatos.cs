namespace BossMod.Stormblood.Foray.Hydatos;

[ConfigDisplay(Name = "Hydatos", Parent = typeof(EurekaConfig))]
public class HydatosConfig : ConfigNode
{
    public NotoriousMonster FarmTarget = NotoriousMonster.None;
}

public enum NotoriousMonster : uint
{
    None,
    [NM(8063, 8024, 1412)]
    Khalamari,
    [NM(8069, 8028, 1413)]
    Stego,
    [NM(8070, 8031, 1414)]
    Molech,
    [NM(8073, 8032, 1415)]
    Piasa,
    [NM(8072, 8037, 1416)]
    Frostmane,
    [NM(7967, 8038, 1417)]
    Daphne,
    [NM(7966, 8058, 1418)]
    Golde,
    [NM(8068, 8045, 1419)]
    Leuke,
    [NM(7965, 8047, 1420)]
    Barong,
    [NM(7955, 8050, 1421)]
    Ceto,
    [NM(8061, 8054, 1423)]
    PW
}

[ZoneModuleInfo(BossModuleInfo.Maturity.WIP, 639)]
public class Hydatos(WorldState ws) : EurekaZone<NotoriousMonster>(ws, "Hydatos")
{
    private readonly HydatosConfig _config = Service.Config.Get<HydatosConfig>();

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
