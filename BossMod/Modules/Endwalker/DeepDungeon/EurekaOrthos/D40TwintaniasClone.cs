namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.D40TwintaniasClone;

public enum OID : uint
{
    Boss = 0x3D1D, // R6.0
    Twister = 0x1E8910, // R0.5
    BitingWind = 0x3D1E, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    BitingWind = 31464, // BitingWind->self, no cast, range 5 circle
    Gust = 31463, // Helper->location, 4.0s cast, range 5 circle
    MeracydianCyclone = 31462, // Boss->self, 3.0s cast, single-target
    MeracydianSquall = 31466, // Helper->location, 5.0s cast, range 5 circle
    MeracydianSquallVisual = 31465, // Boss->self, 3.0s cast, single-target
    Turbine = 31467, // Boss->self, 6.0s cast, range 60 circle, knockback 15, away from source
    TwisterTouch = 31470, // Helper->player, no cast, single-target, player got hit by twister
    TwisterVisual = 31468, // Boss->self, 5.0s cast, single-target
    TwistingDive = 31471 // Boss->self, 5.0s cast, range 50 width 15 rect
}

class Twister(BossModule module) : Components.CastTwister(module, 1.5f, (uint)OID.Twister, AID.TwisterVisual, 0.4f, 0.25f);
class BitingWind(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID.Gust, m => m.Enemies(OID.BitingWind).Where(z => z.EventState != 7), 0.9f);
class MeracydianSquall(BossModule module) : Components.StandardAOEs(module, AID.MeracydianSquall, 5);
class TwistersHint(BossModule module, AID aid) : Components.CastHint(module, aid, "Twisters spawning, keep moving!");
class Twisters1(BossModule module) : TwistersHint(module, AID.TwisterVisual);
class Twisters2(BossModule module) : TwistersHint(module, AID.TwistingDive);
class DiveTwister(BossModule module) : Components.CastTwister(module, 1.5f, (uint)OID.Twister, AID.TwistingDive, 0.4f, 0.25f);

class TwistingDive(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeRect rect = new(50, 7.5f);
    private bool preparing;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor == Module.PrimaryActor)
        {
            if (id == 0x1E3A)
                preparing = true;
            else if (preparing && id == 0x1E43)
                _aoe = new(rect, actor.Position, actor.Rotation, WorldState.FutureTime(6.9f));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TwistingDive)
        {
            _aoe = null;
            preparing = false;
        }
    }
}

class Turbine(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Turbine, 15)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var forbidden = new List<Func<WPos, bool>>();
        var component = Module.FindComponent<BitingWind>()?.ActiveAOEs(slot, actor)?.ToList();
        var source = Sources(slot, actor).FirstOrDefault();
        if (source != default && component != null)
        {
            forbidden.Add(ShapeContains.InvertedCircle(Arena.Center, 5));
            forbidden.AddRange(component.Select(c => ShapeContains.Cone(Arena.Center, 20, Angle.FromDirection(c.Origin - Arena.Center), 20.Degrees())));
            if (forbidden.Count > 0)
                hints.AddForbiddenZone(p => forbidden.Any(f => f(p)), source.Activation);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<BitingWind>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.InBounds(pos);
}

class D40TwintaniasCloneStates : StateMachineBuilder
{
    public D40TwintaniasCloneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Twister>()
            .ActivateOnEnter<Twisters1>()
            .ActivateOnEnter<Twisters2>()
            .ActivateOnEnter<BitingWind>()
            .ActivateOnEnter<MeracydianSquall>()
            .ActivateOnEnter<Turbine>()
            .ActivateOnEnter<TwistingDive>()
            .ActivateOnEnter<DiveTwister>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 900, NameID = 12263)]
public class D40TwintaniasClone(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600, -300), new ArenaBoundsCircle(20));
