namespace BossMod.Stormblood.Dungeon.D09DrownedCityOfSkalla.D093HrodricPoisonTongue;

public enum OID : uint
{
    HrodricPoisontongue = 0x1FAE,
    Helper = 0x18D6, // R0.500, x?
}

public enum AID : uint
{
    AutoAttack = 870, // 1FAE->player, no cast, single-target
    RustingClaw = 9825, // 1FAE->self, 5.0s cast, range 8+R ?-degree cone
    TailDrive = 9827, // 1FAE->self, 5.0s cast, range 30+R ?-degree cone
    TheSpin = 9828, // 1FAE->self, 5.0s cast, range 40+R circle
    _Weaponskill_ = 9830, // 1FAE->self, no cast, single-target
    RingOfChaos = 9831, // 18D6->self, no cast, range ?-20 donut
    EyeOfTheFire = 9829, // 1FAE->self, 3.0s cast, range 40 circle : Gaze Attack
    CircleOfChaos = 9833, // 18D6->self, no cast, range 6 circle
    CrossOfChaos = 9832, // 18D6->self, no cast, range 50 width 8 cross
    WordsOfWoe = 9826, // 1FAE->self, 3.0s cast, range 45+R width 6 rect
}

public enum IconID : uint
{
    RingOfChaosIcon = 121,
    SpreadCross = 122,
    SpreadCircle = 28,
}

// Angle is an estimate. Radius might be less than 17 might be slightly more. It definitely isn't all the way off arena.
sealed class RustingClaw(BossModule module)
    : Components.SimpleAOEs(module, (uint)AID.RustingClaw, new AOEShapeCone(17f, 45f.Degrees()));
//Angle is an estimate
sealed class TailDrive(BossModule module)
    : Components.SimpleAOEs(module, (uint)AID.TailDrive, new AOEShapeCone(50f, 40f.Degrees()));

// Spread to outside of arena
sealed class TheSpin(BossModule module) : Components.ProximityAOEs(module, (uint)AID.TheSpin, 17f);

sealed class EyeOfTheFire(BossModule module) : Components.CastGaze(module, (uint)AID.EyeOfTheFire);

/*
 * This should cover the various baits that use the SpreadCross/SpreadCircle/Donut icons.
 * Might make more sense for cross and circle to be spread from icon.
 */
sealed class ChaosBaits(BossModule module) : Components.GenericBaitAway(module)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        DateTime _nextActivation = WorldState.FutureTime(4.7f);
        // next target is the pc with the icon.
        Actor? _nextTarget = WorldState.Actors.Find(targetID);

        if ((IconID)iconID == IconID.SpreadCross && _nextTarget != null)
        {
            // 180 degrees rotation to keep the cross from spinning with pc.
            CurrentBaits.Add(new(_nextTarget, _nextTarget, new AOEShapeCross(40f, 4f), _nextActivation, customRotation: 180.Degrees()));
        }
        else if ((IconID)iconID == IconID.SpreadCircle && _nextTarget != null)
        {
            CurrentBaits.Add(new(_nextTarget, _nextTarget, new AOEShapeCircle(7f), _nextActivation));
        }
        else if ((IconID)iconID == IconID.RingOfChaosIcon && _nextTarget != null)
        {
            CurrentBaits.Add(new(_nextTarget, _nextTarget, new AOEShapeDonut(10f, 20f), _nextActivation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID
            is (uint)AID.CircleOfChaos
            or (uint)AID.CrossOfChaos)
        {
            CurrentBaits.RemoveAll(bait => bait.Shape is AOEShapeCircle or AOEShapeCross);
        }
        else if (spell.Action.ID == (uint)AID.RingOfChaos)
        {
            CurrentBaits.RemoveAll(bait => bait.Shape is AOEShapeDonut);
        }
        NumCasts++;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var bait in CurrentBaits)
        {
            // bait the circle and cross to the edge of arena.
            if (bait.Source == actor && (bait.Shape is AOEShapeCircle or AOEShapeCross))
                hints.AddForbiddenZone(new AOEShapeCircle(16f), Arena.Center, default, bait.Activation);
            // bait the donut to the center.
            else if (bait.Source == actor && bait.Shape is AOEShapeDonut)
                hints.AddForbiddenZone(new AOEShapeCircle(6f, true), Arena.Center, default, bait.Activation);
        }
        // remember to run base AIHints so it will see the other baits avoidance also.
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class WordsOfWoe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WordsOfWoe, new AOEShapeRect(65f, 3f));

[SkipLocalsInit]
sealed class D093HrodricPoisonTongueStates : StateMachineBuilder
{
    public D093HrodricPoisonTongueStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RustingClaw>()
            .ActivateOnEnter<TailDrive>()
            .ActivateOnEnter<TheSpin>()
            .ActivateOnEnter<ChaosBaits>()
            .ActivateOnEnter<EyeOfTheFire>()
            .ActivateOnEnter<WordsOfWoe>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(D093HrodricPoisonTongueStates),
    ConfigType = null, // replace null with typeof(HrodricPoisontongueConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.HrodricPoisontongue,
    Contributors = "",
    Expansion = BossModuleInfo.Expansion.Stormblood,
    Category = BossModuleInfo.Category.Dungeon,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 279u,
    NameID = 6910u,
    SortOrder = 3,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class D093HrodricPoisonTongue(WorldState ws, Actor primary) : BossModule(ws, primary, new(479f, 4f), new ArenaBoundsCircle(20f));
