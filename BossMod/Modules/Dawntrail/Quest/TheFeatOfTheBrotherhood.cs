namespace BossMod.Dawntrail.Quest.TheFeatOfTheBrotherhood;

public enum OID : uint
{
    Boss = 0x4206,
    Helper = 0x233C,
    OathOfFire = 0x4229, // add that stuns/binds allies
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->420A, no cast, single-target

    RoaringStarVisual = 39304, // Boss->self, 8.0s cast, single-target
    RoaringStarRaidwide = 39305, // Helper->self, 8.0s cast, range 40 circle
    RoaringStarLine = 39302, // 44B6->self, 6.0s cast, range 50 width 10 rect

    CoiledStrikeVisual = 37186, // Boss->self, 5.0+1.0s cast, single-target
    CoiledStrikeVisual2 = 37187, // Boss->self, 5.0+1.0s cast, single-target
    CoiledStrike = 37188, // Helper->self, 6.0s cast, range 30 150-degree cone

    SublimeHeat = 37176, // 4228->self, 5.0s cast, range 10 circle
    Burn = 39299, // Helper->self, 5.0s cast, range 46 width 5 rect
    FirstLight = 39298, // Helper->location, 6.0s cast, range 6 circle
    FallenStar = 37205, // Helper->player/4210/420C/420D/420B/420E/420F/420A, 5.0s cast, range 6 circle

    SteelfoldStrikeVisual = 37182, // Boss->location, 5.0+1.0s cast, single-target
    SteelfoldStrikeVisual2 = 39300, // Boss->location, no cast, single-target
    SteelfoldStrike = 37183, // Helper->self, 6.0s cast, range 30 width 8 cross
    SteelfoldStrikeIDK = 39532, // Helper->self, 6.0s cast, range 30 width 8 cross

    OuterWakeVisual = 37202, // Boss->self, 2.5+2.5s cast, single-target
    OuterWake = 37203, // Helper->self, 5.0s cast, range 6-40 donut

    DualPyres1VisualFirst = 37181, // Boss->self, 7.0s cast, single-target
    DualPyres1First = 37310, // Helper->self, 8.0s cast, range 30 180-degree cone
    DualPyres1VisualSecond = 37309, // Boss->self, no cast, single-target
    DualPyres1Second = 37311, // Helper->self, 10.5s cast, range 30 180-degree cone

    DualPyres2VisualFirst = 37177, // Boss->self, 7.0s cast, single-target
    DualPyres2First = 37179, // Helper->self, 8.0s cast, range 30 180-degree cone
    DualPyres2VisualSecond = 37178, // Boss->self, no cast, single-target
    DualPyres2Second = 37180, // Helper->self, 10.5s cast, range 30 180-degree cone

    LayOfTheSun1 = 37207, // Boss->420A, 4.9+3.1s cast, range 6 circle
    LayOfTheSun1Repeat2 = 37208, // Helper->420A, no cast, range 6 circle
    LayOfTheSun1Repeat3 = 37209, // Helper->420A, no cast, range 6 circle
    LayOfTheSun1Repeat4 = 37210, // Helper->420A, no cast, range 6 circle
    LayOfTheSun1Repeat5 = 37211, // Helper->420A, no cast, range 6 circle
    LayOfTheSun1Repeat6 = 37212, // Helper->420A, no cast, range 6 circle
    LayOfTheSun1Repeat7 = 37213, // Helper->420A, no cast, range 6 circle
    LayOfTheSun1Repeat8 = 37214, // Helper->420A, no cast, range 6 circle
    LayOfTheSun1Repeat9 = 37215, // Helper->420A, no cast, range 6 circle

    LayOfTheSun2 = 40065, // Boss->420A, 4.9+3.1s cast, range 6 circle
    LayOfTheSun2Repeat2 = 40066, // Helper->420A, no cast, range 6 circle
    LayOfTheSun2Repeat3 = 40067, // Helper->420A, no cast, range 6 circle
    LayOfTheSun2Repeat4 = 40068, // Helper->420A, no cast, range 6 circle
    LayOfTheSun2Repeat5 = 40069, // Helper->420A, no cast, range 6 circle
    LayOfTheSun2Repeat6 = 40070, // Helper->420A, no cast, range 6 circle
    LayOfTheSun2Repeat7 = 40071, // Helper->420A, no cast, range 6 circle
    LayOfTheSun2Repeat8 = 40072, // Helper->420A, no cast, range 6 circle

    Oathbind = 37216, // Boss->self, 11.0s cast, single-target, binds allies
    NobleTrail = 37218, // Boss->location, 50.0s cast, width 20 rect charge, "enrage" cast on stunned allies

    InnerWakeVisual = 37200, // Boss->self, 2.5+2.5s cast, single-target
    InnerWake = 37201, // Helper->self, 5.0s cast, range 10 circle

    BattleBreaker = 37192, // Boss->self, 5.0s cast, range 40 width 40 rect

    SagaOfDawnAndDutyVisual = 37193, // Boss->location, no cast, ???
    SagaOfDawnAndDuty = 37196, // Helper->self, no cast, range 60 circle
    Shockwave = 39711, // Helper->self, no cast, range 60 circle, repeated hits during Saga, requires wuk limit break to survive
    HeavyImpact = 37197, // Helper->self, no cast, range 60 circle, Saga finisher
}

