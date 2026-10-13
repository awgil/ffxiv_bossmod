namespace BossMod.Global.Crucible.GuttlerTheGutter;

public enum OID : uint
{
    Boss = 0x4CAA, // R4.000, x1
    Helper = 0x233C, // R0.500, x18, Helper type
    OverpoweringPoint = 0x4E5D, // R1.400, x0 (spawn during fight)
    CombustingBlade = 0x4CAB, // R1.000, x6
    MoltenBlade = 0x4CAC, // R1.000, x0 (spawn during fight)
    ThanatosPiece = 0x4CAD, // R2.000, x0 (spawn during fight)

    DeadlyDemesne = 0x1EC0C3
}

public enum AID : uint
{
    AutoAttack = 49714, // Boss->players, no cast, range 9 120-degree cone
    Jump = 48590, // Boss->location, no cast, single-target
    BeastlyAuraCast = 48606, // Boss->self, 6.0s cast, single-target
    BeastlyAura = 48607, // Helper->self, 7.0s cast, range 80 width 80 rect
    CombustingBladesInstant1 = 48591, // 4CAB->Boss, no cast, single-target
    CombustingBladesInstant2 = 48592, // 4CAB->Boss, no cast, single-target
    CombustingBladesInstant3 = 48593, // 4CAB->Boss, no cast, single-target
    CombustingBlades1 = 48594, // Helper->self, 0.5s cast, range 2 circle
    CombustingBlades2 = 48595, // Helper->self, 0.7s cast, range 2 circle
    CombustingBlades3 = 48596, // Helper->self, 0.9s cast, range 2 circle
    MagicalCombustion = 48597, // 4CAB->self, 5.0s cast, range 8 circle
    CombustingBladesGutting = 48598, // Boss->self, no cast, single-target
    GluttonousGuttingCast = 48599, // Boss->self, 6.0+0.6s cast, single-target
    GluttonousGutting = 48600, // Helper->self, 11.6s cast, range 50 width 40 rect
    CombustingBladesGoring = 48601, // Boss->self, no cast, single-target
    GluttonousGoringCast = 48602, // Boss->self, 6.0+0.6s cast, single-target
    GluttonousGoring = 48603, // Helper->self, 11.6s cast, range 40 circle
    GyrocleaveCast = 48604, // Boss->self, 6.0s cast, single-target
    Gyrocleave = 48605, // Helper->self, 7.0s cast, range 80 width 20 rect
    Thunderbolt = 48620, // Boss->self/player, 5.0s cast, range 50 width 6 rect
    OverpoweringPointBossCast = 48612, // Boss->self, 4.9+3.5s cast, single-target
    OverpoweringPointBoss = 48613, // Boss->self, no cast, single-target
    OverpoweringPoint = 48614, // Helper->self, 3.5s cast, range 60 width 6 rect
    DeadlyDemesne = 48608, // Boss->self, 3.0s cast, single-target
    Fetters = 48609, // Helper->self, no cast, range 10 width 10 rect
    FettersUnk = 48610, // Helper->self, no cast, range 100 circle
    LifeClaim = 48611, // Helper->self, 3.0s cast, range 15 width 10 cross
    MoltenMetalBossCast = 48615, // Boss->self, 6.2+2.1s cast, single-target
    MoltenMetalBoss = 48616, // Boss->self, no cast, single-target
    MoltenMetalUnk = 48617, // 4CAC->Boss, 2.5s cast, single-target
    MoltenMetalPuddle = 48618, // Helper->self, 3.0s cast, range 6 circle
    BeastlyFlare = 48619, // 4CAC->self, 8.0s cast, range 80 circle
    ThanatosAutoAttack = 870, // 4CAD->player, no cast, single-target
    InfernalPain = 50540, // 4CAD->self, 8.0s cast, range 40 circle
}

public enum SID : uint
{
    Unk = 2552, // none->Boss, extra=0x477/0x483/0x482/0x478
    Paralysis = 5388, // Boss->player, extra=0x0
    Bind = 3625, // Helper->player, extra=0x0
    Fetters = 1614, // none->player, extra=0xEC4
}

