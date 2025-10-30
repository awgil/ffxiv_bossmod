namespace BossMod.Endwalker.Trial.T08Asura;

class LowerRealm(BossModule module) : Components.RaidwideCast(module, AID.LowerRealm);
class Ephemerality(BossModule module) : Components.RaidwideCast(module, AID.Ephemerality);
class CuttingJewel(BossModule module) : Components.BaitAwayCast(module, AID.CuttingJewel, new AOEShapeCircle(4), true);
class CuttingJewelHint(BossModule module) : Components.SingleTargetCast(module, AID.CuttingJewel);
class IconographyPedestalPurge(BossModule module) : Components.StandardAOEs(module, AID.IconographyPedestalPurge, 10);
class PedestalPurge(BossModule module) : Components.StandardAOEs(module, AID.PedestalPurge, 60);
class IconographyWheelOfDeincarnation(BossModule module) : Components.StandardAOEs(module, AID.IconographyWheelOfDeincarnation, new AOEShapeDonut(8, 40));
class WheelOfDeincarnation(BossModule module) : Components.StandardAOEs(module, AID.WheelOfDeincarnation, new AOEShapeDonut(48, 96));
class IconographyBladewise(BossModule module) : Components.StandardAOEs(module, AID.IconographyBladewise, new AOEShapeRect(50, 3));
class Bladewise(BossModule module) : Components.StandardAOEs(module, AID.Bladewise, new AOEShapeRect(100, 14));
class Scattering(BossModule module) : Components.StandardAOEs(module, AID.Scattering, new AOEShapeRect(20, 3));
class OrderedChaos(BossModule module) : Components.SpreadFromCastTargets(module, AID.OrderedChaos, 5);

class T08AsuraStates : StateMachineBuilder
{
    public T08AsuraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Ephemerality>()
            .ActivateOnEnter<LowerRealm>()
            .ActivateOnEnter<AsuriChakra>()
            .ActivateOnEnter<Chakra1>()
            .ActivateOnEnter<Chakra2>()
            .ActivateOnEnter<Chakra3>()
            .ActivateOnEnter<Chakra4>()
            .ActivateOnEnter<Chakra5>()
            .ActivateOnEnter<CuttingJewel>()
            .ActivateOnEnter<CuttingJewelHint>()
            .ActivateOnEnter<Laceration>()
            .ActivateOnEnter<IconographyPedestalPurge>()
            .ActivateOnEnter<PedestalPurge>()
            .ActivateOnEnter<IconographyWheelOfDeincarnation>()
            .ActivateOnEnter<WheelOfDeincarnation>()
            .ActivateOnEnter<IconographyBladewise>()
            .ActivateOnEnter<Bladewise>()
            .ActivateOnEnter<SixBladedKhadga>()
            .ActivateOnEnter<MyriadAspects>()
            .ActivateOnEnter<Scattering>()
            .ActivateOnEnter<OrderedChaos>()
            .ActivateOnEnter<ManyFaces>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 944, NameID = 12351)]
public class T08Asura(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(19));
