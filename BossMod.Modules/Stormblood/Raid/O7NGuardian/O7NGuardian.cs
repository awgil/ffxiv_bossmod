namespace BossMod.Stormblood.Raid.O7NGuardian;

public enum OID : uint
{
    Boss = 0x20CB, // R4.200
    Helper = 0x18D6, // R0.500
    EventHelper = 0x233C, // R0.500
    Missile = 0x20CD, // R1.300
    Tentacle = 0x20D0, // R7.200
    Ultros = 0x20CF, // R5.100
    Bibliognost = 0x20BA, // R1.500
    AirForce = 0x20CC, // R3.500
    DemonPad = 0x1EA8D2, // R0.500, paired with 0x1EA8D4
    ChakraPad = 0x1EA92A, // R0.500
    Bomb = 0x1EA8CF, // R0.500
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/Ultros/AirForce->player, no cast, single-target
    Load = 10076, // Boss->self, 5.0s cast, single-target
    DiffractiveLaser = 10081, // Boss->self, 2.0s cast, range 10 circle
    MissileSimulation = 10084, // Boss->self, 3.0s cast, single-target
    MissileLaunch = 10758, // Missile->self, no cast, single-target
    DiffractivePlasma = 10094, // Boss->self, 4.0s cast, range 60 circle
    Explosion = 10085, // Missile->self, no cast, range 3 circle
    Shockwave = 10086, // Boss->self, 2.0s cast, range 60 circle, knockback 20
    Ink = 10077, // Helper->location, 3.0s cast, range 8 circle
    TentacleSimulation = 10078, // Boss->self, 3.0s cast, single-target
    Tentacle = 10079, // Tentacle->self, 3.0s cast, range 8 circle
    Wallop = 10080, // Tentacle->self, 3.0s cast, range 50+R width 10 rect
    RunProgram = 10095, // Boss->self, 4.0s cast, single-target
    ChainCannonStart = 10551, // Boss->self, no cast, single-target
    ChainCannon = 10096, // Boss->location, no cast, range 8 circle
    MainCannon = 10097, // Boss->location, 3.0s cast, range 60 circle, proximity
    ChainCannonEnd = 10552, // Boss->self, no cast, single-target
    DemonSimulation = 10066, // Boss->self, 15.0s cast, single-target
    BibliognostAuto = 10433, // Bibliognost->player, no cast, single-target
    BombDeployment = 10082, // Boss->self, 3.0s cast, single-target
    InterdimensionalExplosion = 10083, // Helper->self, no cast, range 50 circle
    BombCheck = 10721, // Helper->player, no cast, single-target
    MagitekRay = 10091, // Boss->self, 3.0s cast, range 40+R width 6 rect
    SuperChakraBurst = 10089, // Helper->self, no cast, range 60 circle
    ChakraBurst = 10090, // Helper->self, no cast, range 3 circle
    Prey = 10093, // Boss->player, no cast, single-target
    ArmAndHammer = 10092, // Boss->player, 4.0s cast, single-target
    DiffractiveLaserAirForce = 10047, // AirForce->self, 3.0s cast, range 60+R 90-degree cone
}

public enum IconID : uint
{
    Prey = 30, // player
}

class DiffractiveLaser(BossModule module) : Components.StandardAOEs(module, AID.DiffractiveLaser, 10);
class DiffractivePlasma(BossModule module) : Components.RaidwideCast(module, AID.DiffractivePlasma);
class Ink(BossModule module) : Components.StandardAOEs(module, AID.Ink, 8);
class TentacleAOE(BossModule module) : Components.StandardAOEs(module, AID.Tentacle, 8);
class Wallop(BossModule module) : Components.StandardAOEs(module, AID.Wallop, new AOEShapeRect(57.2f, 5));
class MagitekRay(BossModule module) : Components.StandardAOEs(module, AID.MagitekRay, new AOEShapeRect(44.2f, 3));
class AirForceLaser(BossModule module) : Components.StandardAOEs(module, AID.DiffractiveLaserAirForce, new AOEShapeCone(63.5f, 45.Degrees()));
class ArmAndHammer(BossModule module) : Components.SingleTargetCast(module, AID.ArmAndHammer);
class Shockwave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Shockwave, 20, shape: new AOEShapeCircle(60));

