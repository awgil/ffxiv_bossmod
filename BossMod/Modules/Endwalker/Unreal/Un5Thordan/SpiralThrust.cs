using System.Collections.Generic;

namespace BossMod.Endwalker.Unreal.Un5Thordan;

abstract class SpiralThrust : Components.GenericAOEs
{
    private float _predictionDelay;
    private List<AOEInstance> _aoes = new();

    private static AOEShapeRect _shape = new(54.2f, 6);

    public SpiralThrust(float predictionDelay) : base(ActionID.MakeSpell(AID.SpiralThrust))
    {
        _predictionDelay = predictionDelay;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            if (_predictionDelay > 0)
            {
                _predictionDelay = 0;
                _aoes.Clear();
            }
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.KnightAppear:
                if ((OID)caster.OID is OID.Vellguine or OID.Paulecrain or OID.Ignasse && (caster.Position - module.Bounds.Center).LengthSq() > 25 * 25)
                {
                    // prediction
                    _aoes.Add(new(_shape, caster.Position, Angle.FromDirection(module.Bounds.Center - caster.Position), module.WorldState.CurrentTime.AddSeconds(_predictionDelay), risky: false));
                }
                break;
            case AID.SpiralThrust:
                ++NumCasts;
                break;
        }
    }
}

class SpiralThrust1 : SpiralThrust { public SpiralThrust1() : base(10) { } }
class SpiralThrust2 : SpiralThrust { public SpiralThrust2() : base(12.1f) { } }
