namespace BossMod.Endwalker.Dungeon.D09Alzadaal.D091Ambujam;

public enum OID : uint
{
    Boss = 0x3879, // R=9.0
    Helper = 0x233C,
    Unknown = 0x3931, // R4.400, x2 (spawn during fight)
    Actor1e8f2f = 0x1E8F2F, // R0.500, x2 (spawn during fight), EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x3 (spawn during fight), EventObj type
    CyanTentacle = 0x387B, // R2.400, x1
    ScarletTentacle = 0x387A, // R2.400, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    BigWave = 28512, // Boss->self, 5.0s cast, range 40 circle //Raidwide
    CorrosiveFountain = 29556, // Helper->self, 7.0s cast, range 8 circle
    CorrosiveVenomVisual = 29157, // CyanTentacle->self, no cast, single-target
    CorrosiveVenomAOE = 29158, // Helper->self, 2.5s cast, range 21 circle
    TentacleDig1 = 28501, // Boss->self, 3.0s cast, single-target
    TentacleDig2 = 28505, // Boss->self, 3.0s cast, single-target
    ToxicFountainVisual = 29466, // Boss->self, 4.0s cast, single-target
    ToxicFountainAOE = 29467, // Helper->self, 7.0s cast, range 8 circle
    ToxinShowerVisual = 28507, // ScarletTentacle->self, no cast, single-target
    ToxinShowerAOE = 28508, // Helper->self, 2.5s cast, range 21 circle
    Unknown1 = 28502, // Boss->self, no cast, single-target
    Unknown2 = 28506, // Boss->self, no cast, single-target
}

class CorrosiveFountain(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CorrosiveFountain), new AOEShapeCircle(8));
class CorrosiveVenomAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CorrosiveVenomAOE), new AOEShapeCircle(21));
class ToxicFountainAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ToxicFountainAOE), new AOEShapeCircle(8));
class ToxinShowerAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ToxinShowerAOE), new AOEShapeCircle(21));

class BigWave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BigWave));

class D091AmbujamStates : StateMachineBuilder
{
    public D091AmbujamStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CorrosiveFountain>()
            .ActivateOnEnter<CorrosiveVenomAOE>()
            .ActivateOnEnter<ToxicFountainAOE>()
            .ActivateOnEnter<ToxinShowerAOE>()
            .ActivateOnEnter<BigWave>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 844, NameID = 11241)]
public class D091Ambujam(WorldState ws, Actor primary) : BossModule(ws, primary, new(124, -90), new ArenaBoundsCircle(20));
