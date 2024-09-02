namespace BossMod.Heavensward.Dungeon.D04Vault.D042SerGrinnauxTheBull;

public enum OID : uint
{
    Boss = 0x1053, // R0.500, SerGrinnauxTheBull 
    SerGrinnaux = 0x1054, // R2.200, x?
    AetherialTear = 0x1055, // R2.000, x?
    Helper2 = 0xD25,  // "DawnKnight"?
    Helper = 0x233C, // x3
    DimensionalRipVoidzone = 0x1E91C1,
}
public enum AID : uint
{
    Attack = 870, // 105A/105B/104F/104E/1051/1060/1069/105F/1053/1054/1068->player, no cast, single-target
    DimensionalCollapse = 4136, // 1054->self, 2.5s cast, single-target
    DimensionalCollapse2 = 4137, // D25->self, 3.0s cast, range 7+R ?-degree cone - Used in 2xDonut Pattern & Inward Pattern
    DimensionalCollapse3 = 4138, // D25->self, 3.0s cast, range 12+R ?-degree cone - Used in Inward Pattern
    DimensionalCollapse4 = 4139, // D25->self, 3.0s cast, range 17+R ?-degree cone - Used in 2xDonut Pattern
    HeavySwing = 4133, // 1054->players, no cast, range 8+R ?-degree cone
    HyperdimensionalSlash = 4134, // 1054->self, 3.0s cast, range 45+R width 8 rect
    FaithUnmoving = 4135, // 1054->self, 3.0s cast, range 80+R circle
    DimensionalRip = 4140, // 1054->location, 3.0s cast, range 5 circle
    Rive = 1135, // 1053->self, 2.5s cast, range 30+R width 2 rect
    Advent = 4979, // 104E/1053->self, no cast, single-target
    Advent2 = 4980, // D25->self, no cast, range 80 circle
    Advent3 = 4122, // 104E/1053->self, no cast, single-target

    DimensionalTorsion = 4142, // 1055->player, no cast, single-target
    StellarImplosion = 4141, // D25->location, no cast, range 5 circle
}
public enum TetherID : uint
{
    DimensionalTorsion = 9,
}

class DimensionalCollapse2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DimensionalCollapse2), new AOEShapeDonutSector(3, 7.5f, 90.Degrees()));
class DimensionalCollapse3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DimensionalCollapse3), new AOEShapeDonutSector(8, 12.5f, 90.Degrees()));
class DimensionalCollapse4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DimensionalCollapse4), new AOEShapeDonutSector(13, 17.5f, 90.Degrees()));
//class HeavySwing(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.HeavySwing), new AOEShapeCone(8.5f, 45.Degrees()));
class HyperdimensionalSlash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HyperdimensionalSlash), new AOEShapeRect(45, 4));
class FaithUnmoving(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.FaithUnmoving), 13, stopAtWall: true);
class DimensionalRip(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, ActionID.MakeSpell(AID.DimensionalRip), m => m.Enemies(OID.DimensionalRipVoidzone).Where(x => x.EventState != 7), 0.8f);
class Rive(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Rive), new AOEShapeRect(30, 1));
class AetherialTear(BossModule module) : Components.GenericAOEs(module)
{
    private IEnumerable<Actor> AetherialTears => Module.Enemies(OID.AetherialTear).Where(e => !e.IsDead);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AetherialTears.Select(b => new AOEInstance(new AOEShapeCircle(7f), b.Position) with { Color = ArenaColor.Danger });
}
class AddsModule(BossModule module) : Components.Adds(module, (uint)OID.SerGrinnaux);
class D042SerGrinnauxTheBullStates : StateMachineBuilder
{
    public D042SerGrinnauxTheBullStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DimensionalCollapse2>()
            .ActivateOnEnter<DimensionalCollapse3>()
            .ActivateOnEnter<DimensionalCollapse4>()
            .ActivateOnEnter<HyperdimensionalSlash>()
            .ActivateOnEnter<FaithUnmoving>()
            .ActivateOnEnter<DimensionalRip>()
            .ActivateOnEnter<Rive>()
            .ActivateOnEnter<AetherialTear>()
            .ActivateOnEnter<AddsModule>();

    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 34, NameID = 3850)]
public class D042SerGrinnauxTheBull(WorldState ws, Actor primary) : BossModule(ws, primary, new(-0.01f, 71.9f), new ArenaBoundsCircle(20));
