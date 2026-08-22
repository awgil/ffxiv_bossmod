namespace BossMod.Dawntrail.Foray.ForkedTower.FTM2SwordDancer;

public enum OID : uint
{
    Boss = 0x4D76, // R6.000, x1
    Helper = 0x233C, // R0.500, x29, Helper type
    TurnSword = 0x4D77, // R2.000, x4
    CycloSword = 0x4D79, // R2.000, x3
    SteelsbreathSword = 0x4D7A, // R1.000, x5
    UnknownSword = 0x4D7B, // R2.000, x2
    OrdinarySword = 0x4D7C, // R2.000, x16
    SwordDancer = 0x4D7D, // R1.000, x1

    SwordDance = 0x1EC033
}

public enum AID : uint
{
    AutoAttack = 50925, // Boss->player, no cast, single-target
    SwordStormCast = 49617, // Boss->self, 5.0s cast, ???
    SwordStorm = 49684, // Helper->self, no cast, ???
    Jump = 49558, // Boss->location, no cast, single-target
    DeathWall = 49557, // 4D7D->self, no cast, range 24-30 donut
    ThrowingSwordsCast = 49559, // Boss->self, 2.0+1.0s cast, single-target
    ThrowingSwordsInstant = 49560, // Boss->self, no cast, single-target
    Rush1 = 50525, // 4D77->location, 3.0s cast, width 7 rect charge
    Rush2 = 50526, // 4D77->location, 3.0s cast, width 7 rect charge
    RushFixed = 49616, // 4D7C->self, 4.0s cast, range 30 width 6 rect
    Turn1 = 49563, // 4D77->location, 3.5s cast, ???
    Turn2 = 49565, // _Gen_DancingSword4->location, 3.5s cast, ???
    Turn3 = 49566, // _Gen_DancingSword4->location, 3.5s cast, ???
    Turn4 = 49568, // 4D77->location, 3.5s cast, ???
    Turn5 = 49569, // 4D77->location, 3.5s cast, ???
    Turn6 = 49571, // _Gen_DancingSword4->location, 3.5s cast, ???
    Turn7 = 49572, // _Gen_DancingSword4->location, 3.5s cast, ???
    Turn8 = 49574, // 4D77->location, 3.5s cast, ???
    TurnSmall90 = 49575, // Helper->self, 3.5s cast, range 9-14 90-degree donut
    TurnLarge90 = 49577, // Helper->self, 3.5s cast, range 19-24 90-degree donut
    TurnSmall66 = 49578, // Helper->self, 3.5s cast, range 9-14 66-degree donut
    TurnLarge78 = 49580, // Helper->self, 3.5s cast, range 19-24 78-degree donut
    TurnaboutSmall = 49883, // Helper->self, 3.5s cast, range 9-14 66-degree donut
    TurnaboutLarge = 49889, // Helper->self, 3.5s cast, range 19-24 78-degree donut
    MartialMystiqueRight = 49583, // Boss->self, 4.0+1.5s cast, single-target
    MartialMystiqueLeft = 49584, // Boss->self, 4.0+1.5s cast, single-target
    MartialMystiqueAOE = 49585, // Helper->self, 5.5s cast, range 48 width 96 rect
    CycloswordsUnsheathed = 49586, // Boss->self, 3.0s cast, single-target
    Cycloswords = 49587, // Boss->self, 3.0s cast, single-target
    SpinSmallDonut = 49589, // 4D79->self, 1.0s cast, range 15-60 donut
    SpinBigDonut = 49590, // 4D79->self, 1.0s cast, range 20-60 donut
    SpinSmall = 49592, // 4D79->self, 1.0s cast, range 15 circle
    SpinBig = 49593, // 4D79->self, 1.0s cast, range 20 circle
    SwordDanceCast = 49609, // Boss->self, 4.4+0.6s cast, single-target
    SwordDance1 = 49610, // Helper->self, 5.0s cast, ???
    SwordDance2 = 49611, // Helper->self, no cast, ???
    SwordDance3 = 49612, // Helper->self, no cast, ???
    SwordDance4 = 49613, // Helper->self, no cast, ???
    SwordDanceRect = 49614, // Helper->self, 1.5s cast, range 60 width 20 rect
    LeapingLiftCast = 49594, // Boss->self, 3.0s cast, single-target
    Pierce = 49595, // 4D7A->self, 3.6s cast, range 5 circle
    LeapingLift1 = 49596, // Boss->location, no cast, ???
    LeapingLift2 = 49597, // Boss->location, no cast, single-target
    LeapingLift3 = 49598, // Boss->location, no cast, ???
    Swordpointe = 49685, // Boss->self, 2.0+1.0s cast, single-target
    Steelsbreath1 = 49599, // 4D7A->self, 2.0s cast, ???
    Steelsbreath2 = 50359, // Helper->self, 2.0s cast, ???
    SurgeswordsUnsheathed = 49615, // Boss->self, 3.0s cast, single-target
}

public enum SID : uint
{
    Unk3558 = 3558, // none->4D79, extra=0x46E/0x46F
    Unk2056 = 2056, // none->Boss/4D7A, extra=0x47A/0x47B
}

public enum TetherID : uint
{
    SwordRight = 423, // 4D77->Boss
    SwordLeft = 424, // 4D77->Boss
}

