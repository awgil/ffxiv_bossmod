namespace BossMod.Heavensward.Trial.T01Ravana;

public enum OID : uint
{
    Boss = 0xEB4, // R3.500, Ravana
    MoonGana = 0xEB6, // R1.000
    SpiritGana = 0xEB8, // R0.600
    RavanasWill = 0xEBA, // R1.000
    Chandrahas = 0xEBC, // R0.700
    IronGate = 0x10A1, // R7.000
    HelperA = 0x1201, // R3.500
    HelperB = 0x1335, // R3.500
    Helper = 0x13D2, // R0.500
}

public enum AID : uint
{
    AutoAttack = 5093, // Boss->player, no cast, single-target
    DragonflyAvatar = 3712, // Boss->self, no cast, single-target
    ScorpionAvatar = 3713, // Boss->self, no cast, single-target
    BeetleAvatar = 3714, // Boss->self, no cast, single-target
    BlindingBlade = 3715, // Boss->player, no cast, range 7 tankbuster
    TheSeeingTail = 3716, // Boss->self, 1.2s cast, applies directional parry (front+back)
    TheSeeingWing = 3717, // Boss->self, 1.2s cast, applies directional parry (left+right)
    Revengeance = 3718, // Helper->self, no cast, single-target
    PreludeToSlaughterCast = 3719, // Boss->self, 14.7s cast, range 15 circle
    PreludeToSlaughterVisual = 3720, // Boss->self, 2.7s cast, single-target
    PreludeToSlaughterRect = 3721, // Helper->self, no cast, range 40 width 8 rect
    PreludeToSlaughterCircle = 3722, // Boss->self, 2.7s cast, range 20 circle
    SlaughterCast = 3723, // Boss->self, 15.7s cast, range 40 cone
    SlaughterVisual = 3724, // Boss->self, 2.7s cast, single-target
    SlaughterRect = 3725, // Helper->self, 3.7s cast, range 44 width 8 rect
    SlaughterCircle = 3726, // HelperA->self, no cast, range 12 circle
    TapasyaNear = 3727, // Boss->self, no cast, range 9 cone (stay inside)
    TapasyaFar = 3728, // Helper->self, no cast, range 12 cone (stay outside)
    TapasyaFollowUp = 3729, // Boss->self, no cast, single-target (follow-up hit)
    FallingLaughter = 3730, // MoonGana/SpiritGana->self, 9.7s cast, single-target
    BloodyFuller = 3731, // Boss->self, 4.7s cast, range 100 circle
    ChandrahasSpawn = 3732, // Chandrahas->self, no cast, single-target
    LaughingMoon = 3734, // HelperA->self, no cast, star puddle at arena edge
    ChandrahasFinale = 3735, // HelperB->self, no cast, small circle at center (stack in)
    TheRoseOfConviction = 3736, // Boss->self, no cast, single-target
    TheRoseOfConquest = 3737, // RavanasWill->players, no cast, range 6 circle
    PillarsOfHeaven = 3738, // Boss->self, 2.7s cast, range 40 circle
    Surpanakha = 3739, // Boss->player, no cast, tankbuster (cleave)
    TheRoseOfHate = 3740, // Boss->self, 2.7s cast, range 40 width 8 rect
    SwiftSlaughter = 3741, // Boss->self, 16.7s cast, single-target enrage
    PillarsOfHeavenImpact = 3742, // Helper/Boss->self, no cast, range 8 circle, corner drops
    BladesVisual = 4769, // Boss->self, 2.7s cast, single-target, visual
    BladesOfCarnageAndLiberation = 4986, // Boss->self, no cast, single-target, visual
    FireAdd = 5015, // MoonGana->player, 0.7s cast, single-target
    BlizzardAdd = 5016, // SpiritGana->player, 0.7s cast, single-target
    SlaughterCross = 5052, // Helper->self, 3.7s cast, range 40 width 8 rect
    PreludeCircleRepeat = 5059, // Helper->self, 2.7s cast, range 20 circle
    SlaughterCircleRepeat = 5066, // Helper->self, 2.7s cast, range 20 circle
}

public enum IconID : uint
{
    Stack = 41, // player, Rose of Conquest
}

public enum TetherID : uint
{
    Will = 17, // RavanasWill->player
}

public enum SID : uint
{
    Invincibility = 325, // boss jump / cutscene immunity (Lunar Ravana quest)
    DirectionalParry = 680, // Seeing Tail/Wing; extra 0x3 = front+back, 0xC = left+right
}

