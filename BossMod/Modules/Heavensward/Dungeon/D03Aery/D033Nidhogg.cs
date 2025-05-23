namespace BossMod.Heavensward.Dungeon.D03Aery.D033Nidhogg;

public enum OID : uint
{
    Boss = 0x39CA, // R12.000, x?
    TheSablePrice = 0x39CB, // R1.000, x?
    Liegedrake = 0x39CD, // R3.600, x?
    Ahleh = 0x39CC, // R5.000, x?
    Helper = 0x233C, // x3
}

public enum AID : uint
{
    AutoAttack = 872, // 1093->player, no cast, single-target
    DeafeningBellow = 30206, // 39CA->self, 5.0s cast, range 80 circle
    HotTail = 30196, // 39CA->self, 3.0s cast, single-target
    HotTail2 = 30197, // 233C->self, 3.5s cast, range 68 width 16 rect
    HotWing2 = 30194, // 39CA->self, 3.0s cast, single-target - Visual
    HotWing = 30195, // 233C->self, 3.5s cast, range 30 width 68 rect - Helpers
    Cauterize = 30198, // 39CA->self, 5.0s cast, range 80 width 22 rect
    HorridRoar2 = 30200, // 233C->location, 4.0s cast, range 6 circle
    HorridRoar = 30202, // 233C->players, 5.0s cast, range 6 circle
    HorridBlaze = 30224, // Stack
    Massacre = 30207, // 39CA->self, 6.0s cast, range 80 circle
    Touchdown = 30199, // 39CA->self, no cast, range 80 circle

    SableWeave = 30204, // 39CB->player, 15.0s cast, single-target
    TheSablePrice = 30203, // 39CA->self, 3.0s cast, single-target
    TheScarletPrice = 30205, // 39CA->player, 5.0s cast, single-target

    //Ahleh
    Attack = 870, // 109B/1091/1090/109C/11AE/39CA/39CD/39CC->player, no cast, single-target
    Roast = 30209, // 39CC->self, 4.0s cast, range 30 width 8 rect
}

class DeafeningBellow(BossModule module) : Components.RaidwideCast(module, AID.DeafeningBellow);
class HotTail(BossModule module) : Components.StandardAOEs(module, AID.HotTail, new AOEShapeRect(68, 8));
class HotWing(BossModule module) : Components.StandardAOEs(module, AID.HotWing, new AOEShapeRect(30, 34));
class Cauterize(BossModule module) : Components.StandardAOEs(module, AID.Cauterize, new AOEShapeRect(80, 11));
class HorridRoar(BossModule module) : Components.SpreadFromCastTargets(module, AID.HorridRoar, 6);
class HorridRoar2(BossModule module) : Components.StandardAOEs(module, AID.HorridRoar2, 6);
class HorridBlaze(BossModule module) : Components.StackWithCastTargets(module, AID.HorridBlaze, 6, 2);
class Massacre(BossModule module) : Components.RaidwideCast(module, AID.Massacre);
class Touchdown(BossModule module) : Components.RaidwideInstant(module, AID.Touchdown, 7.3f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.Massacre)
            Activation = WorldState.FutureTime(7.3f);
    }
}
class TheScarletPrice(BossModule module) : Components.SingleTargetCast(module, AID.TheScarletPrice);

class MultiAddModule(BossModule module) : Components.AddsMulti(module, [OID.TheSablePrice, OID.Liegedrake, OID.Ahleh])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PrioritizeTargetsByOID(OID.TheSablePrice, 3);
        hints.PrioritizeTargetsByOID([(uint)OID.Liegedrake, (uint)OID.Ahleh], 2);
    }
};
class Roast(BossModule module) : Components.StandardAOEs(module, AID.Roast, new AOEShapeRect(30, 4));

class D033NidhoggStates : StateMachineBuilder
{
    public D033NidhoggStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeafeningBellow>()
            .ActivateOnEnter<HotTail>()
            .ActivateOnEnter<HotWing>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<HorridRoar>()
            .ActivateOnEnter<HorridRoar2>()
            .ActivateOnEnter<HorridBlaze>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<Massacre>()
            .ActivateOnEnter<Roast>()
            .ActivateOnEnter<MultiAddModule>()
            .ActivateOnEnter<TheScarletPrice>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 39, NameID = 3458)]
public class D033Nidhogg(WorldState ws, Actor primary) : BossModule(ws, primary, new(34.9f, -267f), new ArenaBoundsCircle(30f));
