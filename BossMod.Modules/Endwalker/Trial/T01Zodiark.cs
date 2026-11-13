namespace BossMod.Endwalker.Trial.T01Zodiark;

public enum OID : uint
{
    Boss = 0x324C, // R21.000
    Helper = 0x233C, // R0.500
    EsotericRay = 0x3592, // R1.000
    EsotericSquare = 0x3593, // R1.000
    EsotericTriangle = 0x3594, // R1.000
    Behemoth = 0x3659, // R1.000
    Python = 0x365A, // R1.000
}

public enum AID : uint
{
    AutoAttack = 27431, // Boss->player, no cast, single-target
    AutoAttack2 = 27764, // Boss->player, no cast, single-target
    Kokytos = 27687, // Boss->self, 4.0s cast, range 60 circle
    Kokytos2 = 27743, // Boss->self, 4.0s cast, range 60 circle
    Exoterikos = 27702, // Boss->self, 5.0s cast, single-target
    Exoterikos2 = 26561, // Boss->self, 5.0s cast, single-target
    TrimorphosExoterikos = 26562, // Boss->self, 13.0s cast, single-target
    EsotericSect = 26572, // EsotericTriangle->self, 6.0s cast, range 47 60-degree cone
    EsotericDyad = 26571, // EsotericSquare->self, 6.0s cast, range 21 width 42 rect
    TripleEsotericRay = 26564, // Boss->self, 7.0s cast, single-target
    EsotericRay = 26570, // EsotericRay->self, 6.0s cast, range 42 width 14 rect
    Ania = 26581, // Boss->self, 4.0s cast, single-target
    Ania2 = 26580, // Boss->self, 4.0s cast, single-target
    AniaAOE = 27490, // Helper->player, 5.0s cast, range 3 circle
    Paradeigma = 26568, // Boss->self, 3.0s cast, single-target
    Paradeigma2 = 26559, // Boss->self, 3.0s cast, single-target
    MeteorosEidolon = 26566, // Helper->self, 0.5s cast, range 15 circle
    OpheosEidolon = 26567, // Helper->self, no cast, range 42 width 11 rect
    Phlegethon = 26576, // Boss->self, 3.0s cast, single-target
    Phlegethon2 = 26575, // Boss->self, 3.0s cast, single-target
    PhlegethonAOE = 26574, // Helper->location, 3.5s cast, range 5 circle
    Styx = 26587, // Boss->self, 5.0s cast, single-target
    Styx2 = 26586, // Boss->self, 5.0s cast, single-target
    StyxAOE = 26588, // Helper->player, no cast, range 5 circle, five hits
    CompleteControl = 26420, // Boss->self, 5.0s cast, single-target
    AstralFlow = 28361, // Boss->self, 10.0s cast, single-target
    AstralFlowCW = 26159, // Boss->self, 10.0s cast, single-target
    AstralFlowCCW = 26160, // Boss->self, 10.0s cast, single-target
    Adikia = 26584, // Boss->self, 6.0s cast, single-target
    AdikiaL = 25512, // Helper->self, 7.0s cast, range 21 circle
    AdikiaR = 26585, // Helper->self, 7.8s cast, range 21 circle
    AlgedonNW = 26578, // Boss->self, 7.0s cast, single-target
    AlgedonNE = 26577, // Boss->self, 7.0s cast, single-target
    AlgedonAOE = 26579, // Helper->self, 8.0s cast, range 60 width 30 rect
    AstralEclipse = 26563, // Boss->self, 5.0s cast, single-target
    Explosion = 26573, // Helper->self, 3.0s cast, range 10 circle
}

public enum TetherID : uint
{
    Triangle = 164, // Boss->EsotericTriangle
    Square = 171, // Boss->EsotericSquare
}

public enum IconID : uint
{
    Ania = 218, // player
    Styx = 316, // player
}

class Kokytos(BossModule module) : Components.RaidwideCast(module, AID.Kokytos, "HP reduced to 1");
class Kokytos2(BossModule module) : Components.RaidwideCast(module, AID.Kokytos2, "HP reduced to 1");
class Ania(BossModule module) : Components.BaitAwayCast(module, AID.AniaAOE, new AOEShapeCircle(3), centerAtTarget: true);
class Phlegethon(BossModule module) : Components.StandardAOEs(module, AID.PhlegethonAOE, 5);
class Adikia(BossModule module) : Components.GroupedAOEs(module, [AID.AdikiaL, AID.AdikiaR], new AOEShapeCircle(21));
class Algedon(BossModule module) : Components.StandardAOEs(module, AID.AlgedonAOE, new AOEShapeRect(60, 15));

