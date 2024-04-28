namespace BossMod.Endwalker.Extreme.Ex3Endsigner;

// used both for single planets (elegeia) and successions (fatalism)
class Planets(BossModule module) : BossComponent(module)
{
    private Actor? _head;
    private readonly List<WPos> _planetsFiery = [];
    private readonly List<WPos> _planetsAzure = [];

    private static readonly AOEShapeCone _aoeHead = new(20, 90.Degrees());
    private static readonly AOEShapeCircle _aoePlanet = new(30);
    private const float _knockbackDistance = 25;
    private const float _planetOffset = 19.8f; // == 14 * sqrt(2)

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoeHead.Check(actor.Position, _head))
        {
            hints.Add("GTFO from head aoe!");
        }
        if (_planetsFiery.Count > 0 && _aoePlanet.Check(actor.Position, _planetsFiery[0]))
        {
            hints.Add("GTFO from planet aoe!");
        }
        if (_planetsAzure.Count > 0)
        {
            var offsetLocation = Components.Knockback.AwayFromSource(actor.Position, _planetsAzure[0], _knockbackDistance);
            if (!Module.InBounds(offsetLocation))
            {
                hints.Add("About to be knocked into wall!");
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        _aoeHead.Draw(Arena, _head);
        if (_planetsFiery.Count > 0)
        {
            _aoePlanet.Draw(Arena, _planetsFiery[0]);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_planetsAzure.Count > 0)
        {
            var offsetLocation = Components.Knockback.AwayFromSource(pc.Position, _planetsAzure[0], _knockbackDistance);
            Arena.AddLine(pc.Position, offsetLocation, ArenaColor.Danger);
            Arena.Actor(offsetLocation, pc.Rotation, ArenaColor.Danger);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DiairesisElegeia)
            _head = caster;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_head == caster)
            _head = null;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FatalismFieryStar1:
                AddPlanet(caster, false, true);
                break;
            case AID.FatalismFieryStar2:
            case AID.FieryStarVisual:
                AddPlanet(caster, false, false);
                break;
            case AID.FatalismAzureStar1:
                AddPlanet(caster, true, true);
                break;
            case AID.FatalismAzureStar2:
            case AID.AzureStarVisual:
                AddPlanet(caster, true, false);
                break;
            case AID.RubistellarCollision:
            case AID.FatalismRubistallarCollisionAOE:
                if (_planetsFiery.Count > 0)
                    _planetsFiery.RemoveAt(0);
                else
                    ReportError("Unexpected fiery cast, no casters available");
                break;
            case AID.CaerustellarCollision:
            case AID.FatalismCaerustallarCollisionAOE:
                if (_planetsAzure.Count > 0)
                    _planetsAzure.RemoveAt(0);
                else
                    ReportError("Unexpected azure cast, no casters available");
                break;
        }
    }

    private void AddPlanet(Actor caster, bool azure, bool firstOfPair)
    {
        var origin = Module.Center + _planetOffset * caster.Rotation.ToDirection();
        var planets = azure ? _planetsAzure : _planetsFiery;
        int index = firstOfPair ? 0 : planets.Count;
        planets.Insert(index, origin);
    }
}
