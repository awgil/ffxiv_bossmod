namespace BossMod.Shadowbringers.Alliance.A36FalseIdol;

class MixedSignals(BossModule module) : Components.GenericAOEs(module, AID.Crash)
{
    private DateTime _activation;
    private readonly bool[] _lanes = new bool[5];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
        {
            for (var i = 0; i < _lanes.Length; i++)
            {
                if (_lanes[i])
                {
                    var center = Arena.Center + new WDir(0, 10 * (i - 2));
                    yield return new AOEInstance(new AOEShapeRect(5, 25, 5), center, default, _activation);
                }
            }
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index >= 8 && index <= 0x0C && state == 0x00200010)
        {
            _lanes[index - 8] = true;
            _activation = WorldState.FutureTime(8.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Array.Fill(_lanes, false);
            _activation = default;
        }
    }
}
