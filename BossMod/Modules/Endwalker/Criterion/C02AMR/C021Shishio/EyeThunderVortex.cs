namespace BossMod.Endwalker.Criterion.C02AMR.C021Shishio;

class EyeThunderVortex(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shapeCircle = new(15);
    private static readonly AOEShapeDonut _shapeDonut = new(8, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NEyeOfTheThunderVortexFirst:
            case AID.SEyeOfTheThunderVortexFirst:
                _aoes.Add(new(_shapeCircle, caster.Position, default, spell.NPCFinishAt));
                _aoes.Add(new(_shapeDonut, caster.Position, default, spell.NPCFinishAt.AddSeconds(4)));
                break;
            case AID.NVortexOfTheThunderEyeFirst:
            case AID.SVortexOfTheThunderEyeFirst:
                _aoes.Add(new(_shapeDonut, caster.Position, default, spell.NPCFinishAt));
                _aoes.Add(new(_shapeCircle, caster.Position, default, spell.NPCFinishAt.AddSeconds(4)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NEyeOfTheThunderVortexFirst or AID.NEyeOfTheThunderVortexSecond or AID.NVortexOfTheThunderEyeFirst or AID.NVortexOfTheThunderEyeSecond
            or AID.SEyeOfTheThunderVortexFirst or AID.SEyeOfTheThunderVortexSecond or AID.SVortexOfTheThunderEyeFirst or AID.SVortexOfTheThunderEyeSecond)
        {
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
            ++NumCasts;
        }
    }
}
