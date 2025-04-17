namespace BossMod.Dawntrail.Hunt.RankS.Sansheya;

public enum OID : uint
{
    Boss = 0x43DD, // R4.000, x1
}
public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    VeilOfHeat = 39283, // Boss->self, 5.0s cast, range 15 circle
    HaloOfHeat = 39284, // Boss->self, 5.0s cast, range 10-40 donut
    FiresDomain = 39285, // Boss->players, 5.0s cast, width 6 rect charge
    TwinscorchL = 39601, // Boss->self, 4.0s cast, range 40 180-degree cone, visual (L->R cleaves)
    TwinscorchR = 39602, // Boss->self, 4.0s cast, range 40 180-degree cone, visual (R->L cleaves)
    ScorchingLeftFirst = 39557, // Boss->self, no cast, range 40 180-degree cone
    ScorchingRightFirst = 39558, // Boss->self, no cast, range 40 180-degree cone
    ScorchingLeftSecond = 39528, // Boss->self, 1.0s cast, range 40 180-degree cone
    ScorchingRightSecond = 39529, // Boss->self, 1.0s cast, range 40 180-degree cone
    PyreOfRebirth = 39288, // Boss->self, 4.0s cast, range 32 circle, raidwide + apply boiling
    TwinscorchedHaloL = 39289, // Boss->self, 5.0s cast, range 40 180-degree cone, visual (L->R->In)
    TwinscorchedVeilL = 39290, // Boss->self, 5.0s cast, range 40 180-degree cone, visual (L->R->Out)
    TwinscorchedHaloR = 39291, // Boss->self, 5.0s cast, range 40 180-degree cone, visual (R->L->In)
    TwinscorchedVeilR = 39292, // Boss->self, 5.0s cast, range 40 180-degree cone, visual (R->L->Out)
    VeilOfHeatShort = 39293, // Boss->self, 1.0s cast, range 15 circle
    HaloOfHeatShort = 39294, // Boss->self, 1.0s cast, range 10-40 donut
    CaptiveBolt = 39296, // Boss->players, 5.0s cast, range 6 circle stack
    CullingBlade = 39295, // Boss->self, 4.0s cast, range 80 circle, raidwide
}

public enum SID : uint
{
    Boiling = 4140, // Boss->player, extra=0x0
    Pyretic = 960, // none->player, extra=0x0
}

public enum IconID : uint
{
    FiresDomain = 23, // player
    CaptiveBolt = 62, // player
}

class VeilHaloTwinscorch(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shapeOut = new(15);
    private static readonly AOEShapeDonut _shapeIn = new(10, 40);
    private static readonly AOEShapeCone _shapeCleave = new(40, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var aoe in _aoes.Skip(1))
            yield return aoe with { Risky = false };
        if (_aoes.Count > 0)
            yield return _aoes[0] with { Color = ArenaColor.Danger };
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_aoes.Count > 1)
        {
            // if next is another cleave, stay close
            if (_aoes[1].Shape == _shapeCleave)
                hints.AddForbiddenZone(ShapeContains.Rect(_aoes[1].Origin, _aoes[1].Rotation, 40, -3, 40), _aoes[1].Activation);
            // if last is out, we should really stay at max melee, otherwise because of pyretic we might not get out in time
            if (_aoes[^1].Shape == _shapeOut)
                hints.AddForbiddenZone(ShapeContains.Circle(_aoes[^1].Origin, 6), _aoes[^1].Activation);
            // if last is in, just add it as-is, no reason to stay out
            if (_aoes[^1].Shape == _shapeIn)
                hints.AddForbiddenZone(_aoes[^1].Shape.CheckFn(_aoes[^1].Origin, default), _aoes[^1].Activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.VeilOfHeat:
                _aoes.Add(new(_shapeOut, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case AID.HaloOfHeat:
                _aoes.Add(new(_shapeIn, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case AID.TwinscorchL:
            case AID.TwinscorchR:
                _aoes.Add(new(_shapeCleave, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 0.2f)));
                _aoes.Add(new(_shapeCleave, caster.Position, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 2.4f)));
                break;
            case AID.TwinscorchedHaloL:
            case AID.TwinscorchedHaloR:
                _aoes.Add(new(_shapeCleave, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                _aoes.Add(new(_shapeCleave, caster.Position, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 2.2f)));
                _aoes.Add(new(_shapeIn, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 4.5f)));
                break;
            case AID.TwinscorchedVeilL:
            case AID.TwinscorchedVeilR:
                _aoes.Add(new(_shapeCleave, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                _aoes.Add(new(_shapeCleave, caster.Position, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 2.2f)));
                _aoes.Add(new(_shapeOut, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 4.5f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.VeilOfHeat or AID.HaloOfHeat or AID.ScorchingLeftFirst or AID.ScorchingRightFirst or AID.ScorchingLeftSecond or AID.ScorchingRightSecond or AID.VeilOfHeatShort or AID.HaloOfHeatShort && _aoes.Count > 0)
            _aoes.RemoveAt(0);
    }
}

class FiresDomain(BossModule module) : Components.GenericAOEs(module, AID.FiresDomain)
{
    private Actor? _target;
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_target != null)
        {
            var offset = _target.Position - Module.PrimaryActor.Position;
            yield return new(new AOEShapeRect(offset.Length(), 3), Module.PrimaryActor.Position, Angle.FromDirection(offset), _activation, Risky: actor != _target);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _target = WorldState.Actors.Find(spell.TargetID);
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _target = null;
            _activation = default;
        }
    }
}

class PyreOfRebirth(BossModule module) : Components.RaidwideCast(module, AID.PyreOfRebirth, "Raidwide + apply boiling");

class PyreOfRebirthPyretic(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var state = StateForStatus(status);
        if (state.Requirement != Requirement.None)
            SetState(Raid.FindSlot(actor.InstanceID), state);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        var state = StateForStatus(status);
        if (state.Requirement != Requirement.None)
            ClearState(Raid.FindSlot(actor.InstanceID), state.Priority);
    }

    private PlayerState StateForStatus(ActorStatus status) => (SID)status.ID switch
    {
        SID.Boiling => new(Requirement.Stay, status.ExpireAt),
        SID.Pyretic => new(Requirement.Stay, WorldState.CurrentTime, 1),
        _ => default
    };
}

class CaptiveBolt(BossModule module) : Components.StackWithCastTargets(module, AID.CaptiveBolt, 6, 4);
class CullingBlade(BossModule module) : Components.RaidwideCast(module, AID.CullingBlade);

class SansheyaStates : StateMachineBuilder
{
    public SansheyaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VeilHaloTwinscorch>()
            .ActivateOnEnter<FiresDomain>()
            .ActivateOnEnter<PyreOfRebirth>()
            .ActivateOnEnter<PyreOfRebirthPyretic>()
            .ActivateOnEnter<CaptiveBolt>()
            .ActivateOnEnter<CullingBlade>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 13399)]
public class Sansheya(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
