namespace BossMod.Dawntrail.Dungeon.D11MesoTerminal.D111ChirurgeonGeneral;

public enum OID : uint
{
    Boss = 0x488F, // R2.720, x1
    Helper = 0x233C, // R0.500, x10, Helper type
}

public enum AID : uint
{
    AutoAttack = 44246, // Boss->player, no cast, single-target
    MedicineField = 43798, // Boss->self, 5.0s cast, range 60 circle
    NoMansLand = 43804, // Boss->self, no cast, single-target
    PungentAerosol = 43807, // Helper->location, 5.5s cast, range 60 circle
    SterileSphereSmall = 43806, // Helper->self, 5.5s cast, range 8 circle
    SterileSphereLarge = 43805, // Helper->self, 5.5s cast, range 15 circle
    BiochemicalFront = 43802, // Boss->self, 5.0s cast, range 40 width 65 rect
    SensoryDeprivation = 43797, // Boss->self, 3.0s cast, range 60 circle
    ConcentratedDose = 43799, // Boss->player, 5.0s cast, single-target
}

class MedicineField(BossModule module) : Components.RaidwideCast(module, AID.MedicineField);
class PungentAerosol(BossModule module) : Components.KnockbackFromCastTarget(module, AID.PungentAerosol, 24)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // TODO: add hints to avoid big circle AOEs, not sure if 5.5s is enough time to get from risky corner to safety
        foreach (var c in Sources(slot, actor))
            if (!IsImmune(slot, c.Activation))
                hints.AddForbiddenZone(p =>
                {
                    var dir = (p - c.Origin).Normalized() * 24;
                    var proj = p + dir;
                    return !Arena.InBounds(proj);
                }, c.Activation);
    }
}
class SterileSphereSmall(BossModule module) : Components.StandardAOEs(module, AID.SterileSphereSmall, 8);
class SterileSphereLarge(BossModule module) : Components.StandardAOEs(module, AID.SterileSphereLarge, 15);
class BiochemicalFront(BossModule module) : Components.StandardAOEs(module, AID.BiochemicalFront, new AOEShapeRect(40, 32.5f));
class ConcentratedDose(BossModule module) : Components.SingleTargetCast(module, AID.ConcentratedDose);

class D111ChirurgeonGeneralStates : StateMachineBuilder
{
    public D111ChirurgeonGeneralStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MedicineField>()
            .ActivateOnEnter<PungentAerosol>()
            .ActivateOnEnter<SterileSphereSmall>()
            .ActivateOnEnter<SterileSphereLarge>()
            .ActivateOnEnter<BiochemicalFront>()
            .ActivateOnEnter<ConcentratedDose>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1028, NameID = 13970)]
public class D111ChirurgeonGeneral(WorldState ws, Actor primary) : BossModule(ws, primary, new(270, 12), new ArenaBoundsSquare(20));