class PreludeToSlaughterCast(BossModule module) : Components.StandardAOEs(module, AID.PreludeToSlaughterCast, 15);
class PreludeToSlaughterCircle(BossModule module) : Components.GroupedAOEs(module, [AID.PreludeToSlaughterCircle, AID.PreludeCircleRepeat, AID.SlaughterCircleRepeat], new AOEShapeCircle(20));
// TODO: PreludeToSlaughterRect (instant helper rects) still unwired
class SlaughterRect(BossModule module) : Components.StandardAOEs(module, AID.SlaughterRect, new AOEShapeRect(44, 4));
class SlaughterCross(BossModule module) : Components.StandardAOEs(module, AID.SlaughterCross, new AOEShapeRect(40, 4));
class SlaughterCast(BossModule module) : Components.StandardAOEs(module, AID.SlaughterCast, new AOEShapeCone(40, 90.Degrees()));
class TheRoseOfHate(BossModule module) : Components.StandardAOEs(module, AID.TheRoseOfHate, new AOEShapeRect(40, 4));
class BloodyFuller(BossModule module) : Components.RaidwideCast(module, AID.BloodyFuller);
class PillarsOfHeaven(BossModule module) : Components.RaidwideCast(module, AID.PillarsOfHeaven);
class SwiftSlaughter(BossModule module) : Components.CastHint(module, AID.SwiftSlaughter, "Enrage!", true);
class BladesVisual(BossModule module) : Components.CastHint(module, AID.BladesVisual, "Blades of Carnage", true);
class BladesOfCarnageCombo(BossModule module) : Components.CastCounter(module, AID.BladesOfCarnageAndLiberation)
{
    private DateTime _hintUntil;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (WorldState.CurrentTime < _hintUntil)
            hints.Add("Out → star → cross → stack center");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            _hintUntil = WorldState.FutureTime(12f);
    }
}

class LaughingMoon(BossModule module) : Components.GenericAOEs(module, AID.LaughingMoon)
{
    private readonly List<(WPos pos, DateTime expire)> _active = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var now = WorldState.CurrentTime;
        _active.RemoveAll(a => a.expire <= now);
        foreach (var a in _active)
            yield return new(new AOEShapeCircle(8), a.pos, Activation: a.expire);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            _active.Add((new WPos(spell.TargetPos.XZ()), WorldState.FutureTime(1.5f)));
    }
}

class ChandrahasFinale(BossModule module) : Components.GenericAOEs(module, AID.ChandrahasFinale, invertedText: "Stack in center!")
{
    private DateTime _expire;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_expire <= WorldState.CurrentTime)
            yield break;
        yield return new(new AOEShapeCircle(10), Module.PrimaryActor.Position, Activation: _expire, Inverted: true);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            _expire = WorldState.FutureTime(2f);
    }
}

class FallingLaughter(BossModule module) : Components.CastInterruptHint(module, AID.FallingLaughter, hintExtra: "add", showNameInHint: true);
class RavanaSeeing(BossModule module) : Components.DirectionalParry(module, (uint)OID.Boss)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var sides = (AID)spell.Action.ID switch
        {
            AID.TheSeeingTail => Side.Front | Side.Back, // matches parry extra 0x3
            AID.TheSeeingWing => Side.Left | Side.Right, // matches parry extra 0xC
            _ => Side.None
        };
        if (sides != Side.None)
            PredictParrySide(caster.InstanceID, sides);
    }
}

abstract class RavanaBossOriginAOEs(BossModule module, Enum aid, AOEShape shape, float duration = 1.2f) : Components.GenericAOEs(module, aid)
{
    private readonly List<(WPos origin, Angle rot, DateTime expire)> _active = [];
    protected abstract WPos Origin(Actor caster);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var now = WorldState.CurrentTime;
        _active.RemoveAll(a => a.expire <= now);
        foreach (var a in _active)
            yield return new(shape, a.origin, a.rot, a.expire);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action != WatchedAction)
            return;
        var origin = Origin(caster);
        _active.Add((origin, caster.Rotation, WorldState.FutureTime(duration)));
    }
}

class SlaughterCircle(BossModule module) : RavanaBossOriginAOEs(module, AID.SlaughterCircle, new AOEShapeCircle(12), 1.5f)
{
    protected override WPos Origin(Actor caster) => Module.PrimaryActor.Position;
}

class Surpanakha(BossModule module) : Components.GenericSharedTankbuster(module, AID.Surpanakha, new AOEShapeCone(40, 45.Degrees()))
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action != WatchedAction)
            return;
        ++NumCasts;
        Source = caster;
        Target = WorldState.Actors.Find(caster.TargetID)
            ?? spell.Targets.Select(t => WorldState.Actors.Find(t.ID)).FirstOrDefault(a => a != null && a.Type is ActorType.Player or ActorType.DutySupport);
        Activation = WorldState.FutureTime(0.6f);
    }

    public override void Update()
    {
        if (Activation != default && WorldState.CurrentTime >= Activation)
            Source = Target = null;
    }
}

class TapasyaNear(BossModule module) : RavanaBossOriginAOEs(module, AID.TapasyaNear, new AOEShapeDonut(9, 40), 1.5f)
{
    protected override WPos Origin(Actor caster) => Module.PrimaryActor.Position;
}

class TapasyaFar(BossModule module) : RavanaBossOriginAOEs(module, AID.TapasyaFar, new AOEShapeCircle(12), 1.5f)
{
    protected override WPos Origin(Actor caster) => Module.PrimaryActor.Position;
}

