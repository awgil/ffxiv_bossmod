namespace BossMod.Endwalker.Trial.T08Asura;

class LowerRealm : Components.RaidwideCast
{
    public LowerRealm() : base(ActionID.MakeSpell(AID.LowerRealm)) { }

    public override void Update(BossModule module) //deathwall appears after 1st cast
    {
        if (NumCasts > 0)
            module.Arena.Bounds = new ArenaBoundsCircle(new(100, 100), 19);
    }
}

class Ephemerality : Components.RaidwideCast
{
    public Ephemerality() : base(ActionID.MakeSpell(AID.Ephemerality)) { }
}

class CuttingJewel : Components.BaitAwayCast
{
    public CuttingJewel() : base(ActionID.MakeSpell(AID.CuttingJewel), new AOEShapeCircle(4), true) { }
}

class CuttingJewelHint : Components.SingleTargetCast
{
    public CuttingJewelHint() : base(ActionID.MakeSpell(AID.CuttingJewel)) { }
}

class IconographyPedestalPurge : Components.SelfTargetedAOEs
{
    public IconographyPedestalPurge() : base(ActionID.MakeSpell(AID.IconographyPedestalPurge), new AOEShapeCircle(10)) { }
}

class PedestalPurge : Components.SelfTargetedAOEs
{ //Note, this is not a raidwide, origin is outside of the arena
    public PedestalPurge() : base(ActionID.MakeSpell(AID.PedestalPurge), new AOEShapeCircle(27)) { }
}

class IconographyWheelOfDeincarnation : Components.SelfTargetedAOEs
{
    public IconographyWheelOfDeincarnation() : base(ActionID.MakeSpell(AID.IconographyWheelOfDeincarnation), new AOEShapeDonut(8, 40)) { }
}

class WheelOfDeincarnation : Components.SelfTargetedAOEs
{
    public WheelOfDeincarnation() : base(ActionID.MakeSpell(AID.WheelOfDeincarnation), new AOEShapeDonut(15, 96)) { }
}

class IconographyBladewise : Components.SelfTargetedAOEs
{
    public IconographyBladewise() : base(ActionID.MakeSpell(AID.IconographyBladewise), new AOEShapeRect(50, 3)) { }
}

class Bladewise : Components.SelfTargetedAOEs
{
    public Bladewise() : base(ActionID.MakeSpell(AID.Bladewise), new AOEShapeRect(100, 14)) { }
}

class Scattering : Components.SelfTargetedAOEs
{
    public Scattering() : base(ActionID.MakeSpell(AID.Scattering), new AOEShapeRect(20, 3)) { }
}

class OrderedChaos : Components.SpreadFromCastTargets
{
    public OrderedChaos() : base(ActionID.MakeSpell(AID.OrderedChaos), 5) { }
}

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

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 944, NameID = 12351)]
public class T08Asura : BossModule
{
    public T08Asura(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }
}