public enum IconID : uint
{
    Thunderbolt = 471, // player->self
    MoltenMetal = 669, // player->self
}

public enum TetherID : uint
{
    OverpoweringPoint = 1, // Boss->player
}

class AutoAttack(BossModule module) : Components.Cleave(module, AID.AutoAttack, new AOEShapeCone(9, 60.Degrees()), (uint)OID.Boss, activeWhileCasting: false)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var (src, target, dir) in OriginsAndTargets())
        {
            if (target == actor)
            {
                if (WorldState.Actors.Find(WorldState.Client.ActivePet.InstanceID) is { IsDead: false, IsTargetable: true } pet && AIHints.TargetInAOECone(pet, src.Position, 9, dir.ToDirection(), 60.Degrees()))
                    hints.Add("Bait away from pet!");
            }
            else if (actor.Position.InCircleCone(src.Position, 9, dir, 60.Degrees()))
                hints.Add("GTFO from cleave!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (WorldState.Actors.Find(WorldState.Client.ActivePet.InstanceID) is { IsDead: false, IsTargetable: true } pet)
            Arena.AddCircle(pet.Position, pet.HitboxRadius, ArenaColor.Object);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (src, target, dir) in OriginsAndTargets())
        {
            if (target == actor)
            {
                if (WorldState.Actors.Find(WorldState.Client.ActivePet.InstanceID) is { IsDead: false, IsTargetable: true } pet)
                {
                    var petDir = pet.Position - Module.PrimaryActor.Position;
                    var len = petDir.Length();

                    if (len <= pet.HitboxRadius)
                        // pet is too close, can't do anything
                        // TODO: move them away from boss
                        return;
                    else if (len > pet.HitboxRadius + 9)
                        // pet is too far to get hit
                        return;

                    var width = MathF.Atan2(pet.HitboxRadius, (pet.Position - Module.PrimaryActor.Position).Length()).Radians();

                    hints.AddForbiddenZone(ShapeDistance.Cone(Module.PrimaryActor.Position, 9, petDir.ToAngle(), width + 60.Degrees()), DateTime.MaxValue);
                }
            }
            else
                hints.AddForbiddenZone(ShapeDistance.Cone(src.Position, 9, dir, 60.Degrees()), DateTime.MaxValue);
        }
    }
}

class BeastlyAura(BossModule module) : Components.Knockback(module, AID.BeastlyAura)
{
    readonly List<Actor> Casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Remove(caster);
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var c in Casters)
        {
            var ci = c.CastInfo!;
            yield return new(ci.LocXZ, 20, Module.CastFinishAt(ci), new AOEShapeRect(80, 80), ci.Rotation + 90.Degrees(), Kind.DirForward);
            yield return new(ci.LocXZ, 20, Module.CastFinishAt(ci), new AOEShapeRect(80, 80), ci.Rotation - 90.Degrees(), Kind.DirForward);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center, default(Angle), 5, 5, 2.5f), Module.CastFinishAt(Casters[0].CastInfo));
    }
}

class MagicalCombustion(BossModule module) : Components.StandardAOEs(module, AID.MagicalCombustion, 8);
class GluttonousGutting(BossModule module) : Components.StandardAOEs(module, AID.GluttonousGutting, new AOEShapeRect(50, 20));
class GluttonousGoring(BossModule module) : Components.StandardAOEs(module, AID.GluttonousGoring, 40);
class Gyrocleave(BossModule module) : Components.StandardAOEs(module, AID.Gyrocleave, new AOEShapeRect(80, 10));

class Thunderbolt(BossModule module) : Components.BaitAwayCast(module, AID.Thunderbolt, new AOEShapeRect(50, 3));

