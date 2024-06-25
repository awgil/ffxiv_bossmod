namespace BossMod.Shadowbringers.Dungeon.D02DohnMheg.D031AencThon;

public enum OID : uint
{
    Boss = 0xF14, // R=2.5-6.875
    LiarsLyre = 0xF63, // R=2.5
    Helper = 0x233C,
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
    FinaleEnrage = 13520, // Boss->self, 60.0s cast, range 80+R circle
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
    FoolsFigure = 388, // none->Boss, extra=0x123
}

class VirtuosicCapriccio(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.VirtuosicCapriccio), "Raidwide + Bleed");
class CripplingBlow(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.CripplingBlow));
class ImpChoir(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.ImpChoir));
class ToadChoir(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ToadChoir), new AOEShapeCone(19.5f, 75.Degrees()));
class BileBombardment(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BileBombardment), 8);

class FunambulistsFantasia(BossModule module) : BossComponent(module)
{
    private bool WaypointsAdded;
    private static readonly List<WPos> waypoints = [new(-142.88f, -233.2f), new(-140.89f, -246.16f), new(-129.9f, -242.38f), new(-114.19f, -244.35f),
    new(-125.81f, -249.33f), new(-123.5f, -256.17f)];

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FunambulistsFantasia)
            Module.Arena.Bounds = D033AencThon.chasmArena;
        if ((AID)spell.Action.ID == AID.Finale)
            Module.Arena.Bounds = D033AencThon.arena;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.FoolsTumble && actor == Module.Raid.Player())
            WaypointsAdded = false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var lyre = Module.Enemies(OID.LiarsLyre).FirstOrDefault();
        hints.WaypointManager.module = Module;
        if (Module.Arena.Bounds == D033AencThon.chasmArena && lyre != null)
        {
            hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.Sprint), actor, 1, false));
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(lyre.Position, 3));
            if (!WaypointsAdded && Module.Arena.Bounds == D033AencThon.chasmArena)
            {
                hints.WaypointManager.ClearWaypoints();
                hints.WaypointManager.WaypointTimeLimit = 10;
                WaypointsAdded = true;
                hints.WaypointManager.AddWaypointsWithRandomization(waypoints, 0.1f, 10);
            }
            if (Module.Arena.Bounds == D033AencThon.arena)
                hints.WaypointManager.ClearWaypoints();
        }
    }
}

class Finale(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.Finale), $"Enrage, destroy the Liar's Lyre!", true);

class CorrosiveBile(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CorrosiveBileFirst)
            _aoe = new(new AOEShapeCone(24.875f, 45.Degrees()), caster.Position, spell.Rotation, spell.NPCFinishAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CorrosiveBileFirst:
            case AID.CorrosiveBileRest:
                if (++NumCasts == 6)
                {
                    _aoe = null;
                    NumCasts = 0;
                }
                break;
        }
    }
}

class FlailingTentacles(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FlailingTentaclesVisual)
            _aoe = new(new AOEShapeCross(38.875f, 3.5f), caster.Position, Module.PrimaryActor.Rotation + 45.Degrees(), spell.NPCFinishAt.AddSeconds(1));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FlailingTentaclesVisual:
            case AID.FlailingTentacles:
                if (++NumCasts == 5)
                {
                    _aoe = null;
                    NumCasts = 0;
                }
                break;
        }
    }
}

class D033AencThonStates : StateMachineBuilder
{
    public D033AencThonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VirtuosicCapriccio>()
            .ActivateOnEnter<CripplingBlow>()
            .ActivateOnEnter<ImpChoir>()
            .ActivateOnEnter<ToadChoir>()
            .ActivateOnEnter<BileBombardment>()
            .ActivateOnEnter<CorrosiveBile>()
            .ActivateOnEnter<FlailingTentacles>()
            .ActivateOnEnter<FunambulistsFantasia>()
            .ActivateOnEnter<Finale>();

    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 649, NameID = 8146)]
public class D033AencThon(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly List<Shape> union = [new Circle(new(-128.5f, -244), 19.75f)];
    private static readonly List<Shape> difference = [new Rectangle(new(-128.5f, -224), 20, 1.25f)];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex(union, difference);

    private static readonly List<Shape> difference2 = [new Rectangle(new(-128.5f, -224), 20, 1.25f), new Rectangle(new(-128.5f, -244), 20, 10)];
    private static readonly List<Shape> union2 = [new PolygonCustom([new(-142.32f, -233.89f), new(-140.52f, -245.64f), new(-129.91f, -241.9f), new(-113.72f, -243.84f),
    new(-113.81f, -244.74f), new(-125.19f, -249.54f), new(-123.72f, -254.08f), new(-124.58f, -254.05f), new(-126.13f, -249.73f), new(-126.39f, -249.05f),
    new(-115.51f, -244.47f), new(-129.9f, -242.73f), new(-140.47f, -246.47f), new(-141.19f, -246.74f), new(-143.12f, -233.92f)])];
    public static readonly ArenaBounds chasmArena = new ArenaBoundsComplex(union, difference2, union2);

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.LiarsLyre => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }
}
