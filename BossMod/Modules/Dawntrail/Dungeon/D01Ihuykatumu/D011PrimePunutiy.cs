namespace BossMod.Dawntrail.Dungeon.D01Ihuykatumu.D011PrimePunutiy;

public enum OID : uint
{
    Boss = 0x4190, // R7.990, x1
    Helper = 0x233C, // R0.500, x16 (spawn during fight), 523 type
    Actor1eba45 = 0x1EBA45, // R0.500, x0 (spawn during fight), EventObj type
    Actor1eba43 = 0x1EBA43, // R0.500, x0 (spawn during fight), EventObj type
    Actor1eba46 = 0x1EBA46, // R0.500, x0 (spawn during fight), EventObj type
    Actor1eba44 = 0x1EBA44, // R0.500, x0 (spawn during fight), EventObj type
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    Actor1ebca9 = 0x1EBCA9, // R0.500, x1, EventObj type
    IhuykatumuFlytrap = 0x4194, // R1.600, x0 (spawn during fight)
    ProdigiousPunutiy = 0x4191, // R4.230, x0 (spawn during fight)
    Punutiy = 0x4192, // R2.820, x0 (spawn during fight)
    PetitPunutiy = 0x4193, // R2.115, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/PetitPunutiy/Punutiy/ProdigiousPunutiy->player, no cast, single-target

    PunutiyPress = 36492, // Boss->self, 5.0s cast, range 60 circle // raidwide

    Hydrowave = 36493, // Boss->self, 4.0s cast, range 60 30.000-degree cone
    Inhale = 36496, // Helper->self, no cast, range 100 ?-degree cone

    Resurface1 = 36494, // Boss->self, 5.0s cast, range 100 60.000-degree cone
    Resurface2 = 36495, // Boss->self, 7.0s cast, single-target

    Bury1 = 36497, // Helper->self, 4.0s cast, range 12 circle
    Bury2 = 36500, // Helper->self, 4.0s cast, range 35 width 10 rect
    Bury3 = 36498, // Helper->self, 4.0s cast, range 8 circle
    Bury4 = 36501, // Helper->self, 4.0s cast, range 4 circle
    Bury5 = 36499, // Helper->self, 4.0s cast, range 25 width 6 rect
    Bury6 = 36502, // Helper->self, 4.0s cast, range 6 circle
    Bury7 = 36503, // Helper->self, 4.0s cast, range 25 width 6 rect
    Bury8 = 36504, // Helper->self, 4.0s cast, range 35 width 10 rect

    Decay = 36505, // IhuykatumuFlytrap->self, 7.0s cast, range ?-40 donut

    SongOfThePunutiy = 36506, // Boss->self, 5.0s cast, single-target
    UnknownAbility = 36510, // Helper->player, no cast, single-target

    PunutiyFlop1 = 36508, // ProdigiousPunutiy->player, 8.0s cast, range 14 circle
    PunutiyFlop2 = 36513, // PetitPunutiy->player, 8.0s cast, range 6 circle

    Hydrowave1 = 36509, // Punutiy->self, 8.0s cast, single-target
    Hydrowave2 = 36511, // Punutiy->self, no cast, single-target
    Hydrowave3 = 36512, // Helper->self, no cast, range 60 ?-degree cone

    ShoreShaker1 = 36514, // Boss->self, 4.0+1.0s cast, single-target
    ShoreShaker2 = 36515, // Helper->self, 5.0s cast, range 10 circle
    ShoreShaker3 = 36516, // Helper->self, 7.0s cast, range ?-20 donut
    ShoreShaker4 = 36517, // Helper->self, 9.0s cast, range ?-30 donut
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Helper->player, extra=0x1
}

public enum IconID : uint
{
    Stackmarker = 196, // player
    Icon505 = 505, // player
}

public enum TetherID : uint
{
    Tether17 = 17, // ProdigiousPunutiy/Punutiy/PetitPunutiy->player
}

class PunutiyPress(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PunutiyPress));
class Hydrowave(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Hydrowave), new AOEShapeCone(60, 15.Degrees()));
class Resurface1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Resurface1), new AOEShapeCone(100, 30.Degrees()));

class Bury1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Bury1), new AOEShapeCircle(12));
class Bury2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Bury2), new AOEShapeRect(35, 5));
class Bury3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Bury3), new AOEShapeCircle(8));
class Bury4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Bury4), new AOEShapeCircle(4));
class Bury5(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Bury5), new AOEShapeRect(25, 3));
class Bury6(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Bury6), new AOEShapeCircle(6));
class Bury7(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Bury7), new AOEShapeRect(25, 3));
class Bury8(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Bury8), new AOEShapeRect(35, 5));

class Decay(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Decay), new AOEShapeDonut(6, 40));

class PunutiyFlop1(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.PunutiyFlop1), 14);
class PunutiyFlop2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.PunutiyFlop2), 6);

class ShoreShaker2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ShoreShaker2), new AOEShapeCircle(10));
class ShoreShaker3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ShoreShaker3), new AOEShapeDonut(10, 20));
class ShoreShaker4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ShoreShaker4), new AOEShapeDonut(20, 30));

class D011PrimePunutiyStates : StateMachineBuilder
{
    public D011PrimePunutiyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PunutiyPress>()
            .ActivateOnEnter<Hydrowave>()
            .ActivateOnEnter<Resurface1>()
            .ActivateOnEnter<Bury1>()
            .ActivateOnEnter<Bury2>()
            .ActivateOnEnter<Bury3>()
            .ActivateOnEnter<Bury4>()
            .ActivateOnEnter<Bury5>()
            .ActivateOnEnter<Bury6>()
            .ActivateOnEnter<Bury7>()
            .ActivateOnEnter<Bury8>()
            .ActivateOnEnter<Decay>()
            .ActivateOnEnter<PunutiyFlop1>()
            .ActivateOnEnter<PunutiyFlop2>()
            .ActivateOnEnter<ShoreShaker2>()
            .ActivateOnEnter<ShoreShaker3>()
            .ActivateOnEnter<ShoreShaker4>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 826, NameID = 12723)]
public class D011PrimePunutiy(WorldState ws, Actor primary) : BossModule(ws, primary, new(35, -95), new ArenaBoundsSquare(20));
