namespace BossMod.Dawntrail.Foray.FATE.RuinHound;

public enum OID : uint
{
    Boss = 0x4D5E,
    Helper = 0x233C,
    IcePillar = 0x4D5F, // R2.000, x0 (spawn during fight)
    RuinHound = 0x4DA0, // R1.000, x0 (spawn during fight)
    RuinHound1 = 0x4D60, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50536, // Boss->player, no cast, single-target
    IcePillarCast = 49758, // Boss->self, 3.0s cast, single-target
    IcePillar = 49770, // 4D5F->self, 3.0s cast, range 4 circle
    RoaringBlizzard = 49765, // Boss->self, 5.0s cast, range 50 60-degree cone
    Rush = 49759, // 4D5F->self, 4.0s cast, range 80 width 4 rect
    AgeOfEndlessFrostCast = 49760, // Boss->self, 3.0s cast, single-target
    AgeOfEndlessFrost = 49761, // 4DA0->self, 3.0s cast, range 40 60-degree cone
    TheStormWithout = 49757, // Boss->self, 5.0s cast, range 10-40 donut
    TheStormWithout1 = 49767, // 4D60->location, no cast, range 10-40 donut
    TheStormWithin = 49756, // Boss->self, 5.0s cast, range 10 circle
    TheStormWithin1 = 49766, // 4D60->location, no cast, range 10 circle
}

class IcePillar(BossModule module) : Components.StandardAOEs(module, AID.IcePillar, 4);
class RoaringBlizzard(BossModule module) : Components.StandardAOEs(module, AID.RoaringBlizzard, new AOEShapeCone(50, 30.Degrees()));
class Rush(BossModule module) : Components.StandardAOEs(module, AID.Rush, new AOEShapeRect(80, 2));
class AgeOfEndlessFrost(BossModule module) : Components.StandardAOEs(module, AID.AgeOfEndlessFrost, new AOEShapeCone(40, 30.Degrees()));

// each aoe hits twice for some stupid reason
class TheStorm(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TheStormWithin:
                _predicted.Add(new(new AOEShapeCircle(10), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case AID.TheStormWithout:
                _predicted.Add(new(new AOEShapeDonut(10, 40), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TheStormWithin1 or AID.TheStormWithout1 && _predicted.Count > 0)
            _predicted.RemoveAt(0);
    }
}

class RuinHoundStates : StateMachineBuilder
{
    public RuinHoundStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IcePillar>()
            .ActivateOnEnter<RoaringBlizzard>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<AgeOfEndlessFrost>()
            .ActivateOnEnter<TheStorm>();
    }
}

[ModuleInfo(Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14762)]
public class RuinHound(WorldState ws, Actor primary) : BossModule(ws, primary, new(-90, 865), new ArenaBoundsCircle(30));
