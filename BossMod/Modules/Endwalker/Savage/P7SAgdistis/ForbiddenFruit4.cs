using System;

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

            var tetherSource = TetherSources[pcSlot];
            if (tetherSource != null)
            {
                arena.AddLine(tetherSource.Position, pc.Position, TetherColor(tetherSource));

                WDir destOffset;
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
                arena.AddCircle(module.Bounds.Center + destOffset * Border.SmallPlatformOffset, Border.SmallPlatformRadius, ArenaColor.Safe);
            }
            else if (!MinotaursBaited)
            {
                arena.AddCircle(module.Bounds.Center - 2 * PlatformDirection(_bullPlatform).ToDirection(), 2, ArenaColor.Safe);
            }
        }

        protected override DateTime? PredictUntetheredCastStart(BossModule module, Actor fruit)
        {
            if ((OID)fruit.OID == OID.ForbiddenFruitBull)
                _bullPlatform = PlatformIDFromOffset(fruit.Position - module.Bounds.Center);
            return null;
        }
    }
}
