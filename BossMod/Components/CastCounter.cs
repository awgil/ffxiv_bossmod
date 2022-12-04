namespace BossMod.Components
{
    // generic component that counts specified casts
    public class CastCounter : BossComponent
    {
        public ActionID WatchedAction { get; private set; }
        public int NumCasts { get; protected set; } = 0;

        public CastCounter(ActionID aid)
        {
            WatchedAction = aid;
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (spell.Action == WatchedAction)
                ++NumCasts;
        }
    }
}
