namespace BossMod.Endwalker.Savage.P9SKokytos
{
    class Uplift : Components.SelfTargetedAOEs
    {
        public Angle? WallDirection { get; private set; }

        public Uplift() : base(ActionID.MakeSpell(AID.Uplift), new AOEShapeRect(4, 8)) { }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (WallDirection != null)
            {
                for (int i = 0; i < 4; ++i)
                {
                    var center = WallDirection.Value + i * 90.Degrees();
                    arena.PathArcTo(module.Bounds.Center, module.Bounds.HalfSize - 0.5f, (center - 22.5f.Degrees()).Rad, (center + 22.5f.Degrees()).Rad);
                    arena.PathStroke(false, ArenaColor.Border, 2);
                }
            }
        }

        public override void OnEventEnvControl(BossModule module, byte index, uint state)
        {
            // state 00080004 => remove walls
            if (index is 2 or 3 && state == 0x00020001)
                WallDirection = (index == 2 ? 0 : 45).Degrees();
        }
    }
}
