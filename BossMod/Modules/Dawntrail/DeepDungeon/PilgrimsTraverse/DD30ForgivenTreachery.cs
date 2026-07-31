namespace BossMod.Endwalker.DeepDungeon.PilgrimsTraverse.DD30ForgivenTreachery;

public enum OID : uint
{
    ForgivenTreachery = 0x4609, // R14.0
    BoundsOfIndulgence = 0x460A, // R4.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 40556, // ForgivenTreachery->player, no cast, single-target

    LiturgyOfLight = 39487, // ForgivenTreachery->self, 3.0s cast, single-target

    BrutalHaloVisual1 = 39619, // Helper->self, no cast, single-target
    BrutalHaloVisual2 = 39620, // Helper->self, no cast, single-target
    BrutalHaloVisual3 = 39640, // Helper->self, no cast, single-target
    BrutalHaloVisual4 = 39641, // Helper->self, no cast, single-target
    BrutalHalo1 = 39642, // Helper->self, 12.0s cast, range 9-14 donut
    BrutalHalo2 = 39643, // Helper->self, 12.0s cast, range 14-19 donut
    BrutalHalo3 = 39674, // Helper->self, 12.0s cast, range 19-24 donut
    BrutalHalo4 = 39743, // Helper->self, 12.0s cast, range 24-29 donut

    GripOfSalvationVisual1 = 40401, // ForgivenTreachery->self, 7.0s cast, single-target
    GripOfSalvationVisual2 = 40549, // ForgivenTreachery->self, 7.0s cast, single-target
    GripOfSalvation1 = 40551, // Helper->self, 7.7s cast, range 60 width 30 rect, right
    GripOfSalvation2 = 44927, // Helper->self, 7.7s cast, range 60 width 30 rect, left

    SalvationsReachVisual1 = 40411, // ForgivenTreachery->self, no cast, single-target
    SalvationsReachVisual2 = 40550, // ForgivenTreachery->self, no cast, single-target
    SalvationsReach1 = 40552, // Helper->self, 1.0s cast, range 30 220-degree cone, right
    SalvationsReach2 = 40553, // Helper->self, 1.0s cast, range 30 220-degree cone, left

    BoundsOfIndulgence = 39876, // Helper->self, no cast, range 4 circle, voidzone

    DivineFavorVisual = 44917, // ForgivenTreachery->self, 5.0s cast, single-target, chasing AOE
    DivineFavorFirst = 44918, // Helper->location, 3.0s cast, range 4 circle
    DivineFavorRest = 44919 // Helper->location, no cast, range 4 circle
}

public enum IconID : uint
{
    DivineFavor = 197 // player->self
}

[SkipLocalsInit]
sealed class BrutalHalo(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(5)];
    private readonly AOEShapeDonut donut1 = new(9f, 14f), donut2 = new(14f, 19f), donut3 = new(19f, 24f), donut4 = new(24f, 29f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.BrutalHalo1 => donut1,
            (uint)AID.BrutalHalo2 => donut2,
            (uint)AID.BrutalHalo3 => donut3,
            (uint)AID.BrutalHalo4 => donut4,
            _ => null
        };
        if (shape != null)
        {
            var pos = spell.LocXZ;
            _aoes.Add(new(shape, pos, default, Module.CastFinishAt(spell), shapeDistance: shape.Distance(pos, default)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.BrutalHalo1 or (uint)AID.BrutalHalo2 or (uint)AID.BrutalHalo3 or (uint)AID.BrutalHalo4)
        {
            _aoes.RemoveAt(0);
        }
    }
}

