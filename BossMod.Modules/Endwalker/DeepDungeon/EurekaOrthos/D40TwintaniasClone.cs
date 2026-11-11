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

class Twister(BossModule module) : Components.CastTwister(module, 1.25f, (uint)OID.Twister, AID.TwisterVisual, 0.3f, 0.6f, 2.25f);
class BitingWind(BossModule module) : Components.VoidzoneAtCastTarget(module, 5, AID.Gust, OID.BitingWind, 0.9f);
class MeracydianSquall(BossModule module) : Components.StandardAOEs(module, AID.MeracydianSquall, 5);
class DiveTwister(BossModule module) : Components.CastTwister(module, 1.25f, (uint)OID.Twister, AID.TwistingDive, 0.3f, 0.6f, 2.25f);

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

class Turbine(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Turbine, 15);

class D40TwintaniasCloneStates : StateMachineBuilder
{
    public D40TwintaniasCloneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Twister>()
            .ActivateOnEnter<BitingWind>()
            .ActivateOnEnter<MeracydianSquall>()
            .ActivateOnEnter<Turbine>()
            .ActivateOnEnter<TwistingDive>()
            .ActivateOnEnter<DiveTwister>();
    }
}

[ModuleInfo(Contributors = "LegendofIceman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 900, NameID = 12263)]
public class D40TwintaniasClone(ModuleInit init) : BossModule(init, new(-600, -300), new ArenaBoundsCircle(20));
