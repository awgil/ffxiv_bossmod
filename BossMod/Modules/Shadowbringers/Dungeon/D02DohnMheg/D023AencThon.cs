namespace BossMod.Shadowbringers.Dungeon.D02DohnMheg.D023AencThon;

public enum OID : uint
{
    Boss = 0xF14, // R=2.5-6.875
    LiarsLyre = 0xF63, // R=2.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // Boss->player, no cast, single-target
    Teleport = 13206, // Boss->location, no cast, single-target

    CripplingBlow = 13732, // Boss->player, 4.0s cast, single-target
    VirtuosicCapriccio = 13708, // Boss->self, 5.0s cast, range 80+R circle
    ImpChoir = 13552, // Boss->self, 4.0s cast, range 80+R circle
    ToadChoir = 13551, // Boss->self, 4.0s cast, range 17+R 120-degree cone

    FunambulistsFantasia = 13498, // Boss->self, 4.0s cast, single-target, changes arena to planks over a chasm
    FunambulistsFantasiaPull = 13519, // Helper->self, 4.0s cast, range 50 circle, pull 50, between hitboxes

    ChangelingsFantasia = 13521, // Boss->self, 3.0s cast, single-target
    ChangelingsFantasia2 = 13522, // Helper->self, 1.0s cast, single-target

    Malaise = 13549, // Boss->self, no cast, single-target
    BileBombardment = 13550, // Helper->location, 4.0s cast, range 8 circle
    CorrosiveBileFirst = 13547, // Boss->self, 4.0s cast, range 18+R 120-degree cone
    CorrosiveBileRest = 13548, // Helper->self, no cast, range 18+R 120-degree cone
    FlailingTentaclesVisual = 13952, // Boss->self, 5.0s cast, single-target
    FlailingTentacles = 13953, // Helper->self, no cast, range 32+R width 7 rect

    Finale = 15723, // LiarsLyre->self, 60.0s cast, single-target
    FinaleEnrage = 13520 // Boss->self, 60.0s cast, range 80+R circle
}

public enum SID : uint
{
    Bleeding = 273, // Boss->player, extra=0x0
    Imp = 1134, // Boss->player, extra=0x30
    Toad = 439, // Boss->player, extra=0x1
    Stun = 149, // Helper->player, extra=0x0
    Staggered = 715, // Helper->player, extra=0xECA
    FoolsTightrope = 385, // Boss->Boss/LiarsLyre, extra=0x0
    FoolsTumble = 387, // none->player, extra=0x1823
    Unfooled = 386, // none->player, extra=0x0
    FoolsFigure = 388 // none->Boss, extra=0x123
}

class CripplingBlow(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.CripplingBlow));
class VirtuosicCapriccio(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.VirtuosicCapriccio));
class ImpChoir(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.ImpChoir));
class ToadChoir(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ToadChoir), new AOEShapeCone(19.5f, 75.Degrees()));
class BileBombardment(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BileBombardment), 8);

internal class Bounds(BossModule module) : BossComponent(module)
{
    private bool Bridge;

    private static readonly List<WPos> tightrope =
    [
        new(-142.32f, -233.89f), new(-140.6f, -245.85f), new(-129.91f, -241.9f), new(-113.72f, -243.84f),
        new(-113.81f, -244.74f), new(-125.19f, -249.54f), new(-123.72f, -254.08f), new(-124.58f, -254.05f),
        new(-126.13f, -249.73f), new(-126.39f, -249.05f),
        new(-115.51f, -244.47f), new(-129.9f, -242.73f), new(-140.47f, -246.47f), new(-141.19f, -246.74f),
        new(-143.12f, -233.92f)
    ];

    private void Activate()
    {
        Bridge = true;
        Arena.Bounds = BuildHoleBounds();
    }

    private void Deactivate()
    {
        Bridge = false;
        Arena.Bounds = new ArenaBoundsCircle(19.5f);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Bridge)
        {
            var goal = Module.PrimaryActor.Position;
            hints.GoalZones.Add(p => (goal - p).LengthSq() < 0.25f ? 100 : 0);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.FoolsTightrope)
            Deactivate();
    }

    private ArenaBoundsCustom BuildHoleBounds()
    {
        var clipper = Module.Arena.Bounds.Clipper;

        var basic = new PolygonClipper.Operand(CurveApprox.Circle(19.5f, 0.01f));

        var hole = new PolygonClipper.Operand(CurveApprox.Rect(new(20, 0), new(0, 10)));
        var withHole = clipper.Difference(basic, hole);

        var rope = new RelSimplifiedComplexPolygon(tightrope.Select(t => t - Module.Arena.Center));

        var final = clipper.Union(new(withHole), new(rope));
        return new(19.5f, final, 0.25f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FunambulistsFantasiaPull)
            Activate();
    }
}

internal class AencThonLordOfTheLengthsomeGaitStates : StateMachineBuilder
{
    public AencThonLordOfTheLengthsomeGaitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Bounds>()
            .ActivateOnEnter<CripplingBlow>()
            .ActivateOnEnter<VirtuosicCapriccio>()
            .ActivateOnEnter<ImpChoir>()
            .ActivateOnEnter<ToadChoir>()
            .ActivateOnEnter<BileBombardment>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "xan, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 649, NameID = 8146)]
public class AencThonLordOfTheLengthsomeGait(WorldState ws, Actor primary) : BossModule(ws, primary, new(-128.5f, -244), new ArenaBoundsCircle(19.5f));
