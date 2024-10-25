namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.D180Dendainsonne;

public enum OID : uint
{
    Boss = 0x181F, // R11.600, x1
    TornadoVoidZones = 0x18F0, // R1.000, x0 (spawn during fight)
    Actor1E86E0 = 0x1E86E0, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    CharybdisCast = 7163, // Boss->location, 3.0s cast, range 6 circle
    CharybdisTornado = 7164, // TornadoVoidZones->self, no cast, range 6 circle
    EclipticMeteor = 7166, // Boss->self, 6.0s cast, range 50 circle
    Maelstrom = 7167, // TornadoVoidZones->self, 1.3s cast, range 10 circle
    Thunderbolt = 7162, // Boss->self, 2.5s cast, range 5+R 120-degree cone
    Trounce = 7165, // Boss->self, 2.5s cast, range 40+R 60-degree cone
}

class Charybdis(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.CharybdisCast), 6);
class Maelstrom(BossModule module) : Components.PersistentVoidzone(module, 10, m => m.Enemies(OID.TornadoVoidZones));
class Trounce(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Trounce), new AOEShapeCone(51.6f, 30.Degrees()));
class EclipticMeteor(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.EclipticMeteor), "Kill him before he kills you! 80% max HP damage incoming!");
class Thunderbolt(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Thunderbolt), new AOEShapeCone(16.6f, 60.Degrees()));

class EncounterHints(BossModule module) : BossComponent(module)
{
    private int NumCast { get; set; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CharybdisCast or AID.Trounce or AID.Thunderbolt)
            ++NumCast;

        if ((AID)spell.Action.ID is AID.EclipticMeteor)
            NumCast = 11;

        if (NumCast == 10)
        {
            // reset the rotation back to the beginning. This loops continuously until you hit the hp threshold (15.0% here)
            NumCast = 0;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        switch (NumCast)
        {
            case 0:
                hints.Add("Thunderbolt -> Charybdis x2 -> Thunderbolt -> Trounce from South Wall.");
                break;
            case 1:
                hints.Add("Charybdis x2 -> Thunderbolt -> Trounce from South Wall.");
                break;
            case 2:
                hints.Add("Charybdis x1 -> Thunderbolt -> Trounce from South Wall!");
                break;
            case 3:
                hints.Add("Thunderbolt -> Trounce from South Wall!");
                break;
            case 4:
                hints.Add("Boss is running to the South wall to cast Trounce!");
                break;
            case 5:
                hints.Add("Charybdis x2 -> Thunderbolt -> Charybdis -> Trounce from North Wall.");
                break;
            case 6:
                hints.Add("Charybdis x1 -> Thunderbolt -> Charybdis -> Trounce from North Wall.");
                break;
            case 7:
                hints.Add("Thunderbolt -> Charybdis -> Trounce from North Wall!");
                break;
            case 8:
                hints.Add("Charybdis -> Trounce from North Wall!");
                break;
            case 9:
                hints.Add("Boss is running to the North wall to cast Trounce!");
                break;
        }
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will cast Trounce (Cone AOE) from the South and North wall. \nMake sure to stay near him to dodge the AOE. \n{Module.PrimaryActor.Name} will also cast Ecliptic Meteor at 15% HP, plan accordingly!");
    }
}

class D180DendainsonneStates : StateMachineBuilder
{
    public D180DendainsonneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<Charybdis>()
            .ActivateOnEnter<Maelstrom>()
            .ActivateOnEnter<Trounce>()
            .ActivateOnEnter<EclipticMeteor>()
            .ActivateOnEnter<EncounterHints>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 216, NameID = 5461)]
public class D180Dendainsonne : BossModule
{
    public D180Dendainsonne(WorldState ws, Actor primary) : base(ws, primary, new(-300, -300), new ArenaBoundsCircle(25))
    {
        ActivateComponent<Hints>();
    }
}
