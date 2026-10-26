namespace BossMod.Dawntrail.Dungeon.D07StrayboroughDeadwalk.D073Traumerei;

public enum OID : uint
{
    Boss = 0x421F, // R26.000, x1
    Helper = 0x233C, // R0.500, x?, Helper type
    StrayGeist = 0x4221, // R2.000, x0 (spawn during fight)
    StrayPhantagenitrix = 0x4220, // R1.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 16764, // Boss->player, no cast, single-target
    BitterRegretVisual = 37140, // Boss->self, 6.0s cast, single-target, visual
    BitterRegret1 = 37139, // Boss->self, 6.0+0.7s cast, range 40 width 16 rect
    BitterRegret2 = 37147, // Helper->self, 6.7s cast, range 50 width 12 rect
    BitterRegret3 = 37340, // StrayPhantagenitrix->self, 6.0s cast, range 40 width 4 rect
    Poltergeist = 37132, // Boss->self, 3.0s cast, single-target, visual
    MemorialMarch1 = 37136, // Boss->self, 3.0s cast, single-target, visual
    MemorialMarch2 = 37065, // Boss->self, 6.0s cast, single-target, visual
    Impact = 37133, // Helper->self, 6.0s cast, range 40 width 4 rect
    IllIntent = 39607, // StrayGeist->player, 10.0s cast, single-target, tether stretch
    MaliciousMistTether = 37138, // StrayGeist->player, 10.0s cast, single-target, tether stretch
    GhostdusterSpreadVisual = 37145, // Boss->self, 8.0s cast, single-target, visual (spreads; be flesh)
    Ghostduster = 37146, // Helper->player, 8.0s cast, range 8 circle
    MaliciousMistRaidwide = 37168, // Boss->self, 5.0s cast, range 60 circle, raidwide
    Fleshbuster = 37148, // Boss->self, 8.0s cast, range 60 circle, raidwide (be ghost)
    GhostcrusherVisual = 37142, // Boss->self, 5.0s cast, single-target, visual (line stack)
    GhostcrusherMarker = 37144, // Helper->player, no cast, single-target
    Ghostcrusher = 37143, // Helper->self, no cast, range 80 width 8 rect
}

public enum SID : uint
{
    GhostlyGuise = 3949, // none->player
}

class ImpactArenaChange(BossModule module) : BossComponent(module)
{
    private bool _active;
    private GhostlyGuise? _ghost;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x0B)
            return;
        switch (state)
        {
            case 0x00800040:
                _active = true;
                break;
            case 0x00080004:
                _active = false;
                Arena.Bounds = new ArenaBoundsSquare(19.5f);
                break;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!_active)
            return;
        _ghost ??= Module.FindComponent<GhostlyGuise>();
        var ghostly = _ghost?.Ghostly[pcSlot] ?? false;
        var isSquare = Arena.Bounds is ArenaBoundsSquare;
        if (ghostly && !isSquare)
            Arena.Bounds = new ArenaBoundsSquare(19.5f);
        else if (!ghostly && isSquare)
            Arena.Bounds = D073Traumerei.CrossArena;
    }
}