class Missiles(BossModule module) : Components.GenericAOEs(module, AID.Explosion)
{
    private static readonly AOEShapeCircle Body = new(3);
    private static readonly AOEShapeRect Lane = new(40, 3, 3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var missile in Module.Enemies(OID.Missile))
        {
            if (missile.IsDead)
                continue;
            // ensure the actual missile itself is always forbidden to prevent a chain cannon dodge from stepping into it
            yield return new(Body, missile.Position, Activation: WorldState.CurrentTime);
            yield return new(Lane, missile.Position, missile.Rotation, WorldState.CurrentTime);
        }
    }
}

class ChainCannon(BossModule module) : Components.GenericAOEs(module, AID.ChainCannon)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle Shape = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.IsSpell(AID.ChainCannon))
            _aoes.Add(new(Shape, spell.TargetXZ, Activation: WorldState.FutureTime(2.5f)));
    }

    public override void Update() => _aoes.RemoveAll(a => a.Activation <= WorldState.CurrentTime);
}

class MainCannon(BossModule module) : Components.ProximityAOEs(module, AID.MainCannon, 18);

class PreyMarker(BossModule module) : BossComponent(module)
{
    private DateTime _until;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Prey && actor == Raid.Player())
            _until = WorldState.FutureTime(5.2f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.IsSpell(AID.Prey))
            _until = default;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_until > WorldState.CurrentTime)
            hints.Add("Prey. Incoming damage and dot.");
    }
}

class SoakPads(BossModule module, uint oid, int need, string hint) : BossComponent(module)
{
    private const float Radius = 3;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Pads().Any() && !Pads().Any(p => actor.Position.InCircle(p.Position, Radius)))
            hints.Add(hint);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var pad in Pads())
        {
            if (!actor.Position.InCircle(pad.Position, Radius))
                continue;
            var others = Raid.WithoutSlot().Count(p => p != actor && !p.IsDead && p.Position.InCircle(pad.Position, Radius));
            if (others < need)
            {
                var stay = pad.Position;
                hints.GoalZones.Add(p => (p - stay).LengthSq() <= Radius * Radius ? 10 : 0);
                return;
            }
        }

        WPos? best = null;
        var bestDist = float.MaxValue;
        foreach (var pad in Pads())
        {
            var filled = Raid.WithoutSlot().Count(p => !p.IsDead && p.Position.InCircle(pad.Position, Radius));
            if (filled >= need)
                continue;
            var dist = (pad.Position - actor.Position).LengthSq();
            if (dist < bestDist)
            {
                bestDist = dist;
                best = pad.Position;
            }
        }
        if (best is { } pos)
            hints.GoalZones.Add(p => (p - pos).LengthSq() <= Radius * Radius ? 10 : 0);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var pad in Pads())
        {
            Arena.ZoneCircle(pad.Position, Radius, ArenaColor.SafeFromAOE);
            Arena.AddCircle(pad.Position, Radius, ArenaColor.Safe);
        }
    }

    private IEnumerable<Actor> Pads() => Module.Enemies(oid).Where(a => !a.IsDead);
}

class DemonPads(BossModule module) : BossComponent(module)
{
    private readonly List<Components.GenericTowers.Tower> _towers = [];
    private const float Radius = 3;
    private const float SoakerRadius = 1.5f;
    private const int Need = 3;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.DemonPad)
            _towers.Add(new(actor.Position, Radius));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.DemonPad)
            _towers.RemoveAll(t => t.Position.AlmostEqual(actor.Position, 0.5f));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_towers.Count > 0 && !_towers.Any(t => actor.Position.InCircle(t.Position, SoakerRadius)))
            hints.Add("Seal the pad");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_towers.Count == 0)
            return;

        var best = _towers[0];
        var bestCount = -1;
        foreach (var t in _towers)
        {
            var count = Raid.WithSlot().InRadius(t.Position, SoakerRadius).Count();
            if (count > bestCount)
            {
                bestCount = count;
                best = t;
            }
        }

        if (bestCount >= Need)
            _towers.Remove(best);
        else
            hints.AddForbiddenZone(new AOEShapeDonut(SoakerRadius, 50), best.Position);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in _towers)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddCircle(t.Position, t.Radius, 0xFF000000, 3);
            Arena.AddCircle(t.Position, t.Radius, t.ForbiddenSoakers[pcSlot] ? ArenaColor.Danger : ArenaColor.Safe, 2);
        }
    }
}

