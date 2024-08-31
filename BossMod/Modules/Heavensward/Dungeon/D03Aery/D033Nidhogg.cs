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
    Massacre = 30207,
    Touchdown = 30199, // 39CA->self, no cast, range 80 circle

    SableWeave = 30204, // 39CB->player, 15.0s cast, single-target
    TheSablePrice = 30203, // 39CA->self, 3.0s cast, single-target
    TheScarletPrice = 30205, // 39CA->player, 5.0s cast, single-target

    //Ahleh
    Attack = 870, // 109B/1091/1090/109C/11AE/39CA/39CD/39CC->player, no cast, single-target
    Roast = 30209, // 39CC->self, 4.0s cast, range 30 width 8 rect
}

class DeafeningBellow(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.DeafeningBellow), 4);
class HotTail(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HotTail), new AOEShapeRect(60, 8, 60));
//class HotTail2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HotTail2), new AOEShapeRect(-60, 8));
class HotWing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HotWing), new AOEShapeRect(30, 34, -4.5f));
class Cauterize(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Cauterize), new AOEShapeRect(80, 11));
class HorridRoar(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.HorridRoar), 6);
//class HorridRoar2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.HorridRoar2), 6);
class HorridBlaze(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.HorridBlaze), 6, 2);
class Touchdown(BossModule module) : Components.RaidwideInstant(module, ActionID.MakeSpell(AID.Touchdown), 5.1f);
class Massacre(BossModule module) : Components.RaidwideInstant(module, ActionID.MakeSpell(AID.Massacre), 5.1f);
class SableWeave(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.SableWeave));
class TheSablePrice(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.TheSablePrice));
class TheScarletPrice(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.TheScarletPrice));

class MultiAddModule(BossModule module) : Components.AddsMulti(module, [(uint)OID.TheSablePrice, (uint)OID.Liegedrake, (uint)OID.Ahleh])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.TheSablePrice => 4,
                OID.Liegedrake => 3,
                OID.Ahleh => 2,
                OID.Boss => 1,
                _ => 0
            };
    }
};
class Roast(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Roast), new AOEShapeRect(30, 4));

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
            .ActivateOnEnter<HorridBlaze>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<Massacre>()
            .ActivateOnEnter<SableWeave>()
            .ActivateOnEnter<TheSablePrice>()
            .ActivateOnEnter<TheScarletPrice>()
            .ActivateOnEnter<Roast>()
            .ActivateOnEnter<MultiAddModule>();

    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 39, NameID = 3458)]
public class D033Nidhogg(WorldState ws, Actor primary) : BossModule(ws, primary, new(34.9f, -267f), new ArenaBoundsCircle(30f));

