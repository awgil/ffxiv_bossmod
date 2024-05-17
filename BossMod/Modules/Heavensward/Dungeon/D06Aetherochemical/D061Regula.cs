namespace BossMod.Heavensward.Dungeon.D06Aetherochemical.D061Regula;

public enum OID : uint
{
    Boss = 0xE97, // R1.650, x1
    Helper = 0x1B2, // R0.500, x1
    ClockworkHunter = 0xF5C, // R1.250, x6
    MagitekTurretI = 0xE98, // R0.600, x0 (spawn during fight)
    MagitekTurretII = 0xE99, // R0.600, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    AetherochemicalGrenado = 4322, // MagitekTurretII->location, 3.0s cast, range 8 circle
    AetherochemicalLaser = 4321, // MagitekTurretI->player, 3.0s cast, range 50 width 4 rect
    Bastardbluss = 4314, // Boss->player, no cast, single-target

    Judgment = 4317, // Boss->player, no cast, single-target
    JudgmentAOE = 4318, // Helper->self, 3.0s cast, range 8 circle

    SelfDetonate = 4323, // MagitekTurretI/MagitekTurretII->self, 5.0s cast, range 40+R circle

    MagitekSlug = 4315, // Boss->self, 2.5s cast, range 60+R width 4 rect
    MagitekSpread = 4316, // Boss->self, 4.5s cast, range 30+R 240-degree cone
    MagitekTurret = 4320, // Boss->self, no cast, single-target
    Quickstep = 4319, // Boss->location, no cast, single-target
}

public enum TetherID : uint
{
    BaitAway = 17, // MagitekTurretI->player
}

class SelfDetonate(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SelfDetonate));

class AetherochemicalGrenado(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.AetherochemicalGrenado), 8);
class AetherochemicalLaser(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(50, 2), (uint)TetherID.BaitAway, ActionID.MakeSpell(AID.AetherochemicalLaser));
class AetherochemicalLaserAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AetherochemicalLaser), new AOEShapeRect(50, 2));

class JudgmentAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.JudgmentAOE), new AOEShapeCircle(8));
class MagitekSlug(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekSlug), new AOEShapeRect(60, 2));
class MagitekSpread(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MagitekSpread), new AOEShapeCone(35, 120.Degrees()));

class D061RegulaStates : StateMachineBuilder
{
    public D061RegulaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetherochemicalGrenado>()
            .ActivateOnEnter<AetherochemicalLaser>()
            .ActivateOnEnter<AetherochemicalLaserAOE>()
            .ActivateOnEnter<JudgmentAOE>()
            .ActivateOnEnter<MagitekSlug>()
            .ActivateOnEnter<MagitekSpread>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 3818)]
public class D061Regula(WorldState ws, Actor primary) : BossModule(ws, primary, new(-111, -295.5f), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.MagitekTurretI), ArenaColor.Enemy);
    }
}
