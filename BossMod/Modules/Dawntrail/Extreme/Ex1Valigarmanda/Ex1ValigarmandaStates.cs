namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

class Ex1ValigarmandaStates : StateMachineBuilder
{
    public Ex1ValigarmandaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArcaneLightning>()
            .ActivateOnEnter<CalamitousEcho>()
            .ActivateOnEnter<ChillingCataclysm2>()
            .ActivateOnEnter<CracklingCataclysm>()
            .ActivateOnEnter<DisasterZone2>()
            .ActivateOnEnter<DisasterZone4>()
            .ActivateOnEnter<DisasterZone6>()
            .ActivateOnEnter<MountainFire5>()
            .ActivateOnEnter<NorthernCross1>()
            .ActivateOnEnter<NorthernCross2>()
            .ActivateOnEnter<Ruinfall2>()
            .ActivateOnEnter<Ruinfall3>()
            .ActivateOnEnter<Ruinfall4>()
            .ActivateOnEnter<SlitheringStrike4>()
            .ActivateOnEnter<SphereShatter2>()
            .ActivateOnEnter<Spikesicle3>()
            .ActivateOnEnter<Spikesicle4>()
            .ActivateOnEnter<Spikesicle5>()
            .ActivateOnEnter<Spikesicle6>()
            .ActivateOnEnter<Spikesicle7>()
            .ActivateOnEnter<SusurrantBreath4>()
            .ActivateOnEnter<ThunderousBreath2>()
            .ActivateOnEnter<VolcanicDrop2>()
            .ActivateOnEnter<VolcanicDrop3>();
    }
}

class ArcaneLightning(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ArcaneLightning), new AOEShapeRect(50, 2.5f));
class CalamitousEcho(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CalamitousEcho), new AOEShapeCone(20, 10.Degrees()));
class ChillingCataclysm2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ChillingCataclysm2), new AOEShapeCross(40, 2.5f));
class CracklingCataclysm(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.CracklingCataclysm), 6);
class DisasterZone2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DisasterZone2));
class DisasterZone4(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DisasterZone4));
class DisasterZone6(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DisasterZone6));
class MountainFire5(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MountainFire5), new AOEShapeCircle(3));
class NorthernCross1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.NorthernCross1), new AOEShapeRect(60, 60, DirectionOffset: -90.Degrees()));
class NorthernCross2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.NorthernCross2), new AOEShapeRect(60, 60, DirectionOffset: 90.Degrees()));
class Ruinfall2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Ruinfall2), new AOEShapeCircle(6));
class Ruinfall3(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Ruinfall3), 25, stopAtWall: true, kind: Kind.DirForward);
class Ruinfall4(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Ruinfall4), 6);
class SlitheringStrike4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SlitheringStrike4), new AOEShapeCone(24, 90.Degrees()));
class SphereShatter2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SphereShatter2), new AOEShapeCircle(13));
class Spikesicle3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Spikesicle3), new AOEShapeDonut(6, 25));
class Spikesicle4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Spikesicle4), new AOEShapeDonut(6, 30));
class Spikesicle5(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Spikesicle5), new AOEShapeDonut(6, 35));
class Spikesicle6(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Spikesicle6), new AOEShapeDonut(6, 40));
class Spikesicle7(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Spikesicle7), new AOEShapeRect(40, 2.5f));
class StranglingCoil4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.StranglingCoil4), new AOEShapeDonut(6, 60));
class SusurrantBreath4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SusurrantBreath4), new AOEShapeCone(50, 65.Degrees()));
class ThunderousBreath2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderousBreath2), new AOEShapeCone(50, 67.5f.Degrees()));
class VolcanicDrop2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.VolcanicDrop2), 20);
class VolcanicDrop3(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.VolcanicDrop3), 20);
