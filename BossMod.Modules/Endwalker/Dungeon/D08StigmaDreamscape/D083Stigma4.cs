namespace BossMod.Endwalker.Dungeon.D08StigmaDreamscape.D083Stigma4;

public enum OID : uint
{
    Boss = 0x3428, // R2.000, x1
    ProtoRocketPunch = 0x3429, // R1.000, x6
    OmegaFrame = 0x342A, // R3.000, x1
    HybridDragon = 0x342B, // R5.000, x1
    Helper = 0x233C, // R0.500, Helper type
}

public enum AID : uint
{
    Plasmafodder = 25653, // Helper->player, no cast, single-target, auto-attack
    AITakeover = 25641, // Boss->self, 5.0s cast, single-target, visual (adds)
    ProtoWaveCannonA = 25642, // OmegaFrame->self, 7.0s cast, range 60 180-degree cone, not seen yet
    ProtoWaveCannonB = 25643, // OmegaFrame->self, 7.0s cast, range 60 180-degree cone
    Teraflare = 25640, // ?->location, 3.0s cast, range 15 circle, TODO: not seen yet
    SelfDestruct1 = 25644, // add->self, 15.0s cast, range 60 circle, TODO: not seen yet
    SelfDestruct2 = 25647, // add->self, 15.0s cast, range 60 circle, TODO: not seen yet
    Touchdown = 26873, // HybridDragon->self, 2.0s cast, range 7 circle
    FireBreath = 25646, // HybridDragon->self, 7.0s cast, range 40 120-degree cone
    Rush = 25645, // ProtoRocketPunch->location, 5.0s cast, width 5 rect charge
    ElectromagneticReleaseCircleVisual = 25651, // Boss->self, no cast, single-target
    ElectromagneticReleaseCircle = 25652, // Helper->self, 10.0s cast, range 16 circle
    ElectromagneticReleaseDonutVisual = 25649, // Boss->self, no cast, single-target
    ElectromagneticReleaseDonut = 25650, // Helper->self, 10.0s cast, range 10-60 donut
    Mindhack = 25648, // Boss->self, 5.0s cast, range 50 circle, applies forced march orders
    AtomicRay = 25654, // Boss->self, 5.0s cast, range 50 circle, raidwide
    MultiAITakeover = 27723, // Boss->self, 5.0s cast, single-target
}

public enum SID : uint
{
    ForwardMarch = 1958, // Boss->player, extra=0x0
    AboutFace = 1959, // Boss->player, extra=0x0
    LeftFace = 1960, // Boss->player, extra=0x0
    RightFace = 1961, // Boss->player, extra=0x0
    ForcedMarch = 1257, // Boss->player, extra=0x1/0x8
}

class ProtoWaveCannon(BossModule module) : Components.GroupedAOEs(module, [AID.ProtoWaveCannonA, AID.ProtoWaveCannonB], new AOEShapeCone(60, 90.Degrees()));
class Touchdown(BossModule module) : Components.StandardAOEs(module, AID.Touchdown, new AOEShapeCircle(7));
class FireBreath(BossModule module) : Components.StandardAOEs(module, AID.FireBreath, new AOEShapeCone(40, 60.Degrees()));
class Rush(BossModule module) : Components.ChargeAOEs(module, AID.Rush, 2.5f);
class ElectromagneticReleaseCircle(BossModule module) : Components.StandardAOEs(module, AID.ElectromagneticReleaseCircle, new AOEShapeCircle(16));
class ElectromagneticReleaseDonut(BossModule module) : Components.StandardAOEs(module, AID.ElectromagneticReleaseDonut, new AOEShapeDonut(10, 60));
class AtomicRay(BossModule module) : Components.RaidwideCast(module, AID.AtomicRay);

class Mindhack(BossModule module) : Components.StatusDrivenForcedMarch(module, 2, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
        => !Module.InBounds(pos) || Module.Components.OfType<Components.GenericAOEs>().Any(c => c.ActiveAOEs(slot, actor).Any(a => a.Check(pos)));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!State.TryGetValue(actor.InstanceID, out var state) || state.PendingMoves is not [var move, ..])
            return;
        var dist = MovementSpeed * move.duration;
        var facings = new ArcList(actor.Position, dist);
        for (var i = 0; i < 72; ++i)
        {
            var facing = (i * 5).Degrees().Normalized();
            if (DestinationUnsafe(slot, actor, actor.Position + dist * (facing + move.dir).ToDirection()))
                facings.ForbidArcByLength(facing, 2.6f.Degrees());
        }
        hints.AddForbiddenDirections(facings, move.activation);
    }
}

class D083Stigma4States : StateMachineBuilder
{
    public D083Stigma4States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ProtoWaveCannon>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<FireBreath>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<ElectromagneticReleaseCircle>()
            .ActivateOnEnter<ElectromagneticReleaseDonut>()
            .ActivateOnEnter<AtomicRay>()
            .ActivateOnEnter<Mindhack>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 784, NameID = 10404)]
public class D083Stigma4(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsSquare(20))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        => hints.PrioritizeTargetsByOID([(uint)OID.OmegaFrame, (uint)OID.HybridDragon], 1);
}
