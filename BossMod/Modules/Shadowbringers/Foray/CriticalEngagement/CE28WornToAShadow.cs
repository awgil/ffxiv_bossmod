namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE28WornToAShadow;

public enum OID : uint
{
    Boss = 0x31D0, // R7.500, x1
    Helper = 0x233C, // R0.500, x18
    AlkonostsShadow = 0x31D1, // R3.750-7.500, spawn during fight
    VorticalOrb1 = 0x3239, // R0.500, spawn during fight
    VorticalOrb2 = 0x323A, // R0.500, spawn during fight
    VorticalOrb3 = 0x31D2, // R0.500, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Stormcall = 24121, // Boss->self, 5.0s cast, single-target, visual
    Explosion = 24122, // VorticalOrb1/VorticalOrb2/VorticalOrb3->self, 1.5s cast, range 35 circle aoe
    BladedBeak = 24123, // Boss->player, 5.0s cast, single-target, tankbuster
    NihilitysSong = 24124, // Boss->self, 5.0s cast, single-target, visual
    NihilitysSongAOE = 24125, // Helper->self, no cast, ???, raidwide
    Fantod = 24126, // Boss->self, 2.0s cast, single-target, visual
    FantodAOE = 24127, // Helper->location, 3.0s cast, range 3 circle puddle
    Foreshadowing = 24128, // Boss->self, 5.0s cast, single-target, visual
    ShadowsCast = 24129, // AlkonostsShadow->Boss, 5.0s cast, single-target, visual (applies transfiguration status to caster and stores casted spell)
    FrigidPulse = 24130, // Boss->self, 5.0s cast, range 8-25 donut
    PainStorm = 24131, // Boss->self, 5.0s cast, range 36 130-degree cone aoe
    PainfulGust = 24132, // Boss->self, 5.0s cast, range 20 circle aoe
    ForeshadowingPulse = 24133, // AlkonostsShadow->self, 5.0s cast, range 8-25 donut
    ForeshadowingStorm = 24134, // AlkonostsShadow->self, 5.0s cast, range 36 130-degree cone aoe
    ForeshadowingGust = 24135, // AlkonostsShadow->self, 5.0s cast, range 20 circle aoe
}

public enum SID : uint
{
    OrbMovement = 2234, // none->VorticalOrb1/VorticalOrb2/VorticalOrb3, extra=0x1E (fast)/0x49 (slow)
    Transfiguration = 705, // AlkonostsShadow->AlkonostsShadow, extra=0x1A4
}

public enum TetherID : uint
{
    Foreshadowing = 45, // AlkonostsShadow->Boss
}

class Stormcall(BossModule module) : Components.GenericAOEs(module, AID.Explosion)
{
    private readonly List<(Actor source, WPos dest, DateTime activation)> _sources = [];
    private static readonly AOEShapeCircle _shape = new(35);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _sources.Take(2).Select(e => new AOEInstance(_shape, e.dest, default, e.activation));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.OrbMovement)
        {
            var dest = Module.Center + 29 * (actor.Position - Module.Center).Normalized();
            _sources.Add((actor, dest, WorldState.FutureTime(status.Extra == 0x1E ? 9.7f : 19.9f)));
            _sources.SortBy(e => e.activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && _sources.FindIndex(e => e.source == caster) is var index && index >= 0)
            _sources[index] = (caster, caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _sources.RemoveAll(e => e.source == caster);
    }
}

class BladedBeak(BossModule module) : Components.SingleTargetCast(module, AID.BladedBeak);
class NihilitysSong(BossModule module) : Components.RaidwideCast(module, AID.NihilitysSong);
class Fantod(BossModule module) : Components.StandardAOEs(module, AID.FantodAOE, 3);

class Foreshadowing(BossModule module) : Components.GenericAOEs(module)
{
    private AOEShape? _bossAOE;
    private readonly List<(Actor caster, AOEShape? shape)> _addAOEs = []; // shape is null if add starts cast slightly before boss
    private DateTime _addActivation;

    private static readonly AOEShapeDonut _shapePulse = new(8, 25);
    private static readonly AOEShapeCone _shapeStorm = new(36, 65.Degrees());
    private static readonly AOEShapeCircle _shapeGust = new(20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_bossAOE != null)
            yield return new(_bossAOE, Module.PrimaryActor.Position, Module.PrimaryActor.CastInfo!.Rotation, Module.CastFinishAt(Module.PrimaryActor.CastInfo));

        if (_addActivation != default)
            foreach (var add in _addAOEs)
                if (add.shape != null)
                    yield return new(add.shape, add.caster.Position, add.caster.CastInfo?.Rotation ?? add.caster.Rotation, Module.CastFinishAt(add.caster.CastInfo, 0, _addActivation));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FrigidPulse:
                StartBossCast(_shapePulse);
                break;
            case AID.PainStorm:
                StartBossCast(_shapeStorm);
                break;
            case AID.PainfulGust:
                StartBossCast(_shapeGust);
                break;
            case AID.ShadowsCast:
                _addAOEs.Add((caster, _bossAOE)); // depending on timings, this might be null - will be updated when boss aoe starts
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FrigidPulse:
            case AID.PainStorm:
            case AID.PainfulGust:
                _bossAOE = null;
                if (_addAOEs.Count == 4)
                    _addActivation = WorldState.FutureTime(11.1f);
                break;
            case AID.ForeshadowingPulse:
            case AID.ForeshadowingStorm:
            case AID.ForeshadowingGust:
                _addAOEs.RemoveAll(e => e.caster == caster);
                break;
        }
    }

    private void StartBossCast(AOEShape shape)
    {
        _bossAOE = shape;
        _addActivation = new();
        for (int i = 0; i < _addAOEs.Count; ++i)
            if (_addAOEs[i].shape == null)
                _addAOEs[i] = (_addAOEs[i].caster, shape);
    }
}

class AlkonostStates : StateMachineBuilder
{
    public AlkonostStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Stormcall>()
            .ActivateOnEnter<BladedBeak>()
            .ActivateOnEnter<NihilitysSong>()
            .ActivateOnEnter<Fantod>()
            .ActivateOnEnter<Foreshadowing>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 778, NameID = 28)] // bnpcname=9973
public class Alkonost(WorldState ws, Actor primary) : BossModule(ws, primary, new(-480, -690), new ArenaBoundsCircle(30));
