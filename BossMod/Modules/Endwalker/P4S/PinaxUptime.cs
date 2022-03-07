using System.Numerics;

namespace BossMod.P4S
{
    using static BossModule;

    // component showing where to drag boss for max pinax uptime
    class PinaxUptime : Component
    {
        private P4S _module;
        private SettingTheScene _assignments;

        public PinaxUptime(P4S module)
        {
            _module = module;
            _assignments = module.FindComponent<SettingTheScene>()!;
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (pc.Role != Role.Tank)
                return;

            // draw position between lighting and fire squares
            var doubleOffset = _assignments.Direction(_assignments.Assignment(SettingTheScene.Element.Fire)) + _assignments.Direction(_assignments.Assignment(SettingTheScene.Element.Lightning));
            if (doubleOffset == Vector3.Zero)
                return;

            arena.AddCircle(arena.WorldCenter + 9 * doubleOffset, 2, arena.ColorSafe);
        }
    }
}
