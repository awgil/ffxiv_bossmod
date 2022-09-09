namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    // TODO: improve (add hints? show aoes?)
    class ForbiddenFruit4 : ForbiddenFruitCommon
    {
        private int _bullPlatform;

        public ForbiddenFruit4() : base(ActionID.MakeSpell(AID.BullishSwipeAOE)) { }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (NumAssignedTethers == 0)
                return;

            WDir destOffset = new();
            var tetherSource = TetherSources[pcSlot];
            if (tetherSource != null)
            {
                arena.AddLine(tetherSource.Position, pc.Position, TetherColor(tetherSource));

                if ((OID)tetherSource.OID == OID.BullTetherSource)
                {
                    destOffset = PlatformDirection(_bullPlatform).ToDirection();
                }
                else
                {
                    var srcPlatform = PlatformIDFromOffset(tetherSource.Position - module.Bounds.Center);
                    var destPlatform = NextPlatform(srcPlatform);
                    if (destPlatform == _bullPlatform)
                        destPlatform = NextPlatform(destPlatform);
                    destOffset = PlatformDirection(destPlatform).ToDirection();
                }
            }
            arena.AddCircle(module.Bounds.Center + destOffset * Border.SmallPlatformOffset, Border.SmallPlatformRadius, ArenaColor.Safe);
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            base.OnTethered(module, source, tether);
            if ((TetherID)tether.ID == TetherID.Bull)
            {
                _bullPlatform = PlatformIDFromOffset(source.Position - module.Bounds.Center);
            }
        }
    }
}