[SkipLocalsInit]
sealed class BoundsOfIndulgence(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> voidzones = [];
    private readonly AOEShapeCircle circle = new(4f);
    private readonly AOEShapeArcCapsule arcCW = new(4f, 30f.Degrees(), module.Arena.Center), arcCCW = new(4f, -30f.Degrees(), module.Arena.Center);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = voidzones.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = new AOEInstance[count];
        var center = Arena.Center;
        for (var i = 0; i < count; ++i)
        {
            var vz = voidzones[i];
            var pos = vz.Position;
            if (vz.LastFrameMovement == default)
            {
                aoes[i] = new(circle, pos.Quantized());
            }
            else
            {
                var dir = pos - center;
                var ccw = vz.Rotation.ToDirection().OrthoR().Dot(dir) < 0f;
                aoes[i] = new(ccw ? arcCCW : arcCW, pos.Quantized());
            }
        }
        return aoes;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.BoundsOfIndulgence)
        {
            switch (id)
            {
                case 0x1E46:
                    voidzones.Add(actor);
                    break;
                case 0x1E3C:
                    voidzones.Remove(actor);
                    break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = voidzones.Count;
        if (count == 0)
        {
            return;
        }
        var forbiddenNearFuture = WorldState.FutureTime(1.1d);
        var forbiddenSoon = WorldState.FutureTime(3d);
        var forbiddenFarFuture = DateTime.MaxValue;
        var center = Arena.Center;
        var a15 = 15f.Degrees();
        var a25 = 25f.Degrees();
        var a30 = 30f.Degrees();
        for (var i = 0; i < count; ++i)
        {
            var vz = voidzones[i];
            var pos = vz.Position;
            var dir = pos - center;
            var ccw = vz.Rotation.ToDirection().OrthoR().Dot(dir) < 0f;
            var mult = ccw ? -1f : 1f;
            var mov = vz.LastFrameMovement != default;
            if (mov)
            {
                hints.AddForbiddenZone(new SDArcCapsule(pos, center, mult * a15, 4f), forbiddenNearFuture);
                hints.AddForbiddenZone(new SDArcCapsule(pos, center, mult * a25, 4f), forbiddenSoon);
                hints.AddForbiddenZone(new SDArcCapsule(pos, center, mult * a30, 4f), forbiddenFarFuture);
            }
            hints.TemporaryObstacles.Add(new SDCircle(pos.Quantized(), mov ? 4f : 5f));
        }
    }
}

[SkipLocalsInit]
sealed class DivineFavor(BossModule module) : Components.StandardChasingAOEs(module, 4f, (uint)AID.DivineFavorFirst, (uint)AID.DivineFavorRest, 3.5f, 0.6d, 8, true, (uint)IconID.DivineFavor);

[SkipLocalsInit]
sealed class GripOfSalvationsReach(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(2)];
    private readonly AOEShapeCone cone = new(30f, 110f.Degrees());
    private readonly AOEShapeRect rect = new(60f, 15f), rectFake = new(60f, 30f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id is (uint)AID.GripOfSalvation1 or (uint)AID.GripOfSalvation2)
        {
            var rot = spell.Rotation;
            var loc = spell.LocXZ;
            AddAOE(rect, loc, rot, Module.CastFinishAt(spell));
            var rotAdj = rot + (id == (uint)AID.GripOfSalvation1 ? 1f : -1f) * 80f.Degrees();
            var pos = Module.PrimaryActor.Position + 9f * rotAdj.ToDirection();
            AddAOE(rectFake, pos, rotAdj, Module.CastFinishAt(spell, 7.5d)); // fake aoe to encourage early positioning, 2nd aoe often overlaps with 1st
            void AddAOE(AOEShape shape, WPos position, Angle rotation, DateTime activation) => _aoes.Add(new(shape, position, rotation, activation, shapeDistance: shape.Distance(position, rotation)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (_aoes.Count != 0 && id is (uint)AID.GripOfSalvation1 or (uint)AID.GripOfSalvation2 or (uint)AID.SalvationsReach1 or (uint)AID.SalvationsReach2)
        {
            _aoes.RemoveAt(0);
            if (_aoes.Count > 0)
            {
                ref var aoe = ref _aoes.Ref(0);
                var rot = spell.Rotation + (id == (uint)AID.GripOfSalvation1 ? 1f : -1f) * 70f.Degrees();
                aoe.Rotation = rot;
                aoe.Shape = cone;
                aoe.Origin = Module.PrimaryActor.Position.Quantized();
                aoe.ShapeDistance = aoe.Shape.Distance(aoe.Origin, rot);
            }
        }
    }
}

[SkipLocalsInit]
sealed class DD30ForgivenTreacheryStates : StateMachineBuilder
{
    public DD30ForgivenTreacheryStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BrutalHalo>()
            .ActivateOnEnter<BoundsOfIndulgence>()
            .ActivateOnEnter<GripOfSalvationsReach>()
            .ActivateOnEnter<DivineFavor>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(DD30ForgivenTreacheryStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.ForgivenTreachery,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.DeepDungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1034u,
NameID = 13863u,
SortOrder = 1,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class DD30ForgivenTreachery : BossModule
{
    public DD30ForgivenTreachery(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private DD30ForgivenTreachery(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new DonutV(new WPos(-300f, -300f), 14f, 19f, 128), new Polygon(new(-283.51849f, -300f), 7.5f, 64),
        new Polygon(new(-316.48102f, -300f), 7.5f, 64), new Rectangle(new(-300f, -281.5f), 5f, 9.5f), new Rectangle(new(-300f, -318.5f), 5f, 9.5f)], AdjustForHitboxInwards: true);
        return (arena.Center, arena);
    }
}
