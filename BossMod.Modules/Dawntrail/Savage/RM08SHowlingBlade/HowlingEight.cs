namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class HowlingEight(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.HowlingEightFirst1 or AID.HowlingEightRest1)
            Towers.Add(new(caster.Position, 8, maxSoakers: 8, activation: Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HowlingEightFirst1:
            case AID.HowlingEightFirst2:
            case AID.HowlingEightFirst3:
            case AID.HowlingEightFirst4:
            case AID.HowlingEightFirst5:
            case AID.HowlingEightFirst6:
            case AID.HowlingEightFirst7:
            case AID.HowlingEightRest1:
            case AID.HowlingEightRest2:
            case AID.HowlingEightRest3:
            case AID.HowlingEightRest4:
            case AID.HowlingEightRest5:
            case AID.HowlingEightRest6:
            case AID.HowlingEightRest7:
                NumCasts++;
                break;
            case AID.HowlingEightFirst8:
            case AID.HowlingEightRest8:
                NumCasts++;
                Towers.Clear();
                break;
        }
    }
}

class MooncleaverEnrage(BossModule module) : Components.StandardAOEs(module, AID.MooncleaverEnrage, new AOEShapeCircle(8));
