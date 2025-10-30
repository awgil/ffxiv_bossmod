namespace BossMod.Dawntrail.Foray.CriticalEngagement.NeoGarula;

public enum OID : uint
{
    Boss = 0x469E, // R5.000, x1
    Helper = 0x233C, // R0.500, x20, Helper type
    DeathWallHelper = 0x4864, // R0.500, x1
    Chatterbird = 0x469F, // R0.900, x6 (spawn during fight)
}

public enum AID : uint
{
    DeathWall = 41259, // DeathWallHelper->self, no cast, range ?-30 donut
    AutoAttack = 872, // Boss->player, no cast, single-target
    Squash = 41189, // Boss->player, 5.0s cast, single-target
    LightningCrossingVisual1 = 41182, // Boss->self, 3.5s cast, single-target
    LightningCrossingVisual2 = 41183, // Boss->self, 3.5s cast, single-target
    LightningCrossingCast = 41184, // Helper->self, 3.7s cast, range 70 ?-degree cone
    Heave = 43262, // Boss->self, 4.0s cast, range 60 120-degree cone
    AgitatedGroanCast = 41188, // Boss->self, 5.0s cast, ???
    AgitatedGroan = 41190, // Helper->self, no cast, ???
    RushingRumble = 41175, // Boss->self, 6.0s cast, single-target
    RushingRumbleRampage = 41177, // Boss->self, 6.0s cast, single-target
    Rush = 41178, // Boss->location, no cast, width 8 rect charge
    BirdUnk = 41181, // Chatterbird->self, no cast, single-target
    Rumble = 41179, // Boss->self, 1.0s cast, range 30 circle
    LightningCrossingInstant = 42984, // Helper->self, no cast, range 70 ?-degree cone
    BirdserkRush = 41176, // Boss->self, 6.0s cast, single-target
    HeaveFast = 41180, // Boss->self, 2.0s cast, range 60 120-degree cone
    LightningChargeVisual = 41185, // Boss->self, 5.0s cast, single-target
    EpicenterShock = 41186, // Helper->self, 5.0s cast, range 12 circle
    MammothBolt = 41187, // Helper->self, 6.0s cast, range 50 circle
}

public enum SID : uint
{
    LightningCrossing = 2193
}

public enum IconID : uint
{
    Tankbuster = 218, // player->self
    Exclamation = 578, // Chatterbird->self
}

public enum TetherID : uint
{
    Purple = 17, // Boss->Chatterbird
}

class Squash(BossModule module) : Components.SingleTargetCast(module, AID.Squash);
class HeaveSlow(BossModule module) : Components.StandardAOEs(module, AID.Heave, new AOEShapeCone(60, 60.Degrees()));
class LightningCrossingSlow(BossModule module) : Components.StandardAOEs(module, AID.LightningCrossingCast, new AOEShapeCone(70, 22.5f.Degrees()), maxCasts: 4);
class AgitatedGroan(BossModule module) : Components.RaidwideCast(module, AID.AgitatedGroanCast);

class LightningCrossingRush(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _source;
    private Angle _currentRotation;
    private readonly List<AOEInstance> _predicted = [];
    private WPos? _prevDestination;
    private readonly List<(Actor, DateTime)> _icons = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var numAOEs = _predicted.Count;
        var groupSize = numAOEs % 6;
        if (groupSize == 0)
            groupSize = 6;
        return _predicted.Take(groupSize);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Exclamation)
        {
            if (_source != null)
                AddTarget(actor, WorldState.CurrentTime);
            else
                _icons.Add((actor, WorldState.CurrentTime));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RushingRumble or AID.RushingRumbleRampage)
        {
            _source = caster;
            foreach (var (actor, appear) in _icons)
                if (appear.AddSeconds(5) > WorldState.CurrentTime)
                    AddTarget(actor, appear);
            _icons.Clear();
        }

        if ((AID)spell.Action.ID == AID.Rumble && _predicted.Count > 0)
            _predicted.Ref(0).Origin = spell.LocXZ;
    }

    private void AddTarget(Actor actor, DateTime highlightTime)
    {
        if (_source is not { } src)
            return;

        var srcPos = _prevDestination ?? src.Position;

        var borderPos = (actor.Position - Arena.Center).Normalized() * 23;
        var destination = Arena.Center + borderPos;
        var chargeDir = destination - srcPos;
        var chargeDist = chargeDir.Length();
        var rotation = Angle.FromDirection(chargeDir);
        _prevDestination = destination;

        // charge AOE
        _predicted.Add(new AOEInstance(new AOEShapeRect(chargeDist, 4), srcPos, rotation, highlightTime.AddSeconds(6.4f), Color: ArenaColor.Danger));

        // rumble
        _predicted.Add(new AOEInstance(new AOEShapeCircle(30), destination, rotation, highlightTime.AddSeconds(9.4f)));

        for (var i = 0; i < 4; i++)
        {
            var rotNext = rotation + _currentRotation + (90 * i).Degrees();
            _predicted.Add(new AOEInstance(new AOEShapeCone(70, 22.5f.Degrees()), destination, rotNext, highlightTime.AddSeconds(10.4f)));
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.LightningCrossing)
        {
            switch (status.Extra)
            {
                // TODO verify, i'm literally just guessing
                case 350:
                case 848:
                    _currentRotation = default;
                    break;
                case 351:
                case 849:
                    _currentRotation = 45.Degrees();
                    break;
                default:
                    ReportError($"unrecognized extra for status 2193: {status.Extra}");
                    _currentRotation = default;
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Rumble)
        {
            NumCasts++;
            if (_predicted.Count > 0 && _predicted[0].Shape is AOEShapeCircle)
                _predicted.RemoveAt(0);
        }

        if ((AID)spell.Action.ID == AID.Rush)
        {
            NumCasts++;
            if (_predicted.Count > 0 && _predicted[0].Shape is AOEShapeRect)
                _predicted.RemoveAt(0);
        }

        if ((AID)spell.Action.ID == AID.LightningCrossingInstant)
        {
            NumCasts++;
            if (_predicted.Count > 0 && _predicted[0].Shape is AOEShapeCone)
                _predicted.RemoveAt(0);

            if (_predicted.Count == 0)
            {
                _prevDestination = null;
                _source = null;
            }
        }
    }
}

