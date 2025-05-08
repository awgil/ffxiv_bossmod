namespace BossMod.Dawntrail.Hunt.RankA.Nechuciho;

public enum OID : uint
{
    Boss = 0x452C, // R3.420, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    WordOfTheWood = 39872, // Boss->self, 5.0s cast, range 30 180-degree cone
    WhisperOfTheWoodFRLB = 39493, // Boss->self, 3.0s cast, single-target, visual (cleaves: forward->right->left->back)
    WhisperOfTheWoodLFBR = 39494, // Boss->self, 3.0s cast, single-target, visual (cleaves: left->forward->back->right)
    WhisperOfTheWoodBLRF = 39495, // Boss->self, 3.0s cast, single-target, visual (cleaves: back->left->right->forward)
    WordOfTheWoodMulti = 39496, // Boss->self, 7.0s cast, single-target, visual (first cleave)
    WordOfTheWoodAOEOldF = 39785, // Boss->self, no cast, range 30 180-degree cone, before 7.1
    WordOfTheWoodAOEOldB = 39786, // Boss->self, no cast, range 30 180-degree cone, before 7.1
    WordOfTheWoodAOEOldL = 39787, // Boss->self, no cast, range 30 180-degree cone, before 7.1
    WordOfTheWoodAOEOldR = 39788, // Boss->self, no cast, range 30 180-degree cone, before 7.1
    WordOfTheWoodAOEMidF = 42164, // Boss->self, no cast, range 30 180-degree cone, casts 1-3
    WordOfTheWoodAOEMidB = 42165, // Boss->self, no cast, range 30 180-degree cone, casts 1-3
    WordOfTheWoodAOEMidL = 42166, // Boss->self, no cast, range 30 180-degree cone, casts 1-3
    WordOfTheWoodAOEMidR = 42167, // Boss->self, no cast, range 30 180-degree cone, casts 1-3
    WordOfTheWoodAOEEndF = 42168, // Boss->self, no cast, range 30 180-degree cone, cast 4
    WordOfTheWoodAOEEndB = 42169, // Boss->self, no cast, range 30 180-degree cone, cast 4
    WordOfTheWoodAOEEndL = 42170, // Boss->self, no cast, range 30 180-degree cone, cast 4
    WordOfTheWoodAOEEndR = 42171, // Boss->self, no cast, range 30 180-degree cone, cast 4

    Level5DeathSentence = 39492, // Boss->self, 5.0s cast, range 30 circle, interruptible spell applying dooms
    SentinelRoar = 39491, // Boss->self, 5.0s cast, range 40 circle, raidwide
}

class WordOfTheWood(BossModule module) : Components.StandardAOEs(module, AID.WordOfTheWood, new AOEShapeCone(30, 90.Degrees()));

class WhisperOfTheWood(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Angle> _directions = [];
    private DateTime _nextActivation;

    private static readonly AOEShapeCone _shape = new(30, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_directions.Count > 1)
            yield return new(_shape, Module.PrimaryActor.Position, _directions[1], _nextActivation.AddSeconds(2), ArenaColor.AOE, false);
        if (_directions.Count > 0)
            yield return new(_shape, Module.PrimaryActor.Position, _directions[0], _nextActivation, ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.WhisperOfTheWoodFRLB:
                StartSequence(Module.CastFinishAt(spell, 9.6f), spell.Rotation, 0.Degrees(), -90.Degrees(), 90.Degrees(), 180.Degrees());
                break;
            case AID.WhisperOfTheWoodLFBR:
                StartSequence(Module.CastFinishAt(spell, 9.6f), spell.Rotation, 90.Degrees(), 0.Degrees(), 180.Degrees(), -90.Degrees());
                break;
            case AID.WhisperOfTheWoodBLRF:
                StartSequence(Module.CastFinishAt(spell, 9.6f), spell.Rotation, 180.Degrees(), 90.Degrees(), -90.Degrees(), 0.Degrees());
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        Angle? dir = (AID)spell.Action.ID switch
        {
            AID.WordOfTheWoodAOEOldF or AID.WordOfTheWoodAOEMidF or AID.WordOfTheWoodAOEEndF => 0.Degrees(),
            AID.WordOfTheWoodAOEOldB or AID.WordOfTheWoodAOEMidB or AID.WordOfTheWoodAOEEndB => 180.Degrees(),
            AID.WordOfTheWoodAOEOldL or AID.WordOfTheWoodAOEMidL or AID.WordOfTheWoodAOEEndL => 90.Degrees(),
            AID.WordOfTheWoodAOEOldR or AID.WordOfTheWoodAOEMidR or AID.WordOfTheWoodAOEEndR => -90.Degrees(),
            _ => null
        };
        if (dir == null)
            return;

        if (_directions.Count == 0)
        {
            ReportError($"Unexpected resolve aoe {spell.Action}, none expected");
            return;
        }

        if (!_directions[0].AlmostEqual(caster.Rotation, 0.1f))
            ReportError($"Unexpected rotation: {spell.Action} @ {caster.Rotation} (boss @ {Module.PrimaryActor.Rotation}), expected {_directions[0]}");
        _directions.RemoveAt(0);
        _nextActivation = WorldState.FutureTime(2);
    }

    private void StartSequence(DateTime activation, Angle starting, params Angle[] angles)
    {
        _nextActivation = activation;
        foreach (var a in angles)
        {
            starting += a;
            _directions.Add(starting);
        }
    }
}

class Level5DeathSentence(BossModule module) : Components.CastInterruptHint(module, AID.Level5DeathSentence);
class SentinelRoar(BossModule module) : Components.RaidwideCast(module, AID.SentinelRoar);

class NechucihoStates : StateMachineBuilder
{
    public NechucihoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WordOfTheWood>()
            .ActivateOnEnter<WhisperOfTheWood>()
            .ActivateOnEnter<Level5DeathSentence>()
            .ActivateOnEnter<SentinelRoar>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13362)]
public class Nechuciho(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
