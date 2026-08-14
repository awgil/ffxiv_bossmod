namespace BossMod.Dawntrail.Foray.FATE.RagingThrall;

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

    OctupleSwipe = 47600, // Machetaur->self, 10.0s cast, single-target
    OctupleSwipeVisual = 47601, // Machetaur1->self, 1.0s cast, range 40 90-degree cone
    OctupleSwipe1 = 47604, // Machetaur->self, no cast, range 40 90-degree cone
    OctupleSwipe2 = 47605, // Machetaur->self, no cast, range 40 90-degree cone
    OctupleSwipe3 = 47602, // Machetaur->self, no cast, range 40 90-degree cone
    OctupleSwipe4 = 47603, // Boss->self, no cast, range 40 90-degree cone
}

sealed class FocusedTremor(BossModule module) : Components.RaidwideCast(module, (uint)AID.FocusedTremorCast);
sealed class BruntOfTheBattlefield(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BruntOfTheBattlefield, 10.0f);
sealed class Uplift(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Uplift, 6.0f);

sealed class FocusedTremorCircle(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> Casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.FocusedTremorInner => new AOEShapeCircle(10.0f),
            AID.FocusedTremorMiddle => new AOEShapeDonut(10.0f, 20.0f),
            AID.FocusedTremorOuter => new AOEShapeDonut(20.0f, 30.0f),
            _ => null
        };

        if (shape != null)
        {
            var origin = spell.LocXZ;
            var rotation = spell.Rotation;
            Casters.Add(new(shape, origin, rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: shape.Distance(origin, rotation)));
            SortHelpers.SortAOEByActivation(Casters);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FocusedTremorInner or (uint)AID.FocusedTremorMiddle or (uint)AID.FocusedTremorOuter)
        {
            if (Casters.Count > 0)
            {
                Casters.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }

        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(Casters);

        for (var i = 0; i < max; i++)
        {
            ref var aoe = ref aoes[i];
            aoe.Color = i == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = i == 0;
        }

        return aoes[..max];
    }
}

sealed class OctupleSwipe(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCone shape = new(40.0f, 45.0f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OctupleSwipeVisual)
        {
            aoes.Add(new(shape, spell.LocXZ, spell.Rotation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.OctupleSwipe1 or (uint)AID.OctupleSwipe2 or (uint)AID.OctupleSwipe3 or (uint)AID.OctupleSwipe4)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = aoes.Count;
        if (count == 0)
        {
            return [];
        }

        var max = count > 2 ? 2 : count;
        var nextAOEs = CollectionsMarshal.AsSpan(aoes);

        for (var i = 0; i < max; i++)
        {
            ref var aoe = ref nextAOEs[i];
            aoe.Color = i == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = i == 0;
        }

        return nextAOEs[..max];
    }
}

[SkipLocalsInit]
sealed class RagingThrallStates : StateMachineBuilder
{
    public RagingThrallStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FocusedTremor>()
            .ActivateOnEnter<FocusedTremorCircle>()
            .ActivateOnEnter<BruntOfTheBattlefield>()
            .ActivateOnEnter<Uplift>()
            .ActivateOnEnter<OctupleSwipe>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(RagingThrallStates),
    ConfigType = null, // replace null with typeof(MachetaurConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Machetaur,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2074u,
    SortOrder = 3,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class RagingThrall(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);
