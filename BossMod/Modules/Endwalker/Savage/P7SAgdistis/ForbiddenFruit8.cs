using System;

namespace BossMod.Endwalker.Savage.P7SAgdistis
{
    class ForbiddenFruit8 : ForbiddenFruitCommon
    {
        private BitMask _minotaurPlatforms; // index == id+1

        public ForbiddenFruit8() : base(ActionID.MakeSpell(AID.StymphalianStrike)) { }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (NumAssignedTethers == 0)
                return;

            BitMask destMask;
            var tetherSource = TetherSources[pcSlot];
            if (tetherSource != null)
            {
                arena.AddLine(tetherSource.Position, pc.Position, TetherColor(tetherSource));

                destMask = _minotaurPlatforms;
                destMask.Clear(PlatformIDFromOffset(tetherSource.Position - module.Bounds.Center) + 1);
            }
            else
            {
                destMask = ~_minotaurPlatforms;
            }
            arena.AddCircle(module.Bounds.Center + PlatformDirection(destMask.LowestSetBit() - 1).ToDirection() * Border.SmallPlatformOffset, Border.SmallPlatformRadius, ArenaColor.Safe);
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            base.OnTethered(module, source, tether);
            if ((TetherID)tether.ID is TetherID.MinotaurClose or TetherID.MinotaurFar)
            {
                _minotaurPlatforms.Set(PlatformIDFromOffset(source.Position - module.Bounds.Center) + 1);
            }
        }

        protected override DateTime? PredictUntetheredCastStart(BossModule module, Actor fruit) => (OID)fruit.OID == OID.ForbiddenFruitBird ? module.WorldState.CurrentTime.AddSeconds(12.5) : null;
    }
}
