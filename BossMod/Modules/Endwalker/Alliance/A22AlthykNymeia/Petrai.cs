namespace BossMod.Endwalker.Alliance.A22AlthykNymeia
{
    class Petrai : Components.GenericSharedTankbuster
    {
        public Petrai() : base(ActionID.MakeSpell(AID.PetraiAOE), 6) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Petrai)
            {
                Source = caster;
                Target = module.WorldState.Actors.Find(spell.TargetID);
                Activation = spell.NPCFinishAt.AddSeconds(1);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (spell.Action == WatchedAction)
            {
                ++NumCasts;
                Source = Target = null;
            }
        }
    }
}
