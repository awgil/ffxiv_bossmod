namespace BossMod.Endwalker.Dungeon.D06DeadEnds.D062Peacekeeper;

public enum OID : uint
{
    Boss = 0x34C6, // R=9.0
    Helper = 0x233C,
    PerpetualWarMachine = 0x384B, // R0.900, x14
    Helper2 = 0x1EB5F7, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 25977, // Boss->player, no cast, single-target
    Decimation = 25936, // Boss->self, 5.0s cast, range 40 circle //Raidwide
    DisengageHatch = 28356, // Boss->self, no cast, single-target
    EclipsingExhaust = 25931, // Boss->self, 5.0s cast, range 40 circle
    ElectromagneticRepellant = 28360, // Boss->self, 4.0s cast, range 9 circle  //Danger AOE in boss hitbox
    Elimination = 25935, // Boss->self/player, 5.0s cast, range 46 width 10 rect //Tankbuster
    InfantryDeterrent = 28358, // Boss->self, no cast, single-target
    InfantryDeterrentAOE = 28359, // Helper->player, 5.0s cast, range 6 circle
    NoFuture1 = 25925, // Boss->self, 4.0s cast, single-target
    NoFuture2 = 25927, // Helper->self, 4.0s cast, range 6 circle
    NoFuture3 = 25928, // Helper->player, 5.0s cast, range 6 circle
    OrderToFire = 28351, // Boss->self, 5.0s cast, single-target
    Peacefire1 = 25933, // Boss->self, 3.0s cast, single-target
    Peacefire2 = 25934, // Helper->self, 7.0s cast, range 10 circle
    SmallBoreLaser = 28352, // PerpetualWarMachine->self, 5.0s cast, range 20 width 4 rect
    UnknownWeaponskill = 25926, // Boss->self, no cast, single-target
    UnkownAbility1 = 28350, // PerpetualWarMachine->location, no cast, single-target
    UnkownAbility2 = 28357, // Boss->self, no cast, single-target
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Boss/PerpetualWarMachine->player, extra=0x1
    Electrocution = 1899, // none->player, extra=0x0
    Burns = 2194, // none->player, extra=0x0

}

public enum IconID : uint
{
    Icon_139 = 139, // player
    Icon_230 = 230, // player
}
class ElectromagneticRepellant(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 7, ActionID.MakeSpell(AID.ElectromagneticRepellant), m => m.Enemies(OID.Boss).Where(z => z.EventState != 7), 0.8f);

class InfantryDeterrentAOE(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.InfantryDeterrentAOE), 6);
class NoFuture3(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.NoFuture3), 6);

class NoFuture2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.NoFuture2), new AOEShapeCircle(6));
class Peacefire2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Peacefire2), new AOEShapeCircle(10));

class SmallBoreLaser(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SmallBoreLaser), new AOEShapeRect(20, 2));

class Elimination(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Elimination));
class Decimation(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Decimation));
class EclipsingExhaust(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.EclipsingExhaust));

class D062PeacekeeperStates : StateMachineBuilder
{
    public D062PeacekeeperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectromagneticRepellant>()
            .ActivateOnEnter<InfantryDeterrentAOE>()
            .ActivateOnEnter<NoFuture3>()
            .ActivateOnEnter<NoFuture2>()
            .ActivateOnEnter<Peacefire2>()
            .ActivateOnEnter<SmallBoreLaser>()
            .ActivateOnEnter<Elimination>()
            .ActivateOnEnter<Decimation>()
            .ActivateOnEnter<EclipsingExhaust>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 792, NameID = 10315)]
public class D062Peacekeeper(WorldState ws, Actor primary) : BossModule(ws, primary, new(-105, -210), new ArenaBoundsCircle(20));
