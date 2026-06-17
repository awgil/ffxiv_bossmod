namespace BossMod.Dawntrail.Variant.V01NMerchantsTale.V06NDeadlyDandan;

public enum OID : uint
{
    Boss = 0x4ACF, // R24.000, x?
    DeadlyDandan = 0x4B96, // R1.000, x?
    DeadlyDandan1 = 0x4AD2, // R0.000-0.500, x?, Part type
    StingingTentacle = 0x4AD0,
    AiryBubble = 0x4AD1, // R1.300, x0 (spawn during fight)
    AqueousOrb = 0x4C44, // R2.000, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x?, Helper type
}
public enum AID : uint
{
    MurkyWaters = 45615, // 4ACF->location, 5.0s cast, range 50 circle
    DevourStart = 45589, // 4ACF->self, 5.0s cast, single-target
    DevourVisual = 45592, // 4ACF->location, no cast, single-target
    DevourVisual2 = 45593, // 4ACF->location, no cast, single-target
    Devour1 = 45598, // 233C->self, 0.5s cast, range 15 width 20 rect
    Devour2 = 45600, // 233C->self, 0.5s cast, range 15 width 20 rect
    Devour3 = 45599, // 233C->self, 0.5s cast, range 15 width 20 rect
    DevourEnd = 45601, // 233C->self, 0.5s cast, range 15 width 20 rect

    Spit = 45602, // 4ACF->self, 5.0s cast, range 50 120-degree cone
    Dropsea = 47398, // 233C->player, 5.0s cast, range 5 circle
    StingingTentacleStart = 45607, // 4ACF->self, 7.5s cast, single-target
    StingingTentacleAOE = 45608, // 233C->self, 0.5s cast, range 50 width 14 rect
    StrewnBubbles = 45611, // 4ACF->self, 3.0s cast, single-target
    MawOfTheDeepCast = 45609, // 4ACF->self, 3.0s cast, single-target
    MawOfTheDeep = 45610, // 233C->self, 5.0s cast, range 8 circle
    UnfathomableHorrorCast = 48048, // 4ACF->self, 3.0s cast, single-target
    UnfathomableHorror = 48043, // 4C44->self, 5.0s cast, single-target
    UnfathomableHorror1 = 48044, // 233C->self, 5.0s cast, range 8 circle
    UnfathomableHorror2 = 48045, // 233C->self, 7.5s cast, range 8-16 donut
    UnfathomableHorror3 = 48046, // 233C->self, 10.0s cast, range 16-24 donut
    UnfathomableHorror4 = 48047, // 233C->self, 12.5s cast, range 24-36 donut
    TidalGuillotineVisual = 45604, // 4ACF->location, 8.0s cast, single-target
    TidalGuillotine = 45605, // 233C->self, 8.3s cast, range 20 circle
    SwallowedSea = 45616, // 4ACF->self, 5.0s cast, range 50 120-degree cone
    Devour7 = 45596, // 4ACF->location, no cast, single-target
    Devour8 = 45597, // 4ACF->location, no cast, single-target
    Riptide = 45612, // 4AD1->player, no cast, single-target
    WateryGrave = 45613, // 4AD1->player, no cast, single-target
}
public enum SID : uint
{
    ChargeTelegraph = 2195, //ex 0x3F8
    Bind = 2518, // 4AD1->player, extra=0x0
    Dropsy = 2087, // 4AD1->player, extra=0x0
    BubbleGaol = 5047, // none->player, extra=0xB6
}
public enum IconID : uint
{
    SpreadMarker = 558, // player->self
    Counter = 599, // 4AD0->self
}

class MurkyWaters(BossModule module) : Components.RaidwideCast(module, AID.MurkyWaters);
sealed class Devour(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly Angle FirstAngle = 22.5f.Degrees();
    private static readonly Angle FollowupAngle = 157.5f.Degrees();

    private bool _reverse;
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var e in _aoes)
            yield return e with { Color = ArenaColor.AOE };
    }
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((OID)actor.OID == OID.Boss && (SID)status.ID == SID.ChargeTelegraph)
            _reverse = status.Extra == 0x3F9;
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.DevourStart)
            return;

        _aoes.Clear();

        var startPos = caster.Position;
        var inward = (Arena.Center - startPos).ToAngle();

        var firstAngle = inward + (_reverse ? -FirstAngle : FirstAngle);
        var firstDir = firstAngle.ToDirection().Normalized();
        var firstLength = Intersect.RayCircle(startPos, firstDir, Arena.Center, 30f);

        var midPos = startPos + firstDir * firstLength;

        var secondAngle = firstAngle + (_reverse ? FollowupAngle : -FollowupAngle);
        var secondDir = secondAngle.ToDirection().Normalized();
        var secondLength = Intersect.RayCircle(midPos, secondDir, Arena.Center, 30f);

        _aoes.Add(new AOEInstance(new AOEShapeRect(firstLength, 10f), startPos, firstDir.ToAngle()));
        _aoes.Add(new AOEInstance(new AOEShapeRect(secondLength, 10f), midPos, secondDir.ToAngle()));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DevourEnd)
        {
            _reverse = false;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}
