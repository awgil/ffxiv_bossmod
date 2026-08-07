namespace BossMod.Dawntrail.Foray.CriticalEngagement.Metamorph;

public enum OID : uint
{
    Boss = 0x4C77, // R3.000-6.000, x1
    Helper = 0x233C, // R0.500, x39, Helper type
    Metamorph = 0x4DFD, // R1.000, x1
}

public enum AID : uint
{
    AutoAttack = 48334, // Boss->player, no cast, single-target
    AutoAttackCerberus = 48368, // Boss->player, no cast, single-target
    AutoAttackFan = 48369, // Boss->player, no cast, single-target
    BlackenedRainCast = 48335, // Boss->self, 4.0+1.0s cast, single-target
    BlackenedRain = 48336, // Helper->self, 5.0s cast, ???
    DeathWall = 48367, // 4DFD->self, no cast, range 25-30 donut
    ChangeCerberus = 48338, // Boss->self, 4.0s cast, single-target
    ChangeFan = 48339, // Boss->self, 4.0s cast, single-target
    Revert = 48340, // Boss->self, no cast, single-target
    CyclonicRing = 48354, // Boss->self, 4.0s cast, range 10-30 donut
    ShapeshiftingSupercellCast = 48356, // Boss->self, 5.5+0.5s cast, single-target
    ShapeshiftingSupercellConeSlow = 48357, // Helper->self, 6.0s cast, range 60 90-degree cone
    ShapeshiftingSupercellConeFast = 48359, // Helper->self, 1.5s cast, range 60 90-degree cone
    ShapeshiftingSupercellBossRepeat = 48358, // Boss->self, no cast, single-target
    ShapeshiftingSupercellCircleStart = 50767, // Helper->self, 6.0s cast, range 8 circle
    ShapeshiftingSupercellCircleRepeat = 48360, // Helper->self, 6.0s cast, range 8 circle
    ShapeshiftingSupercellDonut1 = 48361, // Helper->self, 6.0s cast, range 8-16 donut
    ShapeshiftingSupercellDonut2 = 48362, // Helper->self, 6.0s cast, range 16-30 donut
    MadeMagicCast = 48363, // Boss->self, 4.0s cast, single-target
    MadeMagicPuddle = 48364, // Helper->self, no cast, range 0 circle
    CycloneCrossingCast = 48365, // Boss->self, 10.5+1.0s cast, single-target
    CycloneCrossing = 48366, // Helper->self, 11.5s cast, range 60 width 16 cross
    DarkDealing = 48337, // Boss->player, 5.0s cast, single-target
    TongueOfFlame = 48341, // Boss->self, 4.0s cast, range 15 circle
    HellfireFetch = 48342, // Boss->self, no cast, single-target
    HellwardBoundFirst = 48343, // Boss->location, 6.0s cast, width 10 rect charge
    HellwardBoundRest = 48344, // Boss->location, no cast, width 10 rect charge
    HellfireFetchPuddle = 48345, // Helper->location, 7.0s cast, range 6 circle
    Jump1 = 50720, // Boss->location, no cast, single-target
    Jump2 = 48353, // Boss->self, no cast, single-target
    HellishBreathPre1 = 48347, // Helper->self, 2.0s cast, range 60 60-degree cone
    HellishBreathPre2 = 48348, // Helper->self, 4.0s cast, range 60 60-degree cone
    HellishBreathPre3 = 48349, // Helper->self, 6.0s cast, range 60 60-degree cone
    HellishBreathCast = 48346, // Boss->self, 6.0s cast, single-target
    HellishBreathBoss1 = 48350, // Boss->self, no cast, single-target
    HellishBreathBoss2 = 48351, // Boss->self, no cast, single-target
    HellishBreathBoss3 = 48352, // Boss->self, no cast, single-target
    HellishBreathAOE1 = 48662, // Helper->self, 1.1s cast, range 60 60-degree cone
    HellishBreathAOE2 = 48663, // Helper->self, 1.1s cast, range 60 60-degree cone
    HellishBreathAOE3 = 50677, // Helper->self, 1.1s cast, range 60 60-degree cone
}

