namespace BossMod.Shadowbringers.Dungeon.D01Holminser.D011ForgivenDissonance;

public enum OID : uint
{
    Boss = 0x278A, // R4.000, x1
    Orbs = 0x2896, // R1.100, spawn during fight
    Helper = 0x233C, // x3
    Helper2 = 0x2A4B, // R3.450, spawn during fight
}

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
}

class Thumbscrew(BossModule module) : Components.ChargeAOEs(module, AID.Thumbscrew, 4);
class ThePathofLight(BossModule module) : Components.RaidwideCast(module, AID.ThePathOfLight);
class GibbetCage(BossModule module) : Components.StandardAOEs(module, AID.GibbetCage, new AOEShapeCircle(8));
class HereticsFork(BossModule module) : Components.StandardAOEs(module, AID.HereticsFork, new AOEShapeCross(40, 3));
class LightShot(BossModule module) : Components.StandardAOEs(module, AID.LightShot, new AOEShapeRect(40, 2));
class WoodenHorse(BossModule module) : Components.StandardAOEs(module, AID.WoodenHorse, new AOEShapeCone(40, 45.Degrees()));
class Pillory(BossModule module) : Components.SingleTargetDelayableCast(module, AID.Pillory);

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
public class D011ForgivenDissonance(WorldState ws, Actor primary) : BossModule(ws, primary, new(-15, 240), new ArenaBoundsCircle(19.5f));
