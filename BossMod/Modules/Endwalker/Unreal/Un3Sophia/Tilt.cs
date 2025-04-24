namespace BossMod.Endwalker.Unreal.Un3Sophia;

class Tilt(BossModule module) : Components.Knockback(module, AID.QuasarTilt)
{
    public const float DistanceShort = 28;
    public const float DistanceLong = 37;

    public float Distance;
    public Angle Direction;
    public DateTime Activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (Distance > 0)
            yield return new(new(), Distance, Activation, null, Direction, Kind.DirForward);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            Distance = 0;
    }
}

class ScalesOfWisdom(BossModule module) : Tilt(module)
{
    public bool RaidwideDone;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        switch ((AID)spell.Action.ID)
        {
            case AID.ScalesOfWisdomStart:
                // prepare for first tilt
                Distance = DistanceShort;
                Direction = -90.Degrees();
                Activation = WorldState.FutureTime(8);
                break;
            case AID.QuasarTilt:
                if (NumCasts == 1)
                {
                    // prepare for second tilt
                    Distance = DistanceShort;
                    Direction = 90.Degrees();
                    Activation = WorldState.FutureTime(4.9f);
                }
                break;
            case AID.ScalesOfWisdomRaidwide:
                RaidwideDone = true;
                break;
        }
    }
}

class Quasar(BossModule module) : Tilt(module)
{
    public int WeightLeft;
    public int WeightRight;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var weight = (AID)spell.Action.ID switch
        {
            AID.QuasarLight => 1,
            AID.QuasarHeavy => 3,
            _ => 0
        };
        if (weight != 0)
        {
            if (caster.Position.X < 0)
                WeightLeft += weight;
            else
                WeightRight += weight;

            Distance = (WeightLeft - WeightRight) switch
            {
                0 => 0,
                1 or -1 => DistanceShort,
                _ => DistanceLong
            };
            Direction = (WeightLeft > WeightRight ? -90 : 90).Degrees();
            Activation = Module.CastFinishAt(spell, 0.7f);
        }
    }
}
