namespace BossMod.Global.Crucible.AethericCharge;

public enum OID : uint
{
    Boss = 0x4C64, // R6.000, x1
    DemonPiece = 0x4C61, // R1.100, x4
    EvilWeaponPiece = 0x4C62, // R2.200, x0 (spawn during fight)
    DeviletPiece = 0x4C63, // R0.840, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x4, Helper type
}

public enum AID : uint
{
    DemonAuto = 50396, // 4C61->player, no cast, single-target
    WeaponAuto = 49680, // 4C62->player, no cast, single-target
    SmiteOfExcessiveRageRect = 48218, // 4C62->self, 7.0s cast, range 40 width 10 rect
    SmiteOfExcessiveRageDonut = 48219, // 4C62->self, 7.0s cast, range 4-30 donut
    VoidFireIII = 48222, // 4C63->self, 10.0s cast, range 60 circle
    DarkEruption = 48217, // Helper->location, 3.0s cast, range 6 circle
    Condemnation = 48223, // 4C61->self, 3.0s cast, range 12 90-degree cone
    VoidParalyze = 48247, // 4C61->none, 5.0s cast, single-target
}

public enum SID : uint
{
    Recharge = 5406, // none->4C61/4C62/4C63, extra=0x1/0x2/0x3/0x4
    DamageUp = 5409, // none->4C62/4C63/4C61, extra=0x0
}

public enum TetherID : uint
{
    Recharge = 205, // 4C61/4C62/4C63->Boss
}

class Adds(BossModule module) : Components.AddsMulti(module, [OID.DemonPiece, OID.EvilWeaponPiece, OID.DeviletPiece])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var target in hints.PotentialTargets.Where(p => OIDs.Contains(p.Actor.OID)))
            target.Priority = target.Actor.Statuses.Any(s => (SID)s.ID is SID.Recharge or SID.DamageUp) ? 1 : 0;
    }
}

class RectOfExcessiveRage(BossModule module) : Components.StandardAOEs(module, AID.SmiteOfExcessiveRageRect, new AOEShapeRect(40, 5));
class DonutOfExcessiveRage(BossModule module) : Components.StandardAOEs(module, AID.SmiteOfExcessiveRageDonut, new AOEShapeDonut(4, 30));
class VoidFireIII(BossModule module) : Components.RaidwideCast(module, AID.VoidFireIII);
class DarkEruption(BossModule module) : Components.StandardAOEs(module, AID.DarkEruption, 6);
class Condemnation(BossModule module) : Components.StandardAOEs(module, AID.Condemnation, new AOEShapeCone(12, 45.Degrees()));
// TODO: need to see it actually try to start casting
class VoidParalyze(BossModule module) : Components.CastInterruptHint(module, AID.VoidParalyze);

class AethericChargeStates : StateMachineBuilder
{
    public AethericChargeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<RectOfExcessiveRage>()
            .ActivateOnEnter<DonutOfExcessiveRage>()
            .ActivateOnEnter<VoidFireIII>()
            .ActivateOnEnter<DarkEruption>()
            .ActivateOnEnter<Condemnation>()
            .ActivateOnEnter<VoidParalyze>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1089, NameID = 14560)]
public class AethericCharge(ModuleInit init) : BossModule(init, new(120, -420), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => PrimaryActor.InCombat;
}

