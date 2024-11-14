using static BossMod.Components.GenericAOEs;

namespace BossMod.Shadowbringers.Foray.Zadnor;

[ConfigDisplay(Parent = typeof(ShadowbringersConfig))]
class ZadnorFarmConfig : ConfigNode
{
    [PropertyDisplay("4th Legion Death Machine (Menenius)")]
    public bool DeathMachine = false;
    [PropertyDisplay("4th Legion Rearguard (Lyon II)")]
    public bool RearGuard = false;
}

[ZoneModuleInfo(BossModuleInfo.Maturity.Verified, 778)]
public class Zadnor : ZoneModule
{
    private readonly ZadnorFarmConfig _config = Service.Config.Get<ZadnorFarmConfig>();
    private readonly EventSubscriptions _subscriptions;
    private readonly List<AOEInstance> _adHocAOEs = [];

    public Zadnor(WorldState ws) : base(ws)
    {
        _subscriptions = new(
            ws.Actors.CastStarted.Subscribe(OnCastStarted),
            ws.Actors.CastFinished.Subscribe(OnCastFinished),
            ws.Client.ActiveFateChanged.Subscribe(OnFateChanged)
        );
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
    }

    void OnCastStarted(Actor caster)
    {
        var cinfo = caster.CastInfo!;
        if (cinfo.Action.ID == 24769)
        {
            _adHocAOEs.Add(new AOEInstance(new AOEShapeCircle(20), cinfo.LocXZ, default, World.FutureTime(cinfo.NPCRemainingTime)));
        }
    }

    void OnCastFinished(Actor caster)
    {
        if (caster.CastInfo?.Action.ID == 24769)
            _adHocAOEs.Clear();
    }

    void OnFateChanged(ClientState.OpActiveFateChange fate)
    {
        if (fate.Value.ID == 1739 && _config.RearGuard)
        {
            _config.RearGuard = false;
            _config.Modified.Fire();
        }
    }

    private IEnumerable<(string, uint)> FarmOIDs
    {
        get
        {
            if (_config.RearGuard)
                yield return ("Rearguard", 0x329E);
            if (_config.DeathMachine)
                yield return ("Death Machine", 0x3284);
        }
    }

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        foreach (var i in _adHocAOEs)
            hints.AddForbiddenZone(i.Shape, i.Origin, i.Rotation, i.Activation);

        if (World.Client.ActiveFate.ID > 0)
            return;

        hints.PathfindMapBounds = new ArenaBoundsSquare(120, 2);

        var oids = FarmOIDs.ToList();

        if (oids.Count > 0)
            foreach (var h in hints.PotentialTargets)
                if (oids.Any(o => h.Actor.OID == o.Item2))
                    h.Priority = Math.Max(h.Priority, 0);
    }

    public override bool WantToBeDrawn() => FarmOIDs.Any();

    public override List<string> CalculateGlobalHints()
    {
        List<string> hints = [];
        var oids = FarmOIDs.ToList();
        if (oids.Count > 0)
        {
            hints.Add($"Currently farming: {string.Join(", ", oids.Select(o => o.Item1))}");
        }

        return hints;
    }
}
