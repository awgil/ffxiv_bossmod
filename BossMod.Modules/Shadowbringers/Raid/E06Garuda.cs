namespace BossMod.Shadowbringers.Raid.E06Garuda;

public enum OID : uint
{
    Boss = 0x2D0E, // R3.500, Garuda
    Helper = 0x233C, // R0.500
    Ifrit = 0x2D0F, // R3.000
    Raktapaksa = 0x2D10, // R3.500
    TumultuousNexus = 0x2D11, // R1.500
    GreatBallOfFire = 0x2D12, // R1.500
    IfritClone = 0x2DA1, // R3.000
    RaktapaksaClone = 0x2DA2, // R3.500
}

public enum AID : uint
{
    AutoAttackGaruda = 870, // Boss->player, no cast, single-target
    AutoAttackIfrit = 872, // Ifrit/Raktapaksa->player, no cast, single-target
    //_Ability_ = 19408, // Boss->location, no cast, single-target
    AirBumpVisual = 19409, // Boss->self, no cast, single-target
    OccludedFront = 19410, // Boss->self, 3.0s cast, single-target
    StrikeSpark = 19411, // Ifrit->self, 3.0s cast, single-target
    AirBumpVisualRaktapaksa = 19412, // Raktapaksa->self, no cast, single-target
    VacuumSlice = 19413, // Boss->self, 4.0s cast, range 42 width 8 rect
    IrresistiblePull = 19414, // Helper->self, 4.0s cast, single-target
    Superstorm = 19415, // Boss->self, 5.0s cast, range 40 circle
    Firestorm = 19416, // Helper->self, no cast, range 40 circle
    AirBump = 19417, // Helper->player, 5.5s cast, range 4 circle
    Thorns = 19418, // Helper->location, 4.0s cast, range 4 circle
    Downburst = 19419, // Helper->location, 4.0s cast, range 8 circle
    DownburstRaktapaksa = 19420, // Helper->location, 4.0s cast, range 8 circle
    FerostormAOE = 19421, // Helper->self, no cast, range 40 90-degree cone
    Ferostorm = 19422, // Boss->self, 5.0s cast, single-target
    FerostormVisual2 = 19423, // Boss->self, 5.0s cast, single-target
    StormOfFury = 19424, // Boss->self, 5.0s cast, range 11 circle
    StormOfFuryCone = 19425, // Helper->self, no cast, range 40 ?-degree cone
    Explosion = 19426, // TumultuousNexus->self, 1.0s cast, range 8 circle
    FerostormRaktapaksa = 19427, // Raktapaksa->self, 5.0s cast, single-target
    FerostormRaktapaksaRotated = 19428, // Raktapaksa->self, 5.0s cast, single-target
    FerostormRaktapaksaAOE = 19429, // Helper->self, no cast, range 40 90-degree cone
    StormOfFuryRaktapaksa = 19430, // Raktapaksa->self, 5.0s cast, range 11 circle
    StormOfFuryRaktapaksaCone = 19431, // Helper->self, no cast, range 40 ?-degree cone
    Touchdown = 19432, // Ifrit->location, 5.0s cast, range 40 circle
    HandsOfFlameCharge = 19433, // Ifrit/Raktapaksa->player, no cast, width 6 rect charge
    HandsOfHellCharge = 19434, // Ifrit/Raktapaksa->player, no cast, width 6 rect charge
    HandsOfHellCloneCharge = 19435, // IfritClone/RaktapaksaClone->player, no cast, width 6 rect charge
    HeatBurst = 19436, // Helper->self, 4.0s cast, range 37 width 80 rect
    InstantIncineration = 19437, // Ifrit->player, 5.0s cast, range 4 circle
    ConflagStrike = 19438, // Raktapaksa->self, 31.0s cast, range 40 270-degree cone
    HotFoot = 19439, // Ifrit->self, 5.0s cast, single-target
    SpikeOfFlame = 19440, // GreatBallOfFire->self, 1.0s cast, range 10 circle
    InfernoHowl = 19441, // Ifrit/Raktapaksa->self, 5.0s cast, range 40 circle
    RadiantPlume = 19442, // Helper->location, 5.0s cast, range 8 circle
    EruptionVisual = 19443, // Ifrit->self, 3.5s cast, single-target
    Eruption = 19444, // Helper->location, 3.5s cast, range 8 circle
    //HotFootKick = 19485, // Helper->GreatBallOfFire/TumultuousNexus, no cast, single-target
    HeatBurstVisual = 19486, // Helper->self, 3.5s cast, single-target
    DownburstKnockback = 19488, // Helper->location, 4.0s cast, range 40 circle
    HandsOfFlame = 19710, // Ifrit/Raktapaksa->self, 5.0s cast, single-target
    HandsOfHell = 19711, // Ifrit/Raktapaksa->self, 8.0s cast, single-target
    //_Weaponskill_3 = 19797, // Helper->self, no cast, single-target
    IrresistiblePullAttract = 20025, // Helper->self, 4.0s cast, range 21 width 42 rect
    IfritTeleport = 20376, // Ifrit->location, no cast, single-target
}

