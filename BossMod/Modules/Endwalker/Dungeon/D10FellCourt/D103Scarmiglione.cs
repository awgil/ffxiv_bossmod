namespace BossMod.Endwalker.Dungeon.D10FellCourt.D103Scarmiglione;

public enum OID : uint
{
    Boss = 0x39C5, // R=7.7
    Necroserf1 = 0x39C7, // R1.400, x8
    Necroserf2 = 0x39C6, // R1.400, x2
    Helper = 0x233C, // R0.500, x24, 523 type
}

public enum AID : uint
{
    AutoAttack = 30258, // Boss->player, no cast, single-target
    AutoAttackMob = 872, // Necroserf1->player, no cast, single-target

    BlightedBedevilment = 30235, // Boss->self, 4.9s cast, range 9 circle
    BlightedBladework1 = 30259, // Boss->location, 10.0s cast, single-target
    BlightedBladework2 = 30260, // Helper->self, 11.0s cast, range 25 circle
    BlightedSweep = 30261, // Boss->self, 7.0s cast, range 40 180-degree cone

    CorruptorsPitch1 = 30245, // Boss->self, no cast, single-target
    CorruptorsPitch2 = 30247, // Helper->self, no cast, range 60 circle
    CorruptorsPitch3 = 30248, // Helper->self, no cast, range 60 circle
    CorruptorsPitch4 = 30249, // Helper->self, no cast, range 60 circle

    CreepingDecay = 30240, // Boss->self, 4.0s cast, single-target
    CursedEcho = 30257, // Boss->self, 4.0s cast, range 40 circle //raidwide

    Nox = 30241, // Helper->self, 5.0s cast, range 10 circle

    RottenRampage1 = 30231, // Boss->self, 8.0s cast, single-target
    RottenRampage2 = 30232, // Helper->location, 10.0s cast, range 6 circle
    RottenRampage3 = 30233, // Helper->players, 10.0s cast, range 6 circle

    UnknownAbility1 = 30237, // Boss->location, no cast, single-target
    UnknownAbility2 = 30244, // Boss->self, no cast, single-target

    UnknownWeaponskill = 30234, // Boss->self, no cast, single-target

    VacuumWave = 30236, // Helper->self, 5.4s cast, range 40 circle //knockback

    VoidVortex1 = 30243, // Helper->players, 5.0s cast, range 6 circle //spread
    VoidVortex2 = 30253, // Boss->self, no cast, single-target
    VoidVortex3 = 30254, // Helper->players, 5.0s cast, range 6 circle //stack

    // missing firedamp, tankbuster
}

public enum SID : uint
{
    Bleeding = 2088, // Boss->player, extra=0x0
    BrainRot = 3282, // Helper->player/39C8, extra=0x1
    VulnerabilityUp = 1789, // Helper->player, extra=0x4
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon_353 = 353, // player
    Icon_161 = 161, // player
}

public enum TetherID : uint
{
    Tether_206 = 206, // Boss->3AE4
}
class VoidVortex1(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.VoidVortex1), 6);
class VoidVortex3(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.VoidVortex3), 6, 8);

class VacuumWave(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.VacuumWave), 30, stopAtWall: true);

class BlightedBedevilment(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlightedBedevilment), new AOEShapeCircle(9));
class BlightedBladework2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlightedBladework2), new AOEShapeCircle(9));
class Nox(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Nox), new AOEShapeCircle(10));

class RottenRampage2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.RottenRampage2), 6);

class BlightedSweep(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlightedSweep), new AOEShapeCone(40, 90.Degrees()));

class CursedEcho(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CursedEcho));

class D103ScarmiglioneStates : StateMachineBuilder
{
    public D103ScarmiglioneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoidVortex1>()
            .ActivateOnEnter<VoidVortex3>()
            .ActivateOnEnter<VacuumWave>()
            .ActivateOnEnter<BlightedBedevilment>()
            .ActivateOnEnter<BlightedBladework2>()
            .ActivateOnEnter<Nox>()
            .ActivateOnEnter<RottenRampage2>()
            .ActivateOnEnter<BlightedSweep>()
            .ActivateOnEnter<CursedEcho>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 869, NameID = 11372)]
public class D103Scarmiglione(WorldState ws, Actor primary) : BossModule(ws, primary, new(-35, -298), new ArenaBoundsCircle(20));
