namespace BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse.D30ForgivenTreachery;

public enum OID : uint
{
    Boss = 0x4609, // R14.000, x1
    Helper = 0x233C, // R0.500, x31, Helper type
    Indulgence = 0x460A, // R4.000, x8
}

public enum AID : uint
{
    AutoAttack = 40556, // Boss->player, no cast, single-target
    LiturgyOfLight = 39487, // Boss->self, 3.0s cast, single-target
    Unk1 = 39619, // Helper->self, no cast, single-target
    Unk2 = 39620, // Helper->self, no cast, single-target
    Unk3 = 39640, // Helper->self, no cast, single-target
    BrutalHaloSmall = 39642, // Helper->self, 12.0s cast, range 9-14 donut
    BrutalHaloMedium = 39643, // Helper->self, 12.0s cast, range 14-19 donut
    BrutalHaloLarge = 39674, // Helper->self, 12.0s cast, range 19-24 donut
    GripOfSalvationBossRight = 40401, // Boss->self, 7.0s cast, single-target
    GripOfSalvationRight = 40551, // Helper->self, 7.7s cast, range 60 width 30 rect
    GripOfSalvationBossLeft = 40549, // Boss->self, 7.0s cast, single-target
    GripOfSalvationLeft = 44927, // Helper->self, 7.7s cast, range 60 width 30 rect
    SalvationsReachBossRight = 40411, // Boss->self, no cast, single-target
    SalvationsReachBossLeft = 40550, // Boss->self, no cast, single-target
    SalvationsReachRight = 40552, // Helper->self, 1.0s cast, range 30 220-degree cone
    SalvationsReachLeft = 40553, // Helper->self, 1.0s cast, range 30 220-degree cone
    BoundsOfIndulgence = 39876, // Helper->self, no cast, range 4 circle
    DivineFavorCast = 44917, // Boss->self, 5.0s cast, single-target
    DivineFavorFirst = 44918, // Helper->location, 3.0s cast, range 4 circle
    DivineFavorRest = 44919, // Helper->location, no cast, range 4 circle
}

class GripOfSalvation(BossModule module) : Components.GroupedAOEs(module, [AID.GripOfSalvationRight, AID.GripOfSalvationLeft], new AOEShapeRect(60, 15));
class SalvationsReach(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _predicted;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_predicted);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.GripOfSalvationRight:
                _predicted = new(new AOEShapeCone(30, 110.Degrees()), caster.Position, caster.Rotation + 70.Degrees(), WorldState.FutureTime(7.3f));
                break;
            case AID.GripOfSalvationLeft:
                _predicted = new(new AOEShapeCone(30, 110.Degrees()), caster.Position, caster.Rotation - 70.Degrees(), WorldState.FutureTime(7.3f));
                break;

            case AID.SalvationsReachRight:
            case AID.SalvationsReachLeft:
                _predicted = null;
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SalvationsReachRight or AID.SalvationsReachLeft && _predicted is { } p)
            p.Activation = Module.CastFinishAt(spell);
    }
}

class BrutalHalo(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<Actor> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var caster in Casters.Take(1))
        {
            var innerRadius = (AID)caster.CastInfo!.Action.ID switch
            {
                AID.BrutalHaloMedium => 14,
                AID.BrutalHaloLarge => 19,
                AID.BrutalHaloSmall => 9,
                _ => -1
            };
            if (innerRadius > 0)
                yield return new(new AOEShapeDonut(innerRadius, innerRadius + 5), caster.CastInfo!.LocXZ, Activation: Module.CastFinishAt(caster.CastInfo));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BrutalHaloMedium or AID.BrutalHaloLarge or AID.BrutalHaloSmall)
            Casters.Add(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BrutalHaloMedium or AID.BrutalHaloLarge or AID.BrutalHaloSmall)
            Casters.Remove(caster);
    }
}

class BoundsOfIndulgence(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _halos = [];

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Indulgence)
        {
            if (id == 0x1E46)
                _halos.Add(actor);
            if (id == 0x1E3C)
                _halos.Remove(actor);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var halo in _halos)
            Arena.ZoneCircle(halo.Position, 4, ArenaColor.AOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_halos.Any(h => actor.Position.InCircle(h.Position, 4)))
            hints.Add("GTFO from aoe!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var halo in _halos)
        {
            hints.AddForbiddenZone(ShapeContains.Circle(halo.Position, 4));

            if (halo.LastFrameMovement != default)
            {
                var fromCenterDir = halo.Position - Arena.Center;
                var dist = fromCenterDir.Length();
                var cw = halo.Rotation.ToDirection().OrthoR().Dot(fromCenterDir) > 0;

                hints.AddForbiddenZone(ShapeContains.DonutSector(Arena.Center, dist - 4, dist + 4, fromCenterDir.ToAngle() + (cw ? 20.Degrees() : -20.Degrees()), 20.Degrees()), WorldState.FutureTime(2));

                var capsuleEndDir = (fromCenterDir.ToAngle() + (cw ? 40.Degrees() : -40.Degrees())).ToDirection() * dist;
                hints.AddForbiddenZone(ShapeContains.Circle(Arena.Center + capsuleEndDir, 4), WorldState.FutureTime(2));
            }
        }
    }
}

class DivineFavor(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(4), AID.DivineFavorFirst, AID.DivineFavorRest, 3.5f, 0.6f, 8);

class ForgivenTreacheryStates : StateMachineBuilder
{
    public ForgivenTreacheryStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BrutalHalo>()
            .ActivateOnEnter<GripOfSalvation>()
            .ActivateOnEnter<SalvationsReach>()
            .ActivateOnEnter<BoundsOfIndulgence>()
            .ActivateOnEnter<DivineFavor>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1034, NameID = 13863)]
public class ForgivenTreachery(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), MakeBounds())
{
    private static ArenaBoundsCustom MakeBounds()
    {
        var rect = new RelSimplifiedComplexPolygon(CurveApprox.Rect(new WDir(4.5f, 0), new(0, 8.5f)));
        var platform = new RelSimplifiedComplexPolygon(CurveApprox.Circle(7, 1 / 75f));

        var c = new PolygonClipper();

        var bounds = c.UnionAll(
            new(CurveApprox.Donut(14.5f, 18.5f, 1 / 75f)),
            new(rect.Transform(new WDir(0, 18), new(0, 1))),
            new(rect.Transform(new WDir(0, -18), new(0, 1))),
            new(platform.Transform(new WDir(16.5f, 0), new(0, 1))),
            new(platform.Transform(new WDir(-16.5f, 0), new(0, 1)))
        );

        return new(27, bounds);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.ActorInsideBounds(PrimaryActor.Position, PrimaryActor.Rotation, ArenaColor.Enemy);
    }
}