class ChakraPads(BossModule module) : SoakPads(module, (uint)OID.ChakraPad, 1, "Catch an orb");

class Bombs(BossModule module) : BossComponent(module)
{
    private const float Offset = 2.5f;
    private const float Radius = 2;
    private readonly Dictionary<ulong, float> _brightX = [];

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID != (uint)OID.Bomb)
            return;
        switch (state)
        {
            case 0x00104000: // E = defuse, W = bomb
                _brightX[actor.InstanceID] = Offset;
                break;
            case 0x01008000: // W = bomb, E = defuse
                _brightX[actor.InstanceID] = -Offset;
                break;
            case 0x00010200:
            case 0x00010002:
                _brightX.Remove(actor.InstanceID);
                break;
        }
    }

    public override void OnActorDestroyed(Actor actor) => _brightX.Remove(actor.InstanceID);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var bombs = Live();
        if (bombs.Count == 0)
            return;
        if (!bombs.Any(b => _brightX.TryGetValue(b.InstanceID, out var x) && actor.Position.InCircle(Wire(b, x), Radius)))
            hints.Add("Defuse the brighter wire");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        WPos? goal = null;
        var goalDist = float.MaxValue;
        foreach (var bomb in Live())
        {
            if (_brightX.TryGetValue(bomb.InstanceID, out var x))
            {
                var bright = Wire(bomb, x);
                hints.AddForbiddenZone(new AOEShapeCircle(Radius), Wire(bomb, -x));
                var dist = (actor.Position - bright).LengthSq();
                if (dist < goalDist)
                {
                    goalDist = dist;
                    goal = bright;
                }
            }
            else
            {
                hints.AddForbiddenZone(new AOEShapeCircle(Radius), Wire(bomb, Offset));
                hints.AddForbiddenZone(new AOEShapeCircle(Radius), Wire(bomb, -Offset));
            }
        }
        if (goal is { } spot)
            hints.GoalZones.Add(p => p.InCircle(spot, 1) ? 10 : 0);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var bomb in Live())
        {
            if (_brightX.TryGetValue(bomb.InstanceID, out var x))
            {
                Arena.AddCircle(Wire(bomb, x), Radius, ArenaColor.Safe);
                Arena.AddCircle(Wire(bomb, -x), Radius, ArenaColor.Danger);
            }
            else
            {
                Arena.AddCircle(Wire(bomb, Offset), Radius, ArenaColor.Danger);
                Arena.AddCircle(Wire(bomb, -Offset), Radius, ArenaColor.Danger);
            }
        }
    }

    private List<Actor> Live() => [.. Module.Enemies(OID.Bomb).Where(b => !b.IsDead)];

    private static WPos Wire(Actor bomb, float x) => bomb.Position + new WDir(x, 0);
}

class O7NGuardianStates : StateMachineBuilder
{
    public O7NGuardianStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DiffractiveLaser>()
            .ActivateOnEnter<DiffractivePlasma>()
            .ActivateOnEnter<Ink>()
            .ActivateOnEnter<TentacleAOE>()
            .ActivateOnEnter<Wallop>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<AirForceLaser>()
            .ActivateOnEnter<ArmAndHammer>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<Missiles>()
            .ActivateOnEnter<ChainCannon>()
            .ActivateOnEnter<MainCannon>()
            .ActivateOnEnter<PreyMarker>()
            .ActivateOnEnter<DemonPads>()
            .ActivateOnEnter<ChakraPads>()
            .ActivateOnEnter<Bombs>();
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 288, NameID = 7107)]
public class O7NGuardian(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsSquare(20));
