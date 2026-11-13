namespace BossMod.Endwalker.Variant.V02MountRokkon.V022Moko;

public enum OID : uint
{
    Boss = 0x3F81, // R6.000, x1, routes 1, 3 and 4
    BossP2 = 0x3FB3, // R3.450, x1, route 2
    AshigaruSoheiFast = 0x3F82, // R1.000, x0 (spawn during fight)
    AshigaruSoheiSlow = 0x3F83, // R1.000, x0 (spawn during fight)
    AshigaruKyuhei = 0x3F84, // R1.000, x0 (spawn during fight)
    OnisClaw = 0x3F85, // R13.200, x0 (spawn during fight)
    IllComeTengu = 0x3F86, // R2.000, x0 (spawn during fight)
    Spiritflame = 0x3F87, // R2.400, x0 (spawn during fight)
    AncientKatana = 0x3F88, // R1.500, x0 (spawn during fight)
    AshigaruKyuheiMarker = 0x1EB8C9, // R0.500, EventObj type
    Helper = 0x233C, // R0.500, Helper type
}

public enum AID : uint
{
    AutoAttack = 34698, // Boss->player, no cast, single-target
    Teleport = 34223, // Boss->location, no cast, single-target
    KenkiRelease = 34221, // Boss->self, 5.0s cast, range 60 circle, raidwide
    AzureAuspice = 34204, // Boss->self, 5.0s cast, range 6-40 donut
    BoundlessAzureVisual = 34205, // Boss->self, 2.4s cast, single-target
    BoundlessAzure = 34206, // Helper->self, 3.0s cast, range 60 width 10 rect
    UpwellFirst = 34207, // Helper->self, 8.0s cast, range 60 width 10 rect
    UpwellRest = 34208, // Helper->self, 1.0s cast, range 60 width 5 rect
    IaiKasumiGiriBack = 34183, // Boss->self, 5.0s cast, range 60 270-degree cone
    IaiKasumiGiriLeft = 34184, // Boss->self, 5.0s cast, range 60 270-degree cone
    IaiKasumiGiriFront = 34185, // Boss->self, 5.0s cast, range 60 270-degree cone
    IaiKasumiGiriRight = 34186, // Boss->self, 5.0s cast, range 60 270-degree cone
    DoubleKasumiGiriFirstBack = 34187, // Boss->self, 11.0s cast, range 60 270-degree cone
    DoubleKasumiGiriFirstLeft = 34188, // Boss->self, 11.0s cast, range 60 270-degree cone
    DoubleKasumiGiriFirstFront = 34189, // Boss->self, 11.0s cast, range 60 270-degree cone
    DoubleKasumiGiriFirstRight = 34190, // Boss->self, 11.0s cast, range 60 270-degree cone
    DoubleKasumiGiriRestBack = 34191, // Boss->self, 1.5s cast, range 60 270-degree cone
    DoubleKasumiGiriRestLeft = 34192, // Boss->self, 1.5s cast, range 60 270-degree cone
    DoubleKasumiGiriRestFront = 34193, // Boss->self, 1.5s cast, range 60 270-degree cone
    DoubleKasumiGiriRestRight = 34194, // Boss->self, 1.5s cast, range 60 270-degree cone
    SoldiersOfDeath = 34195, // Boss->self, 3.0s cast, single-target
    IronRain = 34196, // AshigaruKyuhei->location, 8.0s cast, range 10 circle
    UntemperedSword = 34216, // Boss->self, 3.0s cast, single-target, route 1
    Unsheathing = 34217, // AncientKatana->location, 2.0s cast, range 3 circle
    VeilSever = 34218, // AncientKatana->self, 5.0s cast, range 40 width 5 rect
    ScarletAuspice = 34200, // BossP2->self, 5.0s cast, range 6 circle, route 2
    MoonlessNight = 34219, // BossP2->self, 3.0s cast, range 60 circle, raidwide
    Clearout = 34220, // OnisClaw->self, 5.0s cast, range 22 180-degree cone
    BoundlessScarletVisual = 34201, // BossP2->self, 2.4s cast, single-target
    BoundlessScarlet = 34202, // Helper->self, 3.0s cast, range 60 width 10 rect
    Explosion = 34203, // Helper->self, 10.0s cast, range 60 width 30 rect
    TenguYobi = 34209, // Boss->self, 3.0s cast, single-target, route 3
    YamaKagura = 34210, // IllComeTengu->self, 9.0s cast, range 40 width 5 rect, knockback 33 forward
    GhastlyGraspVisual = 34211, // Boss->self, 11.0s cast, single-target
    GhastlyGrasp = 34212, // Helper->location, 11.0s cast, range 5 circle
    Spiritspark = 34213, // Boss->self, 3.0s cast, single-target, route 4
    Spiritflame = 34214, // Helper->location, 4.0s cast, range 6 circle
    ArmOfPurgatory = 34215, // Helper->player, no cast, single-target, touched spiritflame
    SpearmansOrders = 34197, // Boss->self, 5.5s cast, single-target
    SpearmansOrdersVisual = 34198, // Helper->self, 5.5s cast, range 40 width 40 rect
    SpearpointPushVisual = 34588, // Helper->self, no cast, single-target
    SpearpointPushFast = 34199, // Helper->self, no cast, range 3 width 4 rect
    SpearpointPushSlow = 34514, // Helper->self, no cast, range 2 width 4 rect
}

