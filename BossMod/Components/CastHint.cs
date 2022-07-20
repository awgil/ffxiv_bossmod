namespace BossMod.Components
{
    // generic component that is 'active' during specific primary target's cast
    // useful for simple bosses - outdoor, dungeons, etc.
    public class CastHint : CastCounter
    {
        public string Hint;

        public CastHint(ActionID action, string hint) : base(action)
        {
            Hint = hint;
        }

        public bool Active(BossModule module) => module.PrimaryActor.CastInfo?.Action == WatchedAction;

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (Active(module))
                hints.Add(Hint);
        }
    }
}
