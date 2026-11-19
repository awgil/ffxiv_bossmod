namespace BossMod.Endwalker.Raid.P10NPandaemonium;

public enum OID : uint
{
    Boss = 0x3F19, // R35.000, x1
    Helper = 0x233C, // R0.500, x23, Helper type
    PillarJail = 0x3F1A, // R1.000, x8, imprisonment pillars
    PillarCannon = 0x3F1B, // R1.000, x8, cannonspawn pillars
    WebPoint = 0x3F1C, // R0.200, x6, fixed points the webs join
    ActorFF38D = 0xFF38D, // R0.500, x1, EventNpc type
    Exit = 0x1E850B, // R0.500, x1, EventObj type
    Actor1EA1A1 = 0x1EA1A1, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    Silkspit = 33369, // Boss->self, 3.0s cast, single-target
    SilkspitAOE = 33370, // Helper->player, no cast, range 7 circle spread, binds the target
    EntanglingWebPoint = 33371, // Helper->location, 3.5s cast, range 10 circle
    EntanglingWebPlayer = 33372, // Helper->location, 3.5s cast, range 5 circle
    PandaemoniacPillars = 33373, // Boss->self, 5.5+1.2s cast, single-target
    Bury = 33374, // PillarJail/PillarCannon->self, no cast, single-target
    BuryAOE = 33375, // Helper->self, 6.7s cast, range 2 circle, tower to stand in
    UnmitigatedExplosion = 33376, // Helper->self, no cast, range 70 circle, empty bury tower
    Imprisonment = 33377, // PillarJail->self, no cast, single-target
    ImprisonmentAOE = 33378, // Helper->self, 4.0s cast, range 4 circle
    Cannonspawn = 33379, // PillarCannon->self, no cast, single-target
    CannonspawnAOE = 33380, // Helper->self, 4.0s cast, range 3-7 donut
    PandaemoniacRayVisual = 33381, // Boss->self, 6.0+0.2s cast, single-target
    PandaemoniacRay = 33382, // Helper->self, 6.2s cast, range 30 width 50 rect
    Touchdown = 33384, // Boss->self, 10.0+1.0s cast, single-target
    TouchdownAOE = 33385, // Helper->self, 11.0s cast, range 4 circle (+16 area-of-influence-up, P10S)
    HarrowingHell = 33386, // Boss->self, 5.0+0.8s cast, single-target
    HarrowingHellAOE1 = 33387, // Helper->self, no cast, range 60 width 60 rect
    HarrowingHellAOE2 = 33388, // Helper->self, no cast, range 60 width 60 rect
    HarrowingHellAOE3 = 33389, // Helper->self, no cast, range 60 width 60 rect
    HarrowingHellAOE4 = 33390, // Helper->self, no cast, range 60 width 60 rect
    HarrowingHellKnockback = 33391, // Helper->self, 3.9s cast, range 60 width 60 rect, knockback 25 forward
    PartedPlumes = 33392, // Boss->self, 3.0s cast, single-target
    PartedPlumesAOE = 33393, // Helper->self, 4.0s cast, range 50 20-degree cone
    WickedStep = 33394, // Boss->self, 6.0+1.1s cast, single-target
    WickedStepAOE1 = 33395, // Helper->self, 7.1s cast, range 4 circle, tank tower
    WickedStepAOE2 = 33396, // Helper->self, 9.5s cast, range 4 circle, tank tower
    WickedStepUnmitigatedExplosion = 33397, // Helper->self, 1.0s cast, range 70 circle, empty tank tower
    PandaemoniacMeltdown = 33398, // Boss->self, 5.5+0.6s cast, single-target
    PandaemoniacMeltdownAOE = 33399, // Helper->self, no cast, range 50 width 6 rect, line stack
    SoulGrasp = 33400, // Boss->self, 5.0+0.6s cast, single-target
    SoulGraspAOE = 33401, // Helper->players, no cast, range 4 circle, shared tankbuster x4
    AutoAttack = 33402, // Boss->player, no cast, single-target
    Ultima = 33403, // Boss->location, 5.0s cast, range 50 circle
    HarrowingHellAOE5 = 34485, // Helper->self, no cast, range 60 width 60 rect
    HarrowingHellAOE6 = 34486, // Helper->self, no cast, range 60 width 60 rect
    HarrowingHellAOE7 = 34487, // Helper->self, no cast, range 60 width 60 rect
    HarrowingHellAOE8 = 34488, // Helper->self, no cast, range 60 width 60 rect
    EntanglingWeb = 34593, // Boss->self, 3.0s cast, single-target
    PandaemoniacMeltdownTargetSelect = 34722, // Helper->player, no cast, single-target
}

