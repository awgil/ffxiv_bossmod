namespace BossMod.Endwalker.Variant.V01Sildihn.V012Silkie;

public enum OID : uint
{
    Boss = 0x39EF, // R6.000, x1
    SilkenPuff = 0x39F0, // R1.000
    EasternEwer = 0x39F1, // R2.400
    WaterVoidzone = 0x1E9230, // R0.500, EventObj type
    Cotton = 0x1EB76E, // R0.500, EventObj type
    Helper = 0x233C, // R0.500, Helper type
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    CarpetBeater = 30507, // Boss->player, 5.0s cast, single-target, tankbuster
    TotalWash = 30508, // Boss->self, 5.0s cast, range 60 circle, raidwide + arena shrink
    DustBluster = 30532, // Boss->location, 5.0s cast, range 60 circle, knockback 16 away from source
    BracingSuds = 30517, // Boss->self, 5.0s cast, single-target, applies green status
    ChillingSuds = 30518, // Boss->self, 5.0s cast, single-target, applies blue status
    SoapsUp = 30519, // Boss->self, 4.0s cast, single-target, removes colour statuses
    FreshPuff = 30525, // Boss->self, 4.0s cast, single-target, visual (statuses on puffs)
    SoapingSpreeBoss = 30526, // Boss->self, 6.0s cast, single-target, visual
    SoapingSpreePuff = 30529, // SilkenPuff->self, 6.0s cast, single-target, removes colour statuses
    ChillingDuster1 = 30520, // Helper->self, 5.0s cast, range 60 width 10 cross
    ChillingDuster2 = 30523, // Helper->self, 8.5s cast, range 60 width 10 cross
    ChillingDuster3 = 30527, // Helper->self, 7.0s cast, range 60 width 10 cross
    BracingDuster1 = 30521, // Helper->self, 5.0s cast, range 5-60 donut
    BracingDuster2 = 30524, // Helper->self, 8.5s cast, range 5-60 donut
    BracingDuster3 = 30528, // Helper->self, 8.5s cast, range 5-60 donut
    SlipperySoap = 30522, // Boss->location, 5.0s cast, width 10 rect charge
    SpotRemoverVisual = 30530, // Boss->self, 3.5s cast, single-target
    SpotRemover = 30531, // Helper->location, 3.5s cast, range 5 circle, spawns voidzone
    SqueakyCleanVisualE = 30509, // Boss->self, 4.5s cast, single-target
    SqueakyClean1E = 30511, // Helper->self, 6.0s cast, range 60 90-degree cone
    SqueakyClean2E = 30512, // Helper->self, 7.7s cast, range 60 90-degree cone
    SqueakyClean3E = 30513, // Helper->self, 9.2s cast, range 60 225-degree cone
    SqueakyCleanVisualW = 30510, // Boss->self, 4.5s cast, single-target
    SqueakyClean1W = 30514, // Helper->self, 6.0s cast, range 60 90-degree cone
    SqueakyClean2W = 30515, // Helper->self, 7.7s cast, range 60 90-degree cone
    SqueakyClean3W = 30516, // Helper->self, 9.2s cast, range 60 225-degree cone
    EasternEwers = 30535, // Boss->self, 4.0s cast, single-target, route 1
    BrimOver = 30536, // EasternEwer->self, 3.0s cast, range 4 circle
    Rinse = 30537, // Helper->self, no cast, range 4 circle
    WashOutVisual = 30533, // Boss->self, 8.0s cast, single-target, route 2
    WashOut = 30534, // Helper->self, 8.0s cast, range 60 width 60 rect, knockback 35 forward
    Sweep = 30541, // Helper->player, no cast, single-target, route 3 broom, knockback 5 away from source
    PuffAndTumbleVisualFirst = 30538, // SilkenPuff->location, 3.0s cast, single-target, route 4
    PuffAndTumbleVisualRest = 30539, // SilkenPuff->location, no cast, single-target
    PuffAndTumbleFirst = 30540, // Helper->location, 4.6s cast, range 4 circle
    PuffAndTumbleRest = 30656, // Helper->location, 1.6s cast, range 4 circle
}

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TotalWash && Arena.Bounds.Radius > 20)
            _aoe = new(new AOEShapeRect(20, 20, 20), Arena.Center, default, WorldState.FutureTime(1.8f), Inverted: true);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x04 && state == 0x00020001)
        {
            _aoe = null;
            Arena.Bounds = V012Silkie.DefaultBounds;
        }
    }
}

class DustBluster(BossModule module) : Components.KnockbackFromCastTarget(module, AID.DustBluster, 16)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var src = Sources(slot, actor).FirstOrDefault();
        if (src.Distance == 0 || IsImmune(slot, src.Activation))
            return;
        foreach (var z in Module.Enemies(OID.WaterVoidzone))
        {
            var toZone = z.Position - src.Origin;
            var dist = toZone.Length();
            if (dist > 5)
                hints.AddForbiddenZone(ShapeDistance.Cone(src.Origin, 100, Angle.FromDirection(toZone), Angle.Asin(5 / dist)), src.Activation);
        }
    }
}

class WashOut(BossModule module) : Components.KnockbackFromCastTarget(module, AID.WashOut, 35, kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var src = Sources(slot, actor).FirstOrDefault();
        if (src.Distance == 0 || IsImmune(slot, src.Activation))
            return;
        var dir = src.Direction.ToDirection();
        foreach (var z in Module.Enemies(OID.WaterVoidzone))
            hints.AddForbiddenZone(ShapeDistance.Rect(z.Position, dir, 40, 40, 5), src.Activation);
    }
}

