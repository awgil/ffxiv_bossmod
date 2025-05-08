namespace BossMod.Endwalker.Hunt.RankA.MoussePrincess;

public enum OID : uint
{
    Boss = 0x360B, // R6.000, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    PrincessThrenodyPrepare = 27318, // Boss->self, 4.0s cast, range 40 120-degree cone
    PrincessThrenodyResolve = 27319, // Boss->self, 1.0s cast, range 40 120-degree cone
    WhimsyAlaMode = 27320, // Boss->self, 4.0s cast, single-target
    AmorphicFlail = 27321, // Boss->self, 5.0s cast, range 9 circle
    PrincessCacophony = 27322, // Boss->location, 5.0s cast, range 12 circle
    Banish = 27323, // Boss->player, 5.0s cast, single-target
    RemoveWhimsy = 27634, // Boss->self, no cast, single-target, removes whimsy debuffs
}

public enum SID : uint
{
    RightwardWhimsy = 2840,
    LeftwardWhimsy = 2841,
    BackwardWhimsy = 2842,
    ForwardWhimsy = 2958,
}

class PrincessThrenody(BossModule module) : Components.GenericAOEs(module)
{
    private Angle _direction;
    private DateTime _activation;

    private static readonly AOEShapeCone _shape = new(40, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(_shape, Module.PrimaryActor.Position, _direction, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.PrincessThrenodyPrepare)
        {
            _direction = spell.Rotation + ThrenodyDirection();
            _activation = Module.CastFinishAt(spell, 2); //saw delays of upto ~0.3s higher because delay between Prepare and Resolve can vary
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.PrincessThrenodyResolve)
            _activation = default;
    }

    private Angle ThrenodyDirection()
    {
        foreach (var s in Module.PrimaryActor.Statuses)
        {
            switch ((SID)s.ID)
            {
                case SID.RightwardWhimsy: return -90.Degrees();
                case SID.LeftwardWhimsy: return 90.Degrees();
                case SID.BackwardWhimsy: return 180.Degrees();
                case SID.ForwardWhimsy: return 0.Degrees();
            }
        }
        ReportError("Failed to find whimsy status");
        return default;
    }
}

class WhimsyAlaMode(BossModule module) : Components.CastHint(module, AID.WhimsyAlaMode, "Select direction");
class AmorphicFlail(BossModule module) : Components.StandardAOEs(module, AID.AmorphicFlail, new AOEShapeCircle(9));
class PrincessCacophony(BossModule module) : Components.StandardAOEs(module, AID.PrincessCacophony, 12);
class Banish(BossModule module) : Components.SingleTargetCast(module, AID.Banish);

class MoussePrincessStates : StateMachineBuilder
{
    public MoussePrincessStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PrincessThrenody>()
            .ActivateOnEnter<WhimsyAlaMode>()
            .ActivateOnEnter<AmorphicFlail>()
            .ActivateOnEnter<PrincessCacophony>()
            .ActivateOnEnter<Banish>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 10630)]
public class MoussePrincess(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