class OverpoweringBait(BossModule module) : Components.GenericBaitAway(module, AID.OverpoweringPoint)
{
    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.OverpoweringPoint && WorldState.Actors.Find(tether.Target) is { } target)
            CurrentBaits.Add(new(source, target, new AOEShapeRect(60, 3), WorldState.FutureTime(5.1f)));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var bait in ActiveBaitsOn(actor))
            if (Module.Enemies(OID.OverpoweringPoint).Any(g => AIHints.TargetInAOERect(g, bait.Source.Position, bait.Rotation.ToDirection(), 60, 3, 0)))
                hints.Add("Bait away from statue!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var bait in ActiveBaitsOn(actor))
            foreach (var enemy in Module.Enemies(OID.OverpoweringPoint))
                hints.AddForbiddenZone(ShapeDistance.Cone(bait.Source.Position, 100, bait.Source.AngleTo(enemy), MathF.Atan2(3, (enemy.Position - bait.Source.Position).Length()).Radians()), bait.Activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (var bait in ActiveBaitsOn(pc))
            foreach (var enemy in Module.Enemies(OID.OverpoweringPoint))
                Arena.AddCircle(enemy.Position, enemy.HitboxRadius, ArenaColor.Vulnerable);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            CurrentBaits.Clear();
    }
}
class OverpoweringPoint(BossModule module) : Components.StandardAOEs(module, AID.OverpoweringPoint, new AOEShapeRect(60, 3));
class ThanatosPiece(BossModule module) : Components.Adds(module, (uint)OID.ThanatosPiece, 1);

class DeadlyDemesne(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<(Actor cage, DateTime square, DateTime cross)> _cages = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _cages)
        {
            yield return new(new AOEShapeRect(5, 5, 5), c.cage.Position, c.cage.Rotation, c.square);
            yield return new(new AOEShapeCross(15, 5), c.cage.Position, c.cage.Rotation, c.cross);
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.DeadlyDemesne)
        {
            switch (state)
            {
                case 0x00010002:
                case 0x00010010:
                    _cages.Add((actor, WorldState.FutureTime(3.7f), WorldState.FutureTime(12.2f)));
                    break;
                case 0x00040008:
                    _cages.RemoveAll(c => c.cage == actor);
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LifeClaim)
            _cages.Clear();
    }
}

class MoltenMetal(BossModule module) : Components.StandardAOEs(module, AID.MoltenMetalPuddle, 6);
// no idea what the falloff is
class BeastlyFlare(BossModule module) : Components.ProximityAOEs(module, AID.BeastlyFlare, 30)
{
    readonly ResistHelper Resists = module.FindComponent<ResistHelper>()!;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).Select(aoe => aoe with { Risky = aoe.Risky && Resists[ResistHelper.Resistance.Magic] <= aoe.Activation });
}

class GuttlerTheGutterStates : StateMachineBuilder
{
    public GuttlerTheGutterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ResistHelper>()
            .ActivateOnEnter<AutoAttack>()
            .ActivateOnEnter<BeastlyAura>()
            .ActivateOnEnter<MagicalCombustion>()
            .ActivateOnEnter<GluttonousGutting>()
            .ActivateOnEnter<GluttonousGoring>()
            .ActivateOnEnter<Gyrocleave>()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<OverpoweringBait>()
            .ActivateOnEnter<OverpoweringPoint>()
            .ActivateOnEnter<ThanatosPiece>()
            .ActivateOnEnter<DeadlyDemesne>()
            .ActivateOnEnter<MoltenMetal>()
            .ActivateOnEnter<BeastlyFlare>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1090, NameID = 14592)]
public class GuttlerTheGutter(ModuleInit init) : BossModule(init, new(520, -420), CustomBounds)
{
    public static readonly ArenaBoundsCustom CustomBounds = MakeBounds();

    private static ArenaBoundsCustom MakeBounds()
    {
        static PolygonClipper.Operand P(float x, float z) => new(CurveApprox.Rect(new WDir(x, z), new WDir(2.5f, 0), new WDir(0, 2.5f)));

        return new(25, new PolygonClipper().UnionAll(new(CurveApprox.Rect(new(10, 0), new(0, 20))), P(0, 22.5f), P(0, -22.5f), P(12.5f, 2.5f), P(-12.5f, -2.5f), P(12.5f, -7.5f), P(-12.5f, 7.5f)));
    }
}