class BirdserkRush(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _target;
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_target is { } t && _activation != default)
            yield return new AOEInstance(new AOEShapeRect(50, 4), Module.PrimaryActor.Position, Module.PrimaryActor.AngleTo(t), _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BirdserkRush)
        {
            _target = Module.Enemies(OID.Chatterbird).FirstOrDefault(c => c.FindStatus(2056) != null);
            if (_target == null)
                ReportError("unable to find target bird");
            else
                _activation = Module.CastFinishAt(spell, 0.15f);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // preposition near charge location since we don't know which side he's going to face
        if (_target is { } t && _activation != default)
            hints.AddForbiddenZone(ShapeContains.Donut(t.Position, 15, 60), _activation.AddSeconds(4));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_target != null && _activation != default)
            hints.Add("Preposition near bird!", false);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Rush && _target != null)
        {
            NumCasts++;
            _activation = default;
        }
    }
}
class BirdserkPredict(BossModule module) : Components.GenericAOEs(module)
{
    private bool _active;
    private Actor? _bird;
    private WPos _dashLocation;
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default && _bird is { } bird)
        {
            // "cheat" the cone slightly wider since the boss doesn't cast directly at the bird, rather a little closer to the center of the arena
            yield return new AOEInstance(new AOEShapeCone(60, 64.Degrees()), _dashLocation, Angle.FromDirection(bird.Position - _dashLocation), _activation);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (_active && (TetherID)tether.ID == TetherID.Purple && (OID)source.OID == OID.Boss && WorldState.Actors.Find(tether.Target) is { } bird)
            _bird = bird;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BirdserkRush)
            _active = true;

        if ((AID)spell.Action.ID == AID.HeaveFast)
        {
            _active = false;
            _activation = default;
            _bird = default;
            _dashLocation = default;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_active && (AID)spell.Action.ID == AID.Rush)
        {
            _dashLocation = spell.TargetXZ;
            _activation = WorldState.FutureTime(4.2f);
        }
    }
}
class HeaveFast(BossModule module) : Components.StandardAOEs(module, AID.HeaveFast, new AOEShapeCone(60, 60.Degrees()));
class MammothBolt(BossModule module) : Components.StandardAOEs(module, AID.MammothBolt, 30);
class EpicenterShock(BossModule module) : Components.StandardAOEs(module, AID.EpicenterShock, 12);

class NeoGarulaStates : StateMachineBuilder
{
    public NeoGarulaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Squash>()
            .ActivateOnEnter<AgitatedGroan>()
            .ActivateOnEnter<HeaveSlow>()
            .ActivateOnEnter<LightningCrossingSlow>()
            .ActivateOnEnter<LightningCrossingRush>()
            .ActivateOnEnter<BirdserkRush>()
            .ActivateOnEnter<BirdserkPredict>()
            .ActivateOnEnter<HeaveFast>()
            .ActivateOnEnter<MammothBolt>()
            .ActivateOnEnter<EpicenterShock>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13638)]
public class NeoGarula(WorldState ws, Actor primary) : BossModule(ws, primary, new(461, -363), new ArenaBoundsCircle(23))
{
    public override bool DrawAllPlayers => true;
}
