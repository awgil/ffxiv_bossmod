namespace BossMod.Dawntrail.Hunt.RankA.Nechuciho;

public enum OID : uint
{
    Boss = 0x452C, // R3.420, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    WordOfTheWood = 39872, // Boss->self, 5.0s cast, range 30 180-degree cone
    WhisperOfTheWood1 = 39493, // Boss->self, 3.0s cast, single-target, visual (cleaves: forward->right->left->back)
    WhisperOfTheWood2 = 39494, // Boss->self, 3.0s cast, single-target, visual (cleaves: left->forward->back->right)
    WhisperOfTheWood3 = 39495, // Boss->self, 3.0s cast, single-target, visual (cleaves: back->left->right->forward)
    WordOfTheWoodMulti = 39496, // Boss->self, 7.0s cast, single-target, visual (first cleave)
    WordOfTheWoodAOEForward = 39785, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWoodAOERearward = 39786, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWoodAOELeftward = 39787, // Boss->self, no cast, range 30 180-degree cone
    WordOfTheWoodAOERightward = 39788, // Boss->self, no cast, range 30 180-degree cone
    Level5DeathSentence = 39492, // Boss->self, 5.0s cast, range 30 circle, interruptible spell applying dooms
    SentinelRoar = 39491, // Boss->self, 5.0s cast, range 40 circle, raidwide
}

public enum SID : uint
{
    ForwardOmen = 4153, // Boss->Boss, extra=0x0
    RearwardOmen = 4154, // Boss->Boss, extra=0x0
    LeftwardOmen = 4155, // Boss->Boss, extra=0x0
    RightwardOmen = 4156, // Boss->Boss, extra=0x0
}

class WordOfTheWood(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WordOfTheWood), new AOEShapeCone(30, 90.Degrees()));

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

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        Angle? dir = (SID)status.ID switch
        {
            SID.ForwardOmen => 0.Degrees(),
            SID.RearwardOmen => 180.Degrees(),
            SID.LeftwardOmen => 90.Degrees(),
            SID.RightwardOmen => -90.Degrees(),
            _ => null
        };
        if (dir != null)
        {
            var prevDir = _directions.Count > 0 ? _directions[^1] : actor.Rotation;
            _directions.Add(prevDir + dir.Value);
            _nextActivation = WorldState.FutureTime(8.8f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        Angle? dir = (AID)spell.Action.ID switch
        {
            AID.WordOfTheWoodAOEForward => 0.Degrees(),
            AID.WordOfTheWoodAOERearward => 180.Degrees(),
            AID.WordOfTheWoodAOELeftward => 90.Degrees(),
            AID.WordOfTheWoodAOERightward => -90.Degrees(),
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
}

class Level5DeathSentence(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.Level5DeathSentence));
class SentinelRoar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SentinelRoar));

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