class Styx(BossModule module) : Components.UniformStackSpread(module, 5, 0, 8, stackHits: 5)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Styx)
        {
            Stacks.Clear();
            AddStack(WorldState.Actors.Find(targetID) ?? actor, WorldState.FutureTime(6.1f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.StyxAOE)
        {
            foreach (ref var stack in Stacks.AsSpan())
            {
                --stack.Hits;
                stack.Activation = WorldState.FutureTime(1.1f);
            }
            Stacks.RemoveAll(s => s.Hits == 0);
        }
    }
}

class Exoterikos(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Source, DateTime Activation)> _sigils = [];
    private static readonly AOEShapeCone _triangle = new(47, 30.Degrees());
    private static readonly AOEShapeRect _square = new(21, 21);
    private static readonly AOEShapeRect _ray = new(42, 7);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_sigils.Count == 0)
            yield break;
        var deadline = _sigils.Min(s => s.Activation).AddSeconds(1);
        foreach (var (source, activation) in _sigils)
            if (activation < deadline && Shape(source) is { } shape)
                yield return new(shape, source.Position, source.Rotation, activation);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source == Module.PrimaryActor && (TetherID)tether.ID is TetherID.Triangle or TetherID.Square
            && WorldState.Actors.Find(tether.Target) is { } sigil && Shape(sigil) != null)
        {
            var delay = (AID)(source.CastInfo?.Action.ID ?? 0) switch
            {
                AID.TrimorphosExoterikos => 9.7f,
                AID.Exoterikos2 => 13.3f,
                _ => 11.3f
            };
            Add(sigil, WorldState.FutureTime(delay));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.EsotericSect or AID.EsotericDyad or AID.EsotericRay)
            Add(caster, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.EsotericSect or AID.EsotericDyad or AID.EsotericRay)
            _sigils.RemoveAll(s => s.Source == caster);
    }

    private void Add(Actor source, DateTime activation)
    {
        _sigils.RemoveAll(s => s.Source == source);
        _sigils.Add((source, activation));
    }

    private static AOEShape? Shape(Actor actor) => (OID)actor.OID switch
    {
        OID.EsotericTriangle => _triangle,
        OID.EsotericSquare => _square,
        OID.EsotericRay => _ray,
        _ => null
    };
}