class PillarsOfHeavenImpact(BossModule module) : Components.GenericAOEs(module, AID.PillarsOfHeavenImpact)
{
    private readonly List<(WPos pos, DateTime expire)> _active = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var now = WorldState.CurrentTime;
        _active.RemoveAll(a => a.expire <= now);
        foreach (var a in _active)
            yield return new(new AOEShapeCircle(8), a.pos, Activation: a.expire); // TODO: verify impact radius
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            _active.Add((new WPos(spell.TargetPos.XZ()), WorldState.FutureTime(1.5f)));
    }
}

class TheRoseOfConquest(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stack, AID.TheRoseOfConquest, 6, 5.1f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action != StackAction)
            return;

        if (Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID
            || spell.Targets.Any(t => t.ID == s.Target.InstanceID)) == 0)
            Stacks.Clear();

        ++NumFinishedStacks;
    }

    public override void Update()
    {
        var now = WorldState.CurrentTime;
        Stacks.RemoveAll(s => s.Activation != default && now >= s.Activation.AddSeconds(1.5f));
    }
}

class RavanaAdds(BossModule module) : Components.AddsMulti(module, [OID.MoonGana, OID.SpiritGana, OID.Chandrahas, OID.IronGate, OID.RavanasWill], 1)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        hints.PrioritizeTargetsByOID((uint)OID.RavanasWill, 3);
        foreach (var g in Module.Enemies((uint)OID.MoonGana).Concat(Module.Enemies((uint)OID.SpiritGana)))
        {
            if (g.IsTargetable && !g.IsDead && g.CastInfo != null)
                hints.SetPriority(g, 2);
        }
    }
}

class IronGateWalls(BossModule module) : BossComponent(module)
{
    private readonly List<WPos> _holes = [];
    private static readonly ArenaBoundsCircle DefaultBounds = new(20);
    private const float HoleRadius = 6f;

    public override void Update()
    {
        var changed = false;
        foreach (var g in Module.Enemies(OID.IronGate))
        {
            if (!g.IsDead && g.IsTargetable)
                continue;
            if (_holes.Any(h => h.AlmostEqual(g.Position, 1)))
                continue;
            _holes.Add(g.Position);
            changed = true;
        }
        if (changed)
            Rebuild();
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID != OID.IronGate)
            return;
        if (!_holes.Any(h => h.AlmostEqual(actor.Position, 1)))
        {
            _holes.Add(actor.Position);
            Rebuild();
        }
    }

    private void Rebuild()
    {
        if (_holes.Count == 0)
        {
            Arena.Bounds = DefaultBounds;
            return;
        }

        var clipper = DefaultBounds.Clipper;
        var poly = clipper.Simplify(new PolygonClipper.Operand(CurveApprox.Circle(20, 0.05f)));
        foreach (var h in _holes)
        {
            var rel = h - Arena.Center;
            poly = clipper.Difference(new(poly), new(CurveApprox.Circle(HoleRadius, 0.05f).Select(p => p + rel)));
        }
        Arena.Bounds = new ArenaBoundsCustom(20, poly);
    }
}

class T01RavanaStates : StateMachineBuilder
{
    public T01RavanaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PreludeToSlaughterCast>()
            .ActivateOnEnter<PreludeToSlaughterCircle>()
            .ActivateOnEnter<SlaughterRect>()
            .ActivateOnEnter<SlaughterCross>()
            .ActivateOnEnter<TheRoseOfHate>()
            .ActivateOnEnter<BloodyFuller>()
            .ActivateOnEnter<PillarsOfHeaven>()
            .ActivateOnEnter<SwiftSlaughter>()
            .ActivateOnEnter<SlaughterCast>()
            .ActivateOnEnter<SlaughterCircle>()
            .ActivateOnEnter<RavanaSeeing>()
            .ActivateOnEnter<Surpanakha>()
            .ActivateOnEnter<TapasyaNear>()
            .ActivateOnEnter<TapasyaFar>()
            .ActivateOnEnter<PillarsOfHeavenImpact>()
            .ActivateOnEnter<BladesVisual>()
            .ActivateOnEnter<BladesOfCarnageCombo>()
            .ActivateOnEnter<LaughingMoon>()
            .ActivateOnEnter<ChandrahasFinale>()
            .ActivateOnEnter<FallingLaughter>()
            .ActivateOnEnter<TheRoseOfConquest>()
            .ActivateOnEnter<IronGateWalls>()
            .ActivateOnEnter<RavanaAdds>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 86, NameID = 3660)] // TODO: clear Incomplete after Blades combo + Tapasya verify
public class T01Ravana(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsCircle(20))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateModuleAIHints(slot, actor, assignment, hints);
        foreach (var h in hints.PotentialTargets)
        {
            if (h.Actor.FindStatus((uint)SID.Invincibility) != null)
                h.Priority = AIHints.Enemy.PriorityInvincible;
        }
    }
}
