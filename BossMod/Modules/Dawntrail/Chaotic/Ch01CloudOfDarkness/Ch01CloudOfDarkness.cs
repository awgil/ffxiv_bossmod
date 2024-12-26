namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class DelugeOfDarkness1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DelugeOfDarkness1));
class Flare(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(25), (uint)IconID.Flare, ActionID.MakeSpell(AID.FlareAOE), 8.1f, true);
class FloodOfDarkness(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FloodOfDarkness));
class DelugeOfDarkness2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DelugeOfDarkness2));
class StygianShadow(BossModule module) : Components.Adds(module, (uint)OID.StygianShadow);
class Atomos(BossModule module) : Components.Adds(module, (uint)OID.Atomos);
class DarkDominion(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DarkDominion));
class GhastlyGloomCross(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GhastlyGloomCross), new AOEShapeCross(40, 15));
class GhastlyGloomDonut(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GhastlyGloomDonut), new AOEShapeDonut(21, 40));
class FloodOfDarknessAdd(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.FloodOfDarknessAdd)); // TODO: only if add is player's?..
class Excruciate(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.Excruciate), new AOEShapeCircle(4), true);
class LoomingChaos(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.LoomingChaosAOE), "Raidwide + swap positions");

class Ch01CloudOfDarknessStates : StateMachineBuilder
{
    public Ch01CloudOfDarknessStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        SimpleState(id + 0xFF0000, 10000, "???")
            .ActivateOnEnter<BladeOfDarkness>()
            .ActivateOnEnter<DelugeOfDarkness1>()
            .ActivateOnEnter<GrimEmbraceBait>()
            .ActivateOnEnter<GrimEmbraceAOE>()
            .ActivateOnEnter<RazingVolleyParticleBeam>()
            .ActivateOnEnter<RapidSequenceParticleBeam>()
            .ActivateOnEnter<EndeathVortex>()
            .ActivateOnEnter<EndeathAOE>()
            .ActivateOnEnter<EnaeroKnockback>()
            .ActivateOnEnter<EnaeroAOE>()
            .ActivateOnEnter<Break>()
            .ActivateOnEnter<Flare>()
            .ActivateOnEnter<UnholyDarkness>()
            .ActivateOnEnter<FloodOfDarkness>()
            .ActivateOnEnter<DelugeOfDarkness2>()
            .ActivateOnEnter<StygianShadow>()
            .ActivateOnEnter<Atomos>()
            .ActivateOnEnter<DarkDominion>()
            .ActivateOnEnter<ThirdArtOfDarknessCleave>()
            .ActivateOnEnter<ThirdArtOfDarknessHyperFocusedParticleBeam>()
            .ActivateOnEnter<ThirdArtOfDarknessMultiProngedParticleBeam>()
            .ActivateOnEnter<GhastlyGloomCross>()
            .ActivateOnEnter<GhastlyGloomDonut>()
            .ActivateOnEnter<CurseOfDarkness>()
            .ActivateOnEnter<DarkEnergyParticleBeam>()
            .ActivateOnEnter<FloodOfDarknessAdd>()
            .ActivateOnEnter<ChaosCondensedParticleBeam>()
            .ActivateOnEnter<DiffusiveForceParticleBeam>()
            .ActivateOnEnter<Phaser>()
            .ActivateOnEnter<Excruciate>()
            .ActivateOnEnter<ActivePivotParticleBeam>()
            .ActivateOnEnter<LoomingChaos>();
    }
}

// TODO: mechanic phase bounds
// TODO: flood bounds & squares
// TODO: particle concentration towers
// TODO: evil seed
// TODO: chaser beam
// TODO: tankswap hints?
[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1010, NameID = 13624)]
public class Ch01CloudOfDarkness(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultCenter, InitialBounds)
{
    public static readonly WPos DefaultCenter = new(100, 100);
    public static readonly ArenaBoundsCircle InitialBounds = new(40);
    public static readonly ArenaBoundsCustom Phase1Bounds = new(InitialBounds.Radius, new(BuildPhase1BoundsContour()));
    public static readonly ArenaBoundsCustom Phase2Bounds = new(InitialBounds.Radius, BuildPhase2BoundsPoly());
    public static readonly WPos Phase1Midpoint = DefaultCenter + Phase1Bounds.Poly.Parts[0].Vertices[1] + Phase1Bounds.Poly.Parts[0].Vertices[3];

