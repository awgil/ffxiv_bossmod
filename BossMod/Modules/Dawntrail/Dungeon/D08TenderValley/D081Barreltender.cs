namespace BossMod.Dawntrail.Dungeon.D08TenderValley.D081Barreltender;

public enum OID : uint
{
    Boss = 0x4234,
    SmallTenderDrop = 0x1EBBF0,
    LargeTenderDrop = 0x1EBBF1,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872,

    HeavyweightNeedlesVisual = 37384,
    HeavyweightNeedles = 37386,
    TenderDrop = 37387,
    NeedleStorm = 37388,
    NeedleSuperstorm = 37389,
    BarrelBreaker = 37390,
    SucculentStomp = 37391,
    BarbedBellow = 37392,
    BarrelBreakerAOE = 37393,
    PricklyRight = 39154,
    PricklyLeft = 39155,
    TenderFury = 39242
}

class BarbedBellow(BossModule module) : Components.RaidwideCast(module, AID.BarbedBellow);
class NeedleStorm(BossModule module) : Components.StandardAOEs(module, AID.NeedleStorm, 6);
class NeedleSuperstorm(BossModule module) : Components.StandardAOEs(module, AID.NeedleSuperstorm, 11);
class SucculentStomp(BossModule module) : Components.StackWithCastTargets(module, AID.SucculentStomp, 6, 4);
class TenderFury(BossModule module) : Components.SingleTargetCast(module, AID.TenderFury);

class HeavyweightNeedles(BossModule module) : Components.StandardAOEs(module, AID.HeavyweightNeedles, new AOEShapeCone(36, 25.Degrees()));
class PricklyRight(BossModule module) : Components.StandardAOEs(module, AID.PricklyRight, new AOEShapeCone(36, 165.Degrees()));
class PricklyLeft(BossModule module) : Components.StandardAOEs(module, AID.PricklyLeft, new AOEShapeCone(36, 165.Degrees()));

class BarrelBreaker(BossModule module) : Components.Knockback(module)
{
    private readonly List<Source> _sources = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor) => _sources;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BarrelBreaker)
            _sources.Add(new(Module.Center, 15, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BarrelBreaker)
            _sources.Clear();
    }
}

class D081BarreltenderStates : StateMachineBuilder
{
    public D081BarreltenderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BarbedBellow>()
            .ActivateOnEnter<HeavyweightNeedles>()
            .ActivateOnEnter<NeedleStorm>()
            .ActivateOnEnter<NeedleSuperstorm>()
            .ActivateOnEnter<BarrelBreaker>()
            .ActivateOnEnter<SucculentStomp>()
            .ActivateOnEnter<PricklyRight>()
            .ActivateOnEnter<PricklyLeft>()
            .ActivateOnEnter<TenderFury>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "CerQ", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12889)]
public class D081Barreltender(WorldState ws, Actor primary) : BossModule(ws, primary, new(-65, 470), new ArenaBoundsRect(17, 15.5f));