public enum SID : uint
{
    Bind = 2518, // Helper->player, from silkspit or a web
    BindingSoulSnare = 3547, // none->player, stacks while held in a web
    Heavy = 1595, // crossing a web line, which also breaks it
    ConcentratedPoison = 3692, // standing off the platforms
}

public enum IconID : uint
{
    Silkspit = 462, // player
    SoulGrasp = 467, // player
}

public enum TetherID : uint
{
    Silkspit = 57, // player->player
    Web = 226, // WebPoint->WebPoint/player
}

class EntanglingWebPoint(BossModule module) : Components.StandardAOEs(module, AID.EntanglingWebPoint, 10);
class EntanglingWebPlayer(BossModule module) : Components.StandardAOEs(module, AID.EntanglingWebPlayer, 5);
class Bury(BossModule module) : Components.CastTowers(module, AID.BuryAOE, 2, 1, 2);
class Imprisonment(BossModule module) : Components.StandardAOEs(module, AID.ImprisonmentAOE, 4);
class Cannonspawn(BossModule module) : Components.StandardAOEs(module, AID.CannonspawnAOE, new AOEShapeDonut(3, 7));
class PandaemoniacRay(BossModule module) : Components.StandardAOEs(module, AID.PandaemoniacRay, new AOEShapeRect(30, 25));
class Touchdown(BossModule module) : Components.StandardAOEs(module, AID.TouchdownAOE, 20);
class HarrowingHell(BossModule module) : Components.CastCounter(module, AID.HarrowingHellKnockback)
{
    private bool _active;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_active)
            hints.Add("8x raidwide, then knockback");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HarrowingHell)
            _active = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.HarrowingHellKnockback)
        {
            ++NumCasts;
            _active = false;
        }
    }
}
class HarrowingHellKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.HarrowingHellKnockback, 25, ignoreImmunes: true, kind: Kind.DirForward);
class PandaemoniacMeltdown(BossModule module) : Components.SimpleLineStack(module, 3, 50, AID.PandaemoniacMeltdownTargetSelect, AID.PandaemoniacMeltdownAOE, 6.2f);
class SoulGrasp(BossModule module) : Components.IconSharedTankbuster(module, (uint)IconID.SoulGrasp, AID.SoulGraspAOE, 4, 5.8f)
{
    private int _hits;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        base.OnEventIcon(actor, iconID, targetID);
        if (iconID == (uint)IconID.SoulGrasp)
            _hits = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action != WatchedAction)
            return;
        ++NumCasts;
        if (++_hits >= 4)
            Source = Target = null;
        else
            Activation = WorldState.FutureTime(1.6f);
    }
}
class Ultima(BossModule module) : Components.RaidwideCast(module, AID.Ultima);

class PartedPlumes(BossModule module) : Components.StandardAOEs(module, AID.PartedPlumesAOE, new AOEShapeCone(50, 10.Degrees()), 6, highlightImminent: true);

class PartedPlumesVoidzone(BossModule module) : Components.GenericAOEs(module, default, "GTFO from voidzone!")
{
    private static readonly AOEShapeCircle _shape = new(8);
    private bool _active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_active)
            yield return new(_shape, new WPos(100, 100));
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 4 && state is 0x00020001 or 0x00080004)
            _active = state == 0x00020001;
    }
}

class WickedStep(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WickedStepAOE1 or AID.WickedStepAOE2)
        {
            var activation = Module.CastFinishAt(spell);
            if (Towers.Count > 0 && Towers.Min(t => t.Activation) is var first && first < activation)
                activation = first;
            for (var i = 0; i < Towers.Count; ++i)
                if (Towers[i].Activation > activation)
                    Towers[i] = Towers[i] with { Activation = activation };
            Towers.Add(new(caster.Position, 4, 1, 1, Raid.WithSlot().WhereActor(a => a.Role != Role.Tank).Mask(), activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WickedStepAOE1 or AID.WickedStepAOE2)
        {
            ++NumCasts;
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
        }
    }
}

class Silkspit(BossModule module) : Components.UniformStackSpread(module, 0, 7)
{
    private readonly IReadOnlyList<Actor> _points = module.Enemies(OID.WebPoint);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (IsSpreadTarget(actor) && _points.InRadius(actor.Position, SpreadRadius).Any())
            hints.Add("GTFO from web points!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (IsSpreadTarget(actor))
            foreach (var p in _points)
                hints.AddForbiddenZone(ShapeDistance.Circle(p.Position, SpreadRadius));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        Arena.Actors(_points, ArenaColor.Object, true);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Silkspit)
            AddSpread(actor, WorldState.FutureTime(10.1f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SilkspitAOE)
            Spreads.Clear();
    }
}

