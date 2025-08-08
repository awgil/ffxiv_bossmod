namespace BossMod.Dawntrail.Dungeon.D11MesoTerminal.D113ImmortalRemains;

public enum OID : uint
{
    Boss = 0x48BE,
    Helper = 0x233C,
    PreservedTerror = 0x48C0, // R0.500, x35
    BygoneAerostat = 0x48BF, // R2.300, x10
}

public enum AID : uint
{
    AutoAttack = 43826, // Boss->player, no cast, single-target
    Recollection = 43825, // Boss->self, 5.0s cast, range 60 circle
    Memento = 43809, // Boss->self, 4.0+1.0s cast, single-target
    Electray = 43810, // BygoneAerostat->self, 5.0s cast, range 45 width 8 rect
    MemoryOfTheStormCast = 43821, // Boss->self, 4.0+1.0s cast, single-target
    MemoryOfTheStormAOE = 43822, // Helper->self, no cast, range 60 width 12 rect
    BombardmentLarge = 43812, // Helper->location, 1.5s cast, range 14 circle
    BombardmentSmall = 43811, // Helper->location, 1.5s cast, range 3 circle
    TurmoilRightHand = 43814, // Boss->self, no cast, single-target
    TurmoilLeftHand = 43815, // Boss->self, no cast, single-target
    TurmoilHit = 43816, // Helper->self, no cast, range 40 width 20 rect
    ImpressionVisual = 43817, // Boss->self, no cast, single-target
    ImpressionAOE = 43818, // Helper->location, 5.0s cast, range 10 circle
    ImpressionKnockback = 43819, // Helper->location, 5.0s cast, range 30 circle
    MemoryOfThePyreCast = 43823, // Boss->self, 4.0+1.0s cast, single-target
    MemoryOfThePyre = 43824, // Helper->player, 5.0s cast, single-target
    KeraunographyPre = 43813, // Helper->self, 4.0s cast, single-target
    Keraunography = 45176, // Helper->self, 1.5s cast, range 60 width 20 rect
}

public enum IconID : uint
{
    Laser = 525, // Boss->player
}

class Recollection(BossModule module) : Components.RaidwideCast(module, AID.Recollection);

class Electray(BossModule module) : Components.StandardAOEs(module, AID.Electray, new AOEShapeRect(45f, 4f), highlightImminent: true);

class MemoryOfTheStorm(BossModule module) : Components.IconLineStack(module, 6, 60, (uint)IconID.Laser, AID.MemoryOfTheStormAOE, 6);

class ImpressionVoidzone(BossModule module) : Components.StandardAOEs(module, AID.ImpressionAOE, 10);
class ImpressionKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.ImpressionAOE, 11)
{
    private Bombardment? _bomb;

    private readonly List<Func<WPos, bool>> _safeSpots = [];

    public override void Update()
    {
        _bomb ??= Module.FindComponent<Bombardment>();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
        {
            _safeSpots.Clear();
            var largeAOEs = _bomb?.LargeAOEs.ToList() ?? [];

            for (var i = 0; i < 4; i++)
            {
                var toCorner = (45 + 90 * i).Degrees();
                if (!largeAOEs.Any(l => l.InCone(Arena.Center, toCorner, 20.Degrees())))
                    _safeSpots.Add(ShapeContains.Donut(Arena.Center + toCorner.ToDirection() * 12, 2, 100));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (spell.Action == WatchedAction)
            _bomb?.Risky = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
            if (!IsImmune(slot, src.Activation) && _safeSpots.Count > 0)
                hints.AddForbiddenZone(ShapeContains.Intersection(_safeSpots), src.Activation);
    }
}

class Turmoil(BossModule module) : Components.GenericAOEs(module, AID.TurmoilHit)
{
    private AOEInstance? _predicted;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_predicted);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TurmoilRightHand:
                _predicted = new(new AOEShapeRect(40, 10), caster.Position - new WDir(10, 0), default, WorldState.FutureTime(4.4f));
                break;
            case AID.TurmoilLeftHand:
                _predicted = new(new AOEShapeRect(40, 10), caster.Position + new WDir(10, 0), default, WorldState.FutureTime(4.4f));
                break;
            case AID.TurmoilHit:
                NumCasts++;
                _predicted = null;
                break;
        }
    }
}

class Bombardment(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _tethered = [];
    private readonly List<Actor> _active = [];

    private readonly List<AOEInstance> _aoes = [];

    public IEnumerable<WPos> LargeAOEs => _aoes.Where(a => ((AOEShapeCircle)a.Shape).Radius == 14).Select(a => a.Origin);

    public bool Risky = true;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Select(a => a with { Risky = Risky });

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.PreservedTerror)
            _tethered.Add(source);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.PreservedTerror && _tethered.Count > 0)
        {
            if (_tethered.Count == 8)
            {
                Risky = false; // will be toggled back on by the ImpressionKnockback component
                Predict(10.8f);
            }

            // 6 tethers is the first "tutorial" iteration of this mechanic
            else if (_tethered.Count == 6 && _active.Count > 6)
                // TODO: do these actually have the same delay?
                Predict(10.8f);

            // otherwise: <4 tethers is Electray, 6 tethers with only 6 active mobs is keraunography
            _tethered.Clear();
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((OID)actor.OID == OID.PreservedTerror && status.ID == 2552)
            _active.Add(actor);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((OID)actor.OID == OID.PreservedTerror && status.ID == 2552)
            _active.Remove(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BombardmentSmall or AID.BombardmentLarge)
            _aoes.RemoveAll(a => a.Origin.AlmostEqual(spell.TargetXZ, 0.5f));
    }

    private void Predict(float delay)
    {
        foreach (var t in _tethered)
        {
            var groupCenter = t.Position + t.Rotation.ToDirection() * 3.5f;
            if (_active.Count(a => a.Position.InCircle(groupCenter, 4)) >= 5)
                _aoes.Add(new(new AOEShapeCircle(14), groupCenter, default, WorldState.FutureTime(delay)));
            else
                _aoes.Add(new(new AOEShapeCircle(3), t.Position, t.Rotation, WorldState.FutureTime(delay)));
        }
    }
}

class Keraunography(BossModule module) : Components.GenericAOEs(module, AID.Keraunography)
{
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.KeraunographyPre)
        {
            _aoes.Add(new(new AOEShapeRect(60, 10), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 3)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _aoes.RemoveAt(0);
        }
    }
}

class D113ImmortalRemainsStates : StateMachineBuilder
{
    public D113ImmortalRemainsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Recollection>()
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<MemoryOfTheStorm>()
            .ActivateOnEnter<ImpressionVoidzone>()
            .ActivateOnEnter<ImpressionKnockback>()
            .ActivateOnEnter<Keraunography>()
            .ActivateOnEnter<Turmoil>()
            .ActivateOnEnter<Bombardment>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "erdelf, xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1028, NameID = 13974)]
public class D113ImmortalRemains(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsSquare(20));

