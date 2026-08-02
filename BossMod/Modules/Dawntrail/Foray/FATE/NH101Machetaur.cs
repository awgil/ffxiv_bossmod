namespace BossMod.Dawntrail.Foray.FATE.NH101Machetaur;

public enum OID : uint
{
    Machetaur = 0x4C26,
    Helper = 0x233C,
    Machetaur1 = 0x4C27, // R1.000, x0 (spawn during fight)
    Machetaur2 = 0x4C52, // R0.500, x0 (spawn during fight)
    Machetaur3 = 0x4EBF, // R0.500, x0 (spawn during fight)
    Machetaur4 = 0x4EC0, // R0.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50534, // Machetaur->player, no cast, single-target
    FocusedTremorCast = 47606, // Machetaur->self, 3.0s cast, single-target
    FocusedTremor = 48374, // Machetaur1->self, 2.2s cast, range 30 circle

    FocusedTremorInner = 47607, // Machetaur1->location, 6.0s cast, range 10 circle
    FocusedTremorMiddle = 47608, // Machetaur1->location, 8.0s cast, range 10-20 donut
    FocusedTremorOuter = 47609, // Machetaur1->location, 10.0s cast, range 20-30 donut

    BruntOfTheBattlefieldCast = 47610, // Machetaur->self, 3.0s cast, single-target
    BruntOfTheBattlefield = 48373, // Machetaur1->self, 4.5s cast, range 10 circle
    Uplift = 47611, // Machetaur2/Machetaur3/Machetaur1/Machetaur4->location, 3.0s cast, range 6 circle

    // ORDER:
    OctupleSwipe = 47600, // Machetaur->self, 10.0s cast, single-target
    OctupleSwipe1 = 47601, // Machetaur1->self, 1.0s cast, range 40 ?-degree cone
    OctupleSwipe2 = 47604, // Machetaur->self, no cast, range 40 ?-degree cone
    OctupleSwipe3 = 47605, // Machetaur->self, no cast, range 40 ?-degree cone
    OctupleSwipe4 = 47602, // Machetaur->self, no cast, range 40 ?-degree cone
}

sealed class FocusedTremor(BossModule module) : Components.RaidwideCast(module, (uint)AID.FocusedTremor);
sealed class BruntOfTheBattlefield(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BruntOfTheBattlefield, 10f);
sealed class Uplift(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Uplift, 6f);

// TODO make it a sequence one instead if its always a single one
sealed class FocusedTremorCircle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FocusedTremorInner)
        {
            aoes.Add(new(new AOEShapeCircle(10), caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
        }

        if (spell.Action.ID == (uint)AID.FocusedTremorMiddle)
        {
            aoes.Add(new(new AOEShapeDonut(10, 20), caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
        }

        if (spell.Action.ID == (uint)AID.FocusedTremorOuter)
        {
            aoes.Add(new(new AOEShapeDonut(20, 30), caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FocusedTremorInner or (uint)AID.FocusedTremorMiddle or (uint)AID.FocusedTremorOuter)
        {
            aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        int show = 0;
        var currentAOEs = aoes.OrderBy(a => a.Activation).Take(2).ToList();

        foreach (ref var aoe in CollectionsMarshal.AsSpan(currentAOEs))
        {
            aoe.Color = show == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = show == 0;
            show++;
        }

        return CollectionsMarshal.AsSpan(currentAOEs);
    }
}

[SkipLocalsInit]
sealed class MachetaurStates : StateMachineBuilder
{
    public MachetaurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FocusedTremor>()
            .ActivateOnEnter<FocusedTremorCircle>()
            .ActivateOnEnter<BruntOfTheBattlefield>()
            .ActivateOnEnter<Uplift>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(MachetaurStates),
    ConfigType = null, // replace null with typeof(MachetaurConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = null, // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Machetaur,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14735u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Machetaur(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);

