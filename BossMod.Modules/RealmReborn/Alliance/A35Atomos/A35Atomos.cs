namespace BossMod.RealmReborn.Alliance.A35Atomos;

public enum OID : uint
{
    Boss = 0xDA4, // R4.000, x3, howling/moaning/roaring Atomos
    AtomosPrime = 0xE6E, // R1.000
    Helper = 0x1B2, // R0.500
    SacrificedSoldier = 0xDA5, // R3.000
    MoonringBlade = 0xBD8, // R1.000
    UpperPad1 = 0x1E9738,
    UpperPad2 = 0x1E9739,
    UpperPad3 = 0x1E973A,
    UpperPad4 = 0x1E973B,
    UpperPad5 = 0x1E973C,
    UpperPad6 = 0x1E973D,
    UpperPad7 = 0x1E973E,
    UpperPad8 = 0x1E973F,
    UpperPad9 = 0x1E9740,
    UpperPad10 = 0x1E9741,
    UpperPad11 = 0x1E9742,
    UpperPad12 = 0x1E9743,
}

public enum AID : uint
{
    AutoAttack = 870, // SacrificedSoldier->player, no cast, single-target
    AetherialDistribution = 3419, // AtomosPrime->Boss, no cast, single-target
    BlackHole = 3414, // Boss->self, no cast, range 15+R cone
    WhiteHole = 3415, // Boss->self, 6.0s cast, range 15+R 120-degree cone
    JumpPadVisual = 3438, // Helper->self, no cast
    Shockwave = 3416, // Boss->self, 4.0s cast, range 19 circle
    HuntersMoon = 2345, // SacrificedSoldier->Helper, no cast, single-target
    Aura = 3081, // Helper->location, no cast, range 8 circle
    HuntersMoonResolve = 3418, // Helper->MoonringBlade, no cast, range 5 circle
    AstralPunch = 2346, // SacrificedSoldier->self, no cast, range 8+R 90-degree cone
}

static class Platforms
{
    // since some people might not have gone to the right platform, establish the right one by proximity to atomos
    public static Actor? OurAtomos(BossModule module, Actor actor)
        => module.Enemies(OID.Boss).Where(b => !b.IsDeadOrDestroyed).MinBy(b => (b.Position - actor.Position).LengthSq());

    public static bool OnAtomosPlatform(BossModule module, Actor actor, WPos pos)
    {
        var mine = OurAtomos(module, actor);
        return mine != null && pos.InCircle(mine.Position, A35Atomos.PlatformRadius + 2);
    }

    public static bool OnRing(Actor actor) => actor.PosRot.Y < A35Atomos.HeightThreshold;
}

class WhiteHole(BossModule module) : Components.StandardAOEs(module, AID.WhiteHole, new AOEShapeCone(19, 60.Degrees()))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Platforms.OnRing(actor))
            return;
        foreach (var c in Casters.Where(c => Platforms.OnAtomosPlatform(Module, actor, c.Position)))
            hints.AddForbiddenZone(Shape.Distance(c.Position, c.CastInfo!.Rotation), Module.CastFinishAt(c.CastInfo));
    }
}

class Shockwave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Shockwave, 19, shape: new AOEShapeCircle(19))
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
        => !Module.Enemies(OID.Boss).Any(b => !b.IsDeadOrDestroyed && pos.InCircle(b.Position, A35Atomos.PlatformRadius));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Platforms.OnRing(actor))
            return;
        foreach (var c in Casters.Where(c => Platforms.OnAtomosPlatform(Module, actor, c.Position)))
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(c.Position, 1.5f), Module.CastFinishAt(c.CastInfo));
    }
}

class ShockwaveHint(BossModule module) : Components.CastHint(module, AID.Shockwave, "Knockback - stay under boss!");

class BlackHole(BossModule module) : Components.Cleave(module, AID.BlackHole, new AOEShapeCone(19, 45.Degrees()), activeWhileCasting: false)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Platforms.OnRing(actor))
            return;
        foreach (var (origin, target, angle) in OriginsAndTargets())
            if (actor != target && Platforms.OnAtomosPlatform(Module, actor, origin.Position))
                hints.AddForbiddenZone(Shape, origin.Position, angle, NextExpected);
    }
}

class SacrificedSoldier(BossModule module) : Components.Adds(module, (uint)OID.SacrificedSoldier, 1)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var target in hints.PotentialTargets.Where(t => t.Actor.OID == ObjectID && Platforms.OnAtomosPlatform(Module, actor, t.Actor.Position)))
            target.Priority = Priority;
    }
}

// upper pads: EANM 0x00800100 = glowing, 0x00040008 = off
// lower pads: mid-angles between uppers at RingOuterRadius from Atomos
class JumpPads(BossModule module) : BossComponent(module)
{
    private readonly HashSet<ulong> _glowingUpper = [];

