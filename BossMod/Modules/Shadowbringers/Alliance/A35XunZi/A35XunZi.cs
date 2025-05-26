
namespace BossMod.Shadowbringers.Alliance.A35XunZi;

public enum OID : uint
{
    Boss = 0x3195, // R15.000, x1
    MengZi = 0x3196, // R15.000, x1
    Helper = 0x233C, // R0.500, x4, Helper type
    _Gen_Energy = 0x3197, // R1.000, x24
    _Gen_SerialJointedModel = 0x3199, // R2.400, x4
    _Gen_SmallFlyer = 0x3198, // R1.320, x0 (spawn during fight)
}

public enum AID : uint
{
    _Weaponskill_DeployArmaments = 23555, // Boss/MengZi->self, 6.0s cast, range 50 width 18 rect
    _Weaponskill_DeployArmaments1 = 23557, // Helper->self, 7.0s cast, range 50 width 18 rect
    _Weaponskill_DeployArmaments2 = 23552, // Boss/MengZi->self, 6.0s cast, range 50 width 18 rect
    _Weaponskill_DeployArmaments3 = 23554, // Helper->self, 6.7s cast, range 50 width 18 rect
    _Weaponskill_DeployArmaments4 = 23553, // MengZi/Boss->self, 7.0s cast, range 50 width 18 rect
    _Weaponskill_DeployArmaments5 = 24696, // Helper->self, 7.7s cast, range 50 width 18 rect
    _Weaponskill_HighPoweredLaser = 23561, // _Gen_SerialJointedModel->self, no cast, range 70 width 4 rect
    _Weaponskill_UniversalAssault = 23558, // MengZi->self, 5.0s cast, range 50 width 50 rect
    _Weaponskill_LowPoweredOffensive = 23559, // _Gen_SmallFlyer->self, 2.0s cast, single-target
    _Ability_EnergyBomb = 23560, // _Gen_Energy->player/31A8, no cast, single-target
}

public enum IconID : uint
{
    _Gen_Icon_ude_lockon01i = 164, // player->self
}

class DeployArmaments(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_DeployArmaments1, AID._Weaponskill_DeployArmaments3, AID._Weaponskill_DeployArmaments5], new AOEShapeRect(50, 9));
class UniversalAssault(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_UniversalAssault);
class Energy : Components.PersistentVoidzone
{
    private readonly List<Actor> _balls = [];

    public Energy(BossModule module) : base(module, 2, _ => [], 8)
    {
        Sources = _ => _balls;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID._Gen_Energy)
        {
            if (id == 0x11D2)
                _balls.Add(actor);
            else if (id == 0x11E7)
                _balls.Remove(actor);
        }
    }
}

class HighPoweredLaser(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_HighPoweredLaser)
{
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            foreach (var m in Module.Enemies(OID._Gen_SerialJointedModel))
                yield return new(new AOEShapeRect(70, 2), m.Position, m.Rotation, _activation);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID._Gen_Icon_ude_lockon01i)
            _activation = WorldState.FutureTime(6.5f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
        }
    }
}

class A35XunZiStates : StateMachineBuilder
{
    public A35XunZiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeployArmaments>()
            .ActivateOnEnter<UniversalAssault>()
            .ActivateOnEnter<Energy>()
            .ActivateOnEnter<HighPoweredLaser>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed
                && module.Enemies(OID.MengZi).All(m => m.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9921)]
public class A35XunZi(WorldState ws, Actor primary) : BossModule(ws, primary, new(800, 800), new ArenaBoundsSquare(24.5f));