public enum SID : uint
{
    GiriDirection = 2970, // none->Boss, extra=0x248 (back) / 0x24A (front) / 0x249 (left) / 0x24B (right)
}

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.KenkiRelease && Arena.Bounds.Radius > 20)
            _aoe = new(new AOEShapeRect(20, 20, 20), Arena.Center, default, Module.CastFinishAt(spell, 2.1f), Inverted: true);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x5B && state == 0x00020001)
        {
            _aoe = null;
            Arena.Bounds = new ArenaBoundsSquare(20);
        }
    }
}

class Giri(BossModule module) : Components.GenericAOEs(module)
{
    private enum SafeDirection { None, Front, Back, Left, Right }

    private static readonly AOEShapeCone _cone = new(60, 135.Degrees());
    private readonly List<AOEInstance> _aoes = [];
    private SafeDirection _next;
    private (WPos origin, Angle rotation, DateTime activation)? _initial;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.IaiKasumiGiriBack or AID.IaiKasumiGiriLeft or AID.IaiKasumiGiriFront or AID.IaiKasumiGiriRight
            or AID.DoubleKasumiGiriFirstBack or AID.DoubleKasumiGiriFirstLeft or AID.DoubleKasumiGiriFirstFront or AID.DoubleKasumiGiriFirstRight)
        {
            _initial = (spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
            InitIfReady();
        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID != SID.GiriDirection)
            return;
        _next = status.Extra switch
        {
            0x248 => SafeDirection.Back,
            0x24A => SafeDirection.Front,
            0x249 => SafeDirection.Left,
            0x24B => SafeDirection.Right,
            _ => SafeDirection.None
        };
        InitIfReady();
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count == 0)
            return;
        if ((AID)spell.Action.ID is >= AID.IaiKasumiGiriBack and <= AID.DoubleKasumiGiriRestRight)
        {
            _aoes.RemoveAt(0);
            if (_aoes.Count == 0)
            {
                _initial = null;
                _next = SafeDirection.None;
            }
        }
    }

    private void InitIfReady()
    {
        if (_next == SafeDirection.None || _initial is not var (origin, rotation, activation))
            return;
        var second = _aoes.Count == 1;
        var rot = _next switch
        {
            SafeDirection.Front => second ? rotation + 180.Degrees() : rotation,
            SafeDirection.Left => second ? rotation - 90.Degrees() : rotation,
            SafeDirection.Right => second ? rotation + 90.Degrees() : rotation,
            _ => rotation
        };
        _aoes.Add(new(_cone, origin, rot, second ? activation.AddSeconds(3.5f) : activation));
    }
}

abstract class MarchingSoldiers(BossModule module, OID soldier, AID push, float lenFront, float lenBack, int explosions, int shown) : Components.Exaflare(module, new AOEShapeRect(lenFront, 2, lenBack))
{
    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x25E9 && (OID)actor.OID == soldier)
            Lines.Add(new() { Next = actor.Position, Advance = (lenFront + lenBack) * actor.Rotation.ToDirection(), NextExplosion = WorldState.FutureTime(7.8f), TimeToMove = 0.6f, ExplosionsLeft = explosions, MaxShownExplosions = shown });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == push)
        {
            ++NumCasts;
            var index = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 1));
            if (index < 0)
                return;
            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}
class SpearmanOrdersFast(BossModule module) : MarchingSoldiers(module, OID.AshigaruSoheiFast, AID.SpearpointPushFast, 3, 0.474f, 12, 4);
class SpearmanOrdersSlow(BossModule module) : MarchingSoldiers(module, OID.AshigaruSoheiSlow, AID.SpearpointPushSlow, 2, 0.316f, 18, 2);

class Spiritflames(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _flames = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _flames.Select(f => new AOEInstance(new AOEShapeRect(8.4f, 2.4f, 2.4f), f.Position, f.Rotation));

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Spiritflame)
        {
            if (id == 0x1E46)
                _flames.Add(actor);
            else if (id == 0x1E3C)
                _flames.Remove(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor) => _flames.Remove(actor);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var f in _flames)
        {
            hints.AddForbiddenZone(ShapeDistance.Circle(f.Position, 2.4f));
            hints.AddForbiddenZone(ShapeDistance.Capsule(f.Position, f.Rotation, 6, 2.4f), WorldState.FutureTime(1.5f));
        }
    }
}

class UpwellFirst(BossModule module) : Components.StandardAOEs(module, AID.UpwellFirst, new AOEShapeRect(60, 5));

