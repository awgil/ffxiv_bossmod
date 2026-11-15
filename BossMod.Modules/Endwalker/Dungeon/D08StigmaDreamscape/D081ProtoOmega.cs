namespace BossMod.Endwalker.Dungeon.D08StigmaDreamscape.D081ProtoOmega;

public enum OID : uint
{
    Boss = 0x3417, // R7.500, x1
    MarkIIGuidedMissile = 0x3418, // R1.000, x2
    Helper = 0x233C, // R0.500, Helper type
    BurnPuddle = 0x1E8D9B, // R0.500, EventObj type, spawned by Burn
    MissilePuddle = 0x1EB24E, // R0.500, EventObj type, spawned by Iron Kiss
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    SideCannons1 = 25376, // Boss->self, 7.0s cast, range 60 180-degree cone
    SideCannons2 = 25377, // Boss->self, 7.0s cast, range 60 180-degree cone
    ForwardInterceptors = 25378, // Boss->self, 5.5s cast, single-target, visual, not seen yet
    ForwardInterceptorsAOE = 25379, // ?->self, 7.5s cast, range 60 180-degree cone, not seen yet
    RearInterceptors = 25380, // Boss->self, 5.5s cast, single-target, visual, not seen yet
    RearInterceptorsAOE = 25381, // ?->self, 7.5s cast, range 60 180-degree cone, not seen yet
    GuidedMissile = 25382, // Boss->self, 3.0s cast, single-target, visual (missiles)
    IronKiss = 25383, // MarkIIGuidedMissile->self, no cast, range 3 circle
    ChemicalMissile = 25384, // Boss->self, 3.0s cast, single-target, visual (spreads)
    Burn = 25385, // Helper->location, no cast, range 6 circle
    ElectricSlide = 25386, // Boss->player, 5.0s cast, range 6 circle stack
    MustardBomb = 25387, // Boss->player, 5.0s cast, range 5 circle, tankbuster
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // MarkIIGuidedMissile->player, extra=0x1
    Burns = 2088, // Boss->player, extra=0x0
}

public enum IconID : uint
{
    ChemicalMissile = 139, // player
    ElectricSlide = 663, // player
    MustardBomb = 218, // player
}

public enum TetherID : uint
{
    GuidedMissile = 17, // MarkIIGuidedMissile->player
}

class SideCannons(BossModule module) : Components.GroupedAOEs(module, [AID.SideCannons1, AID.SideCannons2], new AOEShapeCone(60, 90.Degrees()));
class Interceptors(BossModule module) : Components.GroupedAOEs(module, [AID.ForwardInterceptorsAOE, AID.RearInterceptorsAOE], new AOEShapeCone(60, 90.Degrees()));
class ChemicalMissile(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.ChemicalMissile, AID.Burn, 6, 5.2f);
class ElectricSlide(BossModule module) : Components.StackWithCastTargets(module, AID.ElectricSlide, 6);
class MustardBomb(BossModule module) : Components.BaitAwayCast(module, AID.MustardBomb, new AOEShapeCircle(5), true);
class BurnPuddle(BossModule module) : Components.Voidzone(module, 6, OID.BurnPuddle);
class MissilePuddle(BossModule module) : Components.Voidzone(module, 3, OID.MissilePuddle);

class GuidedMissiles(BossModule module) : Components.GenericAOEs(module, AID.IronKiss)
{
    private readonly Dictionary<ulong, ulong> _targets = [];

    private static readonly AOEShapeCircle _explosion = new(3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => Module.Enemies(OID.MarkIIGuidedMissile).Where(m => !m.IsDestroyed).Select(m => new AOEInstance(_explosion, m.Position));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var m in Module.Enemies(OID.MarkIIGuidedMissile))
            if (!m.IsDestroyed && _targets.GetValueOrDefault(m.InstanceID) == actor.InstanceID)
                hints.AddForbiddenZone(ShapeDistance.Circle(m.Position, 12));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_targets.ContainsValue(actor.InstanceID))
            hints.Add("Kite the missile!", false);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.GuidedMissile)
            _targets[source.InstanceID] = tether.Target;
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.GuidedMissile)
            _targets.Remove(source.InstanceID);
    }
}

class D081ProtoOmegaStates : StateMachineBuilder
{
    public D081ProtoOmegaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SideCannons>()
            .ActivateOnEnter<Interceptors>()
            .ActivateOnEnter<ChemicalMissile>()
            .ActivateOnEnter<ElectricSlide>()
            .ActivateOnEnter<MustardBomb>()
            .ActivateOnEnter<GuidedMissiles>()
            .ActivateOnEnter<BurnPuddle>()
            .ActivateOnEnter<MissilePuddle>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 784, NameID = 10401)]
public class D081ProtoOmega(ModuleInit init) : BossModule(init, new(-144, -136), new ArenaBoundsSquare(20));
