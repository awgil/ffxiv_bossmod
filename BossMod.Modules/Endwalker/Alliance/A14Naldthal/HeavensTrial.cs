namespace BossMod.Endwalker.Alliance.A14Naldthal;

class HeavensTrialCone(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeCone _shape = new(60, 15.Degrees());

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HeavensTrialConeStart:
                var target = WorldState.Actors.Find(spell.MainTargetID);
                if (target != null)
                    CurrentBaits.Add(new(caster, target, _shape));
                break;
            case AID.HeavensTrialSmelting:
                CurrentBaits.Clear();
                ++NumCasts;
                break;
        }
    }
}

class HeavensTrialStack(BossModule module) : Components.StackWithCastTargets(module, AID.HeavensTrialAOE, 6, 8);
