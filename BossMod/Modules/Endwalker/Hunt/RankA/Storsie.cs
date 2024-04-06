namespace BossMod.Endwalker.Hunt.RankA.Storsie;

public enum OID : uint
{
    Boss = 0x35DE, // R5.290, x1
};

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    AspectEarth = 27354, // Boss->self, 3.0s cast, single-target
    AspectWind = 27355, // Boss->self, 3.0s cast, single-target
    AspectLightning = 27356, // Boss->self, 3.0s cast, single-target
    Whorlstorm = 27358, // Boss->self, 1.0s cast, range 10-40 donut
    Defibrillate = 27359, // Boss->self, 1.0s cast, range 22 circle
    EarthenAugur = 27360, // Boss->self, 1.0s cast, range 30 270-degree cone
    FangsEnd = 27361, // Boss->player, 5.0s cast, single-target
    AspectEarthApply = 27870, // Boss->self, no cast, single-target
    AspectWindApply = 27871, // Boss->self, no cast, single-target
    AspectLightningApply = 27872, // Boss->self, no cast, single-target
}

class Aspect : Components.GenericAOEs
{
    private AOEShape? _imminentAOE;
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_imminentAOE != null)
            yield return new(_imminentAOE, module.PrimaryActor.Position, module.PrimaryActor.CastInfo?.Rotation ?? module.PrimaryActor.Rotation, _activation);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (caster != module.PrimaryActor)
            return;
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.AspectEarth => new AOEShapeCone(30, 135.Degrees()),
            AID.AspectWind => new AOEShapeDonut(10, 40),
            AID.AspectLightning => new AOEShapeCircle(22),
            _ => null
        };
        if (shape != null)
        {
            _imminentAOE = shape;
            _activation = module.WorldState.CurrentTime.AddSeconds(10.4f);
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (caster == module.PrimaryActor && (AID)spell.Action.ID is AID.Whorlstorm or AID.Defibrillate or AID.EarthenAugur)
            _imminentAOE = null;
    }
}

class FangsEnd : Components.SingleTargetCast
{
    public FangsEnd() : base(ActionID.MakeSpell(AID.FangsEnd)) { }
}

class StorsieStates : StateMachineBuilder
{
    public StorsieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Aspect>()
            .ActivateOnEnter<FangsEnd>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 10623)]
public class Storsie : SimpleBossModule
{
    public Storsie(WorldState ws, Actor primary) : base(ws, primary) { }
}