class WebLines(BossModule module) : Components.GenericAOEs(module, default, "Don't cross the web!")
{
    private readonly List<(Actor from, Actor to)> _webs = [];

    private static bool HoldsPlayer((Actor from, Actor to) w) => w.from.Type == ActorType.Player || w.to.Type == ActorType.Player;
    private static WPos Mid((Actor from, Actor to) w) => w.from.Position + (w.to.Position - w.from.Position) * 0.5f;
    private bool Held(Actor actor) => _webs.Any(w => HoldsPlayer(w) && (w.from == actor || w.to == actor));

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var w in _webs)
        {
            if (HoldsPlayer(w) || w.from == actor || w.to == actor)
                continue;
            var dir = w.to.Position - w.from.Position;
            yield return new(new AOEShapeRect(dir.Length(), 0.5f), w.from.Position, Angle.FromDirection(dir));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (Held(actor))
            hints.Add("Wait for someone to break your web", false);
        else if (_webs.Where(HoldsPlayer).Select(w => w.from.Type == ActorType.Player ? w.from : w.to).FirstOrDefault() is { } held)
            hints.Add($"Run through the web to free {held.Name}", false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Held(actor) || actor.FindStatus(SID.Heavy) != null)
            return;
        foreach (var w in _webs.Where(HoldsPlayer))
        {
            var mid = Mid(w);
            var nearest = Raid.WithoutSlot().Where(p => !p.IsDead && !Held(p) && p.FindStatus(SID.Heavy) == null).MinBy(p => (p.Position - mid).LengthSq());
            if (nearest == actor)
                hints.GoalZones.Add(AIHints.GoalProximity(mid, 30, 0.5f));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var w in _webs.Where(HoldsPlayer))
            Arena.AddLine(w.from.Position, w.to.Position, ArenaColor.Safe, 2);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Web && WorldState.Actors.Find(tether.Target) is var target && target != null)
            _webs.Add((source, target));
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Web)
            _webs.RemoveAll(w => w.from == source);
    }
}

class Platforms(BossModule module) : BossComponent(module)
{
    private static readonly ArenaBoundsRect _rect = new(30, 22.5f);
    private bool _left;
    private bool _right;

    private static PolygonClipper.Operand Rect(float x, float z, float hx, float hz) => new(CurveApprox.Rect(new WDir(x, z), new WDir(hx, 0), new WDir(0, hz)));

    public static ArenaBoundsCustom Build(bool left, bool right)
    {
        List<PolygonClipper.Operand> pieces = [Rect(0, 7.5f, 13, 15), Rect(25, -7.5f, 4, 15), Rect(-25, -7.5f, 4, 15)];
        if (right)
            pieces.Add(Rect(17, 0, 4.5f, 1));
        if (left)
            pieces.Add(Rect(-17, 0, 4.5f, 1));
        return new(37, _rect.Clipper.UnionAll(pieces[0], [.. pieces.Skip(1)]));
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is not (2 or 3) || state is not (0x00020001 or 0x00080004))
            return;
        if (index == 2)
            _right = state == 0x00020001;
        else
            _left = state == 0x00020001;
        Arena.Bounds = Build(_left, _right);
    }
}

class P10NPandaemoniumStates : StateMachineBuilder
{
    public P10NPandaemoniumStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EntanglingWebPoint>()
            .ActivateOnEnter<EntanglingWebPlayer>()
            .ActivateOnEnter<Bury>()
            .ActivateOnEnter<Imprisonment>()
            .ActivateOnEnter<Cannonspawn>()
            .ActivateOnEnter<PandaemoniacRay>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<HarrowingHell>()
            .ActivateOnEnter<HarrowingHellKnockback>()
            .ActivateOnEnter<PandaemoniacMeltdown>()
            .ActivateOnEnter<SoulGrasp>()
            .ActivateOnEnter<Ultima>()
            .ActivateOnEnter<PartedPlumes>()
            .ActivateOnEnter<PartedPlumesVoidzone>()
            .ActivateOnEnter<WickedStep>()
            .ActivateOnEnter<Silkspit>()
            .ActivateOnEnter<WebLines>()
            .ActivateOnEnter<Platforms>();
    }
}

[ModuleInfo(Contributors = "Nonu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 938, NameID = 12354)]
public class P10NPandaemonium(ModuleInit init) : BossModule(init, new(100, 92.5f), Platforms.Build(false, false));
