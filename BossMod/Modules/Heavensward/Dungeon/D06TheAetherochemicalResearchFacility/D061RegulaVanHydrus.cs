namespace BossMod.Heavensward.Dungeon.D06AetherochemicalResearchFacility.D061RegulaVanHydrus;

public enum OID : uint
{
    Boss = 0xE97, // R1.650, x1
    ClockworkHunter = 0xF5C, // R1.250, x6
    Helper = 0x1B2, // R0.500, x1
    MagitekTurretI = 0xE98, // R0.600, x0 (spawn during fight)
    MagitekTurretII = 0xE99, // R0.600, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AetherochemicalGrenado = 4322, // MagitekTurretII->location, 3.0s cast, range 8 circle
    AetherochemicalLaser = 4321, // MagitekTurretI->player, 3.0s cast, range 50 width 4 rect
    Bastardbluss = 4314, // Boss->player, no cast, single-target, tankbuster + stun
    Judgment = 4317, // Boss->player, no cast, single-target
    JudgmentAOE = 4318, // Helper->self, 3.0s cast, range 8 circle
    MagitekSlug = 4315, // Boss->self, 2.5s cast, range 60+R width 4 rect
    MagitekSpread = 4316, // Boss->self, 4.5s cast, range 30+R 240-degree cone, knockback 20, away from source
    MagitekTurret = 4320, // Boss->self, no cast, single-target
    Quickstep = 4319, // Boss->location, no cast, single-target
    SelfDetonate = 4323, // MagitekTurretI/MagitekTurretII->self, 5.0s cast, range 40+R circle
}

public enum TetherID : uint
{
    BaitAway = 17 // MagitekTurretI->player
}

class AetherochemicalGrenado(BossModule module) : Components.StandardAOEs(module, AID.AetherochemicalGrenado, 8);
class AetherochemicalLaser(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(50, 2), (uint)TetherID.BaitAway, AID.AetherochemicalLaser);
class AetherochemicalLaserAOE(BossModule module) : Components.StandardAOEs(module, AID.AetherochemicalLaser, new AOEShapeRect(50, 2));
class JudgmentAOE(BossModule module) : Components.StandardAOEs(module, AID.JudgmentAOE, new AOEShapeCircle(8));
class MagiteckTurrents(BossModule module) : Components.AddsMulti(module, [OID.MagitekTurretI, OID.MagitekTurretII]);
class MagitekSlug(BossModule module) : Components.StandardAOEs(module, AID.MagitekSlug, new AOEShapeRect(61.65f, 2));
class MagitekSpread(BossModule module) : Components.StandardAOEs(module, AID.MagitekSpread, new AOEShapeCone(31.65f, 120.Degrees()));
class SelfDetonate(BossModule module) : Components.RaidwideCast(module, AID.SelfDetonate);

class D061RegulaVanHydrusStates : StateMachineBuilder
{
    public D061RegulaVanHydrusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetherochemicalGrenado>()
            .ActivateOnEnter<AetherochemicalLaser>()
            .ActivateOnEnter<AetherochemicalLaserAOE>()
            .ActivateOnEnter<JudgmentAOE>()
            .ActivateOnEnter<MagiteckTurrents>()
            .ActivateOnEnter<MagitekSlug>()
            .ActivateOnEnter<MagitekSpread>()
            .ActivateOnEnter<SelfDetonate>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Herc, LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 3818)]
public class D061RegulaVanHydrus(WorldState ws, Actor primary) : BossModule(ws, primary, new(-111, -295), new ArenaBoundsCircle(21)); // edge of arena is -273
