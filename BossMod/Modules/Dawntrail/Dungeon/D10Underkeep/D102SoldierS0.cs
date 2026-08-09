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

    SectorBisectorVisual1 = 42562, // Boss->self, 5.0s cast, single-target, cleave left
    SectorBisectorVisual2 = 42563, // Boss->self, 5.0s cast, single-target, cleave right
    SectorBisectorVisualClone1 = 42568, // SoldierS0Clone->self, no cast, single-target
    SectorBisectorVisualClone2 = 43163, // SoldierS0Clone->self, no cast, single-target
    SectorBisectorVisualClone3 = 42564, // SoldierS0Clone->self, no cast, single-target
    SectorBisectorVisualClone4 = 42569, // SoldierS0Clone->self, no cast, single-target
    SectorBisectorVisualClone5 = 43164, // SoldierS0Clone->self, no cast, single-target
    SectorBisectorVisualClone6 = 42565, // SoldierS0Clone->self, no cast, single-target
    SectorBisector1 = 42566, // Helper->self, 0.5s cast, range 45 180-degree cone, cleave left
    SectorBisector2 = 42567, // Helper->self, 0.5s cast, range 45 180-degree cone, cleave right

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

sealed class FieldOfScorn(BossModule module) : Components.RaidwideCast(module, (uint)AID.FieldOfScorn);
sealed class ThunderousSlash(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ThunderousSlash);
sealed class OrderedFire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OrderedFire, new AOEShapeRect(55f, 4f));
sealed class ElectricExcess(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ElectricExcess, 6f);
sealed class StaticForce(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60f, 15f.Degrees()), (uint)IconID.StaticForce, (uint)AID.StaticForce, 5.1d);

sealed class SectorBisector(BossModule module) : Components.GenericAOEs(module)
{
    // this solution looks a bit complex and confusing, but that is because the pretty and easy solution of just using the tether order only works with good ping + fps
    // at higher latencies the time stamps merge together and tethers start to appear in random order in the logs...
    private static readonly AOEShapeCone cone = new(45f, 90f.Degrees());
    private AOEInstance[] _aoe = [];
    private readonly List<(Actor source, Actor target)> tethers = [with(8)];
    private int cloneCount;
    private bool direction; // false = left, true = right
    private bool active;
    private Actor? firstClone;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.SoldierS0Clone)
        {
            if (modelState == 5)
            {
                direction = false;
            }
            else if (modelState == 6)
            {
                direction = true;
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.BisectorInitial)
        {
            ++cloneCount;
        }
        else if (tether.ID == (uint)TetherID.BisectorEnd)
        {
            tethers.Add((source, WorldState.Actors.Find(tether.Target)!));
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (active && tether.ID == (uint)TetherID.BisectorEnd)
        {
            var count = tethers.Count;
            for (var i = 0; i < count; ++i)
            {
                if (tethers[i].source == source)
                {
                    tethers.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SectorBisector1 or (uint)AID.SectorBisector2)
        {
            _aoe = [];
            cloneCount = 0;
            tethers.Clear();
            firstClone = null;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (firstClone == null && spell.Action.ID is (uint)AID.SectorBisectorVisualClone2 or (uint)AID.SectorBisectorVisualClone5)
        {
            active = true;
            firstClone = caster;
        }
    }

    public override void Update()
    {
        if (active && firstClone != null && _aoe.Length == 0 && tethers.Count < cloneCount)
        {
            var count = tethers.Count;
            for (var i = 0; i < count; ++i)
            {
                var tether = tethers[i];
                if (tether.source == firstClone)
                {
                    active = false;
                    _aoe = [new(cone, tether.target.Position.Quantized(), tether.target.Rotation + (direction ? -1f : 1f) * 90f.Degrees(), WorldState.FutureTime(cloneCount == 6 ? 4.2d : 5.9d))];
                    return;
                }
            }
        }
    }
}

sealed class D102SoldierS0States : StateMachineBuilder
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

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1027u, NameID = 13757u)]
public sealed class D102SoldierS0(WorldState ws, Actor primary) : BossModule(ws, primary, new(0f, -182f), new ArenaBoundsSquare(15.5f));