class LayOfTheSun(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.LayOfTheSun1 or AID.LayOfTheSun2 && WorldState.Actors.Find(spell.TargetID) is Actor target)
            Stacks.Add(new(target, 6, activation: Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.LayOfTheSun1Repeat9 or AID.LayOfTheSun2Repeat8)
            Stacks.Clear();
    }
}
class NobleTrail(BossModule module) : Components.ChargeAOEs(module, AID.NobleTrail, 10)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var e in base.ActiveAOEs(slot, actor))
            if ((e.Activation - WorldState.CurrentTime).TotalSeconds < 10)
                yield return e;
    }
}
class OathOfFire(BossModule module) : Components.Adds(module, (uint)OID.OathOfFire);
class InnerWake(BossModule module) : Components.StandardAOEs(module, AID.InnerWake, new AOEShapeCircle(10));
class DualPyres1First(BossModule module) : Components.StandardAOEs(module, AID.DualPyres1First, new AOEShapeCone(30, 90.Degrees()));
class DualPyres1Second(BossModule module) : Components.StandardAOEs(module, AID.DualPyres1Second, new AOEShapeCone(30, 90.Degrees()))
{
    private DualPyres1First? DP1;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        DP1 ??= Module.FindComponent<DualPyres1First>();
        if (DP1?.ActiveCasters.Any() ?? false)
            yield break;

        foreach (var e in base.ActiveAOEs(slot, actor))
            yield return e;
    }
}
class DualPyres2First(BossModule module) : Components.StandardAOEs(module, AID.DualPyres2First, new AOEShapeCone(30, 90.Degrees()));
class DualPyres2Second(BossModule module) : Components.StandardAOEs(module, AID.DualPyres2Second, new AOEShapeCone(30, 90.Degrees()))
{
    private DualPyres2First? DP2;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        DP2 ??= Module.FindComponent<DualPyres2First>();
        if (DP2?.Casters.Count > 0)
            yield break;

        foreach (var e in base.ActiveAOEs(slot, actor))
            yield return e;
    }
}
class RoaringStar(BossModule module) : Components.RaidwideCast(module, AID.RoaringStarRaidwide);
class RoaringStar1(BossModule module) : Components.StandardAOEs(module, AID.RoaringStarLine, new AOEShapeRect(50, 5));
class CoiledStrike(BossModule module) : Components.StandardAOEs(module, AID.CoiledStrike, new AOEShapeCone(30, 75.Degrees()));
class SublimeHeat(BossModule module) : Components.StandardAOEs(module, AID.SublimeHeat, new AOEShapeCircle(10));
class Burn(BossModule module) : Components.StandardAOEs(module, AID.Burn, new AOEShapeRect(46, 2.5f), maxCasts: 8);
class FirstLight(BossModule module) : Components.StandardAOEs(module, AID.FirstLight, 6);
class SteelfoldStrike(BossModule module) : Components.StandardAOEs(module, AID.SteelfoldStrike, new AOEShapeCross(30, 4));
class OuterWake(BossModule module) : Components.StandardAOEs(module, AID.OuterWake, new AOEShapeDonut(6, 40));
class FallenStar(BossModule module) : Components.SpreadFromCastTargets(module, AID.FallenStar, 6);
class DawnAndDuty(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> Wuks = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == 37195)
            Wuks.Add(caster);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var w in Wuks)
            yield return new AOEInstance(new AOEShapeRect(20, 20), w.Position, w.Rotation, Module.CastFinishAt(w.CastInfo));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.HeavyImpact)
            Wuks.Clear();
    }
}

class GuloolJaJasGloryStates : StateMachineBuilder
{
    public GuloolJaJasGloryStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RoaringStar>()
            .ActivateOnEnter<RoaringStar1>()
            .ActivateOnEnter<CoiledStrike>()
            .ActivateOnEnter<SublimeHeat>()
            .ActivateOnEnter<DualPyres1First>()
            .ActivateOnEnter<DualPyres1Second>()
            .ActivateOnEnter<DualPyres2First>()
            .ActivateOnEnter<DualPyres2Second>()
            .ActivateOnEnter<Burn>()
            .ActivateOnEnter<FirstLight>()
            .ActivateOnEnter<SteelfoldStrike>()
            .ActivateOnEnter<OuterWake>()
            .ActivateOnEnter<FallenStar>()
            .ActivateOnEnter<LayOfTheSun>()
            .ActivateOnEnter<NobleTrail>()
            .ActivateOnEnter<InnerWake>()
            .ActivateOnEnter<OathOfFire>()
            .ActivateOnEnter<DawnAndDuty>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "xan", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70444, NameID = 12734)]
public class GuloolJaJasGlory(WorldState ws, Actor primary) : BossModule(ws, primary, new(353.47f, 596.4f), new ArenaBoundsRect(20, 20, 12.5f.Degrees()))
{
    protected override void DrawArenaForeground(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => x.Type == ActorType.Enemy && x.IsAlly), ArenaColor.PlayerGeneric);
}
