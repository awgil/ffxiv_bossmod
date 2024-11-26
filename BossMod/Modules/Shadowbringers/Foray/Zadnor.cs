using static BossMod.Components.GenericAOEs;

namespace BossMod.Shadowbringers.Foray.Zadnor;

[ConfigDisplay(Parent = typeof(ShadowbringersConfig))]
class ZadnorConfig : ConfigNode
{
    [PropertyDisplay("4th Legion Hexadrone (Dabog)")]
    public bool Hexadrone = false;
    [PropertyDisplay("4th Legion Death Machine (Menenius)")]
    public bool DeathMachine = false;
    [PropertyDisplay("4th Legion Rearguard (Lyon II)")]
    public bool RearGuard = false;
}

public abstract class FarmModule(WorldState ws) : ZoneModule(ws)
{
    protected abstract IEnumerable<(string Name, uint OID)> Targets();

    public override bool WantToBeDrawn() => Targets().Any();

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        if (World.Client.ActiveFate.ID > 0)
            return;

        hints.PathfindMapBounds = new ArenaBoundsSquare(120, 2);

        var oids = Targets().Select(t => t.OID).ToList();

        if (oids.Count > 0)
            foreach (var h in hints.PotentialTargets)
                if (oids.Contains(h.Actor.OID))
                    h.Priority = Math.Max(h.Priority, 0);
    }

    public override List<string> CalculateGlobalHints()
    {
        List<string> hints = [];
        var oids = Targets().ToList();
        if (oids.Count > 0)
        {
            hints.Add($"Currently farming: {string.Join(", ", oids.Select(o => o.Name))}");
        }

        return hints;
    }
}

[ZoneModuleInfo(BossModuleInfo.Maturity.Verified, 778)]
public class Zadnor : FarmModule
{
    private readonly ZadnorConfig _config = Service.Config.Get<ZadnorConfig>();
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
        if (fate.Value.ID == 1722 && _config.Hexadrone)
        {
            _config.Hexadrone = false;
            _config.Modified.Fire();
        }
        if (fate.Value.ID == 1727 && _config.DeathMachine)
        {
            _config.DeathMachine = false;
            _config.Modified.Fire();
        }
        if (fate.Value.ID == 1739 && _config.RearGuard)
        {
            _config.RearGuard = false;
            _config.Modified.Fire();
        }
    }

    protected override IEnumerable<(string Name, uint OID)> Targets()
    {
        if (_config.Hexadrone)
            yield return ("Hexadrone", 0x327A);
        if (_config.DeathMachine)
            yield return ("Death Machine", 0x3284);
        if (_config.RearGuard)
            yield return ("Rearguard", 0x329E);
    }

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        base.CalculateAIHints(playerSlot, player, hints);

        foreach (var i in _adHocAOEs)
            hints.AddForbiddenZone(i.Shape, i.Origin, i.Rotation, i.Activation);
    }
}
