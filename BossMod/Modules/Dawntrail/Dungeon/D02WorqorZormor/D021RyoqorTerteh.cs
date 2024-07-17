namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D021RyoqorTerteh;

public enum OID : uint
{
    Boss = 0x4159, // R5.280, x1
    Helper = 0x233C,
}

public enum AID : uint
{
    FrostingFracas = 36280, // 233C->self, 5.0s cast, range 60 circle
    IceScream = 36270, // 415B->self, 12.0s cast, range 20 width 20 rect
    FrozenSwirl = 36272, // 43A2->self, 12.0s cast, range 15 circle
    SnowBoulder = 36278, // 415C->self, 4.0s cast, range 50 width 6 rect
    SparklingSprinkling = 36281, // 233C->player, 5.0s cast, range 5 circle
}

public enum SID : uint
{
    Frozen = 3944, // none->_Gen_RorrlohTeh/_Gen_QorrlohTeh1, extra=0x0
    Frozen2 = 3445
}

class FrostingFracas(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FrostingFracas));

abstract class FreezableAOEs(BossModule module, ActionID action, AOEShape shape) : Components.GenericAOEs(module)
{
    protected Dictionary<Actor, bool> _casters = [];
    protected byte _numFrozen;
    protected bool _anyCastFinished;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_numFrozen < 2)
            yield break;

        foreach ((var caster, var isFrozen) in _casters)
        {
            var isCastReal = !isFrozen || _anyCastFinished;

            if (isCastReal)
                yield return new AOEInstance(shape, caster.Position, caster.Rotation, Module.CastFinishAt(caster.CastInfo, isFrozen ? 8 : 0));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == action)
            _casters[caster] = false;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == action)
            if (_casters.Remove(caster))
                _anyCastFinished = true;

        if (_casters.Count == 0)
        {
            _anyCastFinished = false;
            _numFrozen = 0;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.Frozen or SID.Frozen2 && _casters.ContainsKey(actor))
        {
            _casters[actor] = true;
            _numFrozen += 1;
        }
    }
}

class IceScream(BossModule module) : FreezableAOEs(module, ActionID.MakeSpell(AID.IceScream), new AOEShapeRect(20, 10));
class FrozenSwirl(BossModule module) : FreezableAOEs(module, ActionID.MakeSpell(AID.FrozenSwirl), new AOEShapeCircle(15));
class SparklingSprinkling(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SparklingSprinkling), 5);
class SnowBoulder(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SnowBoulder), new AOEShapeRect(50, 3), maxCasts: 6);

class D021RyoqorTertehStates : StateMachineBuilder
{
    public D021RyoqorTertehStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FrostingFracas>()
            .ActivateOnEnter<IceScream>()
            .ActivateOnEnter<FrozenSwirl>()
            .ActivateOnEnter<SparklingSprinkling>()
            .ActivateOnEnter<SnowBoulder>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12699)]
public class D021RyoqorTerteh(WorldState ws, Actor primary) : BossModule(ws, primary, new(-108, 119), new ArenaBoundsCircle(20));