class SwordStorm(BossModule module) : Components.RaidwideCast(module, AID.SwordStormCast);
class Rush1(BossModule module) : Components.ChargeAOEs(module, AID.Rush1, 3.5f);
class Rush2(BossModule module) : Components.ChargeAOEs(module, AID.Rush2, 3.5f);
class Rush3(BossModule module) : Components.StandardAOEs(module, AID.RushFixed, new AOEShapeRect(30, 3));
class TurnSmall(BossModule module) : Components.StandardAOEs(module, AID.TurnSmall90, new AOEShapeDonutSector(9, 14, 45.Degrees()));
class TurnLarge(BossModule module) : Components.StandardAOEs(module, AID.TurnLarge90, new AOEShapeDonutSector(19, 24, 45.Degrees()));
class TurnSmallShort(BossModule module) : Components.StandardAOEs(module, AID.TurnSmall66, new AOEShapeDonutSector(9, 14, 33.Degrees()));
class TurnLargeShort(BossModule module) : Components.StandardAOEs(module, AID.TurnLarge78, new AOEShapeDonutSector(19, 24, 39.Degrees()));
class TurnaboutSmall(BossModule module) : Components.StandardAOEs(module, AID.TurnaboutSmall, new AOEShapeDonutSector(9, 14, 33.Degrees()));
class TurnaboutLarge(BossModule module) : Components.StandardAOEs(module, AID.TurnaboutLarge, new AOEShapeDonutSector(19, 24, 39.Degrees()));
class MartialMystique(BossModule module) : Components.StandardAOEs(module, AID.MartialMystiqueAOE, new AOEShapeRect(48, 48));

class Spin(BossModule module) : Components.GenericAOEs(module)
{
    bool Draw;

    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Draw ? _predicted : [];

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if ((OID)actor.OID == OID.CycloSword && animState1 == 1 && animState2 == 0)
        {
            AOEShape? shape = modelState switch
            {
                4 => new AOEShapeDonut(15, 60),
                5 => new AOEShapeDonut(20, 60),
                7 => new AOEShapeCircle(15),
                31 => new AOEShapeCircle(20),
                _ => null
            };

            if (shape != null)
                _predicted.Add(new(shape, actor.Position, default, WorldState.FutureTime(13.3f)));
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.CycloSword && id == 0x25EE)
            Draw = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SpinSmall or AID.SpinSmallDonut or AID.SpinBigDonut or AID.SpinBig && _predicted.Count > 0)
        {
            _predicted.RemoveAt(0);
            Draw = false;
        }
    }
}

class SwordDanceRaidwide(BossModule module) : Components.RaidwideCast(module, AID.SwordDance1)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SwordDance4)
            Casters.Clear();
    }
}

class SwordDance(BossModule module) : Components.GenericAOEs(module, AID.SwordDanceRect)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < Math.Min(3, _predicted.Count); i++)
            yield return _predicted[i] with { Color = i == 0 ? ArenaColor.Danger : ArenaColor.AOE, Risky = i == 0 };
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.SwordDance && state == 0x00010002)
            _predicted.Add(new(new AOEShapeRect(60, 10, 60), actor.Position, actor.Rotation, _predicted.Count > 0 ? _predicted[^1].Activation.AddSeconds(2.5f) : WorldState.FutureTime(8.9f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && _predicted.Count > 0)
            _predicted.RemoveAt(0);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var aoe in ActiveAOEs(slot, actor))
            hints.AddForbiddenZone(aoe.Distance, aoe.Activation);
    }
}

class Pierce(BossModule module) : Components.StandardAOEs(module, AID.Pierce, 5);

class Steelsbreath(BossModule module) : Components.Knockback(module, AID.Steelsbreath2)
{
    readonly List<(WPos Origin, DateTime Activation)> _sources = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var src in _sources)
            yield return new(src.Origin, 24, src.Activation);
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        base.OnStatusGain(actor, status);

        if ((OID)actor.OID == OID.SteelsbreathSword && (SID)status.ID == SID.Unk2056 && status.Extra == 0x47B)
        {
            var activation = _sources.Count > 0 ? _sources[^1].Activation.AddSeconds(2.5f) : WorldState.FutureTime(10.8f);
            _sources.Add((actor.Position, activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && _sources.Count > 0)
            _sources.RemoveAt(0);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var srcs = _sources.SkipWhile(s => IsImmune(slot, s.Activation)).GetEnumerator();
        if (!srcs.MoveNext())
            return;

        var kb0 = srcs.Current;
        var orig = kb0.Origin;
        var goal = srcs.MoveNext() ? ShapeDistance.InvertedCircle(srcs.Current.Origin, 6) : ShapeDistance.InvertedCircle(Arena.Center, 24);

        hints.AddForbiddenZone(Sdf.Discrete(p =>
        {
            var off = (p - orig).Normalized() * 24;
            return goal(p + off) < 0;
        }), kb0.Activation);
    }
}

class FTM2SwordDancerStates : StateMachineBuilder
{
    public FTM2SwordDancerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SwordStorm>()
            .ActivateOnEnter<Rush1>()
            .ActivateOnEnter<Rush2>()
            .ActivateOnEnter<Rush3>()
            .ActivateOnEnter<TurnSmall>()
            .ActivateOnEnter<TurnLarge>()
            .ActivateOnEnter<TurnSmallShort>()
            .ActivateOnEnter<TurnLargeShort>()
            .ActivateOnEnter<TurnaboutSmall>()
            .ActivateOnEnter<TurnaboutLarge>()
            .ActivateOnEnter<MartialMystique>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<SwordDanceRaidwide>()
            .ActivateOnEnter<SwordDance>()
            .ActivateOnEnter<Pierce>()
            .ActivateOnEnter<Steelsbreath>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14820)]
public class FTM2SwordDancer(WorldState ws, Actor primary) : BossModule(ws, primary, new(600, 704), new ArenaBoundsCircle(24))
{
    public override bool DrawAllPlayers => true;
}

