namespace BossMod.Dawntrail.Dungeon.D10Underkeep.D102SoldierS0;

public enum OID : uint
{
    Boss = 0x47AD, // R2.76
    SoldierS0Clone = 0x47AE, // R2.76
    AddBlock = 0x47AF, // R3.2
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Teleport = 42576, // Boss->location, no cast, single-target

    FieldOfScorn = 42579, // Boss->self, 5.0s cast, range 45 circle
    ThunderousSlash = 43136, // Boss->player, 5.0s cast, single-target

    SectorBisectorLeftVisual = 42562, // Boss->self, 5.0s cast, single-target
    SectorBisectorRightVisual = 42563, // Boss->self, 5.0s cast, single-target
    SectorBisectorCloneAppearLeft = 42568, // SoldierS0Clone->self, no cast, single-target
    SectorBisectorCloneVanishLeft = 43163, // SoldierS0Clone->self, no cast, single-target
    SectorBisectorCloneCastLeft = 42564, // SoldierS0Clone->self, no cast, single-target
    SectorBisectorCloneAppearRight = 42569, // SoldierS0Clone->self, no cast, single-target
    SectorBisectorCloneVanishRight = 43164, // SoldierS0Clone->self, no cast, single-target
    SectorBisectorCloneCastRight = 42565, // SoldierS0Clone->self, no cast, single-target
    SectorBisectorLeft = 42566, // Helper->self, 0.5s cast, range 45 180-degree cone
    SectorBisectorRight = 42567, // Helper->self, 0.5s cast, range 45 180-degree cone

    OrderedFireVisual = 42572, // Boss->self, 2.0+2,0s cast, single-target
    OrderedFire = 42573, // AddBlock->self, 5.0s cast, range 55 width 8 rect
    StaticForceVisual = 42574, // Boss->self, 5.0s cast, single-target
    StaticForce = 42575, // Helper->self, no cast, range 60 30-degree cone
    ElectricExcessVisual = 42570, // Boss->self, 4.0+1,0s cast, single-target
    ElectricExcess = 43139 // Helper->players, 5.0s cast, range 6 circle, spread
}

public enum TetherID : uint
{
    BisectorInitial = 313, // SoldierS0Clone->SoldierS0Clone
    BisectorEnd = 327 // SoldierS0Clone->SoldierS0Clone
}

public enum IconID : uint
{
    StaticForce = 591 // Boss->players
}

class FieldOfScorn(BossModule module) : Components.RaidwideCast(module, AID.FieldOfScorn);
class ThunderousSlash(BossModule module) : Components.SingleTargetCast(module, AID.ThunderousSlash);
class OrderedFire(BossModule module) : Components.StandardAOEs(module, AID.OrderedFire, new AOEShapeRect(55, 4));
class ElectricExcess(BossModule module) : Components.SpreadFromCastTargets(module, AID.ElectricExcess, 6);
class StaticForce(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60, 15.Degrees()), (uint)IconID.StaticForce, AID.StaticForce, 5.1f);

class SectorBisector(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Dictionary<ulong, ulong> _tethers = [];
    private Actor? _caster;
    public DateTime Activation { get; private set; }

    public enum Direction
    {
        None,
        Left,
        Right
    }

    public Direction NextDirection { get; private set; }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_caster is { } c)
            yield return new AOEInstance(new AOEShapeCone(45, 90.Degrees()), c.Position, c.Rotation + (NextDirection == Direction.Left ? 90.Degrees() : -90.Degrees()), Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SectorBisectorLeftVisual:
                NextDirection = Direction.Left;
                break;
            case AID.SectorBisectorRightVisual:
                NextDirection = Direction.Right;
                break;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.BisectorEnd)
            _tethers[source.InstanceID] = tether.Target;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SectorBisectorCloneVanishLeft or AID.SectorBisectorCloneVanishRight)
        {
            // tethers are "wound backwards", so the first clone to disappear is tethered to the final clone, which is the caster
            if (_caster == null && _tethers.TryGetValue(caster.InstanceID, out var f))
            {
                _caster = WorldState.Actors.Find(f);
                Activation = WorldState.FutureTime(_tethers.Count == 6 ? 4.6f : 6.3f);
            }
        }

        if ((AID)spell.Action.ID is AID.SectorBisectorLeft or AID.SectorBisectorRight)
        {
            _tethers.Clear();
            _caster = null;
            NextDirection = Direction.None;
        }
    }
}

class D102SoldierS0States : StateMachineBuilder
{
    public D102SoldierS0States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FieldOfScorn>()
            .ActivateOnEnter<ThunderousSlash>()
            .ActivateOnEnter<OrderedFire>()
            .ActivateOnEnter<ElectricExcess>()
            .ActivateOnEnter<StaticForce>()
            .ActivateOnEnter<SectorBisector>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1027, NameID = 13757)]
public class D102SoldierS0(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -182f), new ArenaBoundsSquare(15.5f));