class EasternEwers(BossModule module) : Components.Exaflare(module, 4)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BrimOver)
            Lines.Add(new() { Next = caster.Position, Advance = new(0, 3.38788f), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 0.8f, ExplosionsLeft = 14, MaxShownExplosions = 14 });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.Rinse)
            return;
        var index = Lines.FindIndex(l => l.Next.AlmostEqual(caster.Position, 1));
        if (index < 0)
            return;
        ++NumCasts;
        var line = Lines[index];
        AdvanceLine(line, caster.Position);
        if (line.ExplosionsLeft == 0)
            Lines.RemoveAt(index);
        else if (line.Next.Z > -135)
            line.Next = new(line.Next.X, -135);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.EasternEwer)
            Lines.RemoveAll(l => l.Next.AlmostEqual(actor.Position, 1));
    }
}

class SilkenPuff(BossModule module) : Components.GenericAOEs(module)
{
    private const float Radius = 3;
    private const float Length = 5;
    private static readonly AOEShapeRect _shape = new(Length + Radius, Radius, Radius);
    private readonly List<Actor> _puffs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _puffs.Select(p => new AOEInstance(_shape, p.Position, p.Rotation));

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D1 && (OID)actor.OID == OID.SilkenPuff)
        {
            if (Module.Enemies(OID.Cotton).Count == 0)
                _puffs.Clear();
            else
                _puffs.Add(actor);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var p in _puffs)
        {
            hints.AddForbiddenZone(ShapeDistance.Capsule(p.Position, p.Rotation, 50, Radius), DateTime.MaxValue);
            hints.AddForbiddenZone(ShapeDistance.Capsule(p.Position, p.Rotation, Length, Radius), WorldState.FutureTime(1.1f));
            hints.AddForbiddenZone(ShapeDistance.Circle(p.Position, Radius));
        }
    }
}

class PuffAndTumble(BossModule module) : Components.Exaflare(module, 4)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.PuffAndTumbleFirst)
            Lines.Add(new() { Next = spell.LocXZ, Advance = 10 * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 2.2f, ExplosionsLeft = 5, MaxShownExplosions = 5 });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is not (AID.PuffAndTumbleFirst or AID.PuffAndTumbleRest))
            return;
        var pos = spell.TargetXZ;
        var index = Lines.FindIndex(l => l.Next.AlmostEqual(pos, 1));
        if (index < 0)
            return;
        AdvanceLine(Lines[index], pos);
        if (Lines[index].ExplosionsLeft == 0)
            Lines.RemoveAt(index);
    }
}

class CarpetBeater(BossModule module) : Components.SingleTargetCast(module, AID.CarpetBeater);
class TotalWash(BossModule module) : Components.RaidwideCast(module, AID.TotalWash);
class DustBlusterRaidwide(BossModule module) : Components.RaidwideCast(module, AID.DustBluster);
class BracingDuster(BossModule module) : Components.GroupedAOEs(module, [AID.BracingDuster1, AID.BracingDuster2, AID.BracingDuster3], new AOEShapeDonut(5, 60));
class ChillingDuster(BossModule module) : Components.GroupedAOEs(module, [AID.ChillingDuster1, AID.ChillingDuster2, AID.ChillingDuster3], new AOEShapeCross(60, 5));
class SlipperySoap(BossModule module) : Components.ChargeAOEs(module, AID.SlipperySoap, 5);
class SpotRemover(BossModule module) : Components.VoidzoneAtCastTarget(module, 5, AID.SpotRemover, OID.WaterVoidzone, 0.8f, a => a.EventState == 7);
class SqueakyCleanSmall(BossModule module) : Components.GroupedAOEs(module, [AID.SqueakyClean1E, AID.SqueakyClean2E, AID.SqueakyClean1W, AID.SqueakyClean2W], new AOEShapeCone(60, 45.Degrees()));
class SqueakyCleanBig(BossModule module) : Components.GroupedAOEs(module, [AID.SqueakyClean3E, AID.SqueakyClean3W], new AOEShapeCone(60, 112.5f.Degrees()));

class V012SilkieStates : StateMachineBuilder
{
    public V012SilkieStates(BossModule module) : base(module)
    {
        // TODO: route 3 Silken Sweep (broom, Sweep knockback 5) has no component
        TrivialPhase()
            .ActivateOnEnter<TotalWash>()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<CarpetBeater>()
            .ActivateOnEnter<SqueakyCleanSmall>()
            .ActivateOnEnter<SqueakyCleanBig>()
            .ActivateOnEnter<SlipperySoap>()
            .ActivateOnEnter<ChillingDuster>()
            .ActivateOnEnter<BracingDuster>()
            .ActivateOnEnter<SpotRemover>()
            .ActivateOnEnter<DustBlusterRaidwide>()
            .ActivateOnEnter<DustBluster>()
            .ActivateOnEnter<EasternEwers>()
            .ActivateOnEnter<WashOut>()
            .ActivateOnEnter<SilkenPuff>()
            .ActivateOnEnter<PuffAndTumble>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11369, SortOrder = 3)]
public class V012Silkie(ModuleInit init) : BossModule(init, new(-335, -155), new ArenaBoundsSquare(29.5f))
{
    public static readonly ArenaBoundsSquare DefaultBounds = new(20);
}
