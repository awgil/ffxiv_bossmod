namespace BossMod.Shadowbringers.Dungeon.D04MalikahsWell.D043Storge;

public enum OID : uint
{
    Boss = 0x267B, // R=5.0
    RhapsodicNail = 0x267C, // R=1.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    IntestinalCrank = 15601, // Boss->self, 4.0s cast, range 60 circle
    DeformationVisual1 = 15528, // Boss->self, no cast, single-target
    DeformationVisual2 = 16808, // Boss->self, no cast, single-target
    BreakingWheelWait = 17914, // 2BD6->self, 7.5s cast, single-target, before long Breaking Wheel
    BreakingWheel1 = 15605, // Boss->self, 5.0s cast, range 5-60 donut
    BreakingWheel2 = 15610, // RhapsodicNail->self, 9.0s cast, range 5-60 donut
    BreakingWheel3 = 15887, // Boss->self, 29.0s cast, range 5-60 donut
    CrystalNailVisual = 15606, // Boss->self, 2.5s cast, single-target
    CrystalNail = 15607, // RhapsodicNail->self, 2.5s cast, range 5 circle
    Censure1 = 15927, // Boss->self, 3.0s cast, range 60 circle, activates nails for Breaking Wheel
    Censure2 = 15608, // Boss->self, 3.0s cast, range 60 circle, activates nails for Heretics Fork
    HereticForkWait = 17913, // 2BD6->self, 6.5s cast, single-target, before long Heretics Fork
    HereticsFork1 = 15602, // Boss->self, 5.0s cast, range 60 width 10 cross
    HereticsFork2 = 15609, // RhapsodicNail->self, 8.0s cast, range 60 width 10 cross
    HereticsFork3 = 15886 // Boss->self, 23.0s cast, range 60 width 10 cross
}

class IntestinalCrank(BossModule module) : Components.RaidwideCast(module, AID.IntestinalCrank);
class BreakingWheel(BossModule module) : Components.StandardAOEs(module, AID.BreakingWheel1, new AOEShapeDonut(5, 60));
class HereticsFork(BossModule module) : Components.StandardAOEs(module, AID.HereticsFork1, new AOEShapeCross(60, 5));
class CrystalNail(BossModule module) : Components.StandardAOEs(module, AID.CrystalNail, new AOEShapeCircle(5));

class HereticsForkBreakingWheelStreak(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(5, 60);
    private static readonly AOEShapeCross cross = new(60, 5);
    private readonly List<AOEInstance> _spell1 = [];
    private readonly List<AOEInstance> _spell2 = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_spell1.Count > 0)
            yield return _spell1[0];
        if (_spell1.Count == 0 && _spell2.Count > 0)
            yield return _spell2[0];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HereticsFork2:
                _spell1.Add(new(cross, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case AID.BreakingWheel2:
                _spell1.Add(new(donut, caster.Position, default, Module.CastFinishAt(spell)));
                break;
            case AID.HereticsFork3:
                _spell2.Add(new(cross, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case AID.BreakingWheel3:
                _spell2.Add(new(donut, caster.Position, default, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HereticsFork2:
            case AID.BreakingWheel2:
                _spell1.RemoveAt(0);
                break;
            case AID.HereticsFork3:
            case AID.BreakingWheel3:
                _spell2.RemoveAt(0);
                break;
        }
    }
}

class D043StorgeStates : StateMachineBuilder
{
    public D043StorgeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IntestinalCrank>()
            .ActivateOnEnter<CrystalNail>()
            .ActivateOnEnter<HereticsFork>()
            .ActivateOnEnter<BreakingWheel>()
            .ActivateOnEnter<HereticsForkBreakingWheelStreak>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team (Malediktus), Ported by Herculezz", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 656, NameID = 8249)]
public class D043Storge(WorldState ws, Actor primary) : BossModule(ws, primary, new(195, -95), new ArenaBoundsSquare(14.5f));
