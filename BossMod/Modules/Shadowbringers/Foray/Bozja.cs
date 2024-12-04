using BossMod.Shadowbringers.Foray.Zadnor;

namespace BossMod.Shadowbringers.Foray.Bozja;

[ConfigDisplay(Parent = typeof(ShadowbringersConfig))]
class BozjaConfig : ConfigNode
{
    [PropertyDisplay("4th Legion Slasher (Gabriel)")]
    public bool Slasher = false;
    [PropertyDisplay("4th Legion Vanguard (Lyon)")]
    public bool Vanguard = false;
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
        base.Dispose(disposing);
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
        if (_config.Slasher)
            yield return ("Slasher", 0x2E17);
        if (_config.Scorpion)
            yield return ("Scorpion", 0x2E53);
        if (_config.Vanguard)
            yield return ("Vanguard", 0x2E36);
    }

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        base.CalculateAIHints(playerSlot, player, hints);

        if (World.Client.ActiveFate.ID == 1611) // The Element of Supplies
        {
            foreach (var h in hints.PotentialTargets)
            {
                if (h.Actor.OID == 0x2EA4) // crate
                    h.Priority = 5;
            }
        }
    }
}
