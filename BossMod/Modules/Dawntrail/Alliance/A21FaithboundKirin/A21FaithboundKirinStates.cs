namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

class StonegaIV(BossModule module) : Components.RaidwideCast(module, AID._Spell_StonegaIV);

class SynchronizedStrike(BossModule module) : Components.StandardAOEs(module, AID._Ability_SynchronizedStrike1, new AOEShapeRect(60, 5));
class SynchronizedSmite(BossModule module) : Components.GroupedAOEs(module, [AID._Ability_SynchronizedSmite, AID._Ability_SynchronizedSmite1], new AOEShapeRect(60, 16));
class CrimsonRiddle(BossModule module) : Components.GroupedAOEs(module, [AID._Ability_CrimsonRiddle, AID._Ability_CrimsonRiddle1], new AOEShapeCone(30, 90.Degrees()));

enum Summon
{
    None,
    Seiryu,
    Genbu,
    Suzaku,
    Byakko
}


class SummonShijin(BossModule module) : BossComponent(module)
{
    public Summon NextSummon { get; private set; }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        switch (((OID)actor.OID, id))
        {
            case (OID._Gen_DawnboundSeiryu, 0x11D1):
                NextSummon = Summon.Seiryu;
                break;
            case (OID._Gen_MoonboundGenbu, 0x11D1):
                NextSummon = Summon.Genbu;
                break;
            case (OID._Gen_SunboundSuzaku, 0x11D2):
                NextSummon = Summon.Suzaku;
                break;
            case (OID._Gen_DuskboundByakko, 0x11D5):
                NextSummon = Summon.Byakko;
                break;
            default: break;
        }
    }
}

class MoontideFont(BossModule module) : Components.GroupedAOEs(module, [AID._Ability_MoontideFont, AID._Ability_MoontideFont1], new AOEShapeCircle(9), maxCasts: 11);
class MidwinterMarch(BossModule module) : Components.StandardAOEs(module, AID._Ability_MidwinterMarch1, 12);
class NorthernCurrent(BossModule module) : Components.StandardAOEs(module, AID._Ability_NorthernCurrent, new AOEShapeDonut(12, 60));
class Wringer(BossModule module) : Components.StandardAOEs(module, AID._Ability_Wringer, 14);
class DeadWringer(BossModule module) : Components.StandardAOEs(module, AID._Ability_DeadWringer, new AOEShapeDonut(14, 30));
class StrikingRightLeft(BossModule module) : Components.GroupedAOEs(module, [AID._Ability_StrikingRight2, AID._Ability_StrikingLeft], new AOEShapeCircle(10));
class SmitingRightLeft(BossModule module) : Components.GroupedAOEs(module, [AID._Ability_SmitingRight1, AID._Ability_SmitingLeft], new AOEShapeCircle(30));

class A21FaithboundKirinStates : StateMachineBuilder
{
    public A21FaithboundKirinStates(BossModule module) : base(module)
    {
        DeathPhase(0, P1)
            .ActivateOnEnter<SummonShijin>();
    }

    private void P1(uint id)
    {
        Cast(id, AID._Spell_StonegaIV, 7.1f, 5, "Raidwide")
            .ActivateOnEnter<StonegaIV>()
            .DeactivateOnExit<StonegaIV>();

        Cast(id + 0x100, AID._Ability_WroughtArms, 7.9f, 3.4f);

        Synchro1(id + 0x10000, 6.5f);
        Synchro1(id + 0x10100, 2.5f);

        Cast(id + 0x10200, AID._Ability_CrimsonRiddle, 6.5f, 5, "Half-room cleave 1")
            .ActivateOnEnter<CrimsonRiddle>();
        Cast(id + 0x10300, AID._Ability_CrimsonRiddle1, 2.1f, 5, "Half-room cleave 2")
            .DeactivateOnExit<CrimsonRiddle>();

        Cast(id + 0x20000, AID._Ability_SummonShijin, 7.2f, 7, "Summon primal")
            .ActivateOnEnter<MoontideFont>()
            .ActivateOnEnter<MidwinterMarch>()
            .ActivateOnEnter<NorthernCurrent>();

        Cast(id + 0x30000, AID._Ability_CrimsonRiddle, 36.4f, 5, "Half-room cleave")
            .ActivateOnEnter<CrimsonRiddle>()
            .DeactivateOnExit<CrimsonRiddle>();

        Cast(id + 0x30100, AID._Ability_WroughtArms, 7.2f, 3.4f);
        Cast(id + 0x30200, AID._Ability_Wringer, 3.6f, 4.9f, "Circle AOE")
            .ActivateOnEnter<Wringer>()
            .ActivateOnEnter<DeadWringer>();

        Cast(id + 0x30300, AID._Ability_StrikingRight2, 9.7f, 4.9f, "Circle AOE")
            .ActivateOnEnter<StrikingRightLeft>()
            .ActivateOnEnter<SmitingRightLeft>();

        Cast(id + 0x40000, AID._Spell_StonegaIV, 5.4f, 5, "Raidwide")
            .ActivateOnEnter<StonegaIV>()
            .DeactivateOnExit<StonegaIV>();

        Timeout(id + 0xFF0000, 9999, "???");
    }

    private void Synchro1(uint id, float delay)
    {
        CastStart(id, AID._Ability_SynchronizedStrike, delay)
            .ActivateOnEnter<SynchronizedStrike>()
            .ActivateOnEnter<SynchronizedSmite>();

        ComponentCondition<SynchronizedStrike>(id + 0x10, 4.9f, s => s.NumCasts > 0, "Boss AOE");
        ComponentCondition<SynchronizedSmite>(id + 0x20, 4.5f, s => s.NumCasts > 0, "Arm AOE")
            .DeactivateOnExit<SynchronizedStrike>()
            .DeactivateOnExit<SynchronizedSmite>();
    }
}
