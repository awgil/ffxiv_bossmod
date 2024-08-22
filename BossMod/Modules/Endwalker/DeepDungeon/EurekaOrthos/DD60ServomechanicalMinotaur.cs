namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD60ServomechanicalMinotaur;

public enum OID : uint
{
    Boss = 0x3DA1, // R6.000, x?
    Helper = 0x233C, // R0.500, x?, Helper type
    BallOfLevin = 0x3DA2, // R1.300, x?
}

public enum AID : uint
{
    AutoAttack_Attack = 6499, // Boss->player, no cast, single-target
    Weaponskill_ = 31867, // Helper->self, 1.0s cast, range 40 ?-degree cone // Displays the order of the aoe's going off
    OctupleSwipe = 31872, // Boss->self, 10.8s cast, range 40 ?-degree cone // Windup Cast for N/E/S/W
    BullishSwipe4 = 31871, // Boss->self, no cast, range 40 ?-degree cone
    BullishSwipe3 = 31870, // Boss->self, no cast, range 40 ?-degree cone
    BullishSwipe2 = 31869, // Boss->self, no cast, range 40 ?-degree cone
    BullishSwipe1 = 31868, // Boss->self, no cast, range 40 ?-degree cone
    DisorientingGroan = 31876, // Boss->self, 5.0s cast, range 60 circle, 15 yalm KB // done
    BullishSwipeSingle = 32795, // Boss->self, 5.0s cast, range 40 90-degree cone, usually used after KB to try and catch off guard // done
    Thundercall = 31873, // Boss->self, 5.0s cast, range 60 circle 
    Shock = 31874, // BallOfLevin->self, 2.5s cast, range 5 circle
    BullishSwing = 31875, // Boss->self, 5.0s cast, range 13 circle, PB Circle AOE // done
}

class BullishSwing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BullishSwing), new AOEShapeCircle(13));
class BullishSwipeSingle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BullishSwipeSingle), new AOEShapeCone(40, 45.Degrees()));
class DisorientingGroan(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.DisorientingGroan), 15, true);
class Shock(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Shock), new AOEShapeCircle(5));

// Need to figure out how to telegraph the 8 cleaves going off still

class DD60ServomechanicalMinotaurStates : StateMachineBuilder
{
    public DD60ServomechanicalMinotaurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BullishSwipeSingle>()
            .ActivateOnEnter<BullishSwing>()
            .ActivateOnEnter<DisorientingGroan>()
            .ActivateOnEnter<Shock>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "Puni.sh Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 902, NameID = 12267)]
public class DD60ServomechanicalMinotaur(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600, -300), new ArenaBoundsCircle(20));
