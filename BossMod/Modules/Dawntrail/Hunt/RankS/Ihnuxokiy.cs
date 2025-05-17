namespace BossMod.Dawntrail.Hunt.RankS.Ihnuxokiy;

public enum OID : uint
{
    Boss = 0x4582, // R7.000, x1
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Teleport = 39772, // Boss->location, no cast, single-target
    AbyssalSmogShort = 39773, // Boss->self, 8.0s cast, range 40 180-degree cone (without forced march)
    AbyssalSmogLong = 39828, // Boss->self, 10.0s cast, range 40 180-degree cone (with forced march)
    ChaoticStorm = 39777, // Boss->self, 5.0s cast, range 40 circle, raidwide that applies forced march
    AetherstockThunderspark = 39778, // Boss->self, 4.0s cast, single-target, visual (circle after next cleave)
    AetherstockCyclonicRing = 39779, // Boss->self, 4.0s cast, single-target, visual (donut after next cleave)
    Thunderspark = 39780, // Boss->self, no cast, range 12 circle
    CyclonicRing = 39781, // Boss->self, no cast, range 10-40 donut
    RazorZephyr = 39774, // Boss->self, 3.0s cast, range 50 width 12 rect
    Blade = 39776, // Boss->player, 5.0s cast, single-target, tankbuster
}

public enum SID : uint
{
    ForwardMarch = 2161, // Boss->player, extra=0x0
    AboutFace = 2162, // Boss->player, extra=0x0
    LeftFace = 2163, // Boss->player, extra=0x0
    RightFace = 2164, // Boss->player, extra=0x0
    ForcedMarch = 1257, // Boss->player, extra=0x1/0x2/0x8/0x4
    Aetherstock = 4136, // Boss->Boss, extra=0x0
}

class AbyssalSmogAetherstock(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private AOEShape? _nextFollowup;

    private static readonly AOEShapeCone _shapeCleave = new(40, 90.Degrees());
    private static readonly AOEShapeCircle _shapeOut = new(12);
    private static readonly AOEShapeDonut _shapeIn = new(10, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AbyssalSmogShort:
            case AID.AbyssalSmogLong:
                _aoes.Add(new(_shapeCleave, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                if (_nextFollowup != null)
                {
                    _aoes.Add(new(_nextFollowup, caster.Position, default, Module.CastFinishAt(spell, 2.5f)));
                    _nextFollowup = null;
                }
                break;
            case AID.AetherstockThunderspark:
                _nextFollowup = _shapeOut;
                break;
            case AID.AetherstockCyclonicRing:
                _nextFollowup = _shapeIn;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AbyssalSmogShort or AID.AbyssalSmogLong or AID.Thunderspark or AID.CyclonicRing && _aoes.Count > 0)
            _aoes.RemoveAt(0);
    }
}

class ChaoticStormRaidwide(BossModule module) : Components.RaidwideCast(module, AID.ChaoticStorm, "Raidwide + apply forced march");
class ChaoticStormMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace);
class RazorZephyr(BossModule module) : Components.StandardAOEs(module, AID.RazorZephyr, new AOEShapeRect(50, 6));
class Blade(BossModule module) : Components.SingleTargetCast(module, AID.Blade);

class IhnuxokiyStates : StateMachineBuilder
{
    public IhnuxokiyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AbyssalSmogAetherstock>()
            .ActivateOnEnter<ChaoticStormRaidwide>()
            .ActivateOnEnter<ChaoticStormMarch>()
            .ActivateOnEnter<RazorZephyr>()
            .ActivateOnEnter<Blade>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 13444)]
public class Ihnuxokiy(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
