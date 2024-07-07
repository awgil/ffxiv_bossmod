namespace BossMod.Dawntrail.Trial.T02ZoraalP2;

class T02ZoraalP2States : StateMachineBuilder
{
    public T02ZoraalP2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SmitingCircuit4>()
            .ActivateOnEnter<SmitingCircuit5>()
            .ActivateOnEnter<DawnOfAnAge>()
            .ActivateOnEnter<BitterReaping2>()
            .ActivateOnEnter<ChasmOfVollok1>()
            .ActivateOnEnter<ChasmOfVollok2>()
            .ActivateOnEnter<ForgedTrack2>()
            .ActivateOnEnter<Actualize>()
            .ActivateOnEnter<HalfFull3>()
            .ActivateOnEnter<HalfCircuit3>()
            .ActivateOnEnter<HalfCircuit4>()
            .ActivateOnEnter<HalfCircuit5>()
            .ActivateOnEnter<DawnofanAgeBorder>();
    }
}
class SmitingCircuit4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SmitingCircuit4), new AOEShapeDonut(10, 30));
class SmitingCircuit5(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SmitingCircuit5), new AOEShapeCircle(10));
class DawnOfAnAge(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DawnOfAnAge));
class BitterReaping2(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.BitterReaping2));
class ChasmOfVollok1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ChasmOfVollok1), new AOEShapeRect(5, 2.5f));
class ChasmOfVollok2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ChasmOfVollok2), new AOEShapeRect(5, 2.5f));
class ForgedTrack2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ForgedTrack2), new AOEShapeRect(20, 2.5f));
class Actualize(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Actualize));
class HalfFull3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HalfFull3), new AOEShapeRect(60, 30));
class HalfCircuit3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HalfCircuit3), new AOEShapeRect(60, 30));
class HalfCircuit4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HalfCircuit4), new AOEShapeDonut(10, 30));
class HalfCircuit5(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HalfCircuit5), new AOEShapeCircle(10));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 995, NameID = 12882)]
public class T02ZoraalP2(WorldState ws, Actor primary) : BossModule(ws, primary, DawnofanAgeBorder.Center, DawnofanAgeBorder.NormalBounds);
// Arena swaps to 10x10 square with 45 degree rotation periodically
