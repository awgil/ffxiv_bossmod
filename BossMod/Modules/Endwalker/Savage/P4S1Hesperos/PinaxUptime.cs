namespace BossMod.Endwalker.Savage.P4S1Hesperos;

// component showing where to drag boss for max pinax uptime
class PinaxUptime(BossModule module) : BossComponent(module)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (pc.Role != Role.Tank)
            return;

        // draw position between lighting and fire squares
        var assignments = Module.FindComponent<SettingTheScene>()!;
        var doubleOffset = assignments.Direction(assignments.Assignment(SettingTheScene.Element.Fire)) + assignments.Direction(assignments.Assignment(SettingTheScene.Element.Lightning));
        if (doubleOffset == default)
            return;

        Arena.AddCircle(Module.Center + 9 * doubleOffset, 2, ArenaColor.Safe);
    }
}
