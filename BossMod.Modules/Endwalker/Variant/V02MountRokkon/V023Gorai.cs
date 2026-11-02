namespace BossMod.Endwalker.Variant.V02MountRokkon.V023Gorai;

public enum OID : uint
{
    Boss = 0x3F5C, // R3.450, x1
    ShishuWhiteBaboon = 0x3F5D, // R1.020, x0 (spawn during fight)
    BallOfLevin = 0x3F60, // R0.69-2.3, x0 (spawn during fight)
    FlameAndSulphurFlame = 0x1EB893, // R0.500, EventObj type
    FlameAndSulphurRock = 0x1EB894, // R0.500, EventObj type
    Helper = 0x233C, // R0.500, Helper type
}

public enum AID : uint
{
    AutoAttack = 34004, // Boss->player, no cast, single-target
    Teleport = 34003, // Boss->location, no cast, single-target
    FlickeringFlame = 34005, // Boss->self, 3.0s cast, single-target, summons flames
    SulphuricStone = 34006, // Boss->self, 3.0s cast, single-target, summons stones
    FlameAndSulphur = 34007, // Boss->self, 3.0s cast, single-target, summons both
    BrazenBalladExpanding = 34008, // Boss->self, 5.0s cast, single-target, flame lines widen, stones become circles
    BrazenBalladSplitting = 34009, // Boss->self, 5.0s cast, single-target, flame lines split, stones become donuts
    FireSpreadExpand = 34010, // Helper->self, no cast, range 46 width 10 rect
    FireSpreadSplit = 34011, // Helper->self, no cast, range 46 width 5 rect
    FallingRockExpand = 34013, // Helper->self, no cast, range 11 circle
    FallingRockSplit = 34014, // Helper->self, no cast, range 5-16 donut
    Unenlightenment = 34048, // Boss->self, 5.0s cast, single-target
    UnenlightenmentAOE = 34049, // Helper->self, no cast, range 60 circle, raidwide
    ImpurePurgation = 34022, // Boss->self, 3.6s cast, single-target
    ImpurePurgationFirst = 34023, // Helper->self, 4.0s cast, range 60 45-degree cone
    ImpurePurgationSecond = 34024, // Helper->self, 6.0s cast, range 60 45-degree cone
    MalformedPrayer = 34017, // Boss->self, 4.0s cast, single-target, three sequential towers
    Burst = 34018, // Helper->self, no cast, range 4 circle, tower
    DramaticBurst = 34019, // Helper->self, no cast, range 60 circle, tower fail
    RousingReincarnation1 = 34015, // Boss->self, 5.0s cast, single-target
    RousingReincarnation2 = 34016, // Helper->player, no cast, single-target
    SpikeOfFlameVisual = 34020, // Boss->self, 2.6s cast, single-target
    SpikeOfFlameAOE = 34021, // Helper->location, 3.0s cast, range 5 circle, baited on each player
    StringSnapVisual = 34025, // Boss->self, 2.6s cast, single-target
    StringSnap1 = 34026, // Helper->self, 3.0s cast, range 10 circle
    StringSnap2 = 34027, // Helper->self, 5.0s cast, range 10-20 donut
    StringSnap3 = 34028, // Helper->self, 7.0s cast, range 20-30 donut
    TorchingTormentVisual = 34046, // Boss->player, 5.0s cast, single-target
    TorchingTorment = 34047, // Helper->player, no cast, range 6 circle, tankbuster
    PureShockVisual = 34029, // Boss->self, 2.5s cast, single-target, route 5
    PureShock = 34030, // Helper->self, 3.0s cast, range 40 circle, raidwide
    Pull = 34031, // Helper->player, no cast, single-target, pulls players into octagon
    WilyWall = 34032, // ShishuWhiteBaboon->self, 7.0s cast, single-target
    SelfDestruct = 34033, // ShishuWhiteBaboon->self, 30.0s cast, range 10 circle
    HumbleHammerVisual = 34038, // Boss->self, 5.0s cast, single-target, route 6
    HumbleHammer = 34039, // Helper->location, 3.0s cast, range 3 circle
    Thundercall = 34034, // Boss->self, 3.0s cast, single-target
    ShockLargeVisual = 34012, // BallOfLevin->self, 6.0s cast, range 18 circle
    ShockSmallVisual = 34035, // BallOfLevin->self, 17.0s cast, range 8 circle
    ShockSmall = 34036, // BallOfLevin->self, no cast, range 8 circle
    ShockLarge = 34037, // BallOfLevin->self, no cast, range 8 circle
    WorldlyPursuitFirstCCW = 34042, // Boss->self, 5.0s cast, range 60 width 20 cross, route 7
    WorldlyPursuitFirstCW = 34043, // Boss->self, 5.0s cast, range 60 width 20 cross
    WorldlyPursuitRest = 34044, // Boss->self, 1.5s cast, range 60 width 20 cross
    FightingSpiritsVisual = 34040, // Boss->self, 5.0s cast, single-target
    FightingSpirits = 34041, // Helper->self, 6.2s cast, range 30 circle, raidwide
    BiwaBreakerFirst = 34045, // Boss->self, 4.0s cast, range 30 circle, raidwide x5
    BiwaBreakerRest = 34513, // Boss->self, no cast, range 30 circle
}

