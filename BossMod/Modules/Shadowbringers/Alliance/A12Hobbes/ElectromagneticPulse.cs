namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

class ElectromagneticPulse(BossModule module) : Components.GenericAOEs(module, AID.ElectromagneticPulse)
{
    enum Pattern
    {
        None,
        Odd,
        Even
    }
    private Pattern _pattern;

    public DateTime Activation { get; private set; }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var center = new WPos(-831, -225);
        var angleToCenter = 120.Degrees();
        var startPos = _pattern switch
        {
            Pattern.Odd => center + angleToCenter.ToDirection() * 20,
            Pattern.Even => center + angleToCenter.ToDirection() * 15,
            _ => default
        };
        if (startPos == default)
            yield break;

        for (var i = 0; i < 5; i++)
        {
            yield return new AOEInstance(new AOEShapeRect(5, 20), startPos, -60.Degrees(), Activation: Activation);
            startPos += 300.Degrees().ToDirection() * 10;
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 3)
        {
            if (state == 0x00400020)
            {
                _pattern = Pattern.Even;
                Activation = WorldState.FutureTime(4.2f);
            }
            else if (state == 0x02000100)
            {
                _pattern = Pattern.Odd;
                Activation = WorldState.FutureTime(4.2f);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _pattern = Pattern.None;
        }
    }
}