public enum TetherID : uint
{
    HandsOfFlame = 104, // player->Ifrit/Raktapaksa
    HandsOfHell = 106, // player->Ifrit/Raktapaksa
    StormOfFury = 109, // player->Boss/Raktapaksa
}

class Ferostorm(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone _shape = new(40, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var act = Module.CastFinishAt(spell);
        var r = spell.Rotation;
        switch ((AID)spell.Action.ID)
        {
            case AID.Ferostorm:
            case AID.FerostormRaktapaksa:
                _aoes.Add(new(_shape, caster.Position, r - 45.Degrees(), act));
                _aoes.Add(new(_shape, caster.Position, r + 135.Degrees(), act));
                break;
            case AID.FerostormVisual2:
            case AID.FerostormRaktapaksaRotated:
                _aoes.Add(new(_shape, caster.Position, r + 45.Degrees(), act));
                _aoes.Add(new(_shape, caster.Position, r - 135.Degrees(), act));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FerostormAOE or AID.FerostormRaktapaksaAOE)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}

class Superstorm(BossModule module) : Components.RaidwideCast(module, AID.Superstorm);
class Firestorm(BossModule module) : Components.RaidwideCast(module, AID.Firestorm);
class InfernoHowl(BossModule module) : Components.RaidwideCast(module, AID.InfernoHowl);

class AirBump(BossModule module) : Components.StackWithCastTargets(module, AID.AirBump, 4, 2, 2);
class Thorns(BossModule module) : Components.StandardAOEs(module, AID.Thorns, 4);

class Downburst(BossModule module) : Components.GroupedAOEs(module, [AID.Downburst, AID.DownburstRaktapaksa], new AOEShapeCircle(8));
class DownburstKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.DownburstKnockback, 7)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Sources(slot, actor))
        {
            if (IsImmune(slot, s.Activation))
                continue;
            var origin = s.Origin;
            var dist = s.Distance;
            hints.AddForbiddenZone(Sdf.Discrete(p => !Module.InBounds(AwayFromSource(p, origin, dist))), s.Activation);
        }
    }
}

class StormOfFury(BossModule module) : Components.GroupedAOEs(module, [AID.StormOfFury, AID.StormOfFuryRaktapaksa], new AOEShapeCircle(11));
class StormOfFuryTether(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(40, 15.Degrees()), (uint)TetherID.StormOfFury); // TODO: verify angle

class VacuumSlice(BossModule module) : Components.GenericAOEs(module, AID.VacuumSlice)
{
    public static readonly AOEShapeRect Shape = new(42, 4, 42);
    private AOEInstance? _aoe;

    public WPos? LineOrigin { get; private set; }
    public Angle? LineRotation { get; private set; }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe is { } aoe)
            yield return aoe;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            LineOrigin = caster.Position;
            LineRotation = spell.Rotation;
            _aoe = new(Shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            ++NumCasts;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        // idx 1: 0x00800040 / 0x00020001 appear, 0x02000004 / 0x00080004 clear
        if (index == 1 && state is 0x02000004 or 0x00080004)
        {
            _aoe = null;
            LineOrigin = null;
            LineRotation = null;
        }
    }
}

class IrresistiblePull(BossModule module) : Components.Knockback(module, AID.IrresistiblePullAttract)
{
    public const float Distance = 5;
    private readonly List<Actor> _casters = [];

    public bool Active => _casters.Count > 0;

