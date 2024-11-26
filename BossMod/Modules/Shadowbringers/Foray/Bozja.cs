using BossMod.Shadowbringers.Foray.Zadnor;

namespace BossMod.Shadowbringers.Foray.Bozja;


[ConfigDisplay(Parent = typeof(ShadowbringersConfig))]
class BozjaConfig : ConfigNode
{
    [PropertyDisplay("4th Legion Scorpion (Sartauvoir)")]
    public bool Scorpion = false;
}

[ZoneModuleInfo(BossModuleInfo.Maturity.Verified, 735)]
public class Bozja : FarmModule
{
    private readonly BozjaConfig _config = Service.Config.Get<BozjaConfig>();
    private readonly EventSubscriptions _subscriptions;

    public Bozja(WorldState ws) : base(ws)
    {
        _subscriptions = new(ws.Client.ActiveFateChanged.Subscribe(OnFateChanged));
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
    }

    void OnFateChanged(ClientState.OpActiveFateChange fate)
    {
        if (fate.Value.ID == 1625 && _config.Scorpion)
        {
            _config.Scorpion = false;
            _config.Modified.Fire();
        }
    }

    protected override IEnumerable<(string Name, uint OID)> Targets()
    {
        if (_config.Scorpion)
            yield return ("Scorpion", 0x2E53);
    }
}
