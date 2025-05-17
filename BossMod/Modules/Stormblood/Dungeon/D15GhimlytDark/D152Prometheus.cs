namespace BossMod.Stormblood.Dungeon.D15GhimlytDark.D152Prometheus;

public enum OID : uint
{
    Boss = 0x2515, // R7.8
    TunnelHelper = 0x1EA1A1, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 15046, // Boss->player, no cast, single-target

    FreezingMissileBoss = 13403, // Boss->self, 3.0s cast, single-target
    FreezingMissileHelper = 13404, // Helper->location, 3.5s cast, range 40 circle // actually 8, but
    Heat = 13400, // Helper->self, no cast, range 100+R width 16 rect
    NeedleGun = 13402, // Boss->self, 4.5s cast, range 40+R 90-degree cone
    Nitrospin = 13397, // Boss->self, 4.5s cast, range 50 circle
    OilShower = 13398, // Boss->self, 4.5s cast, range 40+R 270-degree cone
    Tunnel = 13399, // Boss->self, no cast, single-target
    UnbreakableCermetDrill = 13401, // Boss->player, 3.0s cast, single-target
}

class FreezingMissile(BossModule module) : Components.StandardAOEs(module, AID.FreezingMissileHelper, 8);
class NeedleGun(BossModule module) : Components.StandardAOEs(module, AID.NeedleGun, new AOEShapeCone(47.8f, 45.Degrees()));
class Nitrospin(BossModule module) : Components.RaidwideCast(module, AID.Nitrospin);
class OilShower(BossModule module) : Components.StandardAOEs(module, AID.OilShower, new AOEShapeCone(47.8f, 135.Degrees()));
class UnbreakableCermetDrill(BossModule module) : Components.SingleTargetDelayableCast(module, AID.UnbreakableCermetDrill);

class Heat(BossModule module) : Components.GenericAOEs(module) // borrowed this from the fork team, credit where it's due
{
    private static readonly AOEShapeRect rect = new(107.8f, 8);

    private static readonly Dictionary<Angle, (WPos origin, Angle rotation)> aoeSources = new()
    {
        {68.Degrees(), (new(79.481f, -57.598f), 67.482f.Degrees())},
        {-113.Degrees(), (new(188.498f, -12.441f), -112.488f.Degrees())},
        {113.Degrees(), (new(79.481f, -12.441f), 112.477f.Degrees())},
        {23.Degrees(), (new(111.411f, -89.528f), 22.498f.Degrees())},
        {157.Degrees(), (new(111.411f, 19.489f), 157.483f.Degrees())},
        {-157.Degrees(), (new(156.568f, 19.489f), -157.505f.Degrees())},
        {-23.Degrees(), (new(156.568f, -89.528f), -22.487f.Degrees())},
        {-68.Degrees(), (new(188.498f, -57.598f), -67.482f.Degrees())},
    };

    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008 && (OID)actor.OID == OID.TunnelHelper)
        {
            foreach (var r in aoeSources.Keys)
                if (actor.Rotation.AlmostEqual(r, Angle.DegToRad))
                {
                    _aoe = new(rect, aoeSources[r].origin, aoeSources[r].rotation, WorldState.FutureTime(6.7f));
                    break;
                }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Heat)
        {
            if (++NumCasts == 5)
            {
                _aoe = null;
                NumCasts = 0;
            }
        }
    }
}

class D152PrometheusStates : StateMachineBuilder
{
    public D152PrometheusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FreezingMissile>()
            .ActivateOnEnter<NeedleGun>()
            .ActivateOnEnter<Nitrospin>()
            .ActivateOnEnter<OilShower>()
            .ActivateOnEnter<UnbreakableCermetDrill>()
            .ActivateOnEnter<Heat>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 611, NameID = 7856)]
public class D152Prometheus(WorldState ws, Actor primary) : BossModule(ws, primary, new(133.9f, -35.02f), new ArenaBoundsCircle(21));
