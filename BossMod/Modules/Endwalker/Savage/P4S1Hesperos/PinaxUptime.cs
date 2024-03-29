namespace BossMod.Endwalker.Savage.P4S1Hesperos;

// component showing where to drag boss for max pinax uptime
class PinaxUptime : BossComponent
{
    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (pc.Role != Role.Tank)
            return;

        // draw position between lighting and fire squares
        var assignments = module.FindComponent<SettingTheScene>()!;
        var doubleOffset = assignments.Direction(assignments.Assignment(SettingTheScene.Element.Fire)) + assignments.Direction(assignments.Assignment(SettingTheScene.Element.Lightning));
        if (doubleOffset == new WDir())
            return;

        arena.AddCircle(module.Bounds.Center + 9 * doubleOffset, 2, ArenaColor.Safe);
    }
}
