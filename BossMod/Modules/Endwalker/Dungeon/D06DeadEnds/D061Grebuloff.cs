namespace BossMod.Endwalker.Dungeon.D06DeadEnds.D061Grebuloff;

public enum OID : uint
{
    Boss = 0x34C4, // R6.650, x1
    Helper = 0x233C, // R0.500, x12 (spawn during fight), 523 type
    WeepingMiasma = 0x34C5, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Befoulment = 25923, // Boss->self, 5.0s cast, single-target
    BefoulmentSpread = 25924, // Helper->player, 5.2s cast, range 6 circle //Spread mechanic
    BlightedWater = 25921, // Boss->self, 5.0s cast, single-target
    BlightedWaterStack = 25922, // Helper->players, 5.2s cast, range 6 circle //Stack mechanic
    CertainSolitude = 28349, // Boss->self, no cast, range 40 circle
    CoughUp1 = 25917, // Boss->self, 4.0s cast, single-target
    CoughUpAOE = 25918, // Helper->location, 4.0s cast, range 6 circle
    Miasmata = 25916, // Boss->self, 5.0s cast, range 40 circle
    NecroticFluid = 25919, // WeepingMiasma->self, 6.5s cast, range 6 circle
    NecroticMist = 28348, // Helper->location, 1.3s cast, range 6 circle
    PoxFlail = 25920, // Boss->player, 5.0s cast, single-target //Tankbuster
    WaveOfNausea = 28347, // Boss->self, 5.5s cast, range ?-40 donut
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Helper->player, extra=0x1/0x2
    Necrosis = 2965, // Helper->player, extra=0x0
    Weakness = 43, // none->player, extra=0x0
    Transcendent = 418, // none->player, extra=0x0
    CravenCompanionship = 2966, // Boss->player, extra=0x0
}

public enum IconID : uint
{
    Icon55 = 55, // player
    Icon218 = 218, // player
    Icon62 = 62, // player
    Icon139 = 139, // player
}

class BefoulmentSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.BefoulmentSpread), 6);
class BlightedWaterStack(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.BlightedWaterStack), 6, 8);

class CoughUpAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.CoughUpAOE), 6);
class NecroticMist(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.NecroticMist), 6);

class NecroticFluid(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.NecroticFluid), new AOEShapeCircle(6));
class WaveOfNausea(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WaveOfNausea), new AOEShapeDonut(8, 30));

class PoxFlail(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.PoxFlail));

class CertainSolitude(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CertainSolitude));
class Miasmata(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Miasmata));

class D061GrebuloffStates : StateMachineBuilder
{
    public D061GrebuloffStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BefoulmentSpread>()
            .ActivateOnEnter<BlightedWaterStack>()
            .ActivateOnEnter<CoughUpAOE>()
            .ActivateOnEnter<NecroticFluid>()
            .ActivateOnEnter<NecroticMist>()
            .ActivateOnEnter<WaveOfNausea>()
            .ActivateOnEnter<PoxFlail>()
            .ActivateOnEnter<CertainSolitude>()
            .ActivateOnEnter<Miasmata>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 792, NameID = 10313)]
public class D061Grebuloff(WorldState ws, Actor primary) : BossModule(ws, primary, new(266.5f, -178), new ArenaBoundsCircle(20));
