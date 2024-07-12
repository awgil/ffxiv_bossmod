namespace BossMod.Dawntrail.Dungeon.D01Ihuykatumu.D013Apollyon;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x?, 523 type
    Boss = 0x4165, // R7.000, x?
    LightningAOE = 0x1EBA21, // R0.500, x?, EventObj type
    Whirlwind = 0x416C, // R1.000, x?
}

public enum AID : uint
{
    RazorZephyr = 36340, // 4165->self, 4.0s cast, range 50 width 12 rect
    BladeST = 36347, // 4165->none, 4.5s cast, single-target
    HighWind = 36341, // 4165->self, 5.0s cast, range 60 circle
    BladesOfFamine = 36346, // 233C->self, 3.0s cast, range 50 width 12 rect
    LevinsickleSpark = 36349, // 233C->location, 5.0s cast, range 4 circle
    Levinsickle = 36350, // 233C->location, 5.0s cast, range 4 circle
    WingOfLightning = 36351, // 233C->self, 8.0s cast, range 40 ?-degree cone
    ThunderIII = 36353, // 233C->player, 5.0s cast, range 6 circle
    BladeAOE = 36357, // 233C->none, 5.0s cast, range 6 circle
    WindSickle = 36358, // 233C->self, 4.0s cast, range ?-60 donut
    RazorStorm = 36355, // 4165->self, 5.0s cast, range 40 width 40 rect
    Windwhistle = 36359, // 4165->self, 4.0s cast, single-target
    Cuttingwind = 36360, // 233C->self, no cast, range 72 width 8 rect
}

class RazorZephyr(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RazorZephyr), new AOEShapeRect(50, 6));
class Blade(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.BladeST));
class HighWind(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HighWind));
class BladesOfFamine(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BladesOfFamine), new AOEShapeRect(50, 6));
class WingOfLightning(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WingOfLightning), new AOEShapeCone(40, 22.5f.Degrees()), maxCasts: 8);
class LightningHelper(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.LightningAOE).Where(x => x.EventState != 7));
class ThunderIII(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.ThunderIII), 6);
class BladeAOE(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.BladeAOE), new AOEShapeCircle(6), centerAtTarget: true);
class WindSickle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WindSickle), new AOEShapeDonut(5, 60));
class RazorStorm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RazorStorm), new AOEShapeRect(40, 20));
class Levinsickle(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Levinsickle), 4);
class LevinsickleSpark(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.LevinsickleSpark), 4);

class CuttingWind(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect Shape = new(36, 4, 36);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(4);

    private readonly List<WPos> _northPositions = [new(-111.688f, 253.942f), new(-102.276f, 264.313f), new(-108.922f, 276.528f)];
    private readonly List<WPos> _southPositions = [new(-102.935f, 274.357f), new(-108.935f, 262.224f), new(-105.733f, 252.340f)];

    private static readonly float[] CastTimers = [8.9f, 16.9f, 24.9f];
    private static readonly List<Angle> Rotations = [0.Degrees(), 45.Degrees(), 90.Degrees(), 135.Degrees()];

    private void AddAOEs(WPos pos, float delay)
    {
        foreach (var angle in Rotations)
            _aoes.Add(new(Shape, pos, angle, Module.WorldState.FutureTime(delay)));
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Whirlwind)
        {
            var coords = actor.Position.Z < 265 ? _northPositions : _southPositions;
            foreach (var (c, d) in coords.Zip(CastTimers))
                AddAOEs(c, d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.Cuttingwind)
            _aoes.RemoveAt(0);
    }
}

class Whirlwind(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.Whirlwind))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Sources(Module).FirstOrDefault() is Actor w)
            hints.AddForbiddenZone(new AOEShapeRect(6, 4, 4), w.Position, w.Rotation);
    }
}

class D013ApollyonStates : StateMachineBuilder
{
    public D013ApollyonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RazorZephyr>()
            .ActivateOnEnter<Blade>()
            .ActivateOnEnter<HighWind>()
            .ActivateOnEnter<BladesOfFamine>()
            .ActivateOnEnter<WingOfLightning>()
            .ActivateOnEnter<LightningHelper>()
            .ActivateOnEnter<ThunderIII>()
            .ActivateOnEnter<BladeAOE>()
            .ActivateOnEnter<WindSickle>()
            .ActivateOnEnter<RazorStorm>()
            .ActivateOnEnter<Levinsickle>()
            .ActivateOnEnter<LevinsickleSpark>()
            .ActivateOnEnter<CuttingWind>()
            .ActivateOnEnter<Whirlwind>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 826, NameID = 12711)]
public class D013Apollyon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-107, 265), new ArenaBoundsCircle(20));
