namespace BossMod.Heavensward.Dungeon.D04Vault.D042SerGrinnauxTheBull;

public enum OID : uint
{
    Boss = 0x1053, // R0.500, x1
    Helper = 0xD25, // R0.500, x13, mixed types
    WhiteKnight = 0x1066, // R2.000, x1
    BossP2 = 0x1054, // R2.200, x1
    AetherialTear = 0x1055, // R2.000, x0 (spawn during fight)
    DimensionalRipVoidzone = 0x1E91C1,
}

public enum AID : uint
{
    Attack = 870, // Boss/SerGrinnaux->player, no cast, single-target
    Overpower = 2188, // Boss->self, no cast, range 6+R ?-degree cone
    Rive = 1135, // Boss->self, 2.5s cast, range 30+R width 2 rect
    Advent = 4979, // Boss->self, no cast, single-target
    Advent2 = 4980, // Helper2->self, no cast, range 80 circle
    Advent3 = 4122, // Boss->self, no cast, single-target
    DimensionalCollapseCast = 4136, // SerGrinnaux->self, 2.5s cast, single-target
    DimensionalCollapseSmall = 4137, // Helper2->self, 3.0s cast, range 2.1-7 180-degree donut
    DimensionalCollapseMedium = 4138, // Helper2->self, 3.0s cast, range 7.3-12 180-degree donut
    DimensionalCollapseLarge = 4139, // Helper2->self, 3.0s cast, range 11.9-17 180-degree donut
    HeavySwing = 4133, // SerGrinnaux->players, no cast, range 8+R 120-degree cone
    HyperdimensionalSlash = 4134, // SerGrinnaux->self, 3.0s cast, range 45+R width 8 rect
    FaithUnmoving = 4135, // SerGrinnaux->self, 3.0s cast, range 80+R circle
    DimensionalRip = 4140, // SerGrinnaux->location, 3.0s cast, range 5 circle
    StellarImplosion = 4141, // Helper2->location, no cast, range 5 circle
    DimensionalTorsion = 4142, // AetherialTear->player, no cast, single-target
    Retreat = 4257, // Boss->self, no cast, single-target
}

public enum TetherID : uint
{
    DimensionalTorsion = 9
}

class DimensionalCollapseSmall(BossModule module) : Components.StandardAOEs(module, AID.DimensionalCollapseSmall, new AOEShapeDonutSector(2.1f, 7, 90.Degrees()));
class DimensionalCollapseMedium(BossModule module) : Components.StandardAOEs(module, AID.DimensionalCollapseMedium, new AOEShapeDonutSector(7.3f, 12, 90.Degrees()));
class DimensionalCollapseLarge(BossModule module) : Components.StandardAOEs(module, AID.DimensionalCollapseLarge, new AOEShapeDonutSector(11.9f, 17, 90.Degrees()));
class Overpower(BossModule module) : Components.Cleave(module, AID.Overpower, new AOEShapeCone(6.5f, 60.Degrees()), activeWhileCasting: false);
class HeavySwing(BossModule module) : Components.Cleave(module, AID.HeavySwing, new AOEShapeCone(10.2f, 60.Degrees()), enemyOID: (uint)OID.BossP2, activeWhileCasting: false);
class HyperdimensionalSlash(BossModule module) : Components.StandardAOEs(module, AID.HyperdimensionalSlash, new AOEShapeRect(45, 4));
class FaithUnmoving(BossModule module) : Components.KnockbackFromCastTarget(module, AID.FaithUnmoving, 13, stopAtWall: true)
{
    private readonly ArcList _forbidden = new(module.Arena.Center, 20);

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.AetherialTear)
            _forbidden.ForbidCircle(actor.Position, 7);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.AetherialTear)
            _forbidden.Forbidden.Clear();
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.Enemies(OID.AetherialTear).Any(t => pos.InCircle(t.Position, 7));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Sources(slot, actor))
        {
            if (IsImmune(slot, s.Activation))
                continue;

            hints.AddForbiddenZone(p => _forbidden.Forbidden.Contains(Angle.FromDirection(p - Arena.Center).Rad), s.Activation);
        }
    }
}
class DimensionalRip(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID.DimensionalRip, m => m.Enemies(OID.DimensionalRipVoidzone).Where(x => x.EventState != 7), 0.8f);
class Rive(BossModule module) : Components.StandardAOEs(module, AID.Rive, new AOEShapeRect(30, 1));
class AetherialTear(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos, DateTime)> _predicted = [];
    private readonly List<Actor> _tears = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var t in _tears)
            yield return new AOEInstance(new AOEShapeCircle(7), t.Position);
        foreach (var (p, d) in _predicted)
            yield return new AOEInstance(new AOEShapeCircle(7), p, Activation: d);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HyperdimensionalSlash)
        {
            var pos = spell.LocXZ + spell.Rotation.ToDirection() * 16.8f;
            _predicted.Add((pos, WorldState.FutureTime(5)));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.AetherialTear)
        {
            _predicted.RemoveAll(p => p.Item1.AlmostEqual(actor.Position, 1));
            _tears.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.AetherialTear)
            _tears.Remove(actor);
    }
}
class AddsModule(BossModule module) : Components.Adds(module, (uint)OID.BossP2);
class D042SerGrinnauxTheBullStates : StateMachineBuilder
{
    public D042SerGrinnauxTheBullStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Overpower>()
            .ActivateOnEnter<HeavySwing>()
            .ActivateOnEnter<DimensionalCollapseSmall>()
            .ActivateOnEnter<DimensionalCollapseMedium>()
            .ActivateOnEnter<DimensionalCollapseLarge>()
            .ActivateOnEnter<HyperdimensionalSlash>()
            .ActivateOnEnter<FaithUnmoving>()
            .ActivateOnEnter<DimensionalRip>()
            .ActivateOnEnter<Rive>()
            .ActivateOnEnter<AetherialTear>()
            .ActivateOnEnter<AddsModule>();

    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 34, NameID = 3850)]
public class D042SerGrinnauxTheBull(WorldState ws, Actor primary) : BossModule(ws, primary, new(-0.01f, 71.9f), new ArenaBoundsCircle(20));
