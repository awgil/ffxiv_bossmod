namespace BossMod.Global.MaskedCarnivale.Stage23;

public enum OID : uint
{
    Boss = 0x2732, //R=5.8
    Maelstrom = 0x2733, //R=1.0
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Charybdis = 15258, // Boss->location, 3.0s cast, range 6 circle
    Maelstrom = 15259, // Maelstrom->self, 1.0s cast, range 8 circle, pull dist 40 into center, vuln stack
    Trounce = 15256, // Boss->self, 3.5s cast, range 50+R 60-degree cone
    Comet = 15260, // Boss->self, 5.0s cast, single-target
    Comet2 = 15261, // Helper->location, 4.0s cast, range 10 circle
    EclipticMeteor = 15257, // Boss->location, 10.0s cast, range 50 circle
}

class Charybdis(BossModule module) : Components.LocationTargetedAOEs(module, AID.Charybdis, 6);
class Maelstrom(BossModule module) : Components.PersistentVoidzone(module, 8, m => m.Enemies(OID.Maelstrom));
class Trounce(BossModule module) : Components.SelfTargetedAOEs(module, AID.Trounce, new AOEShapeCone(55.8f, 30.Degrees()));
class Comet(BossModule module) : Components.LocationTargetedAOEs(module, AID.Comet2, 10);
class EclipticMeteor(BossModule module) : Components.RaidwideCast(module, AID.EclipticMeteor, "Use Diamondback!");

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"The {Module.PrimaryActor.Name} will use Ecliptic Meteor.\nUse Diamondback to survive it.\nYou can start the Final Sting combination at about 40% health left.\n(Off-guard->Bristle->Moonflute->Final Sting)");
    }
}

class Stage23States : StateMachineBuilder
{
    public Stage23States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Charybdis>()
            .ActivateOnEnter<Maelstrom>()
            .ActivateOnEnter<Trounce>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<EclipticMeteor>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 633, NameID = 8124)]
public class Stage23 : BossModule
{
    public Stage23(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsCircle(16))
    {
        ActivateComponent<Hints>();
    }
}