    public static WPos NearestOnLine(WPos p, WPos origin, Angle rotation)
    {
        var dir = rotation.ToDirection();
        return origin + (p - origin).Dot(dir) * dir;
    }

    public static WPos Predict(WPos p, WPos lineOrigin, Angle lineRotation)
    {
        var nearest = NearestOnLine(p, lineOrigin, lineRotation);
        var offset = nearest - p;
        var len = offset.Length();
        return len > 0.1f ? p + offset.Normalized() * Math.Min(Distance, len) : p;
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_casters.Count == 0)
            yield break;
        var vacuum = Module.FindComponent<VacuumSlice>();
        if (vacuum?.LineOrigin is not { } origin || vacuum.LineRotation is not { } rot)
            yield break;

        // pulls toward the vacuum line
        yield return new(NearestOnLine(actor.Position, origin, rot), Distance, Module.CastFinishAt(_casters[0].CastInfo), Kind: Kind.TowardsOrigin);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var vacuum = Module.FindComponent<VacuumSlice>();
        if (vacuum?.LineOrigin is { } origin && vacuum.LineRotation is { } rot && VacuumSlice.Shape.Check(pos, origin, rot))
            return true;
        return base.DestinationUnsafe(slot, actor, pos);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_casters.Count == 0)
            return;
        var vacuum = Module.FindComponent<VacuumSlice>();
        if (vacuum?.LineOrigin is not { } lineOrigin || vacuum.LineRotation is not { } rot)
            return;
        var activation = Module.CastFinishAt(_casters[0].CastInfo);
        if (IsImmune(slot, activation))
            return;

        hints.AddForbiddenZone(Sdf.Discrete(p =>
        {
            var dest = Predict(p, lineOrigin, rot);
            return VacuumSlice.Shape.Check(dest, lineOrigin, rot) || !Module.InBounds(dest);
        }), activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Remove(caster);
    }
}

class Explosion(BossModule module) : Components.GenericAOEs(module, AID.Explosion)
{
    private readonly List<Actor> _orbs = [];
    private DateTime _activation;
    private static readonly AOEShapeCircle _shape = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default || _orbs.Count == 0)
            yield break;

        var pull = Module.FindComponent<IrresistiblePull>();
        var vacuum = Module.FindComponent<VacuumSlice>();
        var predict = pull?.Active == true && vacuum?.LineOrigin is not null;

        foreach (var orb in _orbs)
        {
            var pos = predict ? IrresistiblePull.Predict(orb.Position, vacuum!.LineOrigin!.Value, vacuum.LineRotation!.Value) : orb.Position;
            yield return new(_shape, pos, default, _activation);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.TumultuousNexus)
        {
            _orbs.Add(actor);
            if (_activation == default)
                _activation = WorldState.FutureTime(9.5f);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.IrresistiblePullAttract:
                _activation = Module.CastFinishAt(spell, 1.2f);
                break;
            case AID.Explosion:
                _activation = Module.CastFinishAt(spell);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _orbs.Remove(caster);
            if (_orbs.Count == 0)
                _activation = default;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (_orbs.Remove(actor) && _orbs.Count == 0)
            _activation = default;
    }
}

class Touchdown(BossModule module) : Components.ProximityAOEs(module, AID.Touchdown, 15);

class HandsOfFlame(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(40, 3), (uint)TetherID.HandsOfFlame, AID.HandsOfFlameCharge)
{
    public override void Update()
    {
        foreach (ref var b in CurrentBaits.AsSpan())
        {
            if (b.Shape is AOEShapeRect shape)
            {
                var length = (b.Target.Position - b.Source.Position).Length();
                if (shape.LengthFront != length)
                    b.Shape = shape with { LengthFront = length };
            }
        }
    }
}

