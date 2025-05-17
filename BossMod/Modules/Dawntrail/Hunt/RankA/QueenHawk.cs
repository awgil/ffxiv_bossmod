namespace BossMod.Dawntrail.Hunt.RankA.QueenHawk;

public enum OID : uint
{
    Boss = 0x452B, // R2.400, x1
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target
    BeeBeGone = 39482, // Boss->self, 4.0s cast, range 12 circle 'voidzone'
    BeeBeHere = 39483, // Boss->self, 4.0s cast, range 10-40 donut 'voidzone'
    StingingVenom = 39488, // Boss->player, no cast, single-target, heavy damage if standing in 'voidzone'
    ResonantBuzz = 39486, // Boss->self, 5.0s cast, range 40 circle, raidwide + forced march
    FrenziedSting = 39489, // Boss->player, 5.0s cast, single-target tankbuster
    StraightSpindle = 39490, // Boss->self, 4.0s cast, range 50 width 5 rect
}

public enum SID : uint
{
    BeeBeGone = 4147, // Boss->Boss, extra=0x0
    BeeBeHere = 4148, // Boss->Boss, extra=0x0
    ForwardMarch = 2161, // Boss->player, extra=0x0
    AboutFace = 2162, // Boss->player, extra=0x0
    LeftFace = 2163, // Boss->player, extra=0x0
    RightFace = 2164, // Boss->player, extra=0x0
    ForcedMarch = 1257, // Boss->player, extra=0x8/0x2/0x1/0x4
}

class BeeBeGoneHere(BossModule module) : Components.GenericAOEs(module, default, "GTFO from voidzone!")
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeCircle _shapeOut = new(12);
    private static readonly AOEShapeDonut _shapeIn = new(10, 40);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.BeeBeGone => _shapeOut,
            AID.BeeBeHere => _shapeIn,
            _ => null
        };
        if (shape != null)
            _aoe = new(shape, caster.Position, default, Module.CastFinishAt(spell, 0.8f));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (actor == Module.PrimaryActor && (SID)status.ID is SID.BeeBeGone or SID.BeeBeHere)
            _aoe = null;
    }
}

class ResonantBuzzRaidwide(BossModule module) : Components.RaidwideCast(module, AID.ResonantBuzz, "Raidwide + apply forced march");
class ResonantBuzzMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace);
class FrenziedSting(BossModule module) : Components.SingleTargetCast(module, AID.FrenziedSting);
class StraightSpindle(BossModule module) : Components.StandardAOEs(module, AID.StraightSpindle, new AOEShapeRect(50, 2.5f));

class QueenHawkStates : StateMachineBuilder
{
    public QueenHawkStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BeeBeGoneHere>()
            .ActivateOnEnter<ResonantBuzzRaidwide>()
            .ActivateOnEnter<ResonantBuzzMarch>()
            .ActivateOnEnter<FrenziedSting>()
            .ActivateOnEnter<StraightSpindle>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13361)]
public class QueenHawk(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
