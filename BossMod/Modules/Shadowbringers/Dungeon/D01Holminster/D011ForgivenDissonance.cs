namespace BossMod.Shadowbringers.Dungeon.D01Holminser.D011ForgivenDissonance;

public enum OID : uint
{
    Boss = 0x278A, // R4.000, x1
    Orbs = 0x2896, // R1.100, spawn during fight
    Helper = 0x233C, // x3
    Helper2 = 0x2A4B, // R3.450, spawn during fight
};

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    ThePathOfLight = 15813, // Boss->self, 4.0s cast, range 40 circle
    BrazenBull = 15817, // Boss->self, 3.0s cast, single-target
    GibbetCage = 15816, // Boss->self, 3.0s cast, range 8 circle
    Thumbscrew = 15814, // Boss->location, 4.8s cast, width 8 rect charge
    HereticsFork = 15822, // Orbs->self, 8.0s cast, range 40 width 6 cross
    LightShot = 15819, // Orbs->self, 3.0s cast, range 40 width 4 rect
    WoodenHorse = 15815, // Boss->self, 5.0s cast, range 40 90-degree cone
    Pillory = 15812, // Boss->player, 5.0s cast, single-target
};

class Thumbscrew : Components.ChargeAOEs
{
    public Thumbscrew() : base(ActionID.MakeSpell(AID.Thumbscrew), 4) { }
}

class ThePathofLight : Components.RaidwideCast
{
    public ThePathofLight() : base(ActionID.MakeSpell(AID.ThePathOfLight)) { }
}

class GibbetCage : Components.SelfTargetedAOEs
{
    public GibbetCage() : base(ActionID.MakeSpell(AID.GibbetCage), new AOEShapeCircle(8)) { }
}

class HereticsFork : Components.SelfTargetedAOEs
{
    public HereticsFork() : base(ActionID.MakeSpell(AID.HereticsFork), new AOEShapeCross(40, 3)) { }
}

class LightShot : Components.SelfTargetedAOEs
{
    public LightShot() : base(ActionID.MakeSpell(AID.LightShot), new AOEShapeRect(40, 2)) { }
}

class WoodenHorse : Components.SelfTargetedAOEs
{
    public WoodenHorse() : base(ActionID.MakeSpell(AID.WoodenHorse), new AOEShapeCone(40, 45.Degrees())) { }
}

class Pillory : Components.SingleTargetDelayableCast
{
    public Pillory() : base(ActionID.MakeSpell(AID.Pillory)) { }
}

class D011ForgivenDissonanceStates : StateMachineBuilder
{
    public D011ForgivenDissonanceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Thumbscrew>()
            .ActivateOnEnter<ThePathofLight>()
            .ActivateOnEnter<GibbetCage>()
            .ActivateOnEnter<HereticsFork>()
            .ActivateOnEnter<LightShot>()
            .ActivateOnEnter<WoodenHorse>()
            .ActivateOnEnter<Pillory>();

    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 676, NameID = 8299)]
public class D011ForgivenDissonance : BossModule
{
    public D011ForgivenDissonance(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-15, 240), 19.5f)) { }
}