public enum SID : uint
{
    Transfiguration = 2548, // Boss->Boss, extra=0x174/0x173
    AreaOfInfluenceUp = 1909, // none->Helper, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7
}

public enum IconID : uint
{
    Tankbuster = 198, // player->self
    TurningRight = 546, // Boss->self
    TurningLeft = 547, // Boss->self
}

class BlackenedRain(BossModule module) : Components.RaidwideCastDelay(module, AID.BlackenedRainCast, AID.BlackenedRain, 1.1f);
class DarkDealing(BossModule module) : Components.SingleTargetCast(module, AID.DarkDealing);

class CyclonicRing(BossModule module) : Components.StandardAOEs(module, AID.CyclonicRing, new AOEShapeDonut(10, 30));

class ShapeshiftingSupercellCone(BossModule module) : Components.GenericRotatingAOE(module)
{
    Angle _nextRotation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).Select(a => a with { Color = ArenaColor.AOE });

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ShapeshiftingSupercellConeSlow)
            Sequences.Add(new(new AOEShapeCone(60, 45.Degrees()), spell.LocXZ, spell.Rotation, _nextRotation, Module.CastFinishAt(spell), 2.4f, 6, 1));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.TurningRight:
                _nextRotation = -30.Degrees();
                for (var i = 0; i < Sequences.Count; i++)
                    Sequences.Ref(i).Increment = _nextRotation;
                break;
            case IconID.TurningLeft:
                _nextRotation = 30.Degrees();
                for (var i = 0; i < Sequences.Count; i++)
                    Sequences.Ref(i).Increment = _nextRotation;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ShapeshiftingSupercellConeSlow or AID.ShapeshiftingSupercellConeFast)
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
    }
}
class ShapeshiftingSupercellDonut(BossModule module) : Components.ConcentricAOEs(module, [Shape1, Shape2, Shape3, Shape1, Shape2, Shape3])
{
    public static readonly AOEShape Shape1 = new AOEShapeCircle(8);
    public static readonly AOEShape Shape2 = new AOEShapeDonut(8, 16);
    public static readonly AOEShape Shape3 = new AOEShapeDonut(16, 30);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ShapeshiftingSupercellCircleStart)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var seq = (AID)spell.Action.ID switch
        {
            AID.ShapeshiftingSupercellCircleStart => 0,
            AID.ShapeshiftingSupercellDonut1 => Sequences[0].NumCastsDone > 2 ? 4 : 1,
            AID.ShapeshiftingSupercellDonut2 => Sequences[0].NumCastsDone > 2 ? 5 : 2,
            AID.ShapeshiftingSupercellCircleRepeat => 3,
            _ => -1
        };

        if (seq >= 0)
        {
            AdvanceSequence(seq, caster.Position, WorldState.FutureTime(10));
            Sequences.RemoveAll(s => s.NumCastsDone >= 6);
        }
    }
}

class CycloneCrossing(BossModule module) : Components.StandardAOEs(module, AID.CycloneCrossing, new AOEShapeCross(60, 8));

class MadeMagic(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<(Actor puddle, DateTime max)> _puddle = [];
    float _radius;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (p, t) in _puddle)
            yield return new(new AOEShapeCircle(17.5f), p.Position, default, t);

        if (_radius > 0)
            foreach (var (p, _) in _puddle)
                yield return new(new AOEShapeCircle(_radius), p.Position, Color: ArenaColor.Danger);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == 0x1EC09C)
        {
            _radius = 0;
            _puddle.Add((actor, WorldState.FutureTime(13.5f)));
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == 0x1EC09C)
            _puddle.RemoveAll(p => p.puddle == actor);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.AreaOfInfluenceUp)
            _radius = status.Extra * 2.5f;
    }
}

class TongueOfFlame(BossModule module) : Components.StandardAOEs(module, AID.TongueOfFlame, 15);

