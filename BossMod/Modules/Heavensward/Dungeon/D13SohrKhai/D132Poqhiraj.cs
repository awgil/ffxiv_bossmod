namespace BossMod.Heavensward.Dungeon.D13SohrKhai.D132Poqhiraj;

public enum OID : uint
{
    Boss = 0x155C, // R2.500, x?
    PrayerWall = 0x155E, // R5.000, x?
    Poqhiraj = 0x1B2, // R0.500, x?
    DarkCloud = 0x155D, // R1.000, x?
}

public enum AID : uint
{
    RearHoof = 6013, // 155C->player, no cast, single-target
    BurningBright = 6011, // 155C->self/players, 3.0s cast, range 26+R width 6 rect
    TouchdownVisual = 6240, // 1B2->self, 3.0s cast, range 40+R circle
    Touchdown = 6012, // 155C->self, no cast, range 40+R circle
    GallopVisual = 5777, // 155C->location, 4.5s cast, width 10 rect charge
    GallopLine = 5778, // 1B2->self, 4.9s cast, range 40+R width 2 rect
    GallopKnockback = 5823, // 1B2->self, 4.9s cast, range 4+R width 40 rect
    CloudCall = 6009, // 155C->self, 4.0s cast, single-target
    LightningBolt = 6010, // 155D->self, 3.0s cast, range 8 circle
}

public enum IconID : uint
{
    CloudCall = 24, // player
}

class Touchdown(BossModule module) : Components.StandardAOEs(module, AID.TouchdownVisual, new AOEShapeCircle(20));
class GallopLine(BossModule module) : Components.StandardAOEs(module, AID.GallopLine, new AOEShapeRect(40, 1));
class GallopKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.GallopKnockback, 10, shape: new AOEShapeRect(6.5f, 20), kind: Kind.DirForward, stopAtWall: true)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => pos.X is > 405 or < 395;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Casters.Count == 0)
            return;

        List<WPos> leftBlockers = [];
        List<WPos> rightBlockers = [];

        foreach (var wall in Module.Enemies(OID.PrayerWall).Where(x => !x.IsDeadOrDestroyed))
        {
            if (wall.Position.X > 400)
                rightBlockers.Add(wall.Position);
            else
                leftBlockers.Add(wall.Position);
        }

        if (rightBlockers.Count + leftBlockers.Count == 0)
            return;

        hints.AddForbiddenZone(p =>
        {
            var blockers = p.X > 400 ? rightBlockers : leftBlockers;
            return !blockers.Any(b => Utils.AlmostEqual(p.Z, b.Z, 5.1f));
        }, Module.CastFinishAt(Casters[0].CastInfo));
    }
}
class BurningBright(BossModule module) : Components.BaitAwayCast(module, AID.BurningBright, new AOEShapeRect(26, 3), endsOnCastEvent: false);
class CloudCall(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(8), (uint)IconID.CloudCall, AID.CloudCall, 4.1f, true);
class LightningBolt(BossModule module) : Components.StandardAOEs(module, AID.LightningBolt, new AOEShapeCircle(8));

class Walls(BossModule module) : BossComponent(module)
{
    public ArenaBoundsCustom BuildBounds()
    {
        RelSimplifiedComplexPolygon def = new(CurveApprox.Rect(new(8.4f, 0), new(0, 20)));
        foreach (var wall in Module.Enemies(OID.PrayerWall).Where(x => !x.IsDeadOrDestroyed))
        {
            var blocked = CurveApprox.Rect(wall.Position, wall.Rotation.ToDirection() * 6.5f, new(0, 5.1f));
            def = Arena.Bounds.Clipper.Difference(new(def), new(blocked.Select(p => p - Arena.Center)));
        }
        return new(20, def);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.PrayerWall)
            Arena.Bounds = BuildBounds();
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        Arena.ZoneRect(Arena.Center + new WDir(5, 0), Arena.Center + new WDir(8.5f, 0), 20, ArenaColor.AOE);
        Arena.ZoneRect(Arena.Center - new WDir(5, 0), Arena.Center - new WDir(8.5f, 0), 20, ArenaColor.AOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.Position.X is > 405 or < 395)
            hints.Add("GTFO from lightning!");
    }
}

class PoqhirajStates : StateMachineBuilder
{
    public PoqhirajStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GallopLine>()
            .ActivateOnEnter<BurningBright>()
            .ActivateOnEnter<CloudCall>()
            .ActivateOnEnter<Walls>()
            .ActivateOnEnter<GallopKnockback>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<LightningBolt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 171, NameID = 4952, Contributors = "xan")]
public class Poqhiraj(WorldState ws, Actor primary) : BossModule(ws, primary, new(400, 104.15f), new ArenaBoundsRect(4.5f, 20));
