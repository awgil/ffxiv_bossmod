namespace BossMod.Stormblood.Dungeon.D03BardamsMettle.D031Garula;

public enum OID : uint
{
    Boss = 0x1A9E, // R4.0
    SteppeSheep = 0x1A9F, // R0.7
    SteppeYamaa1 = 0x1AA1, // R1.92
    SteppeYamaa2 = 0x1AA0, // R1.92
    SteppeCoeurl = 0x1AA2, // R3.15
    Helper = 0x19A
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Heave = 7927, // Boss->self, 2.5s cast, range 9+R 120-degree cone
    CrumblingCrustVisual = 7928, // Boss->self, 4.0s cast, single-target
    CrumblingCrust = 7955, // Helper->location, 1.5s cast, range 3 circle
    Rush = 7929, // Boss->player, 10.0s cast, width 8 rect charge
    WarCry = 7930, // Boss->self, no cast, range 15+R circle
    Earthquake = 7931, // Boss->self, no cast, range 50+R circle
    Lullaby = 9394, // SteppeSheep->self, 3.0s cast, range 3+R circle, applies sleep
    RushYamaa = 7932, // SteppeYamaa1/SteppeYamaa2->location, 4.5s cast, width 8 rect charge
    WideBlaster = 9395, // SteppeCoeurl->self, 5.5s cast, range 26+R 120-degree cone
}

class CrumblingCrust(BossModule module) : Components.StandardAOEs(module, AID.CrumblingCrust, 3);
class Heave(BossModule module) : Components.StandardAOEs(module, AID.Heave, new AOEShapeCone(13, 60.Degrees()));
class WideBlaster(BossModule module) : Components.StandardAOEs(module, AID.WideBlaster, new AOEShapeCone(29.15f, 60.Degrees()));
class Rush(BossModule module) : Components.BaitAwayChargeCast(module, AID.Rush, 4);
class RushTether(BossModule module) : Components.BaitAwayCast(module, AID.RushYamaa, new AOEShapeCircle(5), true)
{
    public override void Update()
    {
        if (Module.PrimaryActor.CastInfo == null)
            CurrentBaits.Clear();
    }
}

class RushYamaa(BossModule module) : Components.ChargeAOEs(module, AID.RushYamaa, 4);
class Lullaby(BossModule module) : Components.StandardAOEs(module, AID.Lullaby, new AOEShapeCircle(3.7f));

class D031GarulaStates : StateMachineBuilder
{
    public D031GarulaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CrumblingCrust>()
            .ActivateOnEnter<Heave>()
            .ActivateOnEnter<WideBlaster>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<RushTether>()
            .ActivateOnEnter<RushYamaa>()
            .ActivateOnEnter<Lullaby>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team (Malediktus), Ported by Herculezz", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 240, NameID = 6173)]
public class D031Garula(WorldState ws, Actor primary) : BossModule(ws, primary, new(3.96f, 251.17f), new ArenaBoundsCircle(19.5f));
