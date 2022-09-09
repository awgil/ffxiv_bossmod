namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    // common parts of various forbidden fruit / harvest mechanics
    // platform id's: 0 = S, -1 = W, +1 = E
    class ForbiddenFruitCommon : Components.CastCounter
    {
        public int NumAssignedTethers { get; private set; }
        public bool MinotaursBaited { get; private set; }
        protected Actor?[] TetherSources = new Actor?[8];

        public ForbiddenFruitCommon(ActionID watchedAction) : base(watchedAction) { }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if ((TetherID)tether.ID is TetherID.Bull or TetherID.MinotaurClose or TetherID.MinotaurFar or TetherID.Bird)
            {
                int slot = module.Raid.FindSlot(tether.Target);
                if (slot >= 0)
                {
                    TetherSources[slot] = source;
                    ++NumAssignedTethers;
                }
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.BullishSwipe)
                MinotaursBaited = true;
        }

        protected uint TetherColor(Actor source) => (OID)source.OID switch
        {
            OID.ImmatureMinotaur => 0xffff00ff,
            OID.BullTetherSource => 0xffffff00,
            OID.ImmatureStymphalide => 0xff00ffff,
            _ => 0
        };

        protected int PlatformIDFromOffset(WDir offset) => offset.Z > 0 ? 0 : offset.X > 0 ? 1 : -1;

        protected Angle PlatformDirection(int id) => id * 120.Degrees();

        protected int NextPlatform(int id) => id == 1 ? -1 : id + 1;
    }
}