class GhostlyGuise(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _circle = new(3);
    private static readonly WPos[] _platforms = [new(137.5f, -443.5f), new(158.5f, -443.5f), new(137.5f, -422.5f), new(158.5f, -422.5f)];
    private readonly AOEInstance[] _circles = new AOEInstance[4];
    private bool _activated;
    private bool _fleshbuster;
    private DateTime _activation;
    private DateTime _fleshAct;
    private bool _risky = true;
    private Func<WPos, float>? _safeZones;
    private Ghostduster? _avoid;
    private IllIntentTethers? _seek;

    public BitMask Ghostly;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_activated)
            yield break;
        _avoid ??= Module.FindComponent<Ghostduster>();
        _seek ??= Module.FindComponent<IllIntentTethers>();
        if (_avoid is { Spreads.Count: > 0 } a)
        {
            _risky = !Ghostly[slot];
            _activation = a.Spreads[0].Activation;
        }
        else if (_fleshbuster)
        {
            _risky = Ghostly[slot];
            _activation = _fleshAct;
        }
        else if (_seek is { Active: true })
            _risky = Ghostly[slot];
        else
            _risky = true;

        var color = _risky ? ArenaColor.AOE : ArenaColor.SafeFromAOE;
        for (var i = 0; i < 4; ++i)
        {
            _circles[i].Color = color;
            _circles[i].Activation = _activation;
            yield return _circles[i];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state != 0x00020001 || index != 0x0C)
            return;
        _activated = true;
        var zones = new List<Func<WPos, float>>(4);
        for (var i = 0; i < 4; ++i)
        {
            var pos = _platforms[i];
            _circles[i] = new(_circle, pos);
            zones.Add(ShapeDistance.InvertedCircle(pos, 3));
        }
        _safeZones = ShapeDistance.Intersection(zones);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Fleshbuster)
        {
            _fleshbuster = true;
            _fleshAct = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Fleshbuster)
            _fleshbuster = false;
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if (status.ID == (uint)SID.GhostlyGuise)
            Ghostly.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if (status.ID == (uint)SID.GhostlyGuise)
            Ghostly.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        _avoid ??= Module.FindComponent<Ghostduster>();
        _seek ??= Module.FindComponent<IllIntentTethers>();
        if (_fleshbuster || _seek is { Active: true })
            hints.Add("Turn into a ghost!", !Ghostly[slot]);
        else if (_avoid is { Spreads.Count: > 0 })
            hints.Add("Turn into flesh!", Ghostly[slot]);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_risky && _safeZones != null)
            hints.AddForbiddenZone(_safeZones, _activation);
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class IllIntentTethers(BossModule module) : BossComponent(module)
{
    private readonly List<(Actor Source, Actor Target)> _tethers = [];
    private GhostlyGuise? _ghost;

    public bool Active => _tethers.Count > 0;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.StrayGeist && WorldState.Actors.Find(tether.Target) is { Type: ActorType.Player } target)
            _tethers.Add((source, target));
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        _tethers.RemoveAll(t => t.Source == source);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.IllIntent or AID.MaliciousMistTether)
            _tethers.RemoveAll(t => t.Source == caster);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_tethers.Any(t => t.Target == actor) && actor.Position.InCircle(_tethers.First(t => t.Target == actor).Source.Position, 20))
            hints.Add("Stretch tether!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        _ghost ??= Module.FindComponent<GhostlyGuise>();
        if (_ghost == null || !_ghost.Ghostly[slot])
            return;
        foreach (var (source, target) in _tethers)
            if (target == actor)
                hints.AddForbiddenZone(ShapeDistance.Circle(source.Position, 20), WorldState.FutureTime(10));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (source, target) in _tethers)
        {
            if (target != pc)
                continue;
            var stretched = !pc.Position.InCircle(source.Position, 20);
            Arena.AddLine(source.Position, pc.Position, stretched ? ArenaColor.Safe : ArenaColor.Danger);
        }
    }
}

class MaliciousMistRaidwide(BossModule module) : Components.RaidwideCast(module, AID.MaliciousMistRaidwide);
class Fleshbuster(BossModule module) : Components.RaidwideCast(module, AID.Fleshbuster);
class BitterRegret1(BossModule module) : Components.StandardAOEs(module, AID.BitterRegret1, new AOEShapeRect(40, 8));
class BitterRegret2(BossModule module) : Components.StandardAOEs(module, AID.BitterRegret2, new AOEShapeRect(50, 6));
class BitterRegret3(BossModule module) : Components.StandardAOEs(module, AID.BitterRegret3, new AOEShapeRect(40, 2), maxCasts: 5);
class Impact(BossModule module) : Components.StandardAOEs(module, AID.Impact, new AOEShapeRect(40, 2));
class Ghostcrusher(BossModule module) : Components.SimpleLineStack(module, 4, 80, AID.GhostcrusherMarker, AID.Ghostcrusher, 5);

class Ghostduster(BossModule module) : Components.SpreadFromCastTargets(module, AID.Ghostduster, 8)
{
    private GhostlyGuise? _ghost;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        _ghost ??= Module.FindComponent<GhostlyGuise>();
        if (_ghost == null || !_ghost.Ghostly[slot])
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class D073TraumereiStates : StateMachineBuilder
{
    public D073TraumereiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IllIntentTethers>()
            .ActivateOnEnter<Ghostduster>()
            .ActivateOnEnter<GhostlyGuise>()
            .ActivateOnEnter<ImpactArenaChange>()
            .ActivateOnEnter<Ghostcrusher>()
            .ActivateOnEnter<MaliciousMistRaidwide>()
            .ActivateOnEnter<Fleshbuster>()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<BitterRegret1>()
            .ActivateOnEnter<BitterRegret2>()
            .ActivateOnEnter<BitterRegret3>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 981, NameID = 12763)]
public class D073Traumerei(ModuleInit init) : BossModule(init, new(148, -433), new ArenaBoundsSquare(19.5f))
{
    public static readonly ArenaBoundsCustom CrossArena = BuildCross();

    private static ArenaBoundsCustom BuildCross()
    {
        var square = new ArenaBoundsSquare(19.5f);
        var remove = new PolygonClipper.Operand([.. CurveApprox.Rect(new WDir(1, 0), 20, 1.5f)]);
        remove.AddContour(CurveApprox.Rect(new WDir(0, 1), 1.5f, 20));
        return new(19.5f, square.Clipper.Difference(new(CurveApprox.Rect(new WDir(0, 1), 19.5f, 19.5f)), remove));
    }
}
