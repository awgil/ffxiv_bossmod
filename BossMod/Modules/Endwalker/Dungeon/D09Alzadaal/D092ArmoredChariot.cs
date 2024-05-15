namespace BossMod.Endwalker.Dungeon.D09Alzadaal.D092ArmoredChariot;

public enum OID : uint
{
    Boss = 0x386C, // R=6.4
    Helper = 0x233C, // R0.500, x23, 523 type
    Actor1eb69c = 0x1EB69C, // R0.500, x0 (spawn during fight), EventObj type
    Unknown = 0x3932, // R7.000, x0 (spawn during fight)
    ArmoredDrudge = 0x386D, // R1.800, x8
}

public enum AID : uint
{
    AutoAttack = 29132, // Boss->player, no cast, single-target

    ArticulatedBits = 28441, // Boss->self, 3.0s cast, range 6 circle //Persistent AOE under boss

    AssaultCannonVisual1First = 28442, // ArmoredDrudge->self, 8.0s cast, single-target
    AssaultCannonVisual2First = 28443, // ArmoredDrudge->self, 8.0s cast, single-target

    AssaultCannonAOE1Rest = 28444, // Helper->self, no cast, range 40 width 8 rect
    AssaultCannonAOE2Rest = 28445, // Helper->self, no cast, range 28 width 8 rect

    DiffusionRay = 28446, // Boss->self, 5.0s cast, range 40 circle //Raidwide

    UnknownAbility1 = 28448, // Boss->self, no cast, single-target
    UnknownAbility2 = 28449, // Boss->self, no cast, single-target
    UnknownAbility3 = 28450, // Boss->self, no cast, single-target
    UnknownAbility4 = 28451, // Boss->self, no cast, single-target
    UnknownAbility5 = 28452, // Boss->self, no cast, single-target
    UnknownAbility6 = 28453, // Boss->self, no cast, single-target

    CannonReflectionVisualFirst = 28454, // Helper->self, 8.0s cast, single-target
    CannonReflectionAOERest = 28455, // Helper->self, no cast, range 30 ?-degree cone
    Assail1 = 28456, // Boss->self, no cast, single-target
    Assail2 = 28457, // Boss->self, no cast, single-target

    GravitonCannon = 29555, // Helper->player, 8.5s cast, range 6 circle //Spread marker
    //Rail Cannon tankbuster not listed
}

public enum SID : uint
{
    Electrocution1 = 3073, // none->player, extra=0x0
    UnknownStatus1 = 2552, // none->ArmoredDrudge, extra=0x180/0x181
    UnknownStatus2 = 2195, // none->Boss, extra=0x183/0x182
    Electrocution2 = 3074, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon_329 = 329, // player
}
class DiffusionRay(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DiffusionRay));
class GravitonCannon(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.GravitonCannon), 6);

class D092ArmoredChariotStates : StateMachineBuilder
{
    public D092ArmoredChariotStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DiffusionRay>()
            .ActivateOnEnter<GravitonCannon>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", PrimaryActorOID = (uint)OID.Boss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 844, NameID = 11239)]
public class D092ArmoredChariot(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -182), new ArenaBoundsSquare(20));
