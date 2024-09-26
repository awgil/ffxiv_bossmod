﻿using BossMod.Autorotation;

namespace BossMod;

class DebugAutorotation(RotationModuleManager autorot)
{
    private readonly UITree _tree = new();

    public void Draw()
    {
        var player = autorot.Bossmods.WorldState.Party[autorot.PlayerSlot];
        if (player == null)
            return;
        new AIHintsVisualizer(autorot.Hints, autorot.Bossmods.WorldState, player, 3).Draw(_tree);
    }
}
