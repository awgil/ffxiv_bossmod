namespace BossMod.Endwalker.Dungeon.D13LunarSubterrane.D132DamcyanAntlion;

public enum OID : uint
{
    Boss = 0x4022, // R=7.5
    StonePillar = 0x4023, // R=3.0
    StonePillar2 = 0x3FD1, // R=1.5
    QuicksandVoidzone = 0x1EB90E,
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 872, // Boss, no cast, single-target
    Sandblast = 34813, // Boss->self, 5.0s cast, range 60 circle
    Landslip = 34818, // Boss->self, 7.0s cast, single-target
    Landslip2 = 34819, // Helper->self, 7.7s cast, range 40 width 10 rect, knockback dir 20 forward
    Teleport = 34824, // Boss->location, no cast, single-target
    AntilonMarchTelegraph = 35871, // Helper->location, 1.5s cast, width 8 rect charge
    AntlionMarch = 34816, // Boss->self, 5.5s cast, single-target
    AntlionMarch2 = 34817, // Boss->location, no cast, width 8 rect charge
    Towerfall = 34820, // StonePillar->self, 2.0s cast, range 40 width 10 rect
    EarthenGeyser = 34821, // Boss->self, 4.0s cast, single-target
    EarthenGeyser2 = 34822, // Helper->players, 5.0s cast, range 10 circle
    PoundSand = 34443, // Boss->location, 6.0s cast, range 12 circle
}

class Sandblast(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Sandblast));

class SandblastVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(19.5f, 5, 19.5f);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Sandblast && Module.Arena.Bounds == D132DamcyanAntlion.startingBounds)
        {
            _aoes.Add(new(rect, Module.Center + new WDir(0, -22.5f), 90.Degrees(), spell.NPCFinishAt));
            _aoes.Add(new(rect, Module.Center + new WDir(0, 22.5f), 90.Degrees(), spell.NPCFinishAt));
        }
    }
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x00)
        {
            Module.Arena.Bounds = D132DamcyanAntlion.defaultBounds;
            _aoes.Clear();
        }
    }
}

class Landslip(BossModule module) : Components.Knockback(module)
{
    public bool TowerDanger;
    private readonly List<Actor> _casters = [];
    public DateTime Activation;
    private static readonly AOEShapeRect rect = new(40, 5);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var c in _casters)
            yield return new(c.Position, 20, Activation, rect, c.Rotation, Kind.DirForward);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Landslip2)
        {
            Activation = spell.NPCFinishAt;
            _casters.Add(caster);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Landslip2)
        {
            _casters.Remove(caster);
            if (++NumCasts > 4)
                TowerDanger = true;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var length = Module.Bounds.Radius * 2; // casters are at the border, orthogonal to borders
        foreach (var c in _casters)
            hints.AddForbiddenZone(ShapeDistance.Rect(c.Position, c.Rotation, length, 20 - length, 5), Activation);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<Towerfall>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.InBounds(pos);
}

class EarthenGeyser(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.EarthenGeyser2), 10);
class QuicksandVoidzone(BossModule module) : Components.PersistentVoidzone(module, 10, m => m.Enemies(OID.QuicksandVoidzone).Where(z => z.EventState != 7));
class PoundSand(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.PoundSand), 12);

class AntlionMarch(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return new(_aoes[0].Shape, _aoes[0].Origin, _aoes[0].Rotation, _activation, ArenaColor.Danger);
        for (var i = 1; i < _aoes.Count; ++i)
            yield return new(_aoes[i].Shape, _aoes[i].Origin, _aoes[i].Rotation, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AntilonMarchTelegraph)
        {
            var dir = spell.LocXZ - caster.Position;
            _aoes.Add(new(new AOEShapeRect(dir.Length(), 4.5f), caster.Position, Angle.FromDirection(dir))); // actual charge is only 4 halfwidth, but the telegraphs and actual AOEs can be in different positions by upto 0.5y according to my logs
        }
        if ((AID)spell.Action.ID == AID.AntlionMarch)
            _activation = spell.NPCFinishAt.AddSeconds(0.2f); //since these are charges of different length with 0s cast time, the activation times are different for each and there are different patterns, so we just pretend that they all start after the telegraphs end
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.AntlionMarch2)
            _aoes.RemoveAt(0);
    }
}

class Towerfall(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(40, 5);
    private static readonly Angle _rot1 = 89.999f.Degrees();
    private static readonly Angle _rot2 = -90.004f.Degrees();
    private static readonly Dictionary<byte, (WPos position, Angle direction)> _towerPositions = new()
        {{ 0x01, (new WPos(-20, 45), _rot1) },
        { 0x02, (new WPos(-20, 55), _rot1) },
        { 0x03, (new WPos(-20, 65), _rot1) },
        { 0x04, (new WPos(-20, 75), _rot1) },
        { 0x05, (new WPos(20, 45), _rot2) },
        { 0x06, (new WPos(20, 55), _rot2) },
        { 0x07, (new WPos(20, 65), _rot2) },
        { 0x08, (new WPos(20, 75), _rot2) }};

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var t in _aoes)
            yield return new(rect, t.Origin, t.Rotation, Module.FindComponent<Landslip>()!.Activation.AddSeconds(0.7f), Risky: Module.FindComponent<Landslip>()!.TowerDanger);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && _towerPositions.TryGetValue(index, out var value))
        {
            var towers = value;
            _aoes.Add(new(rect, towers.position, towers.direction));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Towerfall)
        {
            _aoes.Clear();
            Module.FindComponent<Landslip>()!.TowerDanger = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.FindComponent<Landslip>()!.Sources(slot, actor).Any())
        {
            var forbiddenInverted = new List<Func<WPos, float>>();
            var forbidden = new List<Func<WPos, float>>();
            if (_aoes.Count == 2)
            {
                var distance = Math.Abs(_aoes[0].Origin.Z - _aoes[1].Origin.Z);
                if (distance is 10 or 30)
                    foreach (var t in _aoes)
                        forbiddenInverted.Add(ShapeDistance.InvertedRect(t.Origin, t.Rotation, 40, 0, 5));
                else
                    foreach (var t in _aoes)
                        forbidden.Add(ShapeDistance.Rect(t.Origin, t.Rotation, 40, 0, 5));
            }
            var activation = Module.FindComponent<Landslip>()!.Activation.AddSeconds(0.7f);
            if (forbiddenInverted.Count > 0)
                hints.AddForbiddenZone(p => forbiddenInverted.Select(f => f(p)).Max(), activation);
            if (forbidden.Count > 0)
                hints.AddForbiddenZone(p => forbidden.Select(f => f(p)).Min(), activation);
        }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class StayInBounds(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Module.InBounds(actor.Position))
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.Center, 3));
    }
}

class D132DamcyanAntlionStates : StateMachineBuilder
{
    public D132DamcyanAntlionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StayInBounds>()
            .ActivateOnEnter<SandblastVoidzone>()
            .ActivateOnEnter<Sandblast>()
            .ActivateOnEnter<Landslip>()
            .ActivateOnEnter<EarthenGeyser>()
            .ActivateOnEnter<QuicksandVoidzone>()
            .ActivateOnEnter<PoundSand>()
            .ActivateOnEnter<AntlionMarch>()
            .ActivateOnEnter<Towerfall>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 823, NameID = 12484)]
public class D132DamcyanAntlion(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 60), startingBounds)
{
    public static readonly ArenaBounds startingBounds = new ArenaBoundsRect(19.5f, 25);
    public static readonly ArenaBounds defaultBounds = new ArenaBoundsRect(19.5f, 20);
}
