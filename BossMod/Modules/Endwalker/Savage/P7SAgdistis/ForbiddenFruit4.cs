using System;

namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    class ForbiddenFruit4 : ForbiddenFruitCommon
    {
        private int _bullPlatform;

        public ForbiddenFruit4() : base(ActionID.MakeSpell(AID.BullishSwipeAOE)) { }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);
            if (NumAssignedTethers > 0 && !MinotaursBaited && TetherSources[pcSlot] == null)
            {
                arena.AddCircle(module.Bounds.Center - 2 * PlatformDirection(_bullPlatform).ToDirection(), 2, ArenaColor.Safe);
            }
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            var slot = TryAssignTether(module, source, tether);
            if (slot < 0)
                return;
            switch ((TetherID)tether.ID)
            {
                case TetherID.Bull:
                    SafePlatforms[slot].Set(_bullPlatform);
                    break;
                case TetherID.MinotaurFar:
                case TetherID.MinotaurClose:
                    var safePlatforms = ValidPlatformsMask;
                    safePlatforms.Clear(_bullPlatform);
                    safePlatforms.Clear(PlatformIDFromOffset(source.Position - module.Bounds.Center));
                    SafePlatforms[slot] = safePlatforms;
                    break;
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
