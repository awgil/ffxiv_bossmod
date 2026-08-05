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
    OctupleSwipeVisual = 47601, // Machetaur1->self, 1.0s cast, range 40 ?-degree cone
    OctupleSwipe1 = 47604, // Machetaur->self, no cast, range 40 ?-degree cone
    OctupleSwipe2 = 47605, // Machetaur->self, no cast, range 40 ?-degree cone
    OctupleSwipe3 = 47602, // Machetaur->self, no cast, range 40 ?-degree cone
}

class FocusedTremor(BossModule module) : Components.RaidwideCast(module, AID.FocusedTremor);
class BruntOfTheBattlefield(BossModule module) : Components.StandardAOEs(module, AID.BruntOfTheBattlefield, 10.0f);
class Uplift(BossModule module) : Components.StandardAOEs(module, AID.Uplift, 6.0f);

class FocusedTremorCircle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FocusedTremorInner)
        {
            aoes.Add(new(new AOEShapeCircle(10), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }

        if (spell.Action.ID == (uint)AID.FocusedTremorMiddle)
        {
            aoes.Add(new(new AOEShapeDonut(10, 20), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }

        if (spell.Action.ID == (uint)AID.FocusedTremorOuter)
        {
            aoes.Add(new(new AOEShapeDonut(20, 30), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
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

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (aoes.Count == 0)
        {
            yield break;
        }

        int show = 0;
        foreach (var aoe in aoes.OrderBy(aoe => aoe.Activation).Take(2))
        {
            yield return aoe with { Color = show == 0 ? ArenaColor.Danger : ArenaColor.AOE, Risky = show == 0 };
            show++;
        }
    }
}

class OctupleSwipe(BossModule module) : Components.GenericAOEs(module)
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
        if (spell.Action.ID is (uint)AID.OctupleSwipe1 or (uint)AID.OctupleSwipe2 or (uint)AID.OctupleSwipe3)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
            }
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (aoes.Count == 0)
        {
            yield break;
        }

        int show = 0;
        foreach (var aoe in aoes.OrderBy(aoe => aoe.Activation).Take(2))
        {
            yield return aoe with { Color = show == 0 ? ArenaColor.Danger : ArenaColor.AOE, Risky = show == 0 };
            show++;
        }
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

[ModuleInfo(Incomplete = true, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14735)]
public class Machetaur(WorldState ws, Actor primary) : BossModule(ws, primary, new(724.000f, 220.000f), new ArenaBoundsCircle(30));