class HandsOfHell(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(40, 3), (uint)TetherID.HandsOfHell)
{
    public override void Update()
    {
        foreach (ref var b in CurrentBaits.AsSpan())
        {
            if (b.Shape is AOEShapeRect shape)
            {
                var length = (b.Target.Position - b.Source.Position).Length();
                if (shape.LengthFront != length)
                    b.Shape = shape with { LengthFront = length };
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HandsOfHellCharge or AID.HandsOfHellCloneCharge)
        {
            ++NumCasts;
            CurrentBaits.RemoveAll(b => b.Source == caster || b.Target.InstanceID == spell.MainTargetID);
        }
    }
}

class InstantIncineration(BossModule module) : Components.SingleTargetCast(module, AID.InstantIncineration);
class Eruption(BossModule module) : Components.StandardAOEs(module, AID.Eruption, 8);
class RadiantPlume(BossModule module) : Components.StandardAOEs(module, AID.RadiantPlume, 8);
class HeatBurst(BossModule module) : Components.StandardAOEs(module, AID.HeatBurst, new AOEShapeRect(37, 40));
class ConflagStrike(BossModule module) : Components.StandardAOEs(module, AID.ConflagStrike, new AOEShapeCone(40, 135.Degrees()));

class HotFoot(BossModule module) : Components.GenericAOEs(module, AID.SpikeOfFlame)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle _shape = new(10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    private void Predict(WPos ifritPos, DateTime activation, bool refreshActivation = false)
    {
        var orbs = Module.Enemies(OID.GreatBallOfFire).Where(o => !o.IsDeadOrDestroyed).ToList();
        if (orbs.Count == 0)
            return;

        if (_aoes.Count == 0)
        {
            var kicked = orbs.MinBy(o => (o.Position - ifritPos).LengthSq())!; // ifrit tps next to the orb that gets kicked to center
            foreach (var orb in orbs)
                _aoes.Add(new(_shape, orb == kicked ? Arena.Center : orb.Position, default, activation));
        }
        else if (refreshActivation)
        {
            for (var i = 0; i < _aoes.Count; ++i)
                _aoes[i] = _aoes[i] with { Activation = activation };
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.IfritTeleport:
                if (_aoes.Count == 0) // ignore departure teleport
                    Predict(spell.TargetXZ, WorldState.FutureTime(6.5f));
                break;
            case AID.SpikeOfFlame:
                ++NumCasts;
                _aoes.RemoveAll(a => a.Origin.AlmostEqual(caster.Position, 1.5f));
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HotFoot)
            Predict(caster.Position, Module.CastFinishAt(spell, 1.5f), refreshActivation: true);
    }
}

class E06GarudaStates : StateMachineBuilder
{
    private readonly E06Garuda _module;

    public E06GarudaStates(E06Garuda module) : base(module)
    {
        _module = module;
        TrivialPhase()
            .ActivateOnEnter<Ferostorm>()
            .ActivateOnEnter<Superstorm>()
            .ActivateOnEnter<Firestorm>()
            .ActivateOnEnter<InfernoHowl>()
            .ActivateOnEnter<AirBump>()
            .ActivateOnEnter<Thorns>()
            .ActivateOnEnter<Downburst>()
            .ActivateOnEnter<DownburstKnockback>()
            .ActivateOnEnter<StormOfFury>()
            .ActivateOnEnter<StormOfFuryTether>()
            .ActivateOnEnter<VacuumSlice>()
            .ActivateOnEnter<IrresistiblePull>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<HandsOfFlame>()
            .ActivateOnEnter<HandsOfHell>()
            .ActivateOnEnter<InstantIncineration>()
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<RadiantPlume>()
            .ActivateOnEnter<HeatBurst>()
            .ActivateOnEnter<ConflagStrike>()
            .ActivateOnEnter<HotFoot>()
            .Raw.Update = () => _module.SeenRaktapaksa && (_module.Raktapaksa()?.IsDeadOrDestroyed ?? true);
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 719, NameID = 9287, PrimaryActorOID = (uint)OID.Boss)]
public class E06Garuda(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(19.5f))
{
    private Actor? _ifrit;
    private Actor? _raktapaksa;

    public bool SeenRaktapaksa { get; private set; }
    public Actor? Ifrit() => _ifrit;
    public Actor? Raktapaksa() => _raktapaksa;

    protected override void UpdateModule()
    {
        _ifrit ??= Enemies(OID.Ifrit).FirstOrDefault(a => a.IsTargetable);
        var rakta = Enemies(OID.Raktapaksa).FirstOrDefault(a => a.IsTargetable);
        if (rakta != null)
        {
            _raktapaksa = rakta;
            SeenRaktapaksa = true;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_ifrit, ArenaColor.Enemy);
        Arena.Actor(_raktapaksa, ArenaColor.Enemy);
    }
}
