namespace BossMod.Stormblood.Foray.Anemos;

[ConfigDisplay(Name = "Anemos", Parent = typeof(EurekaConfig))]
public class AnemosConfig : ConfigNode
{
    public NotoriousMonster FarmTarget = NotoriousMonster.None;
}

public enum NotoriousMonster : uint
{
    None,
    [NM(7169, 6816, 1332)]
    SabotenderCorrido,
    [NM(7151, 6820, 1348)]
    LordOfAnemos,
    [NM(7170, 6822, 1333)]
    Teles,
    [NM(7166, 6823, 1328)]
    EmperorOfAnemos,
    [NM(7157, 6826, 1344)]
    Callisto,
    [NM(7160, 6827, 1347)]
    Number,
    [NM(7158, 6983, 1345)]
    Jahannam,
    [NM(7171, 6831, 1334)]
    Amemet,
    [NM(7167, 6835, 1335)]
    Caym,
    [NM(7168, 6987, 1336)]
    Bombadeel,
    [NM(7149, 6837, 1339)]
    Serket,
    [NM(7165, 6840, 1346)]
    JudgmentalJulika,
    [NM(7173, 7185, 1343)]
    WhiteRider,
    [NM(7172, 6843, 1337)]
    Polyphemus,
    [NM(7148, 6845, 1342)]
    SimurghStrider,
    [NM(7152, 6974, 1341)]
    KingHazmat,
    [NM(7150, 6990, 1331)]
    Fafnir,
    [NM(7177, 6977, 1340)]
    Amarok,
    [NM(7147, 6980, 1338)]
    Lamashtu,
    [NM(7143, 6989, 1329)]
    Pazuzu
}

[ZoneModuleInfo(283)]
public class Anemos(WorldState ws) : EurekaZone<NotoriousMonster>(ws, "Anemos")
{
    private readonly AnemosConfig _config = Service.Config.Get<AnemosConfig>();

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
