namespace BossMod.Heavensward.DeepDungeon.PalaceoftheDead.D80Gudanna;

public enum OID : uint
{
    Boss = 0x1816, // R11.600, x1
    Tornado = 0x18F0, // R1.000, spawn during fight
    Helper = 0x233C, // R0.500, x12, 523 type
}

public enum AID : uint
{
    Attack = 6497, // Boss->player, no cast, single-target
    Charybdis = 7096, // Boss->location, 3.0s cast, range 6 circle // Cast that sets the tornado location 
    TornadoVoidzone = 7164, // 18F0->self, no cast, range 6 circle
    EclipticMeteor = 7099, // Boss->self, 20.0s cast, range 50 circle
    Maelstrom = 7167, // 18F0->self, 1.3s cast, range 10 circle
    Thunderbolt = 7095, // Boss->self, 2.5s cast, range 5+R 120-degree cone
    Trounce = 7098, // Boss->self, 2.5s cast, range 40+R 60-degree cone
}

class Charybdis(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Charybdis), 6);
class Maelstrom(BossModule module) : Components.PersistentVoidzone(module, 10, m => m.Enemies(OID.Tornado));
class Trounce(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Trounce), new AOEShapeCone(51.6f, 30.Degrees()));
class EclipticMeteor(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.EclipticMeteor), "Kill him before he kills you! 80% max HP damage incoming!");
class Thunderbolt(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Thunderbolt), new AOEShapeCone(16.6f, 60.Degrees()));

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will use Ecliptic Meteor.\nYou must either kill him before he cast it multiple times, or heal through it.");
    }
}

class D80GudannaStates : StateMachineBuilder
{
    public D80GudannaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<Charybdis>()
            .ActivateOnEnter<Maelstrom>()
            .ActivateOnEnter<Trounce>()
            .ActivateOnEnter<EclipticMeteor>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 206, NameID = 5333)]
public class D80Gudanna : BossModule
{
    public D80Gudanna(WorldState ws, Actor primary) : base(ws, primary, new(-300, -220), new ArenaBoundsCircle(25))
    {
        ActivateComponent<Hints>();
    }
}
