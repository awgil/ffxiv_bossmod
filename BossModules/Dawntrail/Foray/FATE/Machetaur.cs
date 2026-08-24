namespace BossMod.Dawntrail.Foray.FATE.Machetaur;

public enum OID : uint
{
    Boss = 0x4C26,
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

class FocusedTremor(BossModule module) : Components.RaidwideCast(module, AID.FocusedTremorCast);
class BruntOfTheBattlefield(BossModule module) : Components.StandardAOEs(module, AID.BruntOfTheBattlefield, 10);
class Uplift(BossModule module) : Components.StandardAOEs(module, AID.Uplift, 6);

class FocusedTremorCircle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.FocusedTremorInner => new AOEShapeCircle(10),
            AID.FocusedTremorMiddle => new AOEShapeDonut(10, 20),
            AID.FocusedTremorOuter => new AOEShapeDonut(20, 30),
            _ => null
        };

        if (shape != null)
        {
            aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            aoes.SortBy(a => a.Activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FocusedTremorInner or AID.FocusedTremorMiddle or AID.FocusedTremorOuter && aoes.Count > 0)
            aoes.RemoveAt(0);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (i, aoe) in aoes.Take(2).Select((a, i) => (i, a)))
            yield return aoe with { Color = i == 0 ? ArenaColor.Danger : ArenaColor.AOE, Risky = i == 0 };
    }
}

class OctupleSwipe(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCone shape = new(40, 45.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OctupleSwipeVisual)
        {
            var next = aoes.Count > 0 ? aoes[^1].Activation.AddSeconds(2.1f) : Module.CastFinishAt(spell, 7.3f);
            aoes.Add(new(shape, spell.LocXZ, spell.Rotation, next));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.OctupleSwipe1 or AID.OctupleSwipe2 or AID.OctupleSwipe3 or AID.OctupleSwipe4 && aoes.Count > 0)
            aoes.RemoveAt(0);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (i, aoe) in aoes.Take(2).Select((a, i) => (i, a)))
            yield return aoe with { Color = i == 0 ? ArenaColor.Danger : ArenaColor.AOE, Risky = i == 0 };
    }
}

class MachetaurStates : StateMachineBuilder
{
    public MachetaurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FocusedTremor>()
            .ActivateOnEnter<BruntOfTheBattlefield>()
            .ActivateOnEnter<Uplift>()
            .ActivateOnEnter<FocusedTremorCircle>()
            .ActivateOnEnter<OctupleSwipe>();
    }
}

[ModuleInfo(Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14735)]
public class Machetaur(WorldState ws, Actor primary) : BossModule(ws, primary, new(724, 220), new ArenaBoundsCircle(30));
