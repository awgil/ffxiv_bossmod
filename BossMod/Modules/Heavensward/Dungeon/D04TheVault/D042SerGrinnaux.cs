namespace BossMod.Heavensward.Dungeon.D04TheVault.D042SerGrinnaux;

public enum OID : uint
{
    Boss = 0x1054, // R2.200, x1
    Helper = 0xD25, // R0.500, x13, mixed types
    SerGrinnauxTheBull = 0x1053, // R0.500, x1
    StellarImplodeArea = 0x1E91C1, // R0.500, x0 (spawn during fight), EventObj type
    AetherialTear = 0x1055, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    DimensionalCollapse1 = 4136, // Boss->self, 2.5s cast, single-target
    DimensionalCollapse2 = 4137, // Helper->self, 3.0s cast, range 7+R ?-degree cone inner
    DimensionalCollapse3 = 4138, // Helper->self, 3.0s cast, range 12+R ?-degree cone middle
    DimensionalCollapse4 = 4139, // Helper->self, 3.0s cast, range 17+R ?-degree cone outer

    DimensionalRip = 4140, // Boss->location, 3.0s cast, range 5 circle
    DimensionalTorsion = 4142, // AetherialTear->player, no cast, single-target

    FaithUnmoving = 4135, // Boss->self, 3.0s cast, range 80+R circle
    HeavySwing = 4133, // Boss->player, no cast, range 8+R ?-degree cone
    HyperdimensionalSlash = 4134, // Boss->self, 3.0s cast, range 45+R width 8 rect
    Retreat = 4257, // SerGrinnauxTheBull->self, no cast, single-target
    StellarImplosion = 4141, // Helper->location, no cast, range 5 circle
    Unknown1 = 4124, // Boss->self, no cast, single-target
    Unknown2 = 4256, // SerGrinnauxTheBull->self, no cast, single-target
    Advent = 4979,  // SerGrinnauxTheBull->self, no cast, single-target
    Rive = 1135, // SerGrinnauxTheBull->self, 2.5s cast, range 30+R width 2 rect
}

public enum SID : uint
{
    VulnerabilityUp = 202, // AetherialTear->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8
    Heavy = 14, // Helper->player, extra=0x32
}

public enum TetherID : uint
{
    Tether_1 = 1, // AetherialTear->player
}

class DimensionalRip(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.DimensionalRip), 5);
class FaithUnmoving(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.FaithUnmoving), 20, shape: new AOEShapeCircle(80), stopAtWall: true);
class Rive(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Rive), new AOEShapeRect(30, 1));
class StellarImplosion(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, ActionID.MakeSpell(AID.StellarImplosion), m => m.Enemies(OID.StellarImplodeArea).Where(v => v.EventState != 7), 0);
class HyperdimensionalSlash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HyperdimensionalSlash), new AOEShapeRect(45, 4));

class D042SerGrinnauxStates : StateMachineBuilder
{
    public D042SerGrinnauxStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DimensionalRip>()
            .ActivateOnEnter<FaithUnmoving>()
            .ActivateOnEnter<Rive>()
            //.ActivateOnEnter<StellarImplosion>()
            .ActivateOnEnter<HyperdimensionalSlash>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team, Xyzzy", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 34, NameID = 3639)]
public class D042SerGrinnaux(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 72), new ArenaBoundsCircle(20));
