namespace BossMod.Endwalker.Alliance.A14Naldthal
{
    class HeavensTrialCone : Components.GenericBaitAway
    {
        private static AOEShapeCone _shape = new(60, 15.Degrees());

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.HeavensTrialConeStart:
                    var target = module.WorldState.Actors.Find(spell.MainTargetID);
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

    class HeavensTrialStack : Components.StackWithCastTargets
    {
        public HeavensTrialStack() : base(ActionID.MakeSpell(AID.HeavensTrialAOE), 6, 8) { }
    }
}
