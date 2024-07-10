namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D021RyoqorTerteh;

public enum OID : uint
{
    _Gen_Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    _Gen_Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    _Gen_QorrlohTeh = 0x43A2, // R0.500, x4
    Boss = 0x4159, // R5.280, x1
    Helper = 0x233C,
    _Gen_RyoqorTerteh = 0x233C, // R0.500, x4, 523 type
    _Gen_RorrlohTeh = 0x415B, // R1.500, x0 (spawn during fight)
    _Gen_QorrlohTeh1 = 0x415A, // R3.000, x0 (spawn during fight)
    _Gen_Snowball = 0x415C, // R2.500, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->player, no cast, single-target
    _Spell_FrostingFracas = 36280, // 233C->self, 5.0s cast, range 60 circle
    _Spell_FrostingFracas2 = 36279, // Boss->self, 5.0s cast, single-target
    _Spell_FluffleUp = 36265, // Boss->self, 4.0s cast, single-target
    _Spell_ColdFeat = 36266, // Boss->self, 4.0s cast, single-target
    _Spell_IceScream = 36270, // 415B->self, 12.0s cast, range 20 width 20 rect
    _Spell_FrozenSwirl = 36272, // 43A2->self, 12.0s cast, range 15 circle
    _Spell_FrozenSwirl2 = 36271, // 415A->self, 12.0s cast, single-target
    _Spell_Snowscoop = 36275, // Boss->self, 4.0s cast, single-target
    _Spell_SnowBoulder = 36278, // 415C->self, 4.0s cast, range 50 width 6 rect
    _Spell_SparklingSprinkling = 36713, // Boss->self, 5.0s cast, single-target
    _Spell_SparklingSprinkling2 = 36281, // 233C->player, 5.0s cast, range 5 circle
}

public enum SID : uint
{
    _Gen_1 = 2056, // none->_Gen_RorrlohTeh, extra=0x2C4
    _Gen_2 = 3944, // none->_Gen_RorrlohTeh/_Gen_QorrlohTeh1, extra=0x0
    _Gen_3 = 3445, // none->_Gen_RorrlohTeh/_Gen_QorrlohTeh1/_Gen_QorrlohTeh, extra=0xFFF6
}

class FrostingFracas(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Spell_FrostingFracas));

abstract class FreezableAOEs(BossModule module, ActionID action, AOEShape shape) : Components.GenericAOEs(module)
{
    protected Dictionary<Actor, bool> _casters = [];
    protected byte _numFrozen = 0;
    protected bool _anyCastFinished = false;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_numFrozen < 2)
            yield break;

        foreach ((var caster, var isFrozen) in _casters)
        {
            var isCastReal = !isFrozen || _anyCastFinished;

            if (isCastReal)
                yield return new AOEInstance(shape, caster.Position, caster.Rotation, caster.CastInfo!.NPCFinishAt + (isFrozen ? TimeSpan.FromSeconds(8) : default));
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

        if (!_casters.Any())
        {
            _anyCastFinished = false;
            _numFrozen = 0;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID._Gen_1 or SID._Gen_2 or SID._Gen_3)
        {
            if (_casters.ContainsKey(actor))
            {
                _casters[actor] = true;
                _numFrozen += 1;
            }
        }
    }
}

class IceScream(BossModule module) : FreezableAOEs(module, ActionID.MakeSpell(AID._Spell_IceScream), new AOEShapeRect(20, 10));
class FrozenSwirl(BossModule module) : FreezableAOEs(module, ActionID.MakeSpell(AID._Spell_FrozenSwirl), new AOEShapeCircle(15));
class SparklingSprinkling(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID._Spell_SparklingSprinkling2), 5);
class SnowBoulder(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_SnowBoulder), new AOEShapeRect(50, 3), maxCasts: 6);

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

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12699)]
public class D021RyoqorTerteh(WorldState ws, Actor primary) : BossModule(ws, primary, new(-108, 119), new ArenaBoundsCircle(20));