public enum SID : uint
{
    SmallOrb = 2970, // Helper->BallOfLevin, extra=0x261
}

public enum IconID : uint
{
    Tankbuster = 344, // player
}

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Unenlightenment && Arena.Bounds.Radius > 20)
            _aoe = new(new AOEShapeRect(20, 20, 20), Arena.Center, default, Module.CastFinishAt(spell, 0.5f), Inverted: true);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x16 && state == 0x00020001)
        {
            _aoe = null;
            Arena.Bounds = new ArenaBoundsSquare(20);
        }
    }
}

class OctagonGaps(BossModule module) : Components.GenericAOEs(module)
{
    private BitMask _raised;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _raised.SetBits().Select(q => new AOEInstance(new AOEShapeDonut(7.3f, 9.7f), Module.Center + new WDir((q & 1) == 0 ? -10 : 10, q < 2 ? -10 : 10)));

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x3D and <= 0x40 && state != 0x00020001)
            _raised[index - 0x3D] = state == 0x00200010;
    }
}

class FlameAndSulphur(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _flameExpand = new(46, 5);
    private static readonly AOEShapeRect _flameSplit = new(46, 2.5f);
    private static readonly AOEShapeCircle _rockExpand = new(11);
    private static readonly AOEShapeDonut _rockSplit = new(5, 16);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var activation = Module.CastFinishAt(spell, 3.1f);
        switch ((AID)spell.Action.ID)
        {
            case AID.BrazenBalladExpanding:
                foreach (var f in Module.Enemies(OID.FlameAndSulphurFlame))
                    _aoes.Add(new(_flameExpand, f.Position, f.Rotation, activation));
                foreach (var r in Module.Enemies(OID.FlameAndSulphurRock))
                    _aoes.Add(new(_rockExpand, r.Position, r.Rotation, activation));
                break;
            case AID.BrazenBalladSplitting:
                foreach (var f in Module.Enemies(OID.FlameAndSulphurFlame))
                {
                    var offset = 7.5f * f.Rotation.ToDirection().OrthoL();
                    _aoes.Add(new(_flameSplit, f.Position + offset, f.Rotation, activation));
                    _aoes.Add(new(_flameSplit, f.Position - offset, f.Rotation, activation));
                }
                foreach (var r in Module.Enemies(OID.FlameAndSulphurRock))
                    _aoes.Add(new(_rockSplit, r.Position, r.Rotation, activation));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FireSpreadExpand or AID.FireSpreadSplit or AID.FallingRockExpand or AID.FallingRockSplit)
            _aoes.Clear();
    }
}

class ImpurePurgation(BossModule module) : Components.GroupedAOEs(module, [AID.ImpurePurgationFirst, AID.ImpurePurgationSecond], new AOEShapeCone(60, 22.5f.Degrees()), 4);

