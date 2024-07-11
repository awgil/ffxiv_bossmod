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
class LightningHelper(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.LightningAOE));
class ThunderIII(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.ThunderIII), 6);
class BladeAOE(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.BladeAOE), new AOEShapeCircle(6), centerAtTarget: true);
class WindSickle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WindSickle), new AOEShapeDonut(6, 60));
class RazorStorm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RazorStorm), new AOEShapeRect(40, 20));
class Levinsickle(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Levinsickle), 4);
class LevinsickleSpark(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.LevinsickleSpark), 4);

// first aoe is 10 seconds after windwhistle
// rest are 8 seconds after previous
class Whirlwind(BossModule module) : Components.GenericAOEs(module)
{
    private int _activations;
    private DateTime _nextActivation;

    private static readonly List<Angle> Rotations = [0.Degrees(), 45.Degrees(), 90.Degrees(), 135.Degrees()];

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Windwhistle)
            _nextActivation = WorldState.FutureTime(10);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var c in ActiveAOEs(pcSlot, pc))
        {
            c.Shape.Draw(Arena, c.Origin, c.Rotation, c.Color);
            c.Shape.Outline(Arena, c.Origin, c.Rotation, ArenaColor.AOE);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Cuttingwind)
        {
            _activations += 1;
            _nextActivation = WorldState.FutureTime(8);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        _activations = 0;
        _nextActivation = default;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activations >= 12)
            yield break;

        var whirlwind = Module.Enemies(OID.Whirlwind).FirstOrDefault();
        if (whirlwind == null)
            yield break;

        var whirlyHelper = Module.Enemies(OID.Helper).FirstOrDefault(x => x.NameID == 12715);
        if (whirlyHelper == null)
            yield break;

        foreach (var angle in Rotations)
        {
            yield return new AOEInstance(new AOEShapeRect(72, 4, 72), whirlwind.Position, angle, _nextActivation, Shade(_nextActivation), _nextActivation < WorldState.FutureTime(4));
        }
    }

    private uint Shade(DateTime activation)
    {
        var clampedETA = Math.Clamp((activation - WorldState.CurrentTime).TotalSeconds, 0, 4);
        var opacity = 1 - clampedETA / 4;
        var alpha = (uint)(opacity * 96) + 32;
        return 0x008080 + alpha * 0x1000000;
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
            .ActivateOnEnter<Whirlwind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 826, NameID = 12711)]
public class D013Apollyon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-107, 265), new ArenaBoundsCircle(20));
