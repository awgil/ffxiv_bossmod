using BossMod.Endwalker.Dungeon.D12Aetherfont.D121Lyngbakr;

namespace BossMod.Dawntrail.Dungeon.D08Deadwalk.D081Leonogg;

public enum OID : uint
{
    Boss = 0x4183, // R3.600, x1
    Helper = 0x233C, // R0.500, x32, 523 type
    StrayMemory = 0x43F8, // R3.000, x1
    LittleLadyNogginette = 0x41BD, // R1.000, x8
    LittleLordNoggington = 0x41BB, // R1.000, x13
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    NobleNoggin = 0x4205, // R1.000, x12
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    MaliciousMist = 36529, // Boss->self, 5.0s cast, range 50 circle // Raidwide

    FallingNightmare1 = 36526, // Boss->self, 3.0s cast, single-target
    FallingNightmare2 = 36532, // NobleNoggin->self, 2.0s cast, range 2 circle
    FallingNightmare3 = 36536, // NobleNoggin->self, 1.0s cast, range 2 circle

    MorbidFascination = 36528, // Boss->self, no cast, single-target
    TeamSpirit = 36527, // Boss->self, 3.0s cast, single-target // Summons dolls
    SpiritedCharge = 36598, // Boss->self, 3.0s cast, single-target

    UnknownAbility1 = 36533, // LittleLordNoggington/LittleLadyNogginette->self, no cast, single-target
    UnknownAbility2 = 36534, // Helper->self, no cast, range 2 width 1 rect // likely marching AOE caused by dolls

    EvilScheme1 = 39682, // Boss->self, 6.0s cast, single-target // exaflare
    EvilScheme2 = 39683, // Helper->self, 6.0s cast, range 4 circle
    EvilScheme3 = 39684, // Helper->self, no cast, range 4 circle

    Overattachment = 36535, // LittleLadyNogginette/LittleLordNoggington->player, no cast, single-target

    LoomingNightmare1 = 39685, // Boss->self, 5.0s cast, single-target // nox, chasing AOE
    LoomingNightmare2 = 39686, // Helper->self, 2.0s cast, range 4 circle
    LoomingNightmare3 = 39687, // Helper->self, no cast, range 4 circle

    Scream1 = 36530, // Boss->self, 5.0s cast, single-target
    Scream2 = 36531, // Helper->self, 5.0s cast, range 20 60.000-degree cone
    Scream3 = 36541, // Boss->self, no cast, single-target
}

public enum SID : uint
{
    UnknownStatus = 4131, // NobleNoggin->player, extra=0x0
    Benoggined = 3948, // NobleNoggin->player, extra=0x1E
    Bind = 2518, // Helper->player, extra=0x0
    VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2

}
public enum IconID : uint
{
    Nox = 197, // player
}

class MaliciousMist(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MaliciousMist));
class FallingNightmare2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FallingNightmare2), new AOEShapeCircle(2));
class FallingNightmare3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FallingNightmare3), new AOEShapeCircle(2));
class Scream2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Scream2), new AOEShapeCone(20, 30.Degrees()));

class D081LeonoggStates : StateMachineBuilder
{
    public D081LeonoggStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MaliciousMist>()
            .ActivateOnEnter<FallingNightmare2>()
            .ActivateOnEnter<FallingNightmare3>()
            .ActivateOnEnter<Scream2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 981, NameID = 13073)]
public class D081Leonogg(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 150), new ArenaBoundsCircle(20));
