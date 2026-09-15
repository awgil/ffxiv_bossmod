namespace BossMod.Dawntrail.Foray.CriticalEngagement.PhantomHydra;

public enum OID : uint
{
    Boss = 0x4BC5, // R4.800, x1
    BallOfFire = 0x4BC7, // R1.500, x12
    BallOfLevin = 0x4BC9, // R2.300, x3
    SwirlingOrb = 0x4BC8, // R0.500, x3
    HolySphere = 0x4BC6, // R1.200, x2
    Helper = 0x233C, // R0.500, x36, Helper type
}

public enum AID : uint
{
    AutoAttack = 50759, // Boss->player, no cast, single-target
    NighDrawnEruption = 47197, // Boss->self, 5.0+2.0s cast, single-target
    FarFlungEruption = 47198, // Boss->self, 5.0+2.0s cast, single-target
    ElementalCascadeBig1 = 47199, // Helper->location, 7.0s cast, range 8 circle
    ElementalCascadeBig2 = 47200, // Helper->location, 7.0s cast, range 8 circle
    ElementalCascadeBig3 = 47201, // Helper->location, 7.0s cast, range 8 circle
    ElementalCascadeBig4 = 47202, // Helper->location, 7.0s cast, range 8 circle
    ElementalCascadeBig5 = 47203, // Helper->location, 7.0s cast, range 8 circle
    ElementalCascadeBoss = 47184, // Boss->self, 3.0s cast, single-target
    ElementalCascadeSmall1 = 47185, // Helper->location, 3.0s cast, range 6 circle
    ElementalCascadeSmall2 = 47186, // Helper->location, 3.0s cast, range 6 circle
    ElementalCascadeSmall3 = 47187, // Helper->location, 3.0s cast, range 6 circle
    ElementalCascadeSmall4 = 47188, // Helper->location, 3.0s cast, range 6 circle
    ElementalCascadeSmall5 = 47189, // Helper->location, 3.0s cast, range 6 circle
    Dissipate = 47193, // Helper->self, no cast, range 1 circle
    ScarletThread = 47190, // 4BC7->self, 3.0s cast, range 70 width 4 rect
    Shock = 47194, // Helper->location, 4.0s cast, range 10 circle
    LevinRing1 = 47195, // Helper->location, 7.0s cast, range 10-20 donut
    LevinRing2 = 47196, // Helper->location, 10.0s cast, range 20-30 donut
    StunningSheen = 47191, // 4BC6->self, 5.0s cast, range 40 circle
    IceBurst = 47192, // Helper->self, 3.0s cast, range 40 20-degree cone
    DiscordanceCast = 47209, // Boss->self, 5.0s cast, single-target
    Discordance = 47210, // Helper->self, no cast, ???
    RadiantBreath = 47208, // Boss->self, no cast, single-target
    ManyHeadedBreathTelegraph = 47212, // Helper->self, 1.0s cast, range 30 120-degree cone
    ManyHeadedBreathCast = 47213, // Boss->self, 8.0s cast, single-target
    ManyHeadedBreathBoss1 = 47205, // Boss->self, no cast, ???
    ManyHeadedBreathBoss2 = 47206, // Boss->self, no cast, ???
    ManyHeadedBreathBoss3 = 47207, // Boss->self, no cast, ???
    ManyHeadedBreath1 = 50673, // Helper->self, 0.8s cast, range 30 120-degree cone
    ManyHeadedBreath2 = 50674, // Helper->self, 0.8s cast, range 30 120-degree cone
    ManyHeadedBreath3 = 50675, // Helper->self, 0.8s cast, range 30 120-degree cone
}

class ElementalCascadeBig(BossModule module) : Components.GroupedAOEs(module, [AID.ElementalCascadeBig3, AID.ElementalCascadeBig1, AID.ElementalCascadeBig4, AID.ElementalCascadeBig5, AID.ElementalCascadeBig2], new AOEShapeCircle(8));
class ElementalCascadeSmall(BossModule module) : Components.GroupedAOEs(module, [AID.ElementalCascadeSmall4, AID.ElementalCascadeSmall1, AID.ElementalCascadeSmall3, AID.ElementalCascadeSmall5, AID.ElementalCascadeSmall2], new AOEShapeCircle(6));
class ScarletThread(BossModule module) : Components.StandardAOEs(module, AID.ScarletThread, new AOEShapeRect(70, 2));
class LevinRing(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Shock)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var seq = (AID)spell.Action.ID switch
        {
            AID.Shock => 0,
            AID.LevinRing1 => 1,
            AID.LevinRing2 => 2,
            _ => -1
        };

        if (seq >= 0)
            AdvanceSequence(seq, spell.TargetXZ, WorldState.FutureTime(3));
    }
}

class Dissipate(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<(Actor orb, DateTime finish)> _orbs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (orb, d) in _orbs)
            yield return new(new AOEShapeCircle(9), orb.Position, default, d);

        foreach (var (orb, d) in _orbs)
        {
            var radius = Math.Clamp((8.9 - (d - WorldState.CurrentTime).TotalSeconds) / 8.9, 0, 1) * 8 + 1;
            yield return new(new AOEShapeCircle((float)radius), orb.Position, default, default, ArenaColor.Danger);
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EBFC7)
            _orbs.Add((actor, WorldState.FutureTime(8.9f)));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == 0x1EBFC7)
            _orbs.RemoveAll(o => o.orb == actor);
    }
}

class StunningSheen(BossModule module) : Components.CastGaze(module, AID.StunningSheen);
class IceBurst(BossModule module) : Components.StandardAOEs(module, AID.IceBurst, new AOEShapeCone(40, 10.Degrees()));

class ManyHeadedBreath(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < Math.Min(2, _predicted.Count); i++)
            yield return _predicted[i] with { Color = i == 0 ? ArenaColor.Danger : ArenaColor.AOE };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ManyHeadedBreathTelegraph)
        {
            var dt = _predicted.Count > 0 ? _predicted[^1].Activation.AddSeconds(2.1f) : Module.CastFinishAt(spell, 8);
            _predicted.Add(new(new AOEShapeCone(30, 60.Degrees()), spell.LocXZ, spell.Rotation, dt));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ManyHeadedBreath2 or AID.ManyHeadedBreath3 or AID.ManyHeadedBreath1 && _predicted.Count > 0)
            _predicted.RemoveAt(0);
    }
}

class Discordance(BossModule module) : Components.RaidwideCastDelay(module, AID.DiscordanceCast, AID.Discordance, 1);

class PhantomHydraStates : StateMachineBuilder
{
    public PhantomHydraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElementalCascadeBig>()
            .ActivateOnEnter<ElementalCascadeSmall>()
            .ActivateOnEnter<ScarletThread>()
            .ActivateOnEnter<LevinRing>()
            .ActivateOnEnter<Dissipate>()
            .ActivateOnEnter<StunningSheen>()
            .ActivateOnEnter<IceBurst>()
            .ActivateOnEnter<Discordance>()
            .ActivateOnEnter<ManyHeadedBreath>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14523)]
public class PhantomHydra(WorldState ws, Actor primary) : CEModule(ws, primary, new(-82, 485), new ArenaBoundsCircle(19.5f));

