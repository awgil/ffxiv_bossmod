namespace BossMod.Endwalker.Hunt.RankS.Burfurlur;

public enum OID : uint
{
    Boss = 0x360A, // R6.000, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Trolling = 27316, // Boss->self, no cast, range 10 circle
    QuintupleInhale1 = 27307, // Boss->self, 4.0s cast, range 27 45-degree cone
    QuintupleInhale24 = 27308, // Boss->self, 0.5s cast, range 27 45-degree cone
    QuintupleSneeze1 = 27309, // Boss->self, 5.0s cast, range 40 45-degree cone
    QuintupleSneeze24 = 27310, // Boss->self, 0.5s cast, range 40 45-degree cone
    QuintupleInhale35 = 27692, // Boss->self, 0.5s cast, range 27 45-degree cone
    QuintupleSneeze35 = 27693, // Boss->self, 0.5s cast, range 40 45-degree cone
    Uppercut = 27314, // Boss->self, 3.0s cast, range 15 120-degree cone
    RottenSpores = 27313, // Boss->location, 3.0s cast, range 6 circle
}

class QuintupleSneeze(BossModule module) : Components.GenericAOEs(module)
{
    private Angle _referenceAngle;
    private readonly List<Angle> _pendingOffsets = [];
    private DateTime _nextSneeze;

    private static readonly AOEShapeCone _shape = new(40, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_nextSneeze != default)
        {
            if (_pendingOffsets.Count > 1)
                yield return new(_shape, Module.PrimaryActor.Position, _referenceAngle + _pendingOffsets[1], _nextSneeze.AddSeconds(2.2f));
            if (_pendingOffsets.Count > 0)
                yield return new(_shape, Module.PrimaryActor.Position, _referenceAngle + _pendingOffsets[0], _nextSneeze, ArenaColor.Danger);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.QuintupleInhale1:
                _referenceAngle = spell.Rotation;
                _pendingOffsets.Clear();
                _pendingOffsets.Add(new());
                _nextSneeze = default;
                break;
            case AID.QuintupleInhale24:
            case AID.QuintupleInhale35:
                _pendingOffsets.Add(spell.Rotation - _referenceAngle);
                break;
            case AID.QuintupleSneeze1:
                _referenceAngle = spell.Rotation;
                _nextSneeze = Module.CastFinishAt(spell);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_pendingOffsets.Count > 0 && (AID)spell.Action.ID is AID.QuintupleSneeze1 or AID.QuintupleSneeze24 or AID.QuintupleSneeze35)
        {
            _pendingOffsets.RemoveAt(0);
            _nextSneeze = WorldState.FutureTime(2.2f);
        }
    }
}

class Uppercut(BossModule module) : Components.StandardAOEs(module, AID.Uppercut, new AOEShapeCone(15, 60.Degrees()));
class RottenSpores(BossModule module) : Components.StandardAOEs(module, AID.RottenSpores, 6);

class BurfurlurStates : StateMachineBuilder
{
    public BurfurlurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<QuintupleSneeze>()
            .ActivateOnEnter<Uppercut>()
            .ActivateOnEnter<RottenSpores>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 10617)]
public class Burfurlur(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
