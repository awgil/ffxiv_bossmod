namespace BossMod.Dawntrail.Dungeon.D01Ihuykatumu.D013Apollyon;

public enum OID : uint
{
    Boss = 0x4165, // R7.000, x1
    Helper = 0x233C, // R0.500, x20 (spawn during fight), Helper type
    IhuykatumuOcelot = 0x4166, // R3.570, x0 (spawn during fight) - add to be eaten by boss
    IhuykatumuPuma = 0x4167, // R2.520, x0 (spawn during fight) - add to be eaten by boss
    IhuykatumuSandworm1 = 0x4168, // R3.300, x0 (spawn during fight) - add to be eaten by boss
    IhuykatumuSandworm2 = 0x4169, // R3.300, x0 (spawn during fight) - add to be eaten by boss
    Whirlwind = 0x416C, // R1.000, x0 (spawn during fight)
    LevinsickleVoidzone = 0x1EBA21, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttackAdd = 872, // IhuykatumuOcelot/IhuykatumuPuma/IhuykatumuSandworm1/IhuykatumuSandworm2->player/Boss, no cast, single-target
    SandwormAppear = 36354, // IhuykatumuSandworm1/IhuykatumuSandworm2->self, no cast, single-target, visual (spawn)
    Teleport = 36344, // Boss->location, no cast, single-target
    RazorZephyr = 36340, // Boss->self, 4.0s cast, range 50 width 12 rect
    BladeSingle = 36347, // Boss->player, 4.5s cast, single-target, tankbuster
    BladeMulti = 36356, // Boss->player, 4.5s cast, single-target, visual (aoe tankbuster)
    BladeMultiAOE = 36357, // Helper->player, 5.0s cast, range 6 circle tankbuster
    HighWind = 36341, // Boss->self, 5.0s cast, range 60 circle, raidwide that kills adds
    Devour = 36342, // Boss->self, no cast, single-target, visual (eat dead adds)
    SwarmingLocust = 36343, // Boss->self, 3.0s cast, single-target, visual (jumps + aoes)
    BladesOfFamine = 36345, // Boss->self, 2.2+0.8s cast, single-target, visual (aoes after jumps)
    BladesOfFamineAOE = 36346, // Helper->self, 3.0s cast, range 50 width 12 rect
    Levinsickle = 36348, // Boss->self, 4.5+0.5s cast, single-target, visual (puddles + cones)
    LevinsickleAOESpark = 36349, // Helper->location, 5.0s cast, range 4 circle, puddle that is followed by cones
    LevinsickleAOENormal = 36350, // Helper->location, 5.0s cast, range 4 circle, puddle
    WingOfLightning = 36351, // Helper->self, 8.0s cast, range 40 45-degree cone
    ThunderIII = 36352, // Boss->self, 4.0+1.0s cast, single-target, visual (spread)
    ThunderIIIAOE = 36353, // Helper->player, 5.0s cast, range 6 circle spread
    WindSickle = 36358, // Helper->self, 4.0s cast, range 5-60 donut
    RazorStorm = 36355, // Boss->self, 5.0s cast, range 40 width 40 rect
    Windwhistle = 36359, // Boss->self, 4.0s cast, single-target, visual (spawn whirlwind)
    CuttingWind = 36360, // Helper->self, no cast, range 72 width 8 rect
}

public enum IconID : uint
{
    BladeSingle = 218, // player
    ThunderIII = 108, // player
    BladeMulti = 344, // player
    CuttingWind = 506, // Whirlwind
}

class RazorZephyr(BossModule module) : Components.StandardAOEs(module, AID.RazorZephyr, new AOEShapeRect(50, 6));
class BladeSingle(BossModule module) : Components.SingleTargetCast(module, AID.BladeSingle);
class BladeMulti(BossModule module) : Components.BaitAwayCast(module, AID.BladeMultiAOE, new AOEShapeCircle(6), true);
class HighWind(BossModule module) : Components.RaidwideCast(module, AID.HighWind);
class BladesOfFamine(BossModule module) : Components.StandardAOEs(module, AID.BladesOfFamineAOE, new AOEShapeRect(50, 6));
class LevinsickleSpark(BossModule module) : Components.StandardAOEs(module, AID.LevinsickleAOESpark, 4);
class LevinsickleNormal(BossModule module) : Components.StandardAOEs(module, AID.LevinsickleAOENormal, 4);
class LevinsickleVoidzone(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 4, AID.LevinsickleAOESpark, m => m.Enemies(OID.LevinsickleVoidzone).Where(x => x.EventState != 7), 0.8f);
class WingOfLightning(BossModule module) : Components.StandardAOEs(module, AID.WingOfLightning, new AOEShapeCone(40, 22.5f.Degrees()), 8);
class ThunderIII(BossModule module) : Components.SpreadFromCastTargets(module, AID.ThunderIIIAOE, 6);
class WindSickle(BossModule module) : Components.StandardAOEs(module, AID.WindSickle, new AOEShapeDonut(5, 60));
class RazorStorm(BossModule module) : Components.StandardAOEs(module, AID.RazorStorm, new AOEShapeRect(40, 20));

class CuttingWind(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(36, 4, 36);
    private static readonly List<WPos> _northPositions = [new(-111.69f, 253.94f), new(-102.28f, 264.31f), new(-108.92f, 276.53f)];
    private static readonly List<WPos> _southPositions = [new(-102.93f, 274.36f), new(-108.68f, 262.22f), new(-105.73f, 252.34f)];
    private static readonly float[] _castTimers = [8.9f, 16.9f, 24.9f];
    private static readonly List<Angle> _rotations = [0.Degrees(), 45.Degrees(), 90.Degrees(), 135.Degrees()];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(4);

    private void AddAOEs(WPos pos, float delay)
    {
        foreach (var angle in _rotations)
            _aoes.Add(new(_shape, pos, angle, Module.WorldState.FutureTime(delay)));
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Whirlwind)
        {
            var coords = actor.Position.Z < Module.Center.Z ? _northPositions : _southPositions;
            foreach (var (c, d) in coords.Zip(_castTimers))
                AddAOEs(c, d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.CuttingWind)
            _aoes.RemoveAt(0);
    }
}

class Whirlwind(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.Whirlwind), 30);

class D013ApollyonStates : StateMachineBuilder
{
    public D013ApollyonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RazorZephyr>()
            .ActivateOnEnter<BladeSingle>()
            .ActivateOnEnter<BladeMulti>()
            .ActivateOnEnter<HighWind>()
            .ActivateOnEnter<BladesOfFamine>()
            .ActivateOnEnter<LevinsickleSpark>()
            .ActivateOnEnter<LevinsickleNormal>()
            .ActivateOnEnter<LevinsickleVoidzone>()
            .ActivateOnEnter<WingOfLightning>()
            .ActivateOnEnter<ThunderIII>()
            .ActivateOnEnter<WindSickle>()
            .ActivateOnEnter<RazorStorm>()
            .ActivateOnEnter<CuttingWind>()
            .ActivateOnEnter<Whirlwind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 826, NameID = 12711)]
public class D013Apollyon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-107, 265), new ArenaBoundsCircle(20));
