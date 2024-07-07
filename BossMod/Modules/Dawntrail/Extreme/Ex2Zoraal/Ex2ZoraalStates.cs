namespace BossMod.Dawntrail.Extreme.Ex2Zoraal;

class Ex2ZoraalStates : StateMachineBuilder
{
    public Ex2ZoraalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Actualize1>()
            .ActivateOnEnter<Actualize2>()
            .ActivateOnEnter<MultidirectionalDivide1>()
            .ActivateOnEnter<MultidirectionalDivide2>()
            .ActivateOnEnter<MultidirectionalDivide3>()
            .ActivateOnEnter<ChasmOfVollok2>()
            .ActivateOnEnter<ChasmOfVollok3>()
            .ActivateOnEnter<ChasmOfVollok4>()
            .ActivateOnEnter<DawnOfAnAge>()
            .ActivateOnEnter<Vollok3>()
            .ActivateOnEnter<HalfFull3>()
            .ActivateOnEnter<HalfFull4>()
            .ActivateOnEnter<ForgedTrack2>()
            .ActivateOnEnter<BitterWhirlwind2>()
            .ActivateOnEnter<DrumOfVollok2>()
            .ActivateOnEnter<HalfCircuit3>()
            .ActivateOnEnter<HalfCircuit4>()
            .ActivateOnEnter<HalfCircuit5>()
            .ActivateOnEnter<WallsOfVollok>()
            .ActivateOnEnter<StormyEdge2>()
            .ActivateOnEnter<StormyEdge3>()
            .ActivateOnEnter<StormyEdgeKnockback>()
            .ActivateOnEnter<BurningChains>();
    }
}
class Actualize1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Actualize1));
class Actualize2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Actualize2));
class MultidirectionalDivide1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MultidirectionalDivide1), new AOEShapeCross(30, 2));
class MultidirectionalDivide2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MultidirectionalDivide2), new AOEShapeCross(30, 4));
class MultidirectionalDivide3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MultidirectionalDivide3), new AOEShapeRect(20, 2));
class ChasmOfVollok2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ChasmOfVollok2), new AOEShapeRect(10, 5));
class ChasmOfVollok3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ChasmOfVollok3), new AOEShapeRect(5, 2.5f));
class ChasmOfVollok4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ChasmOfVollok4), new AOEShapeRect(5, 2.5f));
class DawnOfAnAge(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DawnOfAnAge));
class Vollok3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Vollok3), new AOEShapeRect(10, 5));
class HalfFull3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HalfFull3), new AOEShapeRect(60, 60));
class HalfFull4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HalfFull4), new AOEShapeRect(60, 60));
class ForgedTrack2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ForgedTrack2), new AOEShapeRect(20, 2.5f));
class BitterWhirlwind2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.BitterWhirlwind2), 5);
class DrumOfVollok2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.DrumOfVollok2), 4);
class HalfCircuit3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HalfCircuit3), new AOEShapeRect(60, 60));
class HalfCircuit4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HalfCircuit4), new AOEShapeDonut(10, 30));
class HalfCircuit5(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HalfCircuit5), new AOEShapeCircle(10));
class WallsOfVollok(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WallsOfVollok), new AOEShapeCircle(4));
class StormyEdge2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.StormyEdge2), new AOEShapeRect(20, 2.5f));
class StormyEdge3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.StormyEdge3), new AOEShapeRect(10, 10));
class StormyEdgeKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.StormyEdge3), 7, stopAtWall: true, kind: Kind.DirForward);
class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, ActionID.MakeSpell(AID.BurningChains1));