class UpwellRest(BossModule module) : Components.Exaflare(module, new AOEShapeRect(30, 2.5f, 30))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.UpwellFirst)
            return;
        var pos = caster.Position;
        var central = pos.AlmostEqual(Arena.Center, 1);
        var north = pos.Z == 530;
        var south = pos.Z == 550;
        var dir = spell.Rotation.ToDirection().OrthoR();
        AddLine(pos + 7.5f * dir, 5 * dir, default, central || north ? 5 : 1);
        AddLine(pos - 7.5f * dir, -5 * dir, 180.Degrees(), central || south ? 5 : 1);

        void AddLine(WPos first, WDir advance, Angle offset, int explosions)
            => Lines.Add(new() { Next = first, Advance = advance, Rotation = spell.Rotation + offset, NextExplosion = Module.CastFinishAt(spell), TimeToMove = 2, ExplosionsLeft = explosions, MaxShownExplosions = 2 });
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.UpwellRest)
            return;
        var index = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 3) && l.Rotation.AlmostEqual(caster.Rotation, Angle.DegToRad));
        if (index < 0)
            return;
        var line = Lines[index];
        AdvanceLine(line, caster.Position + 2.5f * line.Rotation.ToDirection().OrthoR());
        if (line.ExplosionsLeft == 0)
            Lines.RemoveAt(index);
    }
}

class Explosion(BossModule module) : Components.GenericAOEs(module, AID.Explosion)
{
    private static readonly AOEShapeRect _rect = new(60, 15);
    private readonly List<Actor> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var differentRotations = _casters.Count > 1 && _casters[0].CastInfo!.Rotation != _casters[1].CastInfo!.Rotation;
        for (var i = 0; i < Math.Min(_casters.Count, 2); ++i)
        {
            var c = _casters[i];
            yield return new(_rect, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), i == 0 ? ArenaColor.Danger : 0, i == 0 || differentRotations);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Remove(caster);
    }
}

class YamaKagura(BossModule module) : Components.KnockbackFromCastTarget(module, AID.YamaKagura, 33, shape: new AOEShapeRect(40, 2.5f), kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
            hints.AddForbiddenZone(ShapeDistance.Rect(c.Position, c.CastInfo!.Rotation, 40, 0, 2.5f), Module.CastFinishAt(c.CastInfo));
    }
}

class AzureAuspice(BossModule module) : Components.StandardAOEs(module, AID.AzureAuspice, new AOEShapeDonut(6, 60));
class KenkiRelease(BossModule module) : Components.RaidwideCast(module, AID.KenkiRelease);
class MoonlessNight(BossModule module) : Components.RaidwideCast(module, AID.MoonlessNight);
class IronRain(BossModule module) : Components.StandardAOEs(module, AID.IronRain, new AOEShapeCircle(10));
class Unsheathing(BossModule module) : Components.StandardAOEs(module, AID.Unsheathing, new AOEShapeCircle(3));
class VeilSever(BossModule module) : Components.StandardAOEs(module, AID.VeilSever, new AOEShapeRect(40, 2.5f));
class ScarletAuspice(BossModule module) : Components.StandardAOEs(module, AID.ScarletAuspice, new AOEShapeCircle(6));
class Clearout(BossModule module) : Components.StandardAOEs(module, AID.Clearout, new AOEShapeCone(22, 90.Degrees()));
class Boundless(BossModule module) : Components.GroupedAOEs(module, [AID.BoundlessScarlet, AID.BoundlessAzure], new AOEShapeRect(60, 5));
class GhastlyGrasp(BossModule module) : Components.StandardAOEs(module, AID.GhastlyGrasp, new AOEShapeCircle(5));
class Spiritflame(BossModule module) : Components.StandardAOEs(module, AID.Spiritflame, new AOEShapeCircle(6));

class V022MokoStates : StateMachineBuilder
{
    public V022MokoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<KenkiRelease>()
            .ActivateOnEnter<MoonlessNight>()
            .ActivateOnEnter<Giri>()
            .ActivateOnEnter<AzureAuspice>()
            .ActivateOnEnter<ScarletAuspice>()
            .ActivateOnEnter<Boundless>()
            .ActivateOnEnter<IronRain>()
            .ActivateOnEnter<UpwellFirst>()
            .ActivateOnEnter<UpwellRest>()
            .ActivateOnEnter<Unsheathing>()
            .ActivateOnEnter<VeilSever>()
            .ActivateOnEnter<Clearout>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<GhastlyGrasp>()
            .ActivateOnEnter<YamaKagura>()
            .ActivateOnEnter<Spiritflame>()
            .ActivateOnEnter<Spiritflames>()
            .ActivateOnEnter<SpearmanOrdersFast>()
            .ActivateOnEnter<SpearmanOrdersSlow>();
    }
}
class V022MokoP1States(BossModule module) : V022MokoStates(module);
class V022MokoP2States(BossModule module) : V022MokoStates(module);

public abstract class V022Moko(ModuleInit init) : BossModule(init, new(-700, 540), new ArenaBoundsSquare(24.5f));

[ModuleInfo(Incomplete = true, Contributors = "Kagekazu", PrimaryActorOID = (uint)OID.Boss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12357, SortOrder = 3)]
public class V022MokoP1(ModuleInit init) : V022Moko(init);

[ModuleInfo(Incomplete = true, Contributors = "Kagekazu", PrimaryActorOID = (uint)OID.BossP2, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12357, SortOrder = 4)]
public class V022MokoP2(ModuleInit init) : V022Moko(init);
