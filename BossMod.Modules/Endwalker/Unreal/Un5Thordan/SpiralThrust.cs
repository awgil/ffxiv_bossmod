namespace BossMod.Endwalker.Unreal.Un5Thordan;

abstract class SpiralThrust(BossModule module, float predictionDelay) : Components.GenericAOEs(module, AID.SpiralThrust)
{
    private float _predictionDelay = predictionDelay;
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(54.2f, 6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            if (_predictionDelay > 0)
            {
                _predictionDelay = 0;
                _aoes.Clear();
            }
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.KnightAppear:
                if ((OID)caster.OID is OID.Vellguine or OID.Paulecrain or OID.Ignasse && (caster.Position - Module.Center).LengthSq() > 25 * 25)
                {
                    // prediction
                    _aoes.Add(new(_shape, caster.Position, Angle.FromDirection(Module.Center - caster.Position), WorldState.FutureTime(_predictionDelay), Risky: false));
                }
                break;
            case AID.SpiralThrust:
                ++NumCasts;
                break;
        }
    }
}

class SpiralThrust1(BossModule module) : SpiralThrust(module, 10);
class SpiralThrust2(BossModule module) : SpiralThrust(module, 12.1f);
