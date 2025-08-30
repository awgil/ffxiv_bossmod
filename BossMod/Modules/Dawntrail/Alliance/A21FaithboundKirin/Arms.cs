namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

class Wringer(BossModule module) : Components.StandardAOEs(module, AID.WringerSlow, 14);
class DeadWringer(BossModule module) : Components.StandardAOEs(module, AID.DeadWringerSlow, new AOEShapeDonut(14, 30));
class Striking(BossModule module) : Components.GroupedAOEs(module, [AID.StrikingRightBoss, AID.StrikingLeftBoss], new AOEShapeCircle(10));
class Smiting(BossModule module) : Components.GroupedAOEs(module, [AID.SmitingRightSlow, AID.SmitingLeftSlow], new AOEShapeCircle(30));
class SynchronizedSequence(BossModule module) : Components.StandardAOEs(module, AID.SynchronizedSequence, new AOEShapeRect(60, 5));
class DoubleWringer(BossModule module) : Components.StandardAOEs(module, AID.DoubleWringer, 14);
class SmitingSequence(BossModule module) : Components.GroupedAOEs(module, [AID.SmitingRightSequence, AID.SmitingLeftSequence], new AOEShapeCircle(10));

class ArmsMulti(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private int _telegraphs;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        DateTime next = default;
        foreach (var aoe in _aoes)
        {
            if (next == default)
                next = aoe.Activation;

            if (aoe.Activation > next.AddSeconds(1))
                yield break;

            yield return aoe;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.WringerTelegraph:
                if (_telegraphs > 0)
                {
                    _aoes.Add(new(new AOEShapeCircle(14), caster.Position, Activation: WorldState.FutureTime(5.9f)));
                    _aoes.Add(new(new AOEShapeDonut(14, 30), caster.Position, Activation: WorldState.FutureTime(10.9f)));
                    _aoes.SortBy(a => a.Activation);
                }
                _telegraphs++;
                break;
            case AID.StrikingRightTelegraph:
                if (_telegraphs > 0)
                {
                    _aoes.Add(new(new AOEShapeCircle(10), spell.LocXZ, Activation: WorldState.FutureTime(5.9f)));
                    _aoes.Add(new(new AOEShapeCircle(30), new WPos(-872, 780), Activation: WorldState.FutureTime(10.9f)));
                    _aoes.SortBy(a => a.Activation);
                }
                _telegraphs++;
                break;
            case AID.StrikingLeftTelegraph:
                if (_telegraphs > 0)
                {
                    _aoes.Add(new(new AOEShapeCircle(10), spell.LocXZ, Activation: Module.CastFinishAt(spell, 7)));
                    _aoes.Add(new(new AOEShapeCircle(30), new WPos(-828, 780), Activation: Module.CastFinishAt(spell, 12.1f)));
                    _aoes.SortBy(a => a.Activation);
                }
                _telegraphs++;
                break;
            case AID.SynchronizedStrikeTelegraph:
                if (_telegraphs > 0)
                {
                    _aoes.Add(new(new AOEShapeRect(60, 5), spell.LocXZ, Activation: Module.CastFinishAt(spell, 7)));
                    var pos = spell.LocXZ.X > Arena.Center.X ? new WPos(-828, 750) : new WPos(-872, 750);
                    _aoes.Add(new(new AOEShapeRect(60, 16), pos, Activation: Module.CastFinishAt(spell, 12.1f)));
                    _aoes.SortBy(a => a.Activation);
                }
                _pairTelegraphs++;
                if (_pairTelegraphs > 1)
                {
                    _telegraphs++;
                    _pairTelegraphs = 0;
                }
                break;
        }
    }

    private int _pairTelegraphs;
    private int _pairCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.StrikingLeftFast:
            case AID.StrikingRightFast:
            case AID.SmitingLeftFast:
            case AID.SmitingRightFast:
            case AID.WringerFast:
            case AID.DeadWringerFast:
            case AID.SynchronizedSmite2Fast:
            case AID.SynchronizedSmite1Fast:
                Advance();
                break;
            case AID.SynchronizedStrikeFast:
                Advance(true);
                break;
        }
    }

    private void Advance(bool pair = false)
    {
        if (_aoes.Count > 0)
            _aoes.RemoveAt(0);

        if (pair)
        {
            _pairCasts++;
            if (_pairCasts > 1)
            {
                _pairCasts = 0;
                NumCasts++;
            }
        }
        else
            NumCasts++;
    }
}
