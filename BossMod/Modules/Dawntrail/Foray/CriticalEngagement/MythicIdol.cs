namespace BossMod.Dawntrail.Foray.CriticalEngagement.MythicIdol;

public enum OID : uint
{
    Boss = 0x469B, // R4.600, x1
    MythicMirror = 0x469C, // R2.990, x8
    Helper = 0x233C, // R0.500, x55, Helper type
}

public enum AID : uint
{
    AutoAttack = 41536, // Boss->player, no cast, single-target
    MysticHeat = 41137, // Boss->self, 5.0s cast, range 40 60-degree cone
    ShiftingShape = 41126, // Boss->self, 3.0s cast, single-target
    Unk1 = 41127, // Helper->location, no cast, single-target
    Unk2 = 41128, // Boss->self, no cast, single-target
    Unk3 = 41129, // MythicMirror->self, no cast, single-target
    BigBurst = 41130, // MythicMirror->self, 8.0s cast, range 26 circle
    DeathRay = 41133, // MythicMirror->self, 8.0s cast, range 60 90-degree cone
    SteelstrikeHelper = 41132, // Helper->self, 8.0s cast, range 100 width 10 cross
    Steelstrike = 41131, // MythicMirror->self, 8.0s cast, range 100 width 10 cross
    ArcaneDesign = 41134, // Boss->self, 3.0s cast, single-target
    ArcaneOrbAppear = 41135, // Helper->location, 1.0s cast, range 6 circle
    ArcaneOrb = 41136, // Helper->location, no cast, range 6 circle
    LotsCastCast = 41138, // Boss->self, 5.0s cast, single-target
    LotsCastHelper = 41764, // Helper->self, no cast, ???
    LostCastSpread = 41140, // Helper->player, 6.0s cast, range 6 circle
    ArcaneLight = 41141, // Boss->self, 5.0s cast, single-target
    ArcaneLightHelper = 41142, // Helper->self, no cast, ???
}

class ArcaneLight(BossModule module) : Components.RaidwideCast(module, AID.ArcaneLight);
class MysticHeat(BossModule module) : Components.StandardAOEs(module, AID.MysticHeat, new AOEShapeCone(40, 30.Degrees()));
class BigBurst(BossModule module) : Components.StandardAOEs(module, AID.BigBurst, new AOEShapeCircle(26));
class DeathRay(BossModule module) : Components.StandardAOEs(module, AID.DeathRay, new AOEShapeCone(60, 45.Degrees()));
class Steelstrike(BossModule module) : Components.GroupedAOEs(module, [AID.SteelstrikeHelper, AID.Steelstrike], new AOEShapeCross(100, 5));
class LotsCast(BossModule module) : Components.SpreadFromCastTargets(module, AID.LostCastSpread, 6);

class ArcaneOrb(BossModule module) : Components.GenericAOEs(module, AID.ArcaneOrb)
{
    private readonly List<(WPos Position, DateTime Activation)> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(24).Select(p => new AOEInstance(new AOEShapeCircle(6), p.Position, default, p.Activation));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ArcaneOrbAppear)
            _predicted.Add((spell.TargetXZ, WorldState.FutureTime(8.2f)));

        if (spell.Action == WatchedAction)
            _predicted.RemoveAll(p => p.Position.AlmostEqual(spell.TargetXZ, 0.5f));
    }
}

class MythicIdolStates : StateMachineBuilder
{
    public MythicIdolStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArcaneLight>()
            .ActivateOnEnter<MysticHeat>()
            .ActivateOnEnter<BigBurst>()
            .ActivateOnEnter<DeathRay>()
            .ActivateOnEnter<Steelstrike>()
            .ActivateOnEnter<LotsCast>()
            .ActivateOnEnter<ArcaneOrb>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13726, DevOnly = true)]
public class MythicIdol(WorldState ws, Actor primary) : BossModule(ws, primary, new(-800, 245), new ArenaBoundsCircle(24.5f))
{
    public override bool DrawAllPlayers => true;
}