class Paradeigma(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(byte Index, AOEInstance AOE)> _aoes = [];
    private Angle _rotation;
    private bool _awaitingFlow;
    private static readonly AOEShapeCircle _behemoth = new(15);
    private static readonly AOEShapeRect _python = new(42, 5.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => _awaitingFlow ? [] : _aoes.Select(a => a.AOE with { Origin = RotatedPosition(a.AOE.Origin), Rotation = a.AOE.Rotation + _rotation });

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Paradeigma:
            case AID.Paradeigma2:
                _aoes.Clear();
                _rotation = default;
                _awaitingFlow = (AID)spell.Action.ID == AID.Paradeigma2;
                break;
            case AID.AstralFlow:
            case AID.AstralFlowCW:
            case AID.AstralFlowCCW:
                _awaitingFlow = false;
                _rotation = ((AID)spell.Action.ID == AID.AstralFlowCCW ? 90 : -90).Degrees();
                foreach (ref var a in _aoes.AsSpan())
                    a.AOE.Activation = Module.CastFinishAt(spell, (AID)spell.Action.ID == AID.AstralFlow ? 10.8f : a.AOE.Shape == _behemoth ? 6.3f : 5.9f);
                break;
            case AID.MeteorosEidolon:
                foreach (ref var a in _aoes.AsSpan())
                    if (a.AOE.Shape == _behemoth && RotatedPosition(a.AOE.Origin).AlmostEqual(caster.Position, 1))
                        a.AOE.Activation = Module.CastFinishAt(spell);
                break;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 2)
        {
            if (state is 0x00020001 or 0x00200010)
                _rotation = (state == 0x00020001 ? -90 : 90).Degrees();
            return;
        }
        if (index is < 9 or > 20)
            return;
        if (state == 0x00200010)
        {
            _aoes.RemoveAll(a => a.Index == index);
            if (index <= 12)
            {
                var offset = new WDir((index - 9) % 2 == 0 ? -10.5f : 10.5f, index < 11 ? -10.5f : 10.5f);
                _aoes.Add((index, new(_behemoth, Arena.Center + offset, Activation: WorldState.FutureTime(12.6f))));
            }
            else
            {
                var first = index % 2 == 1 ? -15.5f : -5.5f;
                var direction = index switch
                {
                    <= 14 => 0.Degrees(),
                    <= 16 => 180.Degrees(),
                    <= 18 => 90.Degrees(),
                    _ => -90.Degrees()
                };
                for (var i = 0; i < 2; ++i)
                {
                    var along = first + i * 21;
                    var offset = index switch
                    {
                        <= 14 => new WDir(along, -21),
                        <= 16 => new WDir(along, 21),
                        <= 18 => new WDir(-21, along),
                        _ => new WDir(21, along)
                    };
                    _aoes.Add((index, new(_python, Arena.Center + offset, direction, WorldState.FutureTime(11.1f))));
                }
            }
        }
        else if (state is 0x20001000 or 0x40000004 or 0x00080004 or 0x04000004)
        {
            var delay = state == 0x20001000 ? index <= 12 ? 5.6f : 5.05f : index <= 12 ? 0.6f : 0.06f;
            foreach (ref var a in _aoes.AsSpan())
                if (a.Index == index)
                    a.AOE.Activation = WorldState.FutureTime(delay);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.MeteorosEidolon or AID.OpheosEidolon)
            _aoes.RemoveAll(a => RotatedPosition(a.AOE.Origin).AlmostEqual(caster.Position, 1));
    }

    private WPos RotatedPosition(WPos origin)
    {
        var offset = origin - Arena.Center;
        return Arena.Center + (_rotation.Rad < 0 ? offset.OrthoR() : _rotation.Rad > 0 ? offset.OrthoL() : offset);
    }
}

class AstralEclipse(BossModule module) : Components.GenericAOEs(module, AID.Explosion)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle _shape = new(10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            yield break;
        var deadline = _aoes.Min(a => a.Activation).AddSeconds(1);
        foreach (var aoe in _aoes)
            if (aoe.Activation < deadline)
                yield return aoe;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if ((index, state) is not ((6, 0x10000800) or (7, 0x00200010) or (8, 0x00800040)))
            return;
        for (var y = -1; y <= 1; ++y)
            for (var x = -1; x <= 1; ++x)
            {
                var dangerous = state switch
                {
                    0x10000800 => y == 0 || x == -y,
                    0x00200010 => x != 0 && y != 0,
                    0x00800040 => x == 0 || y == 0,
                    _ => false
                };
                if (!dangerous)
                    continue;
                var offset = index switch
                {
                    6 => new WDir(-y, -x),
                    7 => new WDir(-x, y),
                    _ => new WDir(y, x)
                };
                _aoes.Add(new(_shape, Arena.Center + 14 * offset, Activation: WorldState.FutureTime(14.1f + index - 6)));
            }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AstralEclipse)
            _aoes.Clear();
        else if ((AID)spell.Action.ID == AID.Explosion)
        {
            var activation = Module.CastFinishAt(spell);
            _aoes.RemoveAll(a => a.Origin.AlmostEqual(spell.LocXZ, 1) && Math.Abs((a.Activation - activation).TotalSeconds) < 2);
            _aoes.Add(new(_shape, spell.LocXZ, Activation: activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Explosion)
        {
            ++NumCasts;
            _aoes.RemoveAll(a => a.Origin.AlmostEqual(caster.Position, 1) && a.Activation < WorldState.FutureTime(1));
        }
    }
}

class T01ZodiarkStates : StateMachineBuilder
{
    public T01ZodiarkStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Kokytos>()
            .ActivateOnEnter<Kokytos2>()
            .ActivateOnEnter<Ania>()
            .ActivateOnEnter<Styx>()
            .ActivateOnEnter<Exoterikos>()
            .ActivateOnEnter<Phlegethon>()
            .ActivateOnEnter<Adikia>()
            .ActivateOnEnter<Algedon>()
            .ActivateOnEnter<Paradeigma>()
            .ActivateOnEnter<AstralEclipse>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 802, NameID = 10456)]
public class T01Zodiark(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20));