class SwallowedSea(BossModule module) : Components.GroupedAOEs(module, [AID.Spit, AID.SwallowedSea], new AOEShapeCone(50f, 60.Degrees()));
class Dropsea(BossModule module) : Components.IconStackSpread(module, 0, (uint)IconID.SpreadMarker, null, AID.Dropsea, 0, 5f, 4.7f, alwaysShowSpreads: true);
class StingingTentacle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _tents = [];
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeRect rect = new(50f, 7f);
    private DateTime _activation;
    private bool _active;

    private static readonly Angle InwardOffset = 27f.Degrees();

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_active || _tents.Count == 0)
            yield break;

        if (_aoes.Count == 0) //Honestly, just doubling up the hints is simpler
        {
            if (_tents.Count == 2)
            {
                var a = _tents[0];
                var b = _tents[1];

                var aCrossRot = InwardRotationTowardOther(a, b);
                var bCrossRot = InwardRotationTowardOther(b, a);
                foreach (var t in _tents)
                    yield return new AOEInstance(rect, t.Position, t.Rotation, _activation) with { Color = ArenaColor.AOE };

                yield return new AOEInstance(rect, a.Position, aCrossRot, _activation) with { Color = ArenaColor.AOE };
                yield return new AOEInstance(rect, b.Position, bCrossRot, _activation) with { Color = ArenaColor.AOE };
            }
        }
        if (_aoes.Count > 0)
        {
            foreach (var e in _aoes)
                yield return e with { Color = ArenaColor.AOE };
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.StingingTentacle)
            _tents.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.StingingTentacle)
            _tents.Remove(actor);
    }
    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (actor.OID == 0x4B96 && status.ID == 2056)
            _aoes.Add(new AOEInstance(rect, actor.Position, actor.Rotation, _activation));
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StingingTentacleStart)
        {
            _active = true;
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StingingTentacleAOE)
        {
            _active = false;
            _activation = default;
            _tents.Clear();
            _aoes.Clear();
        }
    }

    private static Angle InwardRotationTowardOther(Actor self, Actor other)
    {
        var toOther = (other.Position - self.Position).ToAngle();
        var diff = toOther - self.Rotation;

        return self.Rotation + (diff.Rad >= 0 ? InwardOffset : -InwardOffset);
    }
}
class StrewnBubble : Components.PersistentVoidzone
{
    private readonly List<Actor> _bubbles = [];

    public StrewnBubble(BossModule module) : base(module, 2.6f, _ => [], 8)
    {
        Sources = _ => _bubbles;
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.AiryBubble && !actor.Position.AlmostEqual(Module.Center, 5))
            _bubbles.Add(actor);
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.AiryBubble)
            _bubbles.Remove(actor);
    }
}
class MawOfTheDeep(BossModule module) : Components.StandardAOEs(module, AID.MawOfTheDeep, 8f, 10);
class AqueousOrb(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(8), new AOEShapeDonut(8, 16), new AOEShapeDonut(16, 24), new AOEShapeDonut(24, 36)])
{
    private readonly List<Actor> _orbs = [];
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.AqueousOrb)
            _orbs.Add(actor);
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.AqueousOrb)
            _orbs.Remove(actor);
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.UnfathomableHorror1)
        {
            foreach (var o in _orbs)
            {
                AddSequence(o.Position, Module.CastFinishAt(spell));
            }
            _orbs.Clear();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.UnfathomableHorror1 => 0,
            AID.UnfathomableHorror2 => 1,
            AID.UnfathomableHorror3 => 2,
            AID.UnfathomableHorror4 => 3,
            _ => -1
        };
        if (!AdvanceSequence(order, caster.Position, WorldState.FutureTime(2f)))
            ReportError($"unexpected order {order}");
    }
}
class TidalGuillotine(BossModule module) : Components.StandardAOEs(module, AID.TidalGuillotine, 20f);

class V06NDeadlyDandanStates : StateMachineBuilder
{
    public V06NDeadlyDandanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MurkyWaters>()
            .ActivateOnEnter<Devour>()
            .ActivateOnEnter<SwallowedSea>()
            .ActivateOnEnter<Dropsea>()
            .ActivateOnEnter<StingingTentacle>()
            .ActivateOnEnter<StrewnBubble>()
            .ActivateOnEnter<MawOfTheDeep>()
            .ActivateOnEnter<AqueousOrb>()
            .ActivateOnEnter<TidalGuillotine>();
    }
}

[ModuleInfo(Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1066, NameID = 14475)]
public class V06NDeadlyDandan(WorldState ws, Actor primary) : BossModule(ws, primary, new(805.3f, 669.9f), new ArenaBoundsCircle(20f));