    public static List<WDir> BuildPhase1BoundsContour()
    {
        // north 'diagonal' is at [+/-15, -37] (it almost intersects the initial circle - at x=15 z is ~37.08)
        // the main diagonal is 20, rotated by 45 degrees, which means that side corners are at x=+/- 40/sqrt(2), z = -37 + 40/sqrt(2) - 15
        var nz = -37;
        var nx = 15;
        var halfDiag = 40 / MathF.Sqrt(2);
        var cz = nz + halfDiag - nx;
        return [new(nx, nz), new(halfDiag, cz), new(0, cz + halfDiag), new(-halfDiag, cz), new(-nx, nz)];
    }

    public static RelSimplifiedComplexPolygon BuildPhase2BoundsPoly()
    {
        // mid is union of 4 rects
        var midHalfWidth = 3;
        var midHalfLength = 24;
        var midOffset = 15;
        var op1 = new PolygonClipper.Operand();
        var op2 = new PolygonClipper.Operand();
        op1.AddContour(CurveApprox.Rect(new WDir(0, +midOffset), new(1, 0), midHalfWidth, midHalfLength));
        op1.AddContour(CurveApprox.Rect(new WDir(0, -midOffset), new(1, 0), midHalfWidth, midHalfLength));
        op2.AddContour(CurveApprox.Rect(new WDir(+midOffset, 0), new(0, 1), midHalfWidth, midHalfLength));
        op2.AddContour(CurveApprox.Rect(new WDir(-midOffset, 0), new(0, 1), midHalfWidth, midHalfLength));
        var mid = InitialBounds.Clipper.Union(op1, op2);

        // sides is union of two platforms and the outside ring
        var sideHalfWidth = 7.5f;
        var sideHalfLength = 10;
        var sideOffset = 19 + sideHalfLength;
        var sideRingWidth = 6;
        op1.Clear();
        op2.Clear();
        op1.AddContour(CurveApprox.Rect(new WDir(+sideOffset, 0), new(1, 0), sideHalfWidth, sideHalfLength));
        op1.AddContour(CurveApprox.Rect(new WDir(-sideOffset, 0), new(1, 0), sideHalfWidth, sideHalfLength));
        op2.AddContour(CurveApprox.Circle(InitialBounds.Radius, 0.1f));
        op2.AddContour(CurveApprox.Circle(InitialBounds.Radius - sideRingWidth, 0.1f));
        var side = InitialBounds.Clipper.Union(op1, op2);

        op1.Clear();
        op2.Clear();
        op1.AddPolygon(mid);
        op2.AddPolygon(side);
        return InitialBounds.Clipper.Union(op1, op2);
    }
}

// envcontrols:
// 00 = main bounds telegraph
// - 00200010 - phase 1
// - 00020001 - phase 2
// - 00040004 - remove telegraph (note that actual bounds are controlled by something else!)
// 02 = outer ring
// - 00020001 - become dangerous
// - 00080004 - restore to normal
// 03-1E = mid squares
// - 08000001 - init
// - 00200010 - become occupied
// - 02000001 - become free
// - 00800040 - player is standing for too long, will break soon
// - 00080004 - break
// - 00020001 - repair
// - arrangement:
//      04             0B
//   03 05 06 07 0E 0D 0C 0A
//      08             0F
//      09             10
//      17             1E
//      16             1D
//   11 13 14 15 1C 1B 1A 18
//      12             19
// 1F-2E = 1-man towers
// - 00020001 - appear
// - 00200010 - occupied
// - 00080004 - disappear
// - 08000001 - ? (spot animation)
// - arrangement:
//      25             26
//   21 xx 1F xx xx 20 xx 22
//      23             24
//      xx             xx
//      xx             xx
//      2B             2C
//   29 xx 27 xx xx 28 xx 2A
//      2D             2E
// 2F-3E = 2-man towers
// - 00020001 - appear
// - 00200010 - occupied by 1
// - 00800040 - occupied by 2
// - 00080004 - disappear
// - 08000001 - ? (spot animations)
// - arrangement (also covers intersecting square):
//      35             36
//   31 xx 2F xx xx 30 xx 32
//      33             34
//      xx             xx
//      xx             xx
//      3B             3C
//   39 xx 37 xx xx 38 xx 3A
//      3D             3E
// 3F-46 = 3-man towers
// - 00020001 - appear
// - 00200010 - occupied by 1
// - 00800040 - occupied by 2
// - 02000100 - occupied by 3
// - 00080004 - disappear
// - 08000001 - ? (spot animations)
// - arrangement:
//     3F         43
//   42  40     44  46
//     41         45
// 47-56 = 1-man tower falling orb
// 57-66 = 2-man tower falling orb
// 67-6E = 3-man tower falling orb