class HellwardBound(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<Actor> _arrows = [];
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == 0x1EC09B)
        {
            _arrows.Add(actor);
            Predict();
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        _arrows.Remove(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HellwardBoundFirst)
            Predict();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HellwardBoundFirst or AID.HellwardBoundRest && _predicted.Count > 0)
            _predicted.RemoveAt(0);
    }

    void Predict()
    {
        if (_arrows.Count < 4 || Module.PrimaryActor.CastInfo is not { } ci || _predicted.Count > 0)
            return;

        _arrows.RemoveAll(a => a.Position.AlmostEqual(Arena.Center, 5));

        var pDir = ci.LocXZ - Module.PrimaryActor.Position;

        try
        {
            var a1 = _arrows.First(a => a.Position.InRect(Module.PrimaryActor.Position, pDir.ToAngle(), 90, 0, 3));
            _arrows.Remove(a1);
            var a2 = _arrows.First(a => a.Position.InRect(a1.Position, a1.Rotation, 90, 0, 3));
            _arrows.Remove(a2);
            var a3 = _arrows.First(a => a.Position.InRect(a2.Position, a2.Rotation, 90, 0, 3));

            void dash(WPos start, WPos finish, DateTime activation)
            {
                var d = finish - start;

                _predicted.Add(new(new AOEShapeRect(d.Length(), 5), start, d.ToAngle(), activation));
            }

            dash(Module.PrimaryActor.Position, a1.Position, Module.CastFinishAt(ci));
            dash(a1.Position, a2.Position, Module.CastFinishAt(ci, 2.1f));
            dash(a2.Position, a3.Position, Module.CastFinishAt(ci, 4.2f));
            dash(a3.Position, a3.Position + a3.Rotation.ToDirection() * 35.4f, Module.CastFinishAt(ci, 6.3f));
        }
        catch (InvalidOperationException ex)
        {
            ReportError($"Unable to predict dashes: {ex}");
            // add a single zone so we don't loop forever
            _predicted.Add(new(new AOEShapeRect(pDir.Length(), 5), Module.PrimaryActor.Position, pDir.ToAngle(), Module.CastFinishAt(ci)));
        }
    }
}

class HellfireFetch(BossModule module) : Components.StandardAOEs(module, AID.HellfireFetchPuddle, 6);

class HellishBreath(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < Math.Min(2, _predicted.Count); i++)
        {
            yield return _predicted[i] with { Color = i == 0 ? ArenaColor.Danger : ArenaColor.AOE };
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_predicted.Count > 0)
        {
            var src = _predicted[0].Origin;
            hints.GoalZones.Add(p => p.InCircle(src, 15) ? 0.5f : 0);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HellishBreathPre1:
            case AID.HellishBreathPre2:
            case AID.HellishBreathPre3:
                _predicted.Add(new(new AOEShapeCone(60, 30.Degrees()), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 6.3f)));
                _predicted.SortBy(p => p.Activation);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HellishBreathAOE2 or AID.HellishBreathAOE3 or AID.HellishBreathAOE1 && _predicted.Count > 0)
            _predicted.RemoveAt(0);
    }
}

class MetamorphStates : StateMachineBuilder
{
    public MetamorphStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BlackenedRain>()
            .ActivateOnEnter<DarkDealing>()
            .ActivateOnEnter<CyclonicRing>()
            .ActivateOnEnter<ShapeshiftingSupercellCone>()
            .ActivateOnEnter<ShapeshiftingSupercellDonut>()
            .ActivateOnEnter<CycloneCrossing>()
            .ActivateOnEnter<MadeMagic>()
            .ActivateOnEnter<TongueOfFlame>()
            .ActivateOnEnter<HellwardBound>()
            .ActivateOnEnter<HellfireFetch>()
            .ActivateOnEnter<HellishBreath>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14801)]
public class Metamorph(WorldState ws, Actor primary) : CEModule(ws, primary, new(500, -310), new ArenaBoundsCircle(25));