    public static bool IsUpperPad(uint oid) => oid is >= (uint)OID.UpperPad1 and <= (uint)OID.UpperPad12;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (!IsUpperPad(actor.OID))
            return;
        if (state == 0x00800100)
            _glowingUpper.Add(actor.InstanceID);
        else if (state == 0x00040008)
            _glowingUpper.Remove(actor.InstanceID);
    }

    public override void OnActorDestroyed(Actor actor) => _glowingUpper.Remove(actor.InstanceID);

    private IEnumerable<WPos> LowerPadsNear(Actor atomos)
    {
        foreach (var pad in WorldState.Actors.Where(a => IsUpperPad(a.OID) && a.Position.InCircle(atomos.Position, 16)))
            yield return atomos.Position + (pad.Position - atomos.Position).Normalized().Rotate(45.Degrees()) * A35Atomos.RingOuterRadius;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var mine = Platforms.OurAtomos(Module, actor);
        if (mine == null)
            return;

        if (Platforms.OnRing(actor))
        {
            // clear everything when on the ring since the only thing that matters is getting to a jump pad
            hints.ForbiddenZones.Clear();
            hints.GoalZones.Clear();
            foreach (var pad in LowerPadsNear(mine))
                hints.GoalZones.Add(AIHints.GoalSingleTarget(pad, A35Atomos.JumpPadGoalRadius, 5));
        }
        else
        {
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(mine.Position, A35Atomos.PlatformRadius));
            foreach (var a in WorldState.Actors.Where(a => _glowingUpper.Contains(a.InstanceID) && a.Position.InCircle(mine.Position, 16)))
                hints.GoalZones.Add(AIHints.GoalSingleTarget(a.Position, A35Atomos.JumpPadGoalRadius, 5));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (pc.PosRot.Y < A35Atomos.HeightThreshold)
        {
            foreach (var atomos in Module.Enemies(OID.Boss).Where(b => !b.IsDeadOrDestroyed))
                foreach (var pad in LowerPadsNear(atomos))
                    Arena.AddCircle(pad, A35Atomos.JumpPadGoalRadius, Platforms.OnAtomosPlatform(Module, pc, atomos.Position) ? ArenaColor.Safe : ArenaColor.Danger);
        }
        else
        {
            foreach (var a in WorldState.Actors.Where(a => _glowingUpper.Contains(a.InstanceID)))
                Arena.AddCircle(a.Position, A35Atomos.JumpPadGoalRadius, Platforms.OnAtomosPlatform(Module, pc, a.Position) ? ArenaColor.Safe : ArenaColor.Danger);
        }
    }
}

class A35AtomosStates : StateMachineBuilder
{
    public A35AtomosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WhiteHole>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<ShockwaveHint>()
            .ActivateOnEnter<BlackHole>()
            .ActivateOnEnter<SacrificedSoldier>()
            .ActivateOnEnter<JumpPads>()
            .Raw.Update = () => Module.Enemies(OID.Boss).All(b => b.IsDeadOrDestroyed);
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 111, NameID = 3380)]
public class A35Atomos(ModuleInit init) : BossModule(init, ArenaCenter, CombinedBounds)
{
    public const float PlatformRadius = 18f;
    public const float RingOuterRadius = 26f;
    public const float JumpPadRadius = 3.6f;
    public const float JumpPadGoalRadius = 2.6f;
    public const float HeightThreshold = 93f;

    public static readonly WPos ArenaCenter = new(0, 31);
    public static readonly WDir[] PlatformOffsets = [new(-31, 18), new(0, -36), new(31, 18)];
    private static readonly Angle[] LowerPadBases = [30.Degrees(), 0.Degrees(), -30.Degrees()];

    public static readonly ArenaBoundsCustom CombinedBounds = MakeBounds();

    private static ArenaBoundsCustom MakeBounds()
    {
        var clipper = new PolygonClipper();
        RelSimplifiedComplexPolygon? poly = null;

        void Union(IEnumerable<WDir> shape)
        {
            var next = new RelSimplifiedComplexPolygon(shape);
            poly = poly == null ? next : clipper.Union(new(poly), new(next));
        }

        for (var i = 0; i < PlatformOffsets.Length; ++i)
        {
            var offset = PlatformOffsets[i];
            Union(CurveApprox.Circle(PlatformRadius, 1 / 20f).Select(c => c + offset));
            Union(CurveApprox.Donut(PlatformRadius, RingOuterRadius, 1 / 20f).Select(c => c + offset));
            for (var j = 0; j < 4; ++j)
            {
                var pad = offset + RingOuterRadius * (LowerPadBases[i] + j * 90.Degrees()).ToDirection();
                Union(CurveApprox.Circle(JumpPadRadius, 1 / 20f).Select(c => c + pad));
            }
        }

        return new(PlatformOffsets.Max(o => o.Length()) + RingOuterRadius + JumpPadRadius + 1, poly!, MapResolution: 1);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.Boss), ArenaColor.Enemy);
    }
}