class MalformedPrayer(BossModule module) : Components.GenericTowers(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x1F and <= 0x26 && state == 0x00020001)
        {
            Towers.Add(new(Module.Center + 11 * (180 - 45 * (index - 0x1F)).Degrees().ToDirection(), 4, activation: WorldState.FutureTime(9)));
            UpdateSoakers();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Burst && Towers.Count > 0)
        {
            ++NumCasts;
            Towers.RemoveAt(0);
            UpdateSoakers();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.FindComponent<ImpurePurgation>() is { Casters.Count: > 0 } && Towers.Count > 0 && !Towers[0].IsInside(actor))
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.Center, 3));
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }

    private void UpdateSoakers()
    {
        for (var i = 0; i < Towers.Count; ++i)
        {
            var t = Towers[i];
            (t.MinSoakers, t.MaxSoakers) = i == 0 ? (1, 4) : (0, 0);
            Towers[i] = t;
        }
    }
}

class Thundercall(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private int _numOrbs;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BallOfLevin && _numOrbs++ < 2)
            _aoes.Add(new(new AOEShapeCircle(18), actor.Position, default, WorldState.FutureTime(10.8f)));
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.SmallOrb && (OID)actor.OID == OID.BallOfLevin)
            foreach (var orb in Module.Enemies(OID.BallOfLevin))
                _aoes.Add(new(new AOEShapeCircle(orb == actor ? 8 : 18), orb.Position, default, WorldState.FutureTime(10)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ShockSmall or AID.ShockLarge)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}

class WorldlyPursuit(BossModule module) : Components.GenericRotatingAOE(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var aid = (AID)spell.Action.ID;
        if (aid is AID.WorldlyPursuitFirstCW or AID.WorldlyPursuitFirstCCW)
            Sequences.Add(new(new AOEShapeCross(60, 10), spell.LocXZ, spell.Rotation, aid == AID.WorldlyPursuitFirstCW ? -22.5f.Degrees() : 22.5f.Degrees(), Module.CastFinishAt(spell), 3.7f, 5, 1));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WorldlyPursuitFirstCW or AID.WorldlyPursuitFirstCCW or AID.WorldlyPursuitRest)
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
    }
}

class StringSnap(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StringSnap1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.StringSnap1 => 0,
            AID.StringSnap2 => 1,
            AID.StringSnap3 => 2,
            _ => -1
        };
        AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2));
    }
}

class Unenlightenment(BossModule module) : Components.RaidwideCastDelay(module, AID.Unenlightenment, AID.UnenlightenmentAOE, 0.5f);
class SpikeOfFlame(BossModule module) : Components.StandardAOEs(module, AID.SpikeOfFlameAOE, new AOEShapeCircle(5));
class TorchingTorment(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.Tankbuster, AID.TorchingTorment, 5.9f, true, AIHints.PredictedDamageType.Tankbuster);
class PureShock(BossModule module) : Components.RaidwideCast(module, AID.PureShock);
class SelfDestruct(BossModule module) : Components.StandardAOEs(module, AID.SelfDestruct, new AOEShapeCircle(10));
class HumbleHammer(BossModule module) : Components.StandardAOEs(module, AID.HumbleHammer, new AOEShapeCircle(3));
class FightingSpirits(BossModule module) : Components.RaidwideCast(module, AID.FightingSpirits);
class BiwaBreaker(BossModule module) : Components.RaidwideCast(module, AID.BiwaBreakerFirst, "Raidwide x5");

class V023GoraiStates : StateMachineBuilder
{
    public V023GoraiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<OctagonGaps>()
            .ActivateOnEnter<Unenlightenment>()
            .ActivateOnEnter<TorchingTorment>()
            .ActivateOnEnter<FightingSpirits>()
            .ActivateOnEnter<BiwaBreaker>()
            .ActivateOnEnter<StringSnap>()
            .ActivateOnEnter<SpikeOfFlame>()
            .ActivateOnEnter<FlameAndSulphur>()
            .ActivateOnEnter<MalformedPrayer>()
            .ActivateOnEnter<ImpurePurgation>()
            .ActivateOnEnter<PureShock>()
            .ActivateOnEnter<SelfDestruct>()
            .ActivateOnEnter<HumbleHammer>()
            .ActivateOnEnter<Thundercall>()
            .ActivateOnEnter<WorldlyPursuit>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12373, SortOrder = 5)]
public class V023Gorai(ModuleInit init) : BossModule(init, new(741, -190), new ArenaBoundsSquare(22.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.ShishuWhiteBaboon), ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PrioritizeTargetsByOID(OID.ShishuWhiteBaboon, 1);
    }
}
