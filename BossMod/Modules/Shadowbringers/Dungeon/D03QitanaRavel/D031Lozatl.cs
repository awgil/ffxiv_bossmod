namespace BossMod.Shadowbringers.Dungeon.D03QitanaRavel.D031Lozatl;

public enum OID : uint
{
    Boss = 0x27AF, //R=4.4
    Helper = 0x233C, //R=0.5
}

public enum AID : uint
{
    AutoAttack = 872, // 27AF->player, no cast, single-target
    Stonefist = 15497, // 27AF->player, 4.0s cast, single-target
    SunToss = 15498, // 27AF->location, 3.0s cast, range 5 circle
    LozatlsScorn = 15499, // 27AF->self, 3.0s cast, range 40 circle
    RonkanLightRight = 15500, // 233C->self, no cast, range 60 width 20 rect
    RonkanLightLeft = 15725, // 233C->self, no cast, range 60 width 20 rect
    HeatUp = 15502, // 27AF->self, 3.0s cast, single-target
    HeatUp2 = 15501, // 27AF->self, 3.0s cast, single-target
    LozatlsFuryA = 15504, // 27AF->self, 4.0s cast, range 60 width 20 rect
    LozatlsFuryB = 15503, // 27AF->self, 4.0s cast, range 60 width 20 rect
}

class LozatlsFuryA(BossModule module) : Components.SelfTargetedAOEs(module, AID.LozatlsFuryA, new AOEShapeRect(60, 20, 0, 90.Degrees())); // TODO: verify; there should not be an offset in reality here..., also double halfwidth is strange
class LozatlsFuryB(BossModule module) : Components.SelfTargetedAOEs(module, AID.LozatlsFuryB, new AOEShapeRect(60, 20, 0, -90.Degrees())); // TODO: verify; there should not be an offset in reality here..., also double halfwidth is strange
class Stonefist(BossModule module) : Components.SingleTargetDelayableCast(module, AID.Stonefist);
class LozatlsScorn(BossModule module) : Components.RaidwideCast(module, AID.LozatlsScorn);
class SunToss(BossModule module) : Components.LocationTargetedAOEs(module, AID.SunToss, 5);

class RonkanLight(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(60, 20); //TODO: double halfwidth is strange
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008)
        {
            if (actor.Position.AlmostEqual(new(8, 328), 1))
                _aoe = new(rect, Module.Center, 90.Degrees(), WorldState.FutureTime(8));
            if (actor.Position.AlmostEqual(new(-7, 328), 1))
                _aoe = new(rect, Module.Center, -90.Degrees(), WorldState.FutureTime(8));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RonkanLightLeft or AID.RonkanLightRight)
            _aoe = null;
    }
}

class D031LozatlStates : StateMachineBuilder
{
    public D031LozatlStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LozatlsFuryA>()
            .ActivateOnEnter<LozatlsFuryB>()
            .ActivateOnEnter<Stonefist>()
            .ActivateOnEnter<SunToss>()
            .ActivateOnEnter<RonkanLight>()
            .ActivateOnEnter<LozatlsScorn>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 651, NameID = 8231)]
public class D031Lozatl(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 315), new ArenaBoundsCircle(20));
