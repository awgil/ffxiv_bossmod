namespace BossMod.Heavensward.DeepDungeon.PalaceOfTheDead.D80Gudanna;

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

class Charybdis(BossModule module) : Components.StandardAOEs(module, AID.Charybdis, 6);
class Maelstrom(BossModule module) : Components.PersistentVoidzone(module, 10, m => m.Enemies(OID.Tornado));
class Trounce(BossModule module) : Components.StandardAOEs(module, AID.Trounce, new AOEShapeCone(51.6f, 30.Degrees()));
class EclipticMeteor(BossModule module) : Components.CastHint(module, AID.EclipticMeteor, "Kill him before he kills you! 80% max HP damage incoming!");
class Thunderbolt(BossModule module) : Components.StandardAOEs(module, AID.Thunderbolt, new AOEShapeCone(16.6f, 60.Degrees()));

class EncounterHints(BossModule module) : BossComponent(module)
{
    private bool _disabled;

    enum BossAction
    {
        None,
        Thunderbolt,
        Charybdis,
        TrounceWest,
        TrounceEast
    }

    private int NumCast { get; set; }
    private BossAction NextAction => NumCast switch
    {
        0 or 2 or 5 => BossAction.Thunderbolt,
        1 or 4 or 6 => BossAction.Charybdis,
        3 => BossAction.TrounceEast,
        7 => BossAction.TrounceWest,
        _ => default
    };

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_disabled)
            return;

        switch (NextAction)
        {
            case BossAction.Charybdis:
                // drop charybdis near arena edge (22y or more from center)
                hints.GoalZones.Add(p => (p - Module.Center).LengthSq() >= 484 ? 0.5f : 0);
                break;
            case BossAction.Thunderbolt:
                // stay near boss to make thunderbolt dodge easier
                hints.GoalZones.Add(hints.GoalSingleTarget(Module.PrimaryActor.Position, 6, 0.5f));
                break;
            case BossAction.TrounceEast:
                // stay in eastward 1/4th of arena to prepare for dodging trounce
                hints.GoalZones.Add(p => (p.X - Module.Center.X) >= 20 ? 0.5f : 0);
                break;
            case BossAction.TrounceWest:
                // see above
                hints.GoalZones.Add(p => (p.X - Module.Center.X) <= -20 ? 0.5f : 0);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_disabled)
            return;

        if ((AID)spell.Action.ID is AID.EclipticMeteor)
            _disabled = true;

        if ((AID)spell.Action.ID is AID.Charybdis or AID.Trounce or AID.Thunderbolt)
            ++NumCast;

        if (NumCast == 8)
        {
            // reset the rotation back to the beginning. This loops continuously until you hit the hp threshold (10.0% here)
            NumCast = 0;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_disabled)
            return;

        switch (NumCast)
        {
            case 0:
                hints.Add("Thunderbolt -> Charybdis -> Thunderbolt -> Trounce from East Wall");
                break;
            case 1:
                hints.Add("Charybdis -> Thunderbolt -> Trounce from East Wall");
                break;
            case 2:
                hints.Add("Thunderbolt -> Trounce from East Wall");
                break;
            case 3:
                hints.Add("Trounce from East Wall");
                break;
            case 4:
                hints.Add("Charybdis -> Thunderbolt -> Charybdis -> Trounce from West Wall.");
                break;
            case 5:
                hints.Add("Thunderbolt -> Charybdis -> Trounce from West Wall.");
                break;
            case 6:
                hints.Add("Charybdis -> Trounce from West Wall!");
                break;
            case 7:
                hints.Add("Trounce from West Wall!");
                break;
            default:
                break;
        }
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will cast Trounce (Cone AOE) from the East and West wall. \nMake sure to stay near him to dodge the AOE. \n{Module.PrimaryActor.Name} will also cast Ecliptic Meteor at 10% HP, plan accordingly!");
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
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<EncounterHints>();
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
